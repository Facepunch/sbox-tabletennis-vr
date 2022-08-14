namespace TableTennis;

public partial class VrPlayerHand : AnimatedEntity
{
	/* Statics */
	protected static Material RedMaterialOverride = Material.Load( "materials/hands/vr_hand.vmat" );
	protected static Material BlueMaterialOverride = Material.Load( "materials/hands/vr_hand_blue.vmat" );
	protected static Model LeftHandModel = Model.Load( "models/hands/alyx_hand_left.vmdl" );
	protected static Model RightHandModel = Model.Load( "models/hands/alyx_hand_right.vmdl" );

	[Net, Change( "OnHandChanged" )] private VrHandType handType { get; set; }
	
	public VrHandType HandType
	{
		get => handType;
		set
		{
			var old = handType;
			handType = value;
			OnHandChanged( old, handType );
		}
	}
	
	/* Finger inputs */
	[Net] public float IndexFinger { get; set; }
	[Net] public float MiddleFinger { get; set; }
	[Net] public float RingFinger { get; set; }
	[Net] public float PinkyFinger { get; set; }
	[Net] public float Thumb { get; set; }

	[Net, Change( "OnVisibleHandChanged" )] public bool VisibleHand { get; set; }

	protected virtual void OnVisibleHandChanged( bool before, bool after )
	{
		UpdateHandMaterials();
	}
	
	protected virtual void OnHandChanged( VrHandType before, VrHandType after )
	{
		if ( IsServer ) Model = after == VrHandType.Left ? LeftHandModel : RightHandModel;

		UpdateHandMaterials();
	}

	protected void UpdateHandMaterials()
	{
		var team = Client?.GetTeam();
		if ( team == null ) return;

		if ( IsClient )
			SetMaterialOverride( team is Team.Red ? RedMaterialOverride : BlueMaterialOverride );

		EnableDrawing = VisibleHand;
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

	public bool StartedGripLock { get; set; }
	public bool InGrip
	{
		get
		{
			if ( !ClientPreferences.LocalSettings.ReleaseGripToServe )
			{
				return HandInput.Grip.Value.AlmostEqual( 1, 0.25f );
			}
			else
			{

				var gripInput = HandInput.Grip.Value;
				var isGrippingRightNow = gripInput.AlmostEqual( 1, 0.25f );

				if ( !StartedGripLock && isGrippingRightNow )
				{
					Helpers.TryDisplay( "griplockstart", "Release the grip to throw the ball.", this, 5, "front_hand" );
					StartedGripLock = true;
				}

				if ( StartedGripLock )
				{
					if ( !gripInput.AlmostEqual( 1, 0.25f ) )
					{
						StartedGripLock = false;
						return true;
					}
				}
				return false;
			}
		}
	}
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

	public override void OnNewModel( Model model )
	{
		base.OnNewModel( model );

		UpdateHandMaterials();
	}

	public virtual Transform GetTransform()
	{
		var tr = HandInput.Transform;
		return tr;
	}

	public override void Simulate( Client cl )
	{
		base.Simulate( cl );
		
		Transform = GetTransform().WithScale( VR.Scale );
		Velocity = HandInput.Velocity;
		AngularVelocity = HandInput.AngularVelocity;

		SimulateInput( cl );
		SimulateFingers( cl );

		Animate();
		
	}

	public override void FrameSimulate( Client cl )
	{
		base.FrameSimulate( cl );

		Transform = GetTransform().WithScale( VR.Scale );
	
		SimulateInput( cl );
	}

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
