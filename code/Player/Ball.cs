namespace TableTennis;

public partial class Ball : ModelEntity
{
	/// <summary>
	/// Ignore whatever the fuck the server is telling us, I'm in charge bitch
	/// </summary>
	public bool ClientAuthoritative => true;

	private Vector3 _clientPosition;
	public override Vector3 Position
	{
		get
		{
			if ( ClientAuthoritative && !_clientPosition.IsNearlyZero() ) return _clientPosition;
			return base.Position;
		}
		set
		{
			base.Position = value;
			_clientPosition = value;
		}
	}

	// Velocity isn't networked by default... maybe cause we're keyframed phys?
	[Net]
	private Vector3 _velocity { get; set; }
	private Vector3 _clientVelocity { get; set; }
	public override Vector3 Velocity
	{
		get
		{
			if ( ClientAuthoritative && !_clientVelocity.IsNearlyZero() ) return _clientVelocity;
			return _velocity;
		}
		set
		{
			base.Velocity = value;
			_velocity = value;
			_clientVelocity = value;
		}
	}

	public TimeSince Created { get; set; }

	public Ball()
	{
		Predictable = true;
	}

	public override void Spawn()
	{
		SetModel( "models/tabletennis.ball.vmdl" );

		SetupPhysicsFromModel( PhysicsMotionType.Keyframed, false );

		EnableTraceAndQueries = true;

		PhysicsBody.Mass = BallPhysics.BallMass;
		Predictable = true;

		Tags.Add( "ball" );

		Created = 0;
	}

	public override void ClientSpawn()
	{
		base.ClientSpawn();

		_clientPosition = Position;
		_clientVelocity = Velocity;
	}

	public override void OnNewModel( Model model )
	{
		base.OnNewModel( model );

		if ( IsClient )
			Particles.Create( "particles/ball_trail/ball_trail.vpcf", this );
	}
}
