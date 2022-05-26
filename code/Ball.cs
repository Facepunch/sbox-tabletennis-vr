namespace TableTennis;

public partial class Ball : ModelEntity
{
	public Ball()
	{
		Predictable = true;
	}

	public override void Spawn()
	{
		SetModel( "models/tabletennis.ball.vmdl" );

		Particles.Create( "particles/ball_trail/ball_trail.vpcf", this );

		SetupPhysicsFromModel( PhysicsMotionType.Keyframed, false );

		CollisionGroup = CollisionGroup.Debris;
		EnableTraceAndQueries = true;

		PhysicsBody.Mass = BallPhysics.BallMass;

		Predictable = true;
	}
	
	[Event.Tick]
	public void Tick()
	{
		if ( !IsServer || !IsValid ) return;

		BallPhysics.Move( this );
	}

	public override void FrameSimulate( Client cl )
	{
		base.FrameSimulate( cl );

		// BallPhysics.Move( this );
	}
}
