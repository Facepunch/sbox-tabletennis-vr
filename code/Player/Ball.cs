namespace TableTennis;

public partial class Ball : ModelEntity
{
	// HACK: Without this our position is getting stomped... and only because we're a clientonly ent HMM...
	private Vector3 _position { get; set; }
	public override Vector3 Position
	{
		get => _position;
		set => _position = value;
	}

	// Keyframed PhysicsBody return no velocity, set our own
	private Vector3 _velocity { get; set; }
	public override Vector3 Velocity
	{
		get => _velocity;
		set => _velocity = value;
	}

	public override void Spawn()
	{
		Host.AssertClient();

		SetModel( "models/tabletennis.ball.vmdl" );
		SetupPhysicsFromModel( PhysicsMotionType.Keyframed, false );
		Tags.Add( "ball" );

		EnableTraceAndQueries = true;
		Particles.Create( "particles/ball_trail/ball_trail.vpcf", this );
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
