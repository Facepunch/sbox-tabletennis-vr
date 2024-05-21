namespace TableTennis;

[Icon( "menu_open" )]
public partial class MenuManager : Component
{
	public static MenuManager Instance { get; private set; }

	[Property] public SceneFile GameScene { get; private set; }

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
