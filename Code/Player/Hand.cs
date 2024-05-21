namespace TableTennis;

public partial class Hand : Component, Component.ITriggerListener
{
	/// <summary>
	/// The model renderer associated with this hand.
	/// </summary>
	[Property, Group( "Components" )] public SkinnedModelRenderer Model { get; set; }

	/// <summary>
	/// What the velocity of this hand?
	/// </summary>
	public Vector3 Velocity { get; set; }

	/// <summary>
	/// The VR controller input.
	/// </summary>
	public VRController Controller => HandSource == Source.Left ? Input.VR?.LeftHand : Input.VR?.RightHand;

	private Angles GetAnglesOffset()
	{
		var obj = HeldObject;
		if ( !obj.IsValid() ) return Angles.Zero;

		return HeldObject.HandAnglesOffset;
	}

	private Transform initialLocalTransform;
	protected override void OnStart()
	{
		initialLocalTransform = Transform.Local;
	}

	private Transform GetOffset()
	{
		var tx = new Transform();
		tx = tx.Add( Vector3.Forward * -2f, false );
		tx = tx.WithRotation( tx.Rotation * Rotation.From( 20, -5, 0 ) );
		tx = tx.WithRotation( tx.Rotation * Rotation.From( GetAnglesOffset() ) );
		return tx;
	}

	private void UpdateTrackedLocation()
	{
		if ( Controller is null )
		{
			Transform.Local = global::Transform.Concat( initialLocalTransform, GetOffset() );
			return;
		}

		var tx = Controller.Transform;
		var prevPosition = Transform.World.Position;
		Transform.World = global::Transform.Concat( tx, GetOffset() );

		var newPosition = Transform.World.Position;
		Velocity = newPosition - prevPosition;
	}

	protected override void OnUpdate()
	{
		// Update input first.
		if ( !IsProxy )
		{
			UpdateInput();
		}

		UpdateTrackedLocation();
		UpdatePose();

		// Anything past this point will be owner-only.
		if ( IsProxy )
		{
			return;
		}

		UpdateHoldInput();
	}
}
