namespace TableTennis;

public partial class Hand : Component.ITriggerListener
{
	/// <summary>
	/// The GameObject for the hand's model renderer.
	/// </summary>
	[Property, Group( "Holding" )] public GameObject HandGameObject { get; set; }

	/// <summary>
	/// The current held object
	/// </summary>
	public IHoldableObject HeldObject { get; private set; }

	/// <summary>
	/// Are we holding an object right now?
	/// </summary>
	public bool IsHolding => HeldObject.IsValid();
	
	/// <summary>
	/// Maintains a list of objects that our hand is hovered over, since it can be multiple.
	/// </summary>
	protected HashSet<IHoldableObject> HoveredObjects { get; private set; } = new();

	/// <summary>
	/// Tries to find the highest priority holdable object.
	/// If we're holding something, it'll pick that.
	/// After that, we'll check distance to our hand.
	/// </summary>
	/// <returns></returns>
	protected bool TryFindHoldableObject( out IHoldableObject holdable )
	{
		// Are we already holding something?
		if ( IsHolding )
		{
			holdable = HeldObject;
			return true;
		}

		// Get the closest hovered object (if any)

		var closest = HoveredObjects
			.OrderBy( x => x.GameObject.Transform.Position.Distance( Transform.Position ) )
			.FirstOrDefault();

		holdable = closest;
		return holdable.IsValid();
	}

	/// <summary>
	/// Start holding an object
	/// </summary>
	/// <param name="holdable"></param>
	/// <returns></returns>
	public bool StartHolding( IHoldableObject holdable )
	{
		if ( holdable.HoldObject( this ) )
		{
			HeldObject = holdable;
			return true;
		}

		return false;
	}

	/// <summary>
	/// Stop holding an object
	/// </summary>
	/// <returns></returns>
	public bool StopHolding()
	{
		if ( !HeldObject.IsValid() )
			return false;

		if ( HeldObject.StopHoldObject() )
		{
			HeldObject = null;
			return false;
		}

		return false;
	}

	/// <summary>
	/// Called every <see cref="OnUpdate"/>, responsible for parsing input for holding stuff.
	/// </summary>
	protected void UpdateHoldInput()
	{
;		// Should we be automatically detaching this object?
		bool didRelease = UpdateAutoReleaseObject();

		// Do we have a candidate object to hold?
		if ( TryFindHoldableObject( out var holdable ) && IsDown( holdable.HoldInput ) )
		{
			// We have a holdable object to grab, let's try it!
			StartHolding( holdable );
		}
		else
		{
			// Stop holding an object if we have one.
			StopHolding();
		}
	}

	/// <summary>
	/// Should we be automatically detaching this object?
	/// </summary>
	/// <returns>If we released an object this frame.</returns>
	private bool UpdateAutoReleaseObject()
	{
		if ( HeldObject.IsValid() && HeldObject.HoldInput == HoldInput.Nothing )
		{
			// Are we too far away from the origin of this held object?
			if ( HeldObject.GameObject.Transform.Position.Distance( Transform.Position ) > 3f )
			{
				StopHolding();
				return true;
			}
		}

		return false;
	}

	/// <summary>
	/// Can we hold this object?
	/// </summary>
	/// <param name="holdable"></param>
	/// <returns></returns>
	public bool CanHoldObject( IHoldableObject holdable )
	{
		// Can we hold this holdable?
		return true;
	}

	/// <summary>
	/// Can we stop holding this object?
	/// </summary>
	/// <param name="holdable"></param>
	/// <returns></returns>
	public bool CanStopHoldObject( IHoldableObject holdable )
	{
		// Can we stop holding this holdable?
		return true;
	}

	/// <summary>
	/// Called by the holdable, when we start holding something.
	/// </summary>
	/// <param name="holdable"></param>
	public void OnStartHoldingObject( IHoldableObject holdable )
	{
		AttachHandTo( holdable.GameObject );
		HeldObject = holdable;
	}

	/// <summary>
	/// Called by the holdable, when we stop holding something.
	/// </summary>
	/// <param name="holdable"></param>
	public void OnStopHoldingObject( IHoldableObject holdable )
	{
		DetachHand();
		HeldObject = null;
	}

	/// <summary>
	/// Attaches the hand model to a grab point.
	/// </summary>
	/// <param name="gameObject"></param>
	internal void AttachHandTo( GameObject gameObject )
	{
		HandGameObject.SetParent( gameObject, false );
	}

	/// <summary>
	/// Detaches the hand model from the grab point, puts it back on our hand.
	/// </summary>
	internal void DetachHand()
	{
		HandGameObject.SetParent( GameObject, false );
	}

	/// <summary>
	/// Called when we overlap with another trigger in the world.
	/// </summary>
	/// <param name="other"></param>
	void ITriggerListener.OnTriggerEnter( Collider other )
	{
		if ( other.Components.Get<IHoldableObject>( FindMode.EnabledInSelf ) is { } holdable )
		{
			HoveredObjects.Add( holdable );
		}
	}

	/// <summary>
	/// Called when we stop overlapping with another trigger.
	/// </summary>
	/// <param name="other"></param>
	void ITriggerListener.OnTriggerExit( Collider other )
	{
		if ( other.Components.Get<IHoldableObject>( FindMode.EnabledInSelf ) is { } holdable )
		{
			if ( HoveredObjects.Contains( holdable ) )
			{
				HoveredObjects.Remove( holdable );
			}
		}
	}
}
