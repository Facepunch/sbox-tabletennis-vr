using VRTK;

namespace TableTennis;

public partial class Paddle : Component, IGrabbable
{
	/// <summary>
	/// The hand that is holding this object - will never be null
	/// </summary>
	public Hand Hand { get; set; }

	/// <summary>
	/// The grab reference for this paddle
	/// </summary>
	[Property]
	public GrabReference GrabReference { get; set; }

	// IGrabbable 

	Hand IGrabbable.Hand => Hand;

	GrabInput IGrabbable.GrabInput => GrabInput.None;

	HandPreset IGrabbable.GetHandPreset( Hand hand ) => GrabReference.HandPreset;

	bool IGrabbable.StartGrabbing( Hand hand )
	{
		return false;
	}

	bool IGrabbable.StopGrabbing( Hand hand )
	{
		return false;
	}
}
