namespace TableTennis;

/// <summary>
/// Describes a holdable object.
/// </summary>
public interface IHoldableObject : IValid
{
	/// <summary>
	/// The position at where we'll hold this object.
	/// </summary>
	Transform HoldTransform { get; }

	/// <summary>
	/// The hand preset
	/// </summary>
	HandPreset HandPreset => HandPreset.Grip;

	/// <summary>
	/// The type of input that'll flag the hands to pick us up.
	/// </summary>
	HoldInput HoldInput { get; }

	/// <summary>
	/// The hold type
	/// </summary>
	HoldType HoldType { get; }

	/// <summary>
	/// The GameObject of this object
	/// </summary>
	GameObject GameObject { get; }

	/// <summary>
	/// Hold an object with a hand.
	/// </summary>
	/// <param name="hand"></param>
	/// <returns>Could we hold?</returns>
	public bool HoldObject( Hand hand );

	/// <summary>
	/// Stop holding an object with a hand.
	/// </summary>
	/// <returns>Did we stop holding?</returns>
	public bool StopHoldObject();
}
