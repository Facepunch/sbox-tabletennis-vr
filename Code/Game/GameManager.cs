namespace TableTennis;

[Title( "Game" ), Icon( "sports_tennis" )]
public partial class GameManager : Component
{
	public static GameManager Instance { get; private set; }

	protected override void OnStart()
	{
		Instance = this;
	}
}
