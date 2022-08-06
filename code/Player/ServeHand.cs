namespace TableTennis;

public partial class ServeHand : AnimatedEntity
{
	protected static Material MaterialOverride = Material.Load( "materials/hands/vr_hand.vmat" );

	public ServeHand()
	{
		Predictable = true;
	}

	public override void Spawn()
	{
		SetModel( "models/hands/alyx_hand_left.vmdl" );

		SetupPhysicsFromModel( PhysicsMotionType.Keyframed, false );
		EnableTraceAndQueries = true;

		//PhysicsBody.Mass = BallPhysics.BallMass;
		Predictable = true;

		Tags.Add( "serve_hand" );
	}

	public override void OnNewModel( Model model )
	{
		base.OnNewModel( model );

		if ( IsClient )
		{
			Log.Info( $"Material Override: {MaterialOverride.ResourceName}" );
			SetMaterialOverride( MaterialOverride );
		}
	}
}
