namespace TableTennis;

public partial class ServeHand : VrPlayerHand
{
	public Ball Ball { get; set; }

	public ServeHand()
	{
		Predictable = true;
	}

	public override void Spawn()
	{
		base.Spawn();

		Tags.Add( "serve_hand" );

		HandType = VrHandType.Left;
	}

	Vector3 LastPosition;
	Vector3 VelocityDelta;

	public override void Simulate( Client cl )
	{
		base.Simulate( cl );

		Animate();
	}

	public override void FrameSimulate( Client cl )
	{
		base.FrameSimulate( cl );

		var cachedPos = LastPosition;
		LastPosition = Position;
		VelocityDelta = Position - cachedPos;

		Animate();

		if ( Ball.IsValid() )
		{
			Ball.Position = HoldPosition;
			
			if ( Input.VR.LeftHand.Grip >= 0.9f )
			{
				ThrowBall();
			}
		}
	}

	internal void SetBall( Ball ball )
	{
		Ball = ball;
	}
	
	public Vector3 HoldPosition => Position + Rotation.Forward * 1.35f + Rotation.Right * 1f + Rotation.Up * 1f;

	const float throwPower = 100f;
	public void ThrowBall()
	{
		var ball = Ball;
		Ball = null;
		ball.Velocity = VelocityDelta * throwPower;

		TableTennisGame.IThrewTheBallCunt( ball.Position, ball.Velocity, Time.Now );
	}
}
