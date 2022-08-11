namespace TableTennis;

public partial class Ball : ModelEntity
{
	// HACK: Without this our position is getting stomped... and only because we're a clientonly ent HMM...
	private Vector3 _position { get; set; }
	public override Vector3 Position
	{
		get
		{
			if ( IsClientOnly ) return _position;
			return base.Position;
		}
		set
		{
			_position = value;
			base.Position = value;
		}
	}

	[Net]
	private Vector3 _netVelocity { get; set; }
	private Vector3 _velocity { get; set; }
	public override Vector3 Velocity
	{
		get
		{
			if ( IsClientOnly ) return _velocity;
			return _netVelocity;
		}
		set
		{
			if ( IsClientOnly ) _velocity = value;
			else _netVelocity = value;
		}
	}

	public override void Spawn()
	{
		SetModel( "models/tabletennis.ball.vmdl" );
		SetupPhysicsFromModel( PhysicsMotionType.Keyframed, false );
		Tags.Add( "ball" );

		EnableTraceAndQueries = true;

		if ( IsClientOnly )
		{
			Particles.Create( "particles/ball_trail/ball_trail.vpcf", this );
		}
		else
		{
			EnableDrawing = false;
			EnableTraceAndQueries = false;
			PhysicsEnabled = false;
		}
	}

	public bool IsOnSide( Client cl )
	{
		// Blue -x Red +x

		if ( TableTennisGame.Current.BlueTeam.Client == cl )
		{
			return Position.x < 0;
		}

		if ( TableTennisGame.Current.RedTeam.Client == cl )
		{
			return Position.x > 0;
		}

		return false;
	}
}
