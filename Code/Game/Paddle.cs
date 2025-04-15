using VRTK;

namespace TableTennis;

/// <summary>
/// The paddle. It hits balls very well.
/// </summary>
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
	// endof: IGrabbable
	
	protected override void OnUpdate()
	{
		if ( !Hand.IsValid() )
			return;

		// All we do is update the position of the paddle to follow the player's hand
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
