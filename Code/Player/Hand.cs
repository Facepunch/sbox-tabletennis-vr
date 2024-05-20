namespace TableTennis;

public partial class Hand : Component, Component.ITriggerListener
{
	/// <summary>
	/// The input deadzone, so holding ( flDeadzone * 100 ) percent of the grip down means we've got the grip / trigger down.
	/// </summary>
	const float flDeadzone = 0.25f;

	/// <summary>
	/// The model renderer associated with this hand.
	/// </summary>
	[Property, Group( "Components" )] public SkinnedModelRenderer Model { get; set; }

	/// <summary>
	/// What the velocity of this hand?
	/// </summary>
	public Vector3 Velocity { get; set; }

	/// <summary>
	/// Is the hand grip down?
	/// </summary>
	/// <returns></returns>
	public bool IsGripDown => Controller?.Grip.Value > flDeadzone;

	/// <summary>
	/// Is the hand trigger down?
	/// </summary>
	/// <returns></returns>
	public bool IsTriggerDown => Controller?.Trigger.Value > flDeadzone;

	/// <summary>
	/// The VR controller input.
	/// </summary>
	public VRController Controller => HandSource == Source.Left ? Input.VR?.LeftHand : Input.VR?.RightHand;

	public bool IsDown( HoldInput holdInput )
	{
		return holdInput switch
		{
			HoldInput.GripButton => IsGripDown,
			HoldInput.TriggerButton => throw new NotImplementedException(),
			HoldInput.Nothing => throw new NotImplementedException(),
			_ => throw new NotImplementedException(),
		};
	}

	private void UpdateTrackedLocation()
	{
		if ( Controller is null ) return;

		var tx = Controller.Transform;
		// Bit of a hack, but the alyx controllers have a weird origin that I don't care for.
		tx = tx.Add( Vector3.Forward * -2f, false );
		tx = tx.WithRotation( tx.Rotation * Rotation.From( 20, -5, 0 ) );

		var prevPosition = Transform.World.Position;
		Transform.World = tx;

		var newPosition = Transform.World.Position;
		Velocity = newPosition - prevPosition;
	}

	protected override void OnUpdate()
	{
		UpdateTrackedLocation();
		UpdatePose();

		// Anything past this point will be owner-only.
		if ( !IsProxy )
		{
			return;
		}

		UpdateHoldInput();
	}
}
