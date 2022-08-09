namespace TableTennis;

public partial class Ball : ModelEntity
{
	/// <summary>
	/// Ignore whatever the fuck the server is telling us, I'm in charge bitch
	/// </summary>
	public override bool IsAuthority
	{
		get
		{
			return true;
		}
	}

	private Vector3 _clientPosition;
	public override Vector3 Position
	{
		get
		{
			if ( Host.IsClient && IsAuthority )
			{
				if ( _clientPosition.IsNearlyZero() )
					_clientPosition = base.Position;

				return _clientPosition;
			}
			return base.Position;
		}
		set
		{
			base.Position = value;
			_clientPosition = value;
		}
	}

	[Net]
	private Vector3 _velocity { get; set; }
	private Vector3 _clientVelocity { get; set; }
	public override Vector3 Velocity
	{
		get
		{
			if ( Host.IsClient && IsAuthority )
			{
				if ( _clientVelocity.IsNearlyZero() )
					_clientVelocity = _velocity;
				return _clientVelocity;
			}
			return _velocity;
		}
		set
		{
			_velocity = value;
			_clientVelocity = value;
		}
	}

	public TimeSince Created;

	public override void Spawn()
	{
		SetModel( "models/tabletennis.ball.vmdl" );
		SetupPhysicsFromModel( PhysicsMotionType.Keyframed, false );
		Tags.Add( "ball" );

		EnableTraceAndQueries = true;
		PhysicsBody.Mass = BallPhysics.BallMass;
		Predictable = true;

		Created = 0;
	}

	public override void ClientSpawn()
	{
		base.ClientSpawn();
		Particles.Create( "particles/ball_trail/ball_trail.vpcf", this );
	}
}
