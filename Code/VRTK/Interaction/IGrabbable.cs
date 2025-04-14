using System;

namespace VRTK;

[Flags]
public enum GrabInput : uint
{
	None = 0,
	Trigger = 1,
	Grip = 2
}

/// <summary>
/// Describes something that can be grabbed by the player's hands!
/// </summary>
public interface IGrabbable
{
	/// <summary>
	/// The hand that the grabbable is being held by
	/// </summary>
	public Hand Hand { get; }

	public GameObject GameObject { get; }

	public GrabInput GrabInput { get; }

	/// <summary>
	/// Hand preset
	/// </summary>
	public HandPreset GetHandPreset( Hand hand ) { return null; }

	/// <summary>
	/// Can this grabbable be grabbed by a player's hand
	/// </summary>
	/// <param name="hand"></param>
	/// <returns></returns>
	public bool StartGrabbing( Hand hand )
	{
		return true;
	}

	public bool StopGrabbing( Hand hand )
	{
		return true;
	}
}

/// <summary>
/// A slot, this describes a slot that we can place a <see cref="IGrabbable"/> into.
/// </summary>
public interface ISlot
{
	public IGrabbable Grabbable { get; }

	/// <summary>
	/// Places a grabbable in a slot. Returns if we succeeded or not.
	/// </summary>
	/// <param name="grabbable"></param>
	/// <returns></returns>
	public bool Place( IGrabbable grabbable );

	/// <summary>
	/// Tries to release a grabbable from this slot.
	/// </summary>
	/// <returns></returns>
	public bool Release();
}
