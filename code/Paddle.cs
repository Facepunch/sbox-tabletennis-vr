namespace TableTennis;

public partial class Paddle : ModelEntity
{
	public override void Spawn()
	{
		SetModel( "models/tabletennis.paddle.vmdl" );

		SetupPhysicsFromModel( PhysicsMotionType.Keyframed, false );

		CollisionGroup = CollisionGroup.Default;
		EnableTraceAndQueries = true;

		Predictable = true;
	}
}
