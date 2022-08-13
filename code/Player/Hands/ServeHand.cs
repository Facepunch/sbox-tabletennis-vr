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


	public override void FrameSimulate( Client cl )
	{
		base.FrameSimulate( cl );

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
	
	private Vector3 LeftHandHold => Position + Rotation.Forward * 1.35f + Rotation.Right * 1f + Rotation.Up * 1f;
	private Vector3 RightHandHold => Position + Rotation.Forward * 1.35f + Rotation.Left * 1f + Rotation.Up * 1f;
	public Vector3 HoldPosition => HandType == VrHandType.Left ? LeftHandHold : RightHandHold;

	public void ThrowBall()
	{
		var ball = Ball;
		Ball = null;
		ball.Velocity = Velocity;
		ball.AngularVelocity = AngularVelocity;

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
