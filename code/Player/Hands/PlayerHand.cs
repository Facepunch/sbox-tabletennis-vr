namespace TableTennis;

public partial class VrPlayerHand : AnimatedEntity
{
	/* Statics */
	protected static Material RedMaterialOverride = Material.Load( "materials/hands/vr_hand.vmat" );
	protected static Material BlueMaterialOverride = Material.Load( "materials/hands/vr_hand_blue.vmat" );
	protected static Model LeftHandModel = Model.Load( "models/hands/alyx_hand_left.vmdl" );
	protected static Model RightHandModel = Model.Load( "models/hands/alyx_hand_right.vmdl" );

	[Net] private VrHandType handType { get; set; }
	
	public VrHandType HandType
	{
		get => handType;
		set
		{
			handType = value;

			// Set the model based on the hand type
			Model = handType == VrHandType.Left ? LeftHandModel : RightHandModel;
		}
	}
	
	/* Finger inputs */
	[Net] public float IndexFinger { get; set; }
	[Net] public float MiddleFinger { get; set; }
	[Net] public float RingFinger { get; set; }
	[Net] public float PinkyFinger { get; set; }
	[Net] public float Thumb { get; set; }

	[Net] private bool visibleHand { get; set; }
	public bool VisibleHand
	{
		get => visibleHand;
		set
		{
			visibleHand = value;

			if ( !visibleHand ) Model = null;
		}
	}

	protected virtual void SimulateFingers( Client cl )
	{
		var input = HandInput;
	
		Thumb = input.GetFingerCurl( 0 );
		IndexFinger = input.GetFingerCurl( 1 );
		MiddleFinger = input.GetFingerCurl( 2 );
		RingFinger = input.GetFingerCurl( 3 );
		PinkyFinger = input.GetFingerCurl( 4 );
	}

	/* Hand input */
	protected Input.VrHand HandInput
	{
		get
		{
			return HandType switch
			{
				VrHandType.Left => Input.VR.LeftHand,
				VrHandType.Right => Input.VR.RightHand,
				_ => throw new Exception( "Invalid hand specified for VrPlayerHand" )
			};
		}
	}

	public bool InGrip => HandInput.Grip.Value.AlmostEqual( 1, 0.1f );
	public bool InTrigger => HandInput.Trigger.Value.AlmostEqual( 1, 0.1f );
	public bool InMenu => HandInput.JoystickPress.WasPressed;

	public override void Spawn()
	{
		base.Spawn();

		VisibleHand = true;
		
		Tags.Add( "hand" );
	}

	protected virtual void SimulateInput( Client cl )
	{
		// empty as default
	}

	public override void Simulate( Client cl )
	{
		base.Simulate( cl );
		
		Transform = HandInput.Transform.WithScale( VR.Scale );

		SimulateInput( cl );
		SimulateFingers( cl );
	}

	public override void FrameSimulate( Client cl )
	{
		base.FrameSimulate( cl );

		Transform = HandInput.Transform.WithScale( VR.Scale );
	
		SimulateInput( cl );
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

	[Event.Tick]
	protected virtual void Animate()
	{
		// Weird requirements, but I'll allow it.
		SetAnimParameter( "bGrab", true );
		SetAnimParameter( "BasePose", 1 );
		SetAnimParameter( "GrabMode", 1 );
		
		SetAnimParameter( "FingerCurl_Middle", MiddleFinger );
		SetAnimParameter( "FingerCurl_Ring", RingFinger );
		SetAnimParameter( "FingerCurl_Pinky", PinkyFinger );
		SetAnimParameter( "FingerCurl_Index", IndexFinger );
		SetAnimParameter( "FingerCurl_Thumb", Thumb );
	}

	//
	public enum VrHandType
	{
		Left = 0,
		Right = 1
	}
}
