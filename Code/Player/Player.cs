namespace TableTennis;

/// <summary>
/// The player!
/// </summary>
[Icon( "accessibility_new" )]
public partial class Player : Component
{
	/// <summary>
	/// The paddle prefab.
	/// </summary>
	[Property, Group( "Prefabs" )] public GameObject PaddlePrefab { get; set; }

	/// <summary>
	/// The player's left hand component.
	/// </summary>
	[Property, Group( "Components" )] public Hand LeftHand { get; set; }

	/// <summary>
	/// The player's right hand component.
	/// </summary>
	[Property, Group( "Components" )] public Hand RightHand { get; set; }

	protected override void OnStart()
	{
		// Good for debugging stuff
		GameObject.BreakFromPrefab();

		// Don't hold a paddle if we're in the menu (shit way of detecting it kinda)
		if ( MenuManager.Instance.IsValid() )
		{
			return;
		}

		CreateAndHoldPaddle();
	}

	void CreateAndHoldPaddle()
	{
		var paddleInstance = PaddlePrefab.Clone();
		var holdable = paddleInstance.Components.Get<IHoldableObject>( FindMode.EnabledInSelfAndDescendants );
		RightHand.StartHolding( holdable );
	}
}
