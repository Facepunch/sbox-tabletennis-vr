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

	[Net] public Client AuthoritativeClient { get; set; }

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

		AuthoritativeClient = cl;
		pawn.ServeHand.SetBall( ActiveBall );
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

		if ( cl.Pawn is not PlayerPawn pawn ) return;
		if ( !pawn.Paddle.IsValid() ) return;

		if ( DebugSpawnBallAlways )
		{
			var spawnButtonPressed = Input.VR.LeftHand.ButtonA.WasPressed || Input.Pressed( InputButton.Jump );
			if ( spawnButtonPressed )
				GiveServingBall( cl );
		}

		if ( !ActiveBall.IsValid() )
			return;

		//
		// Figure out who the AuthoritativeClient is simply by looking at the side of the table
		// (Blue) -x +x (Red)
		//
		if ( !DebugBallPhysics )
		{
			var blueCl = BlueTeam.Client;
			var redCl = RedTeam.Client;
			if ( !redCl.IsValid() ) redCl = blueCl;

			AuthoritativeClient = (ActiveBall.Position.x < 0) ? blueCl : redCl;
		}

		//
		// Set the ball position to wherever the AuthoritativeClient wants it
		//
		if ( AuthoritativeClient == cl && ActiveBall.IsValid() && ActiveBall.Created > 0.1f )
		{
			ActiveBall.Position = Input.Position;
			ActiveBall.Velocity = Input.Cursor.Direction;
		}
	}

	public override void BuildInput( InputBuilder inputBuilder )
	{
		base.BuildInput( inputBuilder );

		if ( ActiveBall.IsValid() )
		{
			// Tell the server where I think the ball should be
			inputBuilder.Position = ActiveBall.Position;
			inputBuilder.Cursor.Direction = ActiveBall.Velocity; // This is just abusive, we need a way to do userdata in usercmd
		}
	}

	public override void FrameSimulate( Client cl )
	{
		if ( cl.Pawn is not PlayerPawn pawn ) return;
		if ( !pawn.Paddle.IsValid() ) return;

		// Get where our paddle was last frame and where it is this frame, sweep along that path!
		var oldPaddleTransform = pawn.Paddle.ClientTransform;
		pawn.FrameSimulate( cl );
		var newPaddleTransform = pawn.Paddle.ClientTransform;

		if ( !ActiveBall.IsValid() ) return;

		//
		// If we have no authority don't bother simulating, listen to whatever the server says
		//
		if ( AuthoritativeClient != cl )
			return;
		
		// If the ball is being held by something, don't simulate it.
		if ( ActiveBall.Parent.IsValid() )
			return;

		//
		// Simulate our physics with substeps
		// without substeps a headset locked at 90hz would frequently miss the ball with semi-fast movements
		//
		const float timeStep = 0.005f;
		bool hit = false;
		for ( float timeLeft = Time.Delta; timeLeft > 0.0f; timeLeft -= timeStep )
		{
			// Paddle every substep... But if we hit fuckin stop
			if ( !hit ) hit = BallPhysics.PaddleBall( pawn.Paddle, oldPaddleTransform, newPaddleTransform, ActiveBall );
			
			// Do whatever we have left
			BallPhysics.Move( ActiveBall, MathF.Min( timeStep, timeLeft ) );
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
