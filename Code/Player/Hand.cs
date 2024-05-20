namespace TableTennis;

public partial class Hand : Component, Component.ITriggerListener
{
	[Property] public SkinnedModelRenderer Model { get; set; }

	/// <summary>
	/// The input deadzone, so holding ( flDeadzone * 100 ) percent of the grip down means we've got the grip / trigger down.
	/// </summary>
	const float flDeadzone = 0.25f;

	/// <summary>
	/// What the velocity?
	/// </summary>
	public Vector3 Velocity { get; set; }

	/// <summary>
	/// Is the hand grip down?
	/// </summary>
	/// <returns></returns>
	public bool IsGripDown()
	{
		var src = GetController();
		if ( src is null ) return false;

		return src.Grip.Value > flDeadzone;
	}

	/// <summary>
	/// Is the hand trigger down?
	/// </summary>
	/// <returns></returns>
	public bool IsTriggerDown()
	{
		var src = GetController();
		if ( src is null ) return false;

		return src.Trigger.Value > flDeadzone;
	}

	public VRController GetController()
	{
		return HandSource == HandSources.Left ? Input.VR?.LeftHand : Input.VR?.RightHand;
	}

	private void UpdateTrackedLocation()
	{
		var controller = GetController();
		if ( controller is null ) return;

		var tx = controller.Transform;
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
	}
}
