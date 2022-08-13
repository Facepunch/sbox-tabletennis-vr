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
		else
		{
			// Send the client preferences to the server.
			ClientPreferences.Load();
		}

		Audio.ReverbScale = 3f;
		Audio.ReverbVolume = 3f;
	}

	/// <summary>
	/// Client only
	/// </summary>
	public Ball Ball { get; set; }

	public override void Simulate( Client cl )
	{
		base.Simulate( cl );

		// Everything here is server only
		if ( !IsServer ) return;

		if ( Input.VR.LeftHand.ButtonB.WasPressed )
		{
			ResetGame( true );
		}

		DebugSimulate( cl );
	}

	public override void FrameSimulate( Client cl )
	{
		if ( cl.Pawn is not PlayerPawn pawn ) return;
		if ( !pawn.Paddle.IsValid() ) return;

		// Get where our paddle was last frame and where it is this frame, sweep along that path!
		var startPaddleTransform = pawn.Paddle.Transform;
		pawn.FrameSimulate( cl ); // Set the paddle & hands positions
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

	public override void PostCameraSetup( ref CameraSetup camSetup )
	{
		if ( Local.Pawn != null )
		{
			VR.Anchor = ClientPreferences.LocalSettings.Anchor.GetTransform();
		
			Local.Pawn.PostCameraSetup( ref camSetup );
		}
	}
}
