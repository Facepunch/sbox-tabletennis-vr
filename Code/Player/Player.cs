namespace TableTennis;

/// <summary>
/// The player!
/// </summary>
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
}
