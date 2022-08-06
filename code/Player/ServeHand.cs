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
	protected static Material MaterialOverride = Material.Load( "materials/hands/vr_hand.vmat" );

	[Net] public FingerData FingerData { get; set; }

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
			Log.Info( $"Material Override: {MaterialOverride.ResourceName}" );
			SetMaterialOverride( MaterialOverride );
		}
	}

	public override void Simulate( Client cl )
	{
		base.Simulate( cl );

		//FingerData.DebugLog();

		// Parse finger data
		FingerData.Parse( Input.VR.LeftHand );

		Animate();
	}

	public override void FrameSimulate( Client cl )
	{
		base.FrameSimulate( cl );

		Animate();
	}

	private void Animate()
	{
		SetAnimParameter( "bGrab", true );
		SetAnimParameter( "BasePose", 1 );
		SetAnimParameter( "GrabMode", 1 );

		SetAnimParameter( "FingerCurl_Middle", FingerData.Middle );
		SetAnimParameter( "FingerCurl_Ring", FingerData.Ring );
		SetAnimParameter( "FingerCurl_Pinky", FingerData.Pinky );
		SetAnimParameter( "FingerCurl_Index", FingerData.Index );
		SetAnimParameter( "FingerCurl_Thumb", FingerData.Thumb );
	}
}
