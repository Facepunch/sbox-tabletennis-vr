namespace TableTennis;

public partial class ServeHand : VrPlayerHand
{
	protected static Material RedMaterialOverride = Material.Load( "materials/hands/vr_hand.vmat" );
	protected static Material BlueMaterialOverride = Material.Load( "materials/hands/vr_hand_blue.vmat" );

	public Ball Ball { get; set; }

	public ServeHand()
	{
		Predictable = true;
	}

	public override void Spawn()
	{
		SetModel( "models/hands/alyx_hand_left.vmdl" );
		Tags.Add( "serve_hand" );
	}

	public override void OnNewModel( Model model )
	{
		base.OnNewModel( model );

		if ( IsClient )
		{
			var team = Client.GetTeam();
			SetMaterialOverride( team is Team.Red ? RedMaterialOverride : BlueMaterialOverride );
		}
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

	protected void OkSign()
	{
		SetAnimParameter( "FingerCurl_Middle", 0f );
		SetAnimParameter( "FingerCurl_Ring", 0f );
		SetAnimParameter( "FingerCurl_Pinky", 0f );
		SetAnimParameter( "FingerCurl_Index", 0.55f );
		SetAnimParameter( "FingerCurl_Thumb", 0.8f );
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

	protected void FlipOff()
	{
		SetAnimParameter( "FingerCurl_Middle", 0.2f );
		SetAnimParameter( "FingerCurl_Ring", 1f );
		SetAnimParameter( "FingerCurl_Pinky", 1f );
		SetAnimParameter( "FingerCurl_Index", 1f );
		SetAnimParameter( "FingerCurl_Thumb", 1f );
	}
	
	private void Animate()
	{
		SetAnimParameter( "bGrab", true );
		SetAnimParameter( "BasePose", 1 );
		SetAnimParameter( "GrabMode", 1 );

		if ( Ball.IsValid() )
		{
			var a = 0.5f;
			SetAnimParameter( "FingerCurl_Middle", a );
			SetAnimParameter( "FingerCurl_Ring", a );
			SetAnimParameter( "FingerCurl_Pinky", a );
			SetAnimParameter( "FingerCurl_Index", a );
			SetAnimParameter( "FingerCurl_Thumb", a );

			return;
		}
	}
}
