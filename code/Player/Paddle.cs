namespace TableTennis;

public partial class Paddle : ModelEntity
{
	Transform LocalTransform;

	public override void Spawn()
	{
		SetModel( "models/tabletennis.paddle.vmdl" );

		SetupPhysicsFromModel( PhysicsMotionType.Keyframed, false );

		EnableTraceAndQueries = true;
		Predictable = true;

		Tags.Add( "paddle" );
	}

	public override void Simulate( Client cl )
	{
		base.Simulate( cl );

		// cba doing predicted atm
		if ( !IsServer ) return;

		// This is solely just to sync with other players
		if ( cl.IsUsingVr )
		{
			Transform = Input.VR.RightHand.Transform;
			Velocity = Input.VR.RightHand.Velocity;
			AngularVelocity = Input.VR.RightHand.AngularVelocity;

			Transform = Transform.WithRotation( Transform.Rotation * Rotation.FromPitch( 90 ) * Rotation.FromYaw( 180 ) );
			Transform = Transform.WithPosition( Transform.Position + Transform.Rotation.Down * 2.0f );
		}
	}

	public override void FrameSimulate( Client cl )
	{
		base.FrameSimulate( cl );

		var oldTransform = LocalTransform;

		if ( cl.IsUsingVr )
		{
			Transform = Input.VR.RightHand.Transform;
			Velocity = Input.VR.RightHand.Velocity;
			AngularVelocity = Input.VR.RightHand.AngularVelocity;

			Transform = Transform.WithRotation( Transform.Rotation * Rotation.FromPitch( 90 ) * Rotation.FromYaw( 180 ) );
			Transform = Transform.WithPosition( Transform.Position + Transform.Rotation.Down * 2.0f );
		}

		var activeBall = TableTennisGame.Current.ActiveBall;
		
		Position = activeBall.Position.WithX( 62.0f ).WithZ( 35 );
		Position += Vector3.Left * 4.5f;
		
		{
			var pitch = MathF.Sin( -20 + TableTennisGame.Current.LastSpawn * 10 ) * 70;
			
			Rotation = Rotation.FromRoll( 80 ) * Rotation.FromYaw( -10 ) * Rotation.FromPitch( pitch );
			AngularVelocity = new Angles( 1500, 0, 0 );
		}

		if ( Game.Current is not TableTennisGame game ) return;
		if ( !game.ActiveBall.IsValid() ) return;

		LocalTransform = Transform;
		BallPhysics.PaddleBall( this, oldTransform, LocalTransform, game.ActiveBall );
	}
}
