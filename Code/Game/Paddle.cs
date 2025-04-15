using VRTK;

namespace TableTennis;

public partial class Paddle : Component, IGrabbable
{
	/// <summary>
	/// The paddle's <see cref="Rigidbody"/>
	/// </summary>
	[Property]
	public Rigidbody Rigidbody { get; set; }

	/// <summary>
	/// The grab reference for this paddle
	/// </summary>
	[Property] public GrabReference GrabReference { get; set; }

	/// <summary>
	/// The hand that is holding this object - will never be null
	/// </summary>
	public Hand Hand { get; set; }

	// IGrabbable 
	Hand IGrabbable.Hand => Hand;
	GrabInput IGrabbable.GrabInput => GrabInput.None;
	HandPreset IGrabbable.GetHandPreset( Hand hand ) => GrabReference.HandPreset;

	bool IGrabbable.StartGrabbing( Hand hand )
	{
		Hand = hand;
		return true;
	}

	bool IGrabbable.StopGrabbing( Hand hand )
	{
		return false;
	}

	protected override void OnUpdate()
	{
		if ( !Hand.IsValid() )
			return;

		if ( Hand.IsValid() )
		{
			var reference = GetComponentInChildren<GrabReference>();
			var offset = reference.LocalPosition + reference.GetOffset( Hand.HandSource );
			var rotatedOffset = Hand.WorldRotation * offset;

			WorldPosition = Hand.WorldPosition - rotatedOffset;
			WorldRotation = Hand.WorldRotation;
		}
	}
}
