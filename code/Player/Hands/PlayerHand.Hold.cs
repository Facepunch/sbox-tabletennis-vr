namespace TableTennis;

public partial class VrPlayerHand
{
	[Net, Predicted] private Entity heldEntity { get; set; }

	public IHoldable HeldEntity 
	{
		get => heldEntity as IHoldable; 
		set => heldEntity = value as Entity;
	}
	
	public void SetHeldEntity( IHoldable holdable, bool force = false )
	{
		// We can clear the held entity this way too, I guess?
		if ( !holdable.IsValid() )
		{
			DropHeldEntity();
			return;
		}

		// Only set the held entity if we can pick this up. Or if we're forcing it, ignore this.
		if ( !holdable.CanPickup( this ) && !force ) 
			return;

		HeldEntity = holdable;
		HeldEntity.OnPickedUp( this );
	}

	public void DropHeldEntity()
	{
		HeldEntity?.OnDropped( this );
		HeldEntity = null;
	}

	protected virtual void SimulateHeldEntity( Client cl )
	{
		//
		// So we're not doing parenting for this
		// 
		if ( HeldEntity.IsValid() )
		{
			// Always match the hand's scale. Sorry!
			heldEntity.Position = Position;
			heldEntity.Rotation = Rotation;

			// Here's our offsets.
			heldEntity.LocalPosition = HeldEntity.HoldPosition;
			heldEntity.LocalRotation = HeldEntity.HoldRotation;
		}
	}
}
