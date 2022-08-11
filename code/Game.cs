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

	/// <summary>
	/// Gives a client's pawn the ability to serve the active ball.
	/// </summary>
	[ClientRpc]
	public void GiveServingBall()
	{
		SpawnBall();

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

		if ( cl.Pawn is not PlayerPawn pawn || !pawn.Paddle.IsValid() ) return;
		if ( !ServerBall.IsValid() ) return;

		//
		// Set the ball position to wherever the AuthoritativeClient wants it
		//
		{
			ServerBall.Position = Input.Position;
			ServerBall.Velocity = Input.Cursor.Direction;
		}
	}

	public override void BuildInput( InputBuilder inputBuilder )
	{
		base.BuildInput( inputBuilder );

		if ( !ClientBall.IsValid() ) return;

		// Tell the server where I think the ball should be
		inputBuilder.Position = ClientBall.Position;
		inputBuilder.Cursor.Direction = ClientBall.Velocity; // This is just abusive, we need a way to do userdata in usercmd
	}

	public override void FrameSimulate( Client cl )
	{
		if ( cl.Pawn is not PlayerPawn pawn ) return;
		if ( !pawn.Paddle.IsValid() ) return;

		// Get where our paddle was last frame and where it is this frame, sweep along that path!
		var oldPaddleTransform = pawn.Paddle.Transform;
		pawn.FrameSimulate( cl );
		var newPaddleTransform = pawn.Paddle.Transform;

		// if ( DebugSpawnBallAlways )
		// {
			var spawnButtonPressed = Input.VR.LeftHand.ButtonA.WasPressed || Input.Pressed( InputButton.Jump );
			if ( spawnButtonPressed )
				GiveServingBall();
		// }

		if ( !ClientBall.IsValid() ) return;

		// If we are serving the ball, don't simulate physics.
		if ( pawn.ServeHand.Ball == ClientBall )
			return;

		// If the ball isnt on our side of the table, don't simulate just replicate the servers ball...
		// Actually maybe we can anyway, move this to Simulate?		
		if ( !ClientBall.IsOnSide( Local.Client ) )
		{
			ClientBall.Position = ServerBall.Position;
			ClientBall.Velocity = ServerBall.Velocity;
			return;
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

		// DebugOverlay.Text( "Ball", ActiveBall.Position );
		DebugOverlay.Text( "ClientBall", ClientBall.Position );
	}

	public void SpawnBall()
	{
		if ( ClientBall.IsValid() ) ClientBall.Delete();
		ClientBall = new Ball();
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
