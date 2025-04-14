using VRTK;

[Title( "VRTK - Hand" ), Tint( EditorTint.Green )]
public sealed class Hand : Component
{
	/// <summary>
	/// Represents a controller to use when fetching skeletal data (finger curl/splay values)
	/// </summary>
	public enum HandSources
	{
		/// <summary>
		/// The left controller
		/// </summary>
		Left,

		/// <summary>
		/// The right controller
		/// </summary>
		Right
	}

	/// <summary>
	/// Which hand should we use to update the parameters?
	/// </summary>
	[Property] public HandSources HandSource { get; set; } = HandSources.Left;

	/// <summary>
	/// The hand model
	/// </summary>
	[Property] public SkinnedModelRenderer Renderer { get; set; }

	/// <summary>
	/// A grabbable we'll grab IMMEDIATELY on start.
	/// </summary>
	[Property] private GameObject DefaultGrabbable { get; set; }

	/// <summary>
	/// Are we using the toggle grab mode?
	/// </summary>
	[Property] private bool IsToggleGrab { get; set; } = true;

	public bool WantsKeyboardInput { get; set; }

	public static string[] AnimGraphNames =
	[
		"FingerCurl_Thumb",
		"FingerCurl_Index",
		"FingerCurl_Middle",
		"FingerCurl_Ring",
		"FingerCurl_Pinky"
	];

	/// <summary>
	/// The VR controller
	/// </summary>
	public VRController VRController
	{
		get => HandSource == HandSources.Left ? Input.VR?.LeftHand : Input.VR?.RightHand;
	}

	/// <summary>
	/// The input deadzone, so holding ( flDeadzone * 100 ) percent of the grip down means we've got the grip / trigger down.
	/// </summary>
	const float flDeadzone = 0.25f;

	/// <summary>
	/// What the velocity?
	/// </summary>
	public Vector3 Velocity { get; set; }

	public IPoseProvider PoseProvider = new ProceduralPoseProvider();

	/// <summary>
	/// A reference to our current grabbed object.
	/// </summary>
	private IGrabbable Grabbable;

	/// <summary>
	/// Applies a hand preset for this hand.
	/// </summary>
	/// <param name="preset"></param>
	public void ApplyHandPreset( HandPreset preset = null )
	{
		// Get our controller inputs
		var source = VRController;

		Renderer.Set( "BasePose", 1 );
		Renderer.Set( "bGrab", true );
		Renderer.Set( "GrabMode", 1 );

		for ( FingerValue v = FingerValue.ThumbCurl; v <= FingerValue.PinkyCurl; ++v )
		{
			Renderer.Set( AnimGraphNames[(int)v], source is not null ? source.GetFingerValue( v ) : 0 );
		}

		if ( preset is not null )
		{
			preset.Apply( Renderer );
		}
	}

	public bool Release()
	{
		if ( Grabbable is not null )
		{
			return Grabbable.StopGrabbing( this );
		}
		return false;
	}

	/// <summary>
	/// Is the hand grip down?
	/// </summary>
	/// <returns></returns>
	public bool IsGripDown()
	{
		// For debugging purposes
		if ( !Game.IsRunningInVR ) return (WantsKeyboardInput ? Input.Down( "Run" ) : false);

		var src = VRController;
		if ( src is null ) return false;

		return src.Grip.Value > flDeadzone;
	}

	/// <summary>
	/// Is the hand trigger down?
	/// </summary>
	/// <returns></returns>
	public bool IsTriggerDown()
	{
		// For debugging purposes
		if ( !Game.IsRunningInVR ) return ( WantsKeyboardInput ? Input.Down( "Attack1" ) : false );

		var src = VRController;
		if ( src is null ) return false;

		return src.Trigger.Value > flDeadzone;
	}

	private bool _triggerDown;
	private bool _gripDown;

	/// <summary>
	/// Returns true if the trigger was just pressed this frame.
	/// </summary>
	public bool IsTriggerPressed()
	{
		return IsTriggerDown() && !_triggerDown;
	}

	/// <summary>
	/// Returns true if the trigger was just released this frame.
	/// </summary>
	public bool IsTriggerReleased()
	{
		return !IsTriggerDown() && _triggerDown;
	}

	/// <summary>
	/// Returns true if the grip was just pressed this frame.
	/// </summary>
	public bool IsGripPressed()
	{
		return IsGripDown() && !_gripDown;
	}

	/// <summary>
	/// Returns true if the grip was just released this frame.
	/// </summary>
	public bool IsGripReleased()
	{
		return !IsGripDown() && _gripDown;
	}

	private void UpdateInput()
	{
		bool triggerDownNow = IsTriggerDown();
		bool gripDownNow = IsGripDown();

		// Update last known states
		_triggerDown = triggerDownNow;
		_gripDown = gripDownNow;
	}

