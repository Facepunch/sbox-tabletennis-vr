namespace TableTennis;

[Title( "Game" ), Icon( "sports_tennis" )]
public partial class GameManager : Component, Component.INetworkListener, ISceneStartup
{
	public static GameManager Instance { get; private set; }

	[Property]
	public GameObject PlayerPrefab { get; set; }

	protected override void OnStart()
	{
		Instance = this;
	}

	void ISceneStartup.OnHostInitialize()
	{
		Networking.CreateLobby( new Sandbox.Network.LobbyConfig() { MaxPlayers = 2, DestroyWhenHostLeaves = true } );
	}
	
	/// <summary>
	/// Find a spawn point for a player
	/// </summary>
	/// <param name="player"></param>
	/// <returns></returns>
	private Transform GetSpawnTransform( Player player )
	{
		var spawnPoints = Scene.GetAll<SpawnPoint>();
		var desired = spawnPoints.FirstOrDefault( x => x.Tags.Has( player.Team.Team.ToString().ToLowerInvariant() ) );
		if ( desired.IsValid() )
		{
			return desired.WorldTransform.WithScale( 1f );
		}
		else
		{
			return global::Transform.Zero;
		}
	}

	/// <summary>
	/// Assign a team for a player 
	/// </summary>
	/// <param name="player"></param>
	private void AssignTeam( Player player )
	{
		player.Team.Team = player.Team.GetBestTeam();
		Log.Info( $"Assigned {player.GameObject} to {player.Team.Team}" );
	}

	void INetworkListener.OnActive( Connection channel )
	{
		channel.CanSpawnObjects = false;

		var pl = PlayerPrefab?.Clone( new CloneConfig()
		{
			StartEnabled = true,
			Name = channel.DisplayName
		} );

		var player = pl.GetComponent<Player>();

		AssignTeam( player );

		pl.WorldTransform = GetSpawnTransform( player );
		pl.NetworkSpawn( channel );

		if ( GameSettings.FreePlay )
			State = GameState.FreePlay;
		else
			State = GameState.Serving;

		ServingTeam = Team.Red;
	}
}
