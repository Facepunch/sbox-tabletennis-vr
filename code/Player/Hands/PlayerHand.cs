namespace TableTennis;

public partial class VrPlayerHand : AnimatedEntity
{
	[Net, Predicted] public VrHandType HandType { get; set; } = VrHandType.Left;

	/* Finger inputs */
	[Net, Predicted] public float IndexFinger { get; set; }
	[Net, Predicted] public float MiddleFinger { get; set; }
	[Net, Predicted] public float RingFinger { get; set; }
	[Net, Predicted] public float PinkyFinger { get; set; }
	[Net, Predicted] public float Thumb { get; set; }

	/* Models */
	protected static Model LeftHandModel = Model.Load( "models/hands/alyx_hand_left.vmdl" );
	protected static Model RightHandModel = Model.Load( "models/hands/alyx_hand_right.vmdl" );
	
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

	public bool InGrip => HandInput.Grip.Active;
	public bool InTrigger => HandInput.Trigger.Active;

	public override void Spawn()
	{
		base.Spawn();

		// Set the model based on the hand type
		Model = HandType == VrHandType.Left ? LeftHandModel : RightHandModel;

		Tags.Add( "hand" );
	}

	protected virtual void SimulateInput( Client cl )
	{
		// default behavior - if grip gets held, drop the entity
		if ( InGrip )
		{
			DropHeldEntity();
		}
	}

	public override void Simulate( Client cl )
	{
		base.Simulate( cl );

		SimulateFingers( cl );
		SimulateHeldEntity( cl );
		SimulateInput( cl );
	}

	public override void FrameSimulate( Client cl )
	{
		base.FrameSimulate( cl );

		SimulateFingers( cl );
		SimulateHeldEntity( cl );
		SimulateInput( cl );
	}

	[Event.Frame]
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
