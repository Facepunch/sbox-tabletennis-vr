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
	TimeSince LastSpawn = 0;

	[Event.Tick.Server]
	public void BallTimer()
	{
		if ( LastSpawn < 3.0f ) return;
		SpawnBall();
		LastSpawn = 0;
	}

	[ServerCmd]
	public static void SpawnBall()
	{
		var ball = new Ball();
		ball.Position = new Vector3( 72.0f, Rand.Float( -28.0f, 28.0f ), 56.0f );
		ball.Velocity = Vector3.Backward * Rand.Float(160.0f, 180.0f);

		var paddle = new Paddle();
		paddle.Position = ball.Position.WithX( -62.0f ).WithZ( 35 );
		paddle.Rotation = Rotation.FromRoll( 75 ) * Rotation.FromPitch( -15 );

		paddle.Position += Vector3.Left * 5.0f;

		ball.DeleteAsync( 3.0f );
		paddle.DeleteAsync( 3.0f );
	}

	public override CameraSetup BuildCamera( CameraSetup camSetup )
	{
		camSetup.Position = new Vector3( 32, 88, 64 );
		camSetup.Rotation = Rotation.FromYaw( -105 ) * Rotation.FromPitch( 20 );
		camSetup.FieldOfView = 80;

		return base.BuildCamera( camSetup );
	}
}
