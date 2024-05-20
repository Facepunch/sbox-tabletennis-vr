namespace TableTennis;

public partial class PaddleHand : VrPlayerHand
{
	[Net] public Paddle Paddle { get; set; }

	public override void Spawn()
	{
		Paddle = new();
		Paddle.Owner = Owner;
		
		base.Spawn();

		HandType = VrHandType.Right;
		VisibleHand = true;
		Paddle.Hand = this;
		Transmit = TransmitType.Always;
	}

	public override Transform GetTransform( IClient cl )
	{
		var tr = base.GetTransform( cl );
		tr.Rotation *= Rotation.FromAxis( Vector3.Right, -90f );
		tr.Position -= tr.Rotation.Up * 5f;

		return tr;
	}

	public override void Simulate( IClient cl )
	{
		base.Simulate( cl );
		Paddle?.Simulate( cl );	
	}

	public override void FrameSimulate( IClient cl )
	{
		base.FrameSimulate( cl );
		Paddle?.FrameSimulate( cl );
	}

	protected override void Animate()
	{
		SetAnimParameter( "bGrab", true );
		SetAnimParameter( "BasePose", 1 );
		SetAnimParameter( "GrabMode", 1 );

		SetAnimParameter( "FingerCurl_Middle", MiddleFinger.Clamp( 0.7f, 1f ) );
		SetAnimParameter( "FingerCurl_Ring", RingFinger.Clamp( 0.7f, 1f ) );
		SetAnimParameter( "FingerCurl_Pinky", PinkyFinger.Clamp( 0.7f, 1f ) );
		SetAnimParameter( "FingerCurl_Index", IndexFinger.Clamp( 0.7f, 1f ) );
		SetAnimParameter( "FingerCurl_Thumb", Thumb.Clamp( 0.7f, 1f ) );
	}
}
