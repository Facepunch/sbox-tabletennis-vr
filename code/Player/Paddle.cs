namespace TableTennis;

public partial class Paddle : ModelEntity
{
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

		var oldTransform = Transform;

		if ( cl.IsUsingVr )
		{
			Transform = Input.VR.RightHand.Transform;
			Velocity = Input.VR.RightHand.Velocity;
			AngularVelocity = Input.VR.RightHand.AngularVelocity; // ?

			Transform = Transform.WithRotation( Transform.Rotation * Rotation.FromPitch( 90 ) * Rotation.FromYaw( 180 ) );
			Transform = Transform.WithPosition( Transform.Position + Transform.Rotation.Down * 2.0f );
		}

		if ( Game.Current is not TableTennisGame game ) return;
		if ( !game.ActiveBall.IsValid() ) return;
		
		using ( Prediction.Off() )
		{
			BallPhysics.PaddleBall( this, oldTransform, Transform, game.ActiveBall );
			BallPhysics.Move( game.ActiveBall ); // TODO: Do this after all players simulated and not each! ( I don't think tick event is what we want for that.. )
		}
	}

	public override void FrameSimulate( Client cl )
	{
		base.FrameSimulate( cl );

		if ( cl.IsUsingVr )
		{
			Transform = Input.VR.RightHand.Transform;
			Velocity = Input.VR.RightHand.Velocity;

			Transform = Transform.WithRotation( Transform.Rotation * Rotation.FromPitch( 90 ) * Rotation.FromYaw( 180 ) );
			Transform = Transform.WithPosition( Transform.Position + Transform.Rotation.Down * 2.0f );
		}


		if ( Game.Current is not TableTennisGame game ) return;
		if ( !game.ActiveBall.IsValid() ) return;

		//
		// Run our ball physics clientside each frame so it doesn't look like shit
		//
		// game.ActiveBall.FrameSimulate( cl );
	}
}
