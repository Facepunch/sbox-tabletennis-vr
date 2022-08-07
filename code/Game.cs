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
		cl.Pawn = new PlayerPawn();

		AddPlayerToTeam( cl );

		HintWidget.AddMessage( To.Everyone, $"{cl.Name} joined", $"avatar:{cl.PlayerId}" );
	}

	public override void ClientDisconnect( Client cl, NetworkDisconnectionReason reason )
	{
		if ( cl.Pawn.IsValid() )
		{
			cl.Pawn.Delete();
			cl.Pawn = null;
		}

		HintWidget.AddMessage( To.Everyone, $"{cl.Name} left", $"avatar:{cl.PlayerId}" );
	}

	public override void Simulate( Client cl )
	{
		base.Simulate( cl );

		if ( cl.Pawn is not PlayerPawn pawn ) return;
		if ( !pawn.Paddle.IsValid() ) return;

		if ( ( Input.VR.LeftHand.ButtonA.WasPressed || Input.Pressed( InputButton.Jump ) ) && IsServer )
		{
			SpawnBall();

			if ( cl.IsUsingVr )
			{
				pawn.ServeHand.SetBall( ActiveBall );
			}
			else
				ActiveBall.Position = ActiveBall.Position.WithX( 30.0f ).WithZ( 65 );
		}

		if ( !ActiveBall.IsValid() ) return;

		// Debug for testing
		if ( BallSpawnDebug )
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
	}

	public override CameraSetup BuildCamera( CameraSetup camSetup )
	{
		var cam = FindActiveCamera();

		if ( LastCamera != cam )
		{
			LastCamera?.Deactivated();
			LastCamera = cam as CameraMode;
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

		// VR.Anchor = VR.Anchor.WithPosition( new Vector3( -72.0f, 0, 0.0f ) );

		return camSetup;
	}
}
