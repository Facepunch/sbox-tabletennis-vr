namespace TableTennis;

[Title( "Game" ), Icon( "sports_tennis" )]
public partial class GameManager : Component, Component.INetworkListener, ISceneStartup
{
	public static GameManager Instance { get; private set; }

	protected override void OnStart()
	{
		Instance = this;
	}

	void ISceneStartup.OnHostInitialize()
	{
		Networking.CreateLobby( new Sandbox.Network.LobbyConfig() { MaxPlayers = 2, DestroyWhenHostLeaves = true } );
	}

	void Component.INetworkListener.OnActive( Connection channel )
	{
		channel.CanSpawnObjects = false;

		// TODO: create player here

		State = GameState.Serving;
		ServingTeam = Team.Red;
	}
}
