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
		{
			_ = new HudEntity();

			BlueTeam = new Team.Blue();
			RedTeam = new Team.Red();
		}
	}

	[Net] public Ball ActiveBall { get; set; }

	[Net] public Team BlueTeam { get; set; }

	[Net] public Team RedTeam { get; set; }

	public void AddPlayerToTeam( Client cl )
	{
		if ( !BlueTeam.TryAdd( cl ) )
		{
			if ( !RedTeam.TryAdd( cl ) )
			{
				// TODO - Assign spectators
			}
		}
	}

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

		DebugOverlay.Sphere( Input.VR.LeftHand.Transform.Position, 3, Color.Blue );
		if ( ( Input.VR.LeftHand.ButtonA.WasPressed || Input.Pressed( InputButton.Jump ) ) && IsServer )
		{
			SpawnBall();

			if ( cl.IsUsingVr )
				ActiveBall.Position = Input.VR.LeftHand.Transform.Position;
			else
				ActiveBall.Position = ActiveBall.Position.WithX( 30.0f ).WithZ( 65 );
		}

		if ( !ActiveBall.IsValid() ) return;

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
