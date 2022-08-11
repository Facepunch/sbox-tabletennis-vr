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

			ServerBall = new Ball();
		}

		Audio.ReverbScale = 3f;
		Audio.ReverbVolume = 3f;
		Global.TickRate = 128; // I doubt this is needed this high now that everything is clientside :o
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
	/// Used to replicate the clients ball positions to each other and spectators.
	/// </summary>
	[Net] public Ball ServerBall { get; set; }

	/// <summary>
	/// Each client has their own ball, depending on where it is it's simulated themselves or replicates ServerBall.
	/// </summary>
	public Ball ClientBall { get; set; }

	public void GiveServingBall( Client cl )
	{
		if ( cl.Pawn is not PlayerPawn pawn ) return;

		if ( Host.IsServer )
		{
			if ( ServerBall.IsValid() ) ServerBall.Delete();
			ServerBall = new Ball() { Position = pawn.ServeHand.Position };
		}

		ServingBall( To.Single( cl ) );
	}

	/// <summary>
	/// Gives a client's pawn the ability to serve the active ball.
	/// </summary>
	[ClientRpc]
	public void ServingBall()
	{
		if ( ClientBall.IsValid() ) ClientBall.Delete();
		ClientBall = new Ball();

		var twat = Local.Pawn as PlayerPawn;
		twat.ServeHand.SetBall( ClientBall );
	}

	TimeSince lastChange = 1f;
	public override void Simulate( Client cl )
	{
		base.Simulate( cl );

		// Everything here is server only
		if ( !IsServer ) return;

		if ( Input.VR.LeftHand.ButtonB.WasPressed )
		{
			ResetGame();
		}

		// if ( DebugSpawnBallAlways )
		// {
		var spawnButtonPressed = Input.VR.LeftHand.ButtonA.WasPressed || Input.Pressed( InputButton.Jump );
		if ( spawnButtonPressed )
			GiveServingBall( cl );
		// }

		if ( cl.Pawn is not PlayerPawn pawn || !pawn.Paddle.IsValid() ) return;
		if ( !ServerBall.IsValid() ) return;

		// Just fully trust the client who gives a fuck
		if ( Input.Down( InputButton.Slot9 ) )
		{
			ServerBall.Position = Input.Position;
			ServerBall.Velocity = Input.Cursor.Direction;
		}
	}

	public override void BuildInput( InputBuilder inputBuilder )
	{
		base.BuildInput( inputBuilder );
		
		if ( !ClientBall.IsValid() ) return;

		// Only tell the server where I think the ball should be on my side
		if ( ClientBall.IsOnSide( Local.Client ) )
		{
			inputBuilder.Position = ClientBall.Position;
			inputBuilder.Cursor.Direction = ClientBall.Velocity; // This is just abusive, we need a way to do userdata in usercmd
			inputBuilder.SetButton( InputButton.Slot9, true ); // lol fucking hell
		}
		else
		{
			inputBuilder.SetButton( InputButton.Slot9, false );
		}

	}

	public override void FrameSimulate( Client cl )
	{
		if ( cl.Pawn is not PlayerPawn pawn ) return;
		if ( !pawn.Paddle.IsValid() ) return;

		// Get where our paddle was last frame and where it is this frame, sweep along that path!
		var oldPaddleTransform = pawn.Paddle.Transform;
		pawn.FrameSimulate( cl );
		var newPaddleTransform = pawn.Paddle.Transform;

		if ( !ClientBall.IsValid() ) return;

		// If we are serving the ball, don't simulate physics.
		if ( pawn.ServeHand.Ball == ClientBall )
			return;

		// DebugOverlay.Text( "ServerBall", ServerBall.Position );
		// DebugOverlay.Text( "ClientBall", ClientBall.Position );

		// If the ball isnt on our side of the table make sure we sync up with the server.. This should be in Simulate!!!
		if ( !ClientBall.IsOnSide( Local.Client ) && !ServerBall.IsOnSide( Local.Client ) )
		{
			ClientBall.Position = ServerBall.Position;
			ClientBall.Velocity = ServerBall.Velocity;
		}

		//
		// Simulate our physics with substeps
		// without substeps a headset locked at 90hz would frequently miss the ball with semi-fast movements
		//
		const float timeStep = 0.005f;
		bool hit = false;
		for ( float timeLeft = Time.Delta; timeLeft > 0.0f; timeLeft -= timeStep )
		{
			// Paddle every substep... But if we hit fuckin stop
			if ( !hit ) hit = BallPhysics.PaddleBall( pawn.Paddle, oldPaddleTransform, newPaddleTransform, ClientBall );
			
			// Do whatever we have left
			BallPhysics.Move( ClientBall, MathF.Min( timeStep, timeLeft ) );
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
