namespace TableTennis;

[Title( "Game" ), Icon( "sports_tennis" )]
public partial class GameManager : Component
{
	public static GameManager Instance { get; private set; }

	protected override void OnStart()
	{
		Instance = this;

		DebugStartGameAsync();
	}

	protected async void DebugStartGameAsync()
	{
		await GameTask.DelaySeconds( 1f );

		// We're serving by default, TODO: Networking OnActive
		State = GameState.Serving;
		ServingTeam = Team.Red;
	}
}
