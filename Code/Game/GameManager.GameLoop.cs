namespace TableTennis;

public enum GameState
{
	/// <summary>
	/// Waiting for players...
	/// </summary>
	WaitingForPlayers = 0,

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

	/// <summary>
	/// Free play, no game loop.
	/// </summary>
	FreePlay = 100
}

public partial class GameManager : IGameEvents
{
	/// <summary>
	/// The play area for this game. Used to dictate where the ball is bouncing, defined by two zones created in the editor.
	/// </summary>
	public PlayArea PlayArea => Scene.GetAll<PlayArea>().FirstOrDefault();

	/// <summary>
	/// A game of table tennis has a total of 11 points.
	/// A game must be won by two points.
	/// </summary>
	public static int MaxPoints => 11;

	/// <summary>
	/// How long has it been since the game state changed?
	/// </summary>
	[Sync( SyncFlags.FromHost )] public TimeSince TimeSinceGameStateChanged { get; private set; } = 1f;

	/// <summary>
	/// Which team hit the ball last?
	/// </summary>
	[Sync] public Team LastBallHitTeam { get; set; }

	/// <summary>
	/// How long has it been since the ball was hit?
	/// </summary>
	[Sync] public TimeSince TimeSinceBallHit { get; set; }

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

	/// <summary>
	/// Who's serving?
	/// </summary>
	[Sync] public Team ServingTeam { get; private set; } = Team.None;

	[Sync, Change( nameof( OnStateChanged ) )]
	private GameState _state { get; set; }

	public GameState State
	{
		get => _state;
		private set
		{
			if ( value == _state ) return;

			var oldState = _state;
			_state = value;

			Log.Info( $"Game state has changed from {oldState} to {_state}" );
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
		if ( newState == GameState.Serving )
		{
			RallyCount = 0;
			CurrentBounce = 0;
			LastBallHitTeam = Team.None;

			// Place the ball in the hand of the serving team.
			// Falls back to the first player if we don't have enough players.. Should we cry and break the game?
			PlaceBallInHand( GetFirstPlayer( ServingTeam ) ?? Scene.Components.GetAll<Player>().First() );
		}

		if ( newState == GameState.FailedServe )
		{
			SetStateAsync( GameState.Serving );	
		}
	}

	private async void SetStateAsync( GameState state, float time = 0.5f )
	{
		await GameTask.DelaySeconds( time );
		State = state;
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
		State = default;
		ServingTeam = Team.Red;
	}

	/// <summary>
	/// Who's the opposing team?
	/// </summary>
	/// <param name="team"></param>
	/// <returns></returns>
	protected Team GetOpposingTeam( Team team )
	{
		if ( team == Team.Blue )
			return Team.Red;
		return Team.Blue;
	}

	void IGameEvents.OnBallBounce( Ball ball, bool isTable )
	{
		if ( !Networking.IsHost )
			return;

		// The bounce winner is the person who wins based on where the ball landed.
		var areaTeam = PlayArea.GetTeamForArea( ball.WorldPosition );

		// Get the opposing team for where the ball landed, since they're gonna win this
		var bounceWinner = GetOpposingTeam( areaTeam );

		CurrentBounce++;

		switch ( State )
		{
			case GameState.Serving:
				{
					// If the ball bounces somewhere while we're serving, assume that the paddle was not hit once.
					// The state gets set to Playing if the paddle is hit.
					State = GameState.FailedServe;

				}
				break;
			case GameState.Playing:
				{

					if ( !isTable )
					{
						if ( CurrentBounce == 3 )
						{
							WinRound( ServingTeam );
						}
						else
						{
							WinRound( GetOpposingTeam( ServingTeam ) );
						}
					}
					// RallyCount == 1 assumes that the player just served, but hit the ball once.
					// Let's behave differently here.
					else if ( RallyCount == 1 )
					{
						if ( CurrentBounce == 1 )
						{
							// The ball must hit the serving team's side, otherwise it's illegal play.
							if ( areaTeam == ServingTeam )
							{
								WinRound( GetOpposingTeam( ServingTeam ) );
							}
						}
						// If we hit the third bounce.
						else if ( CurrentBounce == 3 )
						{
							WinRound( bounceWinner );
						}
					}
					// If we're on any other rally other than the first rally, let's play normal.
					// If the ball hits the second bounce, game over.
					else if ( RallyCount > 1 )
					{
						if ( CurrentBounce == 2 )
						{
							WinRound( bounceWinner );
						}
					}

				}
				break;
		}
	}

	void IGameEvents.OnBallHit( Ball ball, Paddle paddle )
	{
		if ( !Networking.IsHost )
			return;

		TimeSinceBallHit = 0;

		// Look for a team component from the paddle's hierarchy.
		LastBallHitTeam = paddle.GetTeam();

		// Up the rally count
		RallyCount++;

		if ( State == GameState.Serving )
		{
			State = GameState.Playing;
		}
		else if ( State == GameState.Playing )
		{
			var bounces = CurrentBounce;

			// Reset bounce count, as it's per rally.
			CurrentBounce = 0;

			// If the paddle is hit by a player, and the ball hasn't bounced yet - award the serving team a point.
			if ( bounces == 0 )
			{
				WinRound( ServingTeam );
			}
		}
	}

	/// <summary>
	/// Get the first player of a team
	/// </summary>
	/// <param name="Team"></param>
	/// <returns></returns>
	public Player GetFirstPlayer( Team Team )
	{
		return Scene.Components.GetAll<Player>()
			.Where( x => x.GetTeam() == Team )
			.FirstOrDefault();
	}
	
	/// <summary>
	/// Get the player's left hand
	/// </summary>
	/// <param name="player"></param>
	/// <returns></returns>
	public Hand GetLeftHand( Player player )
	{
		return player.Components.GetAll<Hand>()
			.FirstOrDefault( x => x.HandSource == Hand.HandSources.Left );
	}

	/// <summary>
	/// Places the ball in a player's hand.
	/// </summary>
	/// <param name="player"></param>
	public void PlaceBallInHand( Player player )
	{
		var hand = GetLeftHand( player );
		hand.Pickup( Ball );
	}
}
