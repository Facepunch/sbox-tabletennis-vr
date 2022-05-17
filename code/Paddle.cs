namespace TableTennis;

public partial class Paddle : ModelEntity
{
	public override void Spawn()
	{
		SetModel( "models/tabletennis.paddle.vmdl" );

		SetupPhysicsFromModel( PhysicsMotionType.Keyframed, false );

		CollisionGroup = CollisionGroup.Debris;
		EnableTraceAndQueries = true;

		Predictable = true;

		Tags.Add( "paddle" );
	}

	public override void Simulate( Client cl )
	{
		base.Simulate( cl );

		// cba doing predicted atm
		if ( !IsServer ) return;

		// TODO: Set position from VR hand

		if ( cl.IsUsingVr )
		{
			Transform = Input.VR.RightHand.Transform;
			Velocity = Input.VR.RightHand.Velocity;

			Transform = Transform.WithRotation( Transform.Rotation * Rotation.FromPitch( 90 ) );
		}

		if ( Game.Current is not TableTennisGame game ) return;
		if ( !game.ActiveBall.IsValid() ) return;

		using ( Prediction.Off() )
		{
			BallPhysics.PaddleBall( this, game.ActiveBall );
		}
	}

	public override void FrameSimulate( Client cl )
	{
		base.FrameSimulate( cl );

		if ( cl.IsUsingVr )
		{
			Transform = Input.VR.RightHand.Transform;
			Velocity = Input.VR.RightHand.Velocity;

			Transform = Transform.WithRotation( Transform.Rotation * Rotation.FromPitch( 90 ) );
		}
	}
}
