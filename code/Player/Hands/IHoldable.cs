namespace TableTennis;

public interface IHoldable : IValid
{
	/// <summary>
	/// Hold position, relative to the hand holding the item
	/// </summary>
	public Vector3 HoldPosition { get; set; }

	/// <summary>
	/// Hold rotation, relative to the hand holding the item
	/// </summary>
	public Rotation HoldRotation { get; set; }

	/// <summary>
	/// Called when the hand drops its current held entity
	/// </summary>
	/// <param name="hand"></param>
	public void OnDropped( VrPlayerHand hand );

	/// <summary>
	/// Called when the hand picks up something
	/// </summary>
	/// <param name="hand"></param>
	public void OnPickedUp( VrPlayerHand hand );

	/// <summary>
	/// Called just before the hand picks up something, specifies whether or not something can be picked up.
	///	Please note, that it can be overriden if <see cref="VrPlayerHand.SetHeldEntity"/>'s force is set to true.
	/// </summary>
	/// <param name="hand"></param>
	public bool CanPickup( VrPlayerHand hand );
}