	private Vector3 previousPosition;
	private void UpdateTrackedLocation()
	{
		var controller = VRController;
		if ( controller is null )
		{
			Velocity = (WorldPosition - previousPosition) / Time.Delta;
			previousPosition = WorldPosition;
			return;
		}

		var tx = controller.Transform;

		var prevPosition = WorldPosition;
		Transform.World = tx;

		var newPosition = WorldPosition;
		Velocity = (newPosition - prevPosition) / Time.Delta;
	}

	protected override void OnEnabled()
	{
		previousPosition = WorldPosition;

		// Try to grab the default grabbable if we have one set
		if ( DefaultGrabbable.IsValid() && DefaultGrabbable.GetComponent<IGrabbable>() is var g && g.StartGrabbing( this ) )
		{
			Grabbable = g;
		}
	}

	IEnumerable<IGrabbable> FindGrabbables()
	{
		var radius = 4f;
		var objects = Scene.FindInPhysics( new Sphere( WorldPosition, radius ) );

		foreach ( var obj in objects )
		{
			var grabbable = obj.GetComponent<IGrabbable>();
			if ( grabbable is not null )
				yield return grabbable;
		}
	}

	private bool IsDown( GrabInput input )
	{
		if ( input.HasFlag( GrabInput.Grip ) && input.HasFlag( GrabInput.Trigger ) ) return IsGripDown() && IsTriggerDown();
		if ( input.HasFlag( GrabInput.Grip ) ) return IsGripDown();
		if ( input.HasFlag( GrabInput.Trigger ) ) return IsTriggerDown();

		return false;
	}

	private bool IsPressed( GrabInput input )
	{
		// This is on purpose.
		if ( input.HasFlag( GrabInput.Grip ) && input.HasFlag( GrabInput.Trigger ) ) return IsGripPressed() && IsTriggerDown();
		if ( input.HasFlag( GrabInput.Grip ) ) return IsGripPressed();
		if ( input.HasFlag( GrabInput.Trigger ) ) return IsTriggerPressed();

		return false;
	}

	private bool IsReleased( GrabInput input )
	{
		// This is on purpose.
		if ( input.HasFlag( GrabInput.Grip ) && input.HasFlag( GrabInput.Trigger ) ) return IsGripReleased() && !IsTriggerDown();
		if ( input.HasFlag( GrabInput.Grip ) ) return IsGripReleased();
		if ( input.HasFlag( GrabInput.Trigger ) ) return IsTriggerReleased();

		return false;
	}

	private void UpdateToggleGrab()
	{
		if ( Grabbable is not null )
		{
			if ( IsPressed( Grabbable.GrabInput ) )
			{
				if ( Grabbable.StopGrabbing( this ) )
				{
					Grabbable = null;
				}
			}
			return;
		}

		var grabbables = FindGrabbables();
		var found = grabbables.FirstOrDefault();
		if ( found is not null )
		{
			Gizmo.Draw.Color = Color.Orange.WithAlpha( 0.2f );
			Gizmo.Draw.SolidSphere( found.GameObject.WorldPosition, 4, 32, 32 );

			if ( IsPressed( found.GrabInput ) )
			{
				if ( found.StartGrabbing( this ) )
				{
					Grabbable = found;
				}
			}
		}
	}

	private void UpdateTryGrabbing()
	{
		if ( Grabbable is not null ) return;

		var grabbables = FindGrabbables();
		var found = grabbables.FirstOrDefault();
		if ( found is not null )
		{
			Gizmo.Draw.Color = Color.Orange.WithAlpha( 0.2f );
			Gizmo.Draw.SolidSphere( found.GameObject.WorldPosition, 4, 32, 32 );

			if ( IsDown( found.GrabInput ) )
			{
				if ( found.StartGrabbing( this ) )
				{
					Grabbable = found;
				}
			}
		}
	}

	private void UpdateTryStopGrabbing()
	{
		if ( Grabbable is null ) return;

		if ( !IsDown( Grabbable.GrabInput ) )
		{
			if ( Grabbable.StopGrabbing( this ) )
			{
				Grabbable = null;
			}
		}
	}

	protected override void OnUpdate()
	{
		UpdateTrackedLocation();

		// Use no preset
		ApplyHandPreset( Grabbable is not null ? Grabbable.GetHandPreset( this ) : null );

		if ( IsToggleGrab )
		{
			UpdateToggleGrab();
		}
		else
		{
			UpdateTryGrabbing();
			UpdateTryStopGrabbing();
		}

		UpdateInput();
	}

	public VRHandJointData[] GetJoints()
	{
		return VRController.GetJoints();
	}

	/// <summary>
	/// Attach the hands to a GameObject
	/// </summary>
	/// <param name="obj"></param>
	public void AttachMeshTo( GameObject obj )
	{
		Renderer.GameObject.SetParent( obj, false );
	}

	/// <summary>
	/// Return the hands to us
	/// </summary>
	public void ReturnMesh()
	{
		Renderer.GameObject.SetParent( GameObject, false );
	}
}
