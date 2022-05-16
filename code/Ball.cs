namespace TableTennis;

public partial class Ball : ModelEntity
{
	public override void Spawn()
	{
		SetModel( "models/tabletennis.ball.vmdl" );

		SetupPhysicsFromModel( PhysicsMotionType.Static, false );

		CollisionGroup = CollisionGroup.Debris;
		EnableTraceAndQueries = false;

		PhysicsBody.Mass = BallPhysics.BallMass;

		Predictable = true;
	}
	
	[Event.Tick]
	public void Tick()
	{
		if ( !IsServer || !IsValid ) return;

		BallPhysics.Move( this );
	}
}
