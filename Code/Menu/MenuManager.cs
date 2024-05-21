namespace TableTennis;

[Icon( "menu_open" )]
public partial class MenuManager : Component
{
	/// <summary>
	/// Singleton
	/// </summary>
	public static MenuManager Instance { get; private set; }

	/// <summary>
	/// A reference to the game scene, the one we'll enter when hitting play.
	/// </summary>
	[Property] public SceneFile GameScene { get; private set; }

	/// <summary>
	/// A reference to the player.
	/// </summary>
	[Property] public GameObject Player { get; set; }

	/// <summary>
	/// The world panel
	/// </summary>
	[Property] public Sandbox.WorldPanel WorldPanel { get; set; }

	protected override void OnStart()
	{
		Instance = this;
	}
	
	/// <summary>
	/// Used by the main menu, loads the main game scene.
	/// </summary>
	public void PlayGame()
	{
		Game.ActiveScene.Load( GameScene );
	}
}
