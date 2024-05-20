namespace TableTennis;

/// <summary>
/// The player!
/// </summary>
[Icon( "accessibility_new" )]
public partial class Player : Component
{
	/// <summary>
	/// The ball prefab.
	/// </summary>
	[Property, Group( "Prefabs" )] public GameObject BallPrefab { get; set; }

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

		CreateAndHoldPaddle();
		CreateAndHoldBall();
	}

	void CreateAndHoldPaddle()
	{
		var paddleInstance = PaddlePrefab.Clone();
		var holdable = paddleInstance.Components.Get<IHoldableObject>( FindMode.EnabledInSelfAndDescendants );
		RightHand.StartHolding( holdable );
	}

	void CreateAndHoldBall()
	{
		var paddleInstance = BallPrefab.Clone();
		var holdable = paddleInstance.Components.Get<IHoldableObject>( FindMode.EnabledInSelfAndDescendants );
		LeftHand.StartHolding( holdable );
	}
}
