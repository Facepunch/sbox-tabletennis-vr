namespace TableTennis;

public partial class ServeHand : VrPlayerHand
{
	public Ball Ball { get; protected set; }

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
	public override void FrameSimulate( Client cl )
	{
		base.FrameSimulate( cl );

		var cachedPos = LastPosition;
		LastPosition = Position;
		VelocityDelta = Position - cachedPos;

		if ( Ball.IsValid() )
		{
			Ball.Position = HoldPosition;
			if ( InGrip ) ThrowBall();
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
	
	protected override void Animate()
	{
		if ( Ball.IsValid() )
		{
			var a = 0.5f;
			SetAnimParameter( "FingerCurl_Middle", a );
			SetAnimParameter( "FingerCurl_Ring", a );
			SetAnimParameter( "FingerCurl_Pinky", a );
			SetAnimParameter( "FingerCurl_Index", a );
			SetAnimParameter( "FingerCurl_Thumb", a );
		}
		else
		{
			base.Animate();
		}
	}
}
