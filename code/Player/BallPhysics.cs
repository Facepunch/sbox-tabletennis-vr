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
	public static readonly float BallRadius = 0.785f; // inches

	//
	// Our desired COR ( coefficient of restitution ) of a ball bouncing on the table should be at least 0.76.
	// Based off the fact that average bouncing height should be at least 23cm from a 30cm drop
	// 
	// The International Table Tennis Federation specifies that the ball shall bounce up 24–26 cm when
	// dropped from a height of 30.5 cm on to a standard steel block thereby having a COR of 0.887 to 0.92.
	//
	// So our table probably has a COR of about 0.14 and our ball has a COR of 0.9.
	// We'll define these in our surface assets.
	//
	// https://sasportssience.blob.core.windows.net/ijtts/IJTTS_3_pdf%20files/IJTTS_3_17_49_Araki_Collisional.pdf
	//

	//
	// Just making my code easy to read/dev for now, throw this on the surface assets afterwards
	//
	public static readonly float BallCOR = 0.89f;
	public static readonly float PaddleRubberCOR = 0.05f;
	public static readonly float TableCOR = 0.1f;

	public static void Move( Ball ball )
	{
		if ( ball.Parent.IsValid() ) return;

		var velocity = ball.Velocity;
		var position = ball.Position;

		// Hit everything but the paddle
		var trace = Trace.Ray( 0, 0 )
			.Radius( BallRadius )
			.WorldAndEntities()
			.Ignore( ball )
			.WithoutTags( "paddle" );

		velocity += Gravity * Time.Delta;

		var drag = -velocity * velocity.Length * 0.0004f / (BallMass * 40); // MKS
		velocity += drag * Time.Delta;

		// TODO: Magnus factor if we're feeling fancy?

		{
			var timeLeft = Time.Delta;
			float travelFraction = 0;
			var hit = false;
			var hitPos = Vector3.Zero;
			Surface hitSurface = null;

			using var moveplanes = new VelocityClipPlanes( velocity );

			for ( int bump = 0; bump < moveplanes.Max; bump++ )
			{
				if ( velocity.Length.AlmostEqual( 0.0f ) )
					break;

				var pm = trace.FromTo( position, position + velocity * timeLeft ).Run();

				if ( pm.StartedSolid )
				{
					position += pm.Normal * 0.01f;

					continue;
				}

				travelFraction += pm.Fraction;

				if ( pm.Fraction > 0.0f )
				{
					position = pm.EndPosition + pm.Normal * 0.01f;

					moveplanes.StartBump( velocity );
				}

				if ( !hit && !pm.StartedSolid && pm.Hit )
				{
					hit = true;
					hitPos = pm.EndPosition;
					hitSurface = pm.Surface;

					// TODO: Different shit depending on surface
					Sound.FromWorld( "tabletennis.bounce", hitPos );
					Particles.Create( "particles/ball_table_hit/ball_table_hit.vpcf", hitPos );

					TableTennisGame.Current?.OnBallBounce( ball, hitPos );
				}

				timeLeft -= timeLeft * pm.Fraction;

				// Surface restitution "absorbs" kinetic energy of the ball
				// var bounce = 0.90f - pm.Surface.Elasticity;
				var bounce = BallCOR - TableCOR;

				if ( !moveplanes.TryAdd( pm.Normal, ref velocity, bounce ) )
					break;
			}

			if ( travelFraction == 0 )
				velocity = 0;
		}

		ball.Position = position;
		ball.Velocity = velocity;
	}

	/// <summary>
	/// Check if the paddle is hitting the ball
	/// </summary>
	public static void PaddleBall( Paddle paddle, Transform from, Transform to, Ball ball )
	{
		// Debug linear velocity at points
		/* for ( float x = 0.0f; x < 8.0f; x++ )
		{
			var pos = paddle.Position + (Vector3.Up * x) * paddle.Rotation;
			DebugOverlay.Sphere( pos, 0.25f, Color.Blue );
			var v = x * MathX.DegreeToRadian( paddle.AngularVelocity.pitch );
			DebugOverlay.Line( pos, pos + paddle.Rotation.Forward * v );
		} */

		var sweep = Trace.Sweep( paddle.PhysicsBody, from, to ).EntitiesOnly().Ignore( paddle ).Run();

		if ( !sweep.Hit ) return;
		if ( sweep.Entity is not Ball ) return;

		// get hit position local to the paddle
		var localHitpos = ( sweep.HitPosition - paddle.Position ) * paddle.Rotation.Inverse;

		// get our velocity at the hit point from the paddle angular velocity ( v = rw )
		var velocityFromAngular = localHitpos.z * MathX.DegreeToRadian( paddle.AngularVelocity.pitch );

		// Our ball will bounce off this normal a tiny bit (cor is gonna be about 0.15 on rubber?)
		ball.Velocity = Vector3.Reflect( ball.Velocity.Normal, sweep.Normal ) * ball.Velocity.Length * 0.15f; ;

		// Probably some shit we can do with the ball mass / paddle mass blah blah, this feels about right for now though
		ball.Velocity += (paddle.Velocity.Length + velocityFromAngular) * 2.0f * sweep.Normal;

		Sound.FromWorld( "tabletennis.paddle", sweep.HitPosition ).SetVolume( ball.Velocity.Length / 300.0f );
	}
}
