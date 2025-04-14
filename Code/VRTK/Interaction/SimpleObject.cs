namespace VRTK;

/// <summary>
/// A simple grabbable object. You just pick it up with one hand. That's it.
/// </summary>
public partial class SimpleObject : Component, IGrabbable
{
	/// <summary>
	/// What inputs should we look out for when picking this up?
	/// </summary>
	[Property]
	public GrabInput GrabInput { get; set; } = GrabInput.Grip;

	[RequireComponent]
	public Rigidbody Rigidbody { get; set; }

	/// <summary>
	/// The hand that is holding this object ( can be null )
	/// </summary>
	public Hand Hand { get; set; }

	public HandPreset GetHandPreset( Hand hand ) => GetComponentInChildren<GrabReference>().HandPreset;

	/// <summary>
	/// Called when we start grabbing this object
	/// </summary>
	private void OnStartGrabbing()
	{
		Rigidbody.MotionEnabled = false;
	}

	/// <summary>
	/// Called when we stop grabbing this object
	/// </summary>
	private void OnStopGrabbing()
	{
		Rigidbody.MotionEnabled = true;
		
		if ( Hand.IsValid() )
		{
			Rigidbody.Velocity = Hand.Velocity;
		}
	}

	/// <summary>
	/// Try to start grabbing this object
	/// </summary>
	/// <param name="hand"></param>
	/// <returns></returns>
	public bool StartGrabbing( Hand hand )
	{
		// If someone's trying to grab this object with a different hand - let it happen if we can
		if ( Hand.IsValid() && Hand != hand )
		{
			if ( Hand.Release() )
			{
				Hand = null;
			}
			else
			{
				return false;
			}
		}

		var can = true;
		if ( can )
		{
			Hand = hand;
			OnStartGrabbing();
			return can;
		}

		return false;
	}

	/// <summary>
	/// Try to stop grabbing this object
	/// </summary>
	/// <returns></returns>
	public bool StopGrabbing( Hand hand )
	{
		var can = true;
		if ( can )
		{
			OnStopGrabbing();
			Hand = null;
			return can;
		}

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
