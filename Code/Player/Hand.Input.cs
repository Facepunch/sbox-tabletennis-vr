namespace TableTennis;

public partial class Hand
{
	/// <summary>
	/// The current input state for this hand.
	/// </summary>
	public InputState InputState { get; set; } = new();

	protected void UpdateInput()
	{
		InputState.Update( Controller );
	}

	public bool IsDown( HoldInput holdInput )
	{
		return holdInput switch
		{
			HoldInput.GripButton => InputState.IsGripDown,
			HoldInput.TriggerButton => InputState.IsTriggerDown,
			HoldInput.Nothing => true,
			_ => false
		};
	}

	public bool WasReleased( HoldInput holdInput )
	{
		return holdInput switch
		{
			HoldInput.GripButton => InputState.WasGripReleased,
			HoldInput.TriggerButton => InputState.WasTriggerReleased,
			HoldInput.Nothing => false,
			_ => false
		};
	}

	/// <summary>
	/// Called every <see cref="OnUpdate"/>, responsible for parsing input for holding stuff.
	/// </summary>
	protected void UpdateHoldInput()
	{
		bool didRelease = UpdateAutoReleaseObject();

		// Do we have a candidate object to hold?
		var holdable = TryFindHoldableObject();

		if ( holdable.IsValid() && holdable.HoldType == HoldType.OnRelease )
		{
			// We releasing the object?
			if ( WasReleased( holdable.HoldInput ) )
			{
				if ( holdable == HeldObject )
				{
					StopHolding();
				}
				else
				{
					StartHolding( holdable );
				}
			}
		}
		else if ( holdable.IsValid() && holdable.HoldType == HoldType.Continuous )
		{
			if ( IsDown( holdable.HoldInput ) )
			{
				StartHolding( holdable );
			}
			else
			{
				StopHolding();
			}
		}
	}
}
