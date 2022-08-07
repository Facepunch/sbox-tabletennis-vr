global using Sandbox;
global using Sandbox.UI;
global using Sandbox.UI.Construct;
global using Sandbox.Component;

global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.ComponentModel;
global using System.Threading.Tasks;

namespace TableTennis;

public partial class TableTennisGame : Game
{
	public static new TableTennisGame Current => Game.Current as TableTennisGame;

	public TableTennisGame()
	{
		if ( IsServer )
		{
			_ = new HudEntity();

			BlueTeam = new Team.Blue();
			RedTeam = new Team.Red();
		}

		Global.TickRate = 128;
	}

	[Net] public Ball ActiveBall { get; set; }

	public override void ClientJoined( Client cl )
	{
		SetupPlayer( cl );
	}

	public override void ClientDisconnect( Client cl, NetworkDisconnectionReason reason )
	{
		if ( cl.Pawn.IsValid() )
		{
			cl.Pawn.Delete();
			cl.Pawn = null;

			// Clear the player's team data
			cl.GetTeam()?.Reset();
		}

		HintWidget.AddMessage( To.Everyone, $"{cl.Name} left", $"avatar:{cl.PlayerId}" );
	}

	/// <summary>
	/// Gives a client's pawn the ability to serve the active ball.
	/// </summary>
	/// <param name="cl"></param>
	public void GiveServingBall( Client cl )
	{
		if ( cl.Pawn is not PlayerPawn pawn ) 
			return;
		
		SpawnBall();

		pawn.ServeHand.SetBall( ActiveBall );
	}

	public override void Simulate( Client cl )
	{
		base.Simulate( cl );

		// TODO - Remove me
		if ( IsServer )
		{
			if ( Input.VR.LeftHand.ButtonB.WasPressed )
			{
				ResetGame();
			}
		}

		if ( cl.Pawn is not PlayerPawn pawn ) return;
		if ( !pawn.Paddle.IsValid() ) return;

		if ( IsServer && DebugSpawnBallAlways )
		{
			var spawnButtonPressed = Input.VR.LeftHand.ButtonA.WasPressed || Input.Pressed( InputButton.Jump );
			if ( spawnButtonPressed )
				GiveServingBall( cl );
		}

		if ( !ActiveBall.IsValid() )
			return;

		// Debug for testing
		if ( DebugBallPhysics )
		{
			DebugPaddle = pawn.Paddle;
			pawn.Paddle.Position = ActiveBall.Position.WithX( 62.0f ).WithZ( 35 );
			pawn.Paddle.Position += Vector3.Left * 5.0f;
		}
	}

	public void SpawnBall()
	{
		if ( ActiveBall.IsValid() ) ActiveBall.Delete();
		ActiveBall = new Ball();

		Log.Info( $"{Host.Name} ball: {ActiveBall}" );
	}

	public override CameraSetup BuildCamera( CameraSetup camSetup )
	{
		var cam = FindActiveCamera();

		if ( LastCamera != cam )
		{
			LastCamera?.Deactivated();
			LastCamera = cam;
			LastCamera?.Activated();
		}

		cam?.Build( ref camSetup );

		// if we have no cam, lets use the pawn's eyes directly
		if ( cam == null && Local.Pawn != null )
		{
			camSetup.Position = new Vector3( -92, -12, 54 );
			camSetup.Rotation = Rotation.FromYaw( 10 ) * Rotation.FromPitch( 15 );
			camSetup.FieldOfView = 60;
		}

		PostCameraSetup( ref camSetup );

		return camSetup;
	}
}
