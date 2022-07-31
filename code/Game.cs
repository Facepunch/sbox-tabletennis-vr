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
	public TableTennisGame()
	{
		if ( IsServer )
			_ = new HudEntity();
	}

	[Net] public Ball ActiveBall { get; set; }

	public Transform[] AnchorTransforms = new Transform[]
	{
		new Transform( new Vector3( 76.0f, 0, 0 ), Rotation.FromYaw( 180 ) ),
		new Transform( new Vector3( -76.0f, 0, 0 ), Rotation.FromYaw( 0 ) ),
	};

	TimeSince LastSpawn = 0;

	[Event.Tick.Server]
	public void BallTimer()
	{
		if ( LastSpawn < 10.0f ) return;
		//SpawnBall();
		//ActiveBall.Position = new Vector3( 72.0f, Rand.Float( -28.0f, 28.0f ), 56.0f );
		//ActiveBall.Velocity = Vector3.Backward * Rand.Float( 160.0f, 180.0f );
		//LastSpawn = 0;
	}

	public override void ClientJoined( Client cl )
	{
		base.ClientJoined( cl );

		cl.Pawn = new PlayerPawn();
		cl.Pawn.Transform = AnchorTransforms[cl.Id % 2];
	}

	public override void Simulate( Client cl )
	{
		base.Simulate( cl );

		if ( cl.Pawn is not PlayerPawn pawn ) return;
		if ( pawn.Paddle is null ) return;

		DebugOverlay.Sphere( Input.VR.LeftHand.Transform.Position, 3, Color.Blue );
		if ( Input.VR.LeftHand.ButtonA.WasPressed && IsServer )
		{
			SpawnBall();
			ActiveBall.Position = Input.VR.LeftHand.Transform.Position;
		}

		if ( !ActiveBall.IsValid() ) return;

		cl.Pawn.Transform = AnchorTransforms[cl.Id % 2];

		DebugOverlay.Sphere( cl.Pawn.Transform.Position, 5, Color.Green );

		// Debug for testing
		if ( !cl.IsUsingVr )
		{
			pawn.Paddle.Position = ActiveBall.Position.WithX( 62.0f ).WithZ( 35 );
			pawn.Paddle.Rotation = Rotation.FromRoll( 80 ) * Rotation.FromYaw( 30 );
			 
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
