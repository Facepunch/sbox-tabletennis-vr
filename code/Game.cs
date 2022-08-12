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

		Audio.ReverbScale = 3f;
		Audio.ReverbVolume = 3f;
	}

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
	/// Client only
	/// </summary>
	public Ball Ball { get; set; }

	/// <summary>
	/// Gives a client's pawn the ability to serve the active ball.
	/// </summary>
	[ClientRpc]
	public void ClientServingBall( Client client )
	{
		// Passing client here cause we could do something for the other players
		if ( Local.Client != client ) return;

		if ( Ball.IsValid() ) Ball.Delete();
		Ball = new Ball();

		var twat = Local.Pawn as PlayerPawn;
		twat.ServeHand.SetBall( Ball );
	}

	public override void Simulate( Client cl )
	{
		base.Simulate( cl );

		// Everything here is server only
		if ( !IsServer ) return;

		if ( Input.VR.LeftHand.ButtonB.WasPressed )
		{
			ResetGame();
		}

		if ( DebugSpawnBallAlways )
		{
			var spawnButtonPressed = Input.VR.LeftHand.ButtonA.WasPressed || Input.Pressed( InputButton.Jump );
			if ( spawnButtonPressed )
				ClientServingBall( To.Everyone, cl );
		}
	}

	[ConCmd.Server]
	public static void IHitTheBallCunt( Vector3 position, Vector3 velocity, float time )
	{
		Current.SetBall( To.Multiple( Client.All.Where( c => c != ConsoleSystem.Caller ) ), position, velocity, time );
	}

	[ClientRpc]
	public void SetBall( Vector3 position, Vector3 velocity, float time )
	{
		Ball.Position = position;
		Ball.Velocity = velocity;
		// BallPhysics.Move( ClientBall, Time.Now - time ); // lag compensation, but it sucks - do it a different way.
	}

	public override void FrameSimulate( Client cl )
	{
		if ( cl.Pawn is not PlayerPawn pawn ) return;
		if ( !pawn.Paddle.IsValid() ) return;

		// Get where our paddle was last frame and where it is this frame, sweep along that path!
		var startPaddleTransform = pawn.Paddle.Transform;
		pawn.FrameSimulate( cl );
		var endPaddleTransform = pawn.Paddle.Transform;

		if ( !Ball.IsValid() ) return;

		// If we are serving the ball, don't simulate physics.
		if ( pawn.ServeHand.Ball == Ball )
			return;

		var hit = BallPhysics.Step( Ball, Time.Delta, pawn.Paddle, startPaddleTransform, endPaddleTransform );
		if ( hit )
		{
			IHitTheBallCunt( Ball.Position, Ball.Velocity, Time.Now );
		}
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
