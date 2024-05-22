namespace TableTennis;

public enum GameState
{
	/// <summary>
	/// Waiting for players to join the game.
	/// </summary>
	WaitingForPlayers,
	/// <summary>
	/// Players are serving.
	/// </summary>
	Serving,
	/// <summary>
	/// Server failed their serve.
	/// </summary>
	FailedServe,
	/// <summary>
	/// Players are playing.
	/// </summary>
	Playing,
	/// <summary>
	/// A point has been awarded.
	/// </summary>
	PointAwarded,
	/// <summary>
	/// The game is over.
	/// </summary>
	GameOver,
}

public partial class GameManager
{
	/// <summary>
	/// A game of table tennis has a total of 11 points.
	/// A game must be won by two points.
	/// </summary>
	public static int MaxPoints => 11;

	/// <summary>
	/// How long has it been since the game state changed?
	/// </summary>
	[Sync] public TimeSince TimeSinceGameStateChanged { get; private set; } = 1f;

	/// <summary>
	/// The current serve. In table tennis, the person who serves gets alternated evey 2 serves.
	/// </summary>
	[Sync] public int CurrentServe { get; private set; } = 0;

	/// <summary>
	/// The amount of serves that have happened in the current rotation.
	/// </summary>
	[Sync] public int ServeRotation { get; private set; } = 0;

	/// <summary>
	/// The current bounce. When this hits two, we can assume a point will be awarded.
	/// This is reset when a ball is hit.
	/// </summary>
	[Sync] public int CurrentBounce { get; private set; } = 0;

	/// <summary>
	/// Incremented every time the ball gets hit
	/// </summary>
	[Sync] public int RallyCount { get; private set; } = 0;

	[Sync] public Team ServingTeam { get; private set; } = Team.None;

	private GameState _state;
	[Sync( Query = true )] public GameState State
	{
		get => _state;
		private set
		{
			if ( value == _state ) return;

			var oldState = _state;
			_state = value;

			Log.Info( $"Game state has changed from {oldState} to {_state}" );
			OnStateChanged( oldState, _state );
			TimeSinceGameStateChanged = 0;
		}
	}

	/// <summary>
	/// Called when the game state changes.
	/// </summary>
	/// <param name="oldState"></param>
	/// <param name="newState"></param>
	protected void OnStateChanged( GameState oldState, GameState newState )
	{
	}

	/// <summary>
	/// Called when a team wins a round
	/// </summary>
	/// <param name="winner"></param>
	protected void WinRound( Team winner )
	{
		if ( winner != Team.None )
		{
			State = GameState.PointAwarded;

			AddServe();
			OnScored( winner );
		}
		else
		{
			State = GameState.Serving;
		}
	}

	/// <summary>
	/// Called when a team scores!
	/// </summary>
	/// <param name="team"></param>
	public void OnScored( Team team )
	{
		// Up the team score,
		// End the game if we reached the point limit
		// Inform the players of new score
	}

	/// <summary>
	/// This is normally called when a point is given to a team.
	/// </summary>
	protected void AddServe()
	{
		CurrentServe++;
		ServeRotation++;

		if ( ServeRotation == 2 )
		{
			ServeRotation = 0;
			ServingTeam = ServingTeam == Team.Blue ? Team.Red : Team.Blue;
		}
	}

	/// <summary>
	/// Resets the state of the game entirely
	/// </summary>
	protected void ResetGame()
	{
		CurrentServe = 0;
		ServeRotation = 0;
		CurrentBounce = 0;
		RallyCount = 0;
		State = GameState.WaitingForPlayers;
		ServingTeam = Team.None;
	}
}
