namespace TableTennis;

/// <summary>
/// Provides deterministic predicted physics for Table Tennis
/// </summary>
/// <remarks>
/// Client should be authoritative on their paddle hitting the ball.
/// </remarks>
public static partial class BallPhysics
{
	//
	// Global tuning constants based on meter-kilogram-seconds (MKS) units. Here adjusted 
	// to inches for Source2  (1 inch ~ 2.5 cm <=> 1 cm ~ 0.4 inch <=> 1 m ~ 40 inch)
	//

	// Gravity ( 9.81m/s2 in MKS )
	public static readonly Vector3 Gravity = Vector3.Down * 386.1f;
	public static readonly float BallMass = 0.0027f; // 2.7g
	public static readonly float BallDiameter = 1.57f; // inches
	public static readonly float BounceFactor = 0.76f; // actual standard

	public static void Move( Ball ball )
	{
		var mover = new MoveHelper( ball.Position, ball.Velocity );
		mover.Trace = mover.Trace.Radius( BallDiameter / 2.0f ).Ignore( ball );
		mover.Bounce = BounceFactor;

		// Apply gravity
		mover.Velocity += Gravity * Time.Delta;

		// Apply drag ( TODO: This can be better )
		var drag = -ball.Velocity * ball.Velocity.Length * 0.0004f / (BallMass * 40);
		mover.Velocity += drag * Time.Delta;

		// TODO: Magnus factor if we're feeling fancy?

		mover.TryMove( Time.Delta );

		if ( mover.Hit )
		{
			// TODO: Different shit depending on surface
			Sound.FromWorld( "tabletennis.bounce", mover.HitPos );
			Particles.Create( "particles/ball_table_hit/ball_table_hit.vpcf",ball );
		}

		ball.Position = mover.Position;
		ball.Velocity = mover.Velocity;

		// DebugOverlay.Line( ball.Position, ball.Position + ball.Velocity.Normal * 32.0f );
	}

	public static void PaddleBall( Paddle paddle, Ball ball )
	{
		var trace = Trace.Ray( ball.Position, ball.Position + ball.Velocity * Time.Delta )
			.Radius( BallDiameter / 2.0f )
			.EntitiesOnly()
			.WithTag( "paddle" )
			.Run();

		if ( !trace.Hit ) return;
		if ( trace.Entity != paddle ) return;

		// DebugOverlay.Line( trace.StartPosition, trace.EndPosition );

		if ( trace.Hit )
		{
			// DebugOverlay.Sphere( trace.EndPosition, BallDiameter / 2, Color.Red );
			Sound.FromWorld( "tabletennis.paddle", trace.EndPosition ).SetVolume( 0.5f );

			ball.Velocity = trace.Normal * ball.Velocity.Length * 2f;
		}
	}
}
