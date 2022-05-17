global using Sandbox;
global using Sandbox.UI;
global using Sandbox.UI.Construct;
global using Sandbox.Component;
global using Hammer;

global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.ComponentModel;
global using System.Threading.Tasks;

namespace TableTennis;

public partial class TableTennisGame : Game
{
	public Ball ActiveBall { get; set; }

	TimeSince LastSpawn = 0;

	[Event.Tick.Server]
	public void BallTimer()
	{
		if ( LastSpawn < 3.0f ) return;
		SpawnBall();
		LastSpawn = 0;
	}

	public override void ClientJoined( Client cl )
	{
		base.ClientJoined( cl );

		cl.Pawn = new PlayerPawn();
	}

	public override void Simulate( Client cl )
	{
		base.Simulate( cl );

		if ( cl.Pawn is not PlayerPawn pawn ) return;
		if ( !ActiveBall.IsValid() ) return;
		if ( pawn.Paddle is null ) return;

		DebugOverlay.ScreenText( "F2 for devcam" );

		pawn.Paddle.Position = ActiveBall.Position.WithX( -62.0f ).WithZ( 35 );
		pawn.Paddle.Rotation = Rotation.FromRoll( 80 ) * Rotation.FromYaw( 30 );

		pawn.Paddle.Position += Vector3.Left * 5.0f;
	}

	public void SpawnBall()
	{
		if ( ActiveBall.IsValid() ) ActiveBall.Delete();

		ActiveBall = new Ball();
		ActiveBall.Position = new Vector3( 72.0f, Rand.Float( -28.0f, 28.0f ), 56.0f );
		ActiveBall.Velocity = Vector3.Backward * Rand.Float(160.0f, 180.0f);
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

		return camSetup;
	}
}
