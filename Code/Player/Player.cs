namespace TableTennis;

/// <summary>
/// The player!
/// </summary>
[Icon( "accessibility_new" )]
public partial class Player : Component
{
	/// <summary>
	/// The player's left hand component.
	/// </summary>
	[Property, Group( "Components" )] public Hand LeftHand { get; set; }

	/// <summary>
	/// The player's right hand component.
	/// </summary>
	[Property, Group( "Components" )] public Hand RightHand { get; set; }

	/// <summary>
	/// The player's head
	/// </summary>
	[Property] public GameObject Head { get; set; }

	/// <summary>
	/// Auto-create a Team Component
	/// </summary>
	[RequireComponent] public TeamComponent Team { get; private set; }
}
