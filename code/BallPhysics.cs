namespace TableTennis;

/// <summary>
/// Deterministic physics for Table Tennis
/// </summary>
public static partial class BallPhysics
{
	// Main thing to these consts, mass is kg, distances are in

	public static readonly Vector3 Gravity = Vector3.Down * 9.81f.MeterToInch(); 
	public static readonly float BallMass = 0.0027f; // kg
	public static readonly float BallRadius = 0.02f.MeterToInch(); // in²
	public static readonly float BallCrossArea = MathF.PI * BallRadius * BallRadius; // in²

	public static readonly float KGM3ToKGI3 = 0.0164f; // kg/m³ -> kg/in³
	public static readonly float AirDensity = 1.204f * KGM3ToKGI3; // kg/in³
	public static readonly float DragCoefficient = 0.5f;

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

	public static bool Step( Ball ball, float dt, Paddle paddle, Transform paddleStart, Transform paddleEnd )
	{
		const float timeStep = 0.005f;
		bool hit = false;
		for ( float timeLeft = dt; timeLeft > 0.0f; timeLeft -= timeStep )
		{
			// Paddle every substep... But if we hit fuckin stop
			if ( !hit ) hit = PaddleBall( paddle, paddleStart, paddleEnd, ball );

			// Do whatever we have left
			Move( ball, MathF.Min( timeStep, timeLeft ) );
		}

		return hit;
	}

	private static Vector3 BallDrag( Vector3 velocity )
	{
		// Standard drag equation, just make sure we give it the proper units
		//
		// Fd         = 1/2  * ρ          * u²                     * A             * Cd
		//                     kg/m³        m/s²                     m²2
		//                     kg/in³       in/s²                    in²2
		var dragForce = 0.5f * AirDensity * velocity.LengthSquared * BallCrossArea * DragCoefficient;
		return -dragForce * velocity.Normal;
	}

	public static void Move( Ball ball, float timeStep )
	{
		var velocity = ball.Velocity;
		var position = ball.Position;

		// Hit everything but the paddle
		var trace = Trace.Ray( 0, 0 )
			.Radius( BallRadius )
			.WorldAndEntities()
			.Ignore( ball )
			.WithoutTags( "paddle" );

		var drag = BallDrag( velocity );
		velocity += (drag + Gravity) * timeStep;

		// TODO: Magnus factor if we're feeling fancy?

		// move and collide with shit
		{
			var timeLeft = timeStep;
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

					// TODO: Different surfaces
					if ( hitSurface.ResourceName == "wood" )
					{
						if ( Math.Abs( velocity.z ) > 10.0f )
						{
							var sound = Sound.FromWorld( "tabletennis.bounce", hitPos );
							sound.SetVolume( velocity.Length / 150.0f );
							Particles.Create( "particles/ball_table_hit/ball_table_hit.vpcf", hitPos );
						}
					}

					TableTennisGame.ServerBallBounce( ball.NetworkIdent, hitPos );
				}

				timeLeft -= timeLeft * pm.Fraction;

				// Surface restitution "absorbs" kinetic energy of the ball
				// var bounce = 0.90f - pm.Surface.Elasticity;
				
				var bounce = BallCOR - TableCOR;
				if ( pm.Surface.ResourceName == "carpet" )
				{
					bounce = 0.5f;
				}

				if ( !moveplanes.TryAdd( pm.Normal, ref velocity, bounce ) )
					break;
			}

			if ( travelFraction == 0 )
				velocity = 0;
		}

		ball.Position = position;
		ball.Velocity = velocity;
	}

	static TimeSince LastHit = 0;

	[ConVar.Client( "tt_debug_paddle_linearvelocity" )]
	public static bool DebugPaddleLinearVelocity { get; set; } = false;

	/// <summary>
	/// Check if the paddle is hitting the ball
	/// </summary>
	public static bool PaddleBall( Paddle paddle, Transform from, Transform to, Ball ball )
	{
		if ( DebugPaddleLinearVelocity )
		{
			// Debug linear velocity at points
			for ( float x = 0.0f; x < 8.0f; x++ )
			{
				var pos = paddle.Position + (Vector3.Up * x) * paddle.Rotation;
				DebugOverlay.Sphere( pos, 0.25f, Color.Blue );
				var v = x * MathX.DegreeToRadian( paddle.AngularVelocity.pitch );
				DebugOverlay.Line( pos, pos + paddle.Rotation.Forward * v );
			}
		}

		//
		// This shouldn't be needed if we do some math
		//
		if ( LastHit < 0.2f ) return false;

		var sweep = Trace.Sweep( paddle.PhysicsBody, from, to ).WithTag( "ball" ).IncludeClientside().Run();
		// var sweep = Trace.Sweep( paddle.PhysicsBody, from, to ).WorldAndEntities().Ignore( paddle ).IncludeClientside().Run();

		if ( !sweep.Hit ) return false;

		if ( sweep.Entity is not Ball ) return false;

		// get hit position local to the paddle
		var localHitpos = ( sweep.HitPosition - paddle.Position ) * paddle.Rotation.Inverse;

		// get our velocity at the hit point from the paddle angular velocity ( v = rw )
		var velocityFromAngular = localHitpos.z * MathX.DegreeToRadian( paddle.AngularVelocity.pitch );

		// Our ball will bounce off this normal a tiny bit (cor is gonna be about 0.15 on rubber?)
		ball.Velocity = Vector3.Reflect( ball.Velocity.Normal, sweep.Normal ) * ball.Velocity.Length * 0.15f; ;

		// Probably some shit we can do with the ball mass / paddle mass blah blah, this feels about right for now though
		ball.Velocity += Math.Abs(paddle.Velocity.Length + velocityFromAngular) * 2.0f * sweep.Normal;

		// DebugOverlay.Line( ball.Position, ball.Position + ball.Velocity, Color.Orange, 5 );

		// This feels good on Oculus, but Oculus haptics are kinda crap
		Input.VR.RightHand.TriggerHapticVibration( 0f, 200.0f, Math.Clamp( 0.4f + ball.Velocity.Length / 500f, 0.4f, 0.8f ) );

		Sound.FromWorld( TableTennisGame.Current?.GetPaddleSound(), sweep.HitPosition ).SetVolume( ball.Velocity.Length / 50f );

		LastHit = 0;

		TableTennisGame.Current?.OnPaddleHit( paddle, ball );
		TableTennisGame.ServerPaddleHit( paddle.NetworkIdent );

		return true;
	}
}
