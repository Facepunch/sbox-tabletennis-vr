using System.Text.Json.Serialization;

namespace TableTennis;

[Icon( "handshake" )]
public partial class BaseHoldable : Component, IHoldableObject
{
	/// <summary>
	/// The hold input type.
	/// </summary>
	[Property, Group( "Input" )] public virtual HoldInput HoldInput { get; set; }

	/// <summary>
	/// The hand that is holding this object.
	/// </summary>
	[Property, ReadOnly, Group( "Runtime" ), JsonIgnore] public Hand HeldHand { get; private set; }
	
	/// <summary>
	/// Can we start holding this holdable object?
	/// </summary>
	/// <param name="hand"></param>
	/// <returns></returns>
	protected virtual bool CanHoldObject( Hand hand )
	{
		// Is already being held
		if ( HeldHand.IsValid() ) return false;
		if ( !HeldHand.CanHoldObject( this ) ) return false;

		return true;
	}

	protected virtual bool CanStopHoldObject()
	{
		// Not being held
		if ( !HeldHand.IsValid() ) return false;
		if ( !HeldHand.CanStopHoldObject( this ) ) return false;

		return true;
	}

	public virtual bool HoldObject( Hand hand )
	{
		if ( !CanHoldObject( hand ) )
			return false;

		HeldHand = hand;
		HeldHand.OnStartHoldingObject( this );

		// Do stuff
		return true;
	}

	bool IHoldableObject.StopHoldObject()
	{
		if ( !CanStopHoldObject() )
			return false;

		HeldHand?.OnStopHoldingObject( this );
		HeldHand = null;

		// Do stuff
		return true;
	}
}
