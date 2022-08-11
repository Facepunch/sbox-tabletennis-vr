namespace TableTennis;

public partial class FingerData : BaseNetworkable
{
	[Net] public float Index { get; set; }
	[Net] public float Middle { get; set; }
	[Net] public float Ring { get; set; }
	[Net] public float Pinky { get; set; }
	[Net] public float Thumb { get; set; }

	public bool IsTriggerDown()
	{
		return Index.AlmostEqual( 1f, 0.1f );
	}

	public void Parse( Input.VrHand input )
	{
		Thumb = input.GetFingerCurl( 0 );
		Index = input.GetFingerCurl( 1 );
		Middle = input.GetFingerCurl( 2 );
		Ring = input.GetFingerCurl( 3 );
		Pinky = input.GetFingerCurl( 4 );
	}

	public void DebugLog()
	{
		Log.Info( $"{Host.Name}: {Thumb}, {Index}, {Middle}, {Ring}, {Pinky}" );
	}
}

public partial class ServeHand : AnimatedEntity
{
	protected static Material RedMaterialOverride = Material.Load( "materials/hands/vr_hand.vmat" );
	protected static Material BlueMaterialOverride = Material.Load( "materials/hands/vr_hand_blue.vmat" );

	[Net] public FingerData FingerData { get; set; }
	[Net] protected bool UsePresets { get; set; } = true;
	public Ball Ball { get; set; }

	public ServeHand()
	{
		Predictable = true;
	}

	public override void Spawn()
	{
		SetModel( "models/hands/alyx_hand_left.vmdl" );
		Tags.Add( "serve_hand" );

		FingerData = new();
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

		//FingerData.DebugLog();

		// Parse finger data
		FingerData.Parse( Input.VR.LeftHand );
		UsePresets = !Input.VR.IsKnuckles;

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
				DropBall();
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
	public void DropBall()
	{
		var ball = Ball;
		Ball = null;
		ball.Velocity = VelocityDelta * throwPower;
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


		if ( !UsePresets )
		{
			SetAnimParameter( "FingerCurl_Middle", FingerData.Middle );
			SetAnimParameter( "FingerCurl_Ring", FingerData.Ring );
			SetAnimParameter( "FingerCurl_Pinky", FingerData.Pinky );
			SetAnimParameter( "FingerCurl_Index", FingerData.Index );
			SetAnimParameter( "FingerCurl_Thumb", FingerData.Thumb );
		}
		else
		{
			if ( FingerData.Index > 0.8f )
			{
				OkSign();
			}
			else if ( FingerData.Ring > 0.8f )
			{
				FlipOff();
			}
			else
			{
				SetAnimParameter( "FingerCurl_Middle", 1f );
				SetAnimParameter( "FingerCurl_Ring", 1f );
				SetAnimParameter( "FingerCurl_Pinky", 1f );
				SetAnimParameter( "FingerCurl_Index", 1f );
				SetAnimParameter( "FingerCurl_Thumb", 1f );
			}
		}
	}
}
