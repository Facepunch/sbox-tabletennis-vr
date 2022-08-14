namespace TableTennis;

public partial class TableTennisGame
{
	/// <summary>
	/// A game of table tennis has a total of 11 points.
	/// A game must be won by two points.
	/// </summary>
	public static int MaxPoints => 11;

	/// <summary>
	/// The game's state. <see cref="GameState"/> for all available states of play.
	/// </summary>
	[Net, Change( "OnStateChanged" )] private GameState _state { get; set; }

	[Net] TimeSince GameStateChanged { get; set; } = 1f;

	public GameState State
	{
		get => _state;
		set
		{
			if ( DebugNoFlow ) return;
			if ( value == _state ) return;

			var oldState = _state;
			_state = value;

			Log.Info( $"{Host.Name}: Game state has changed from {oldState} to {_state}" );

			OnStateChanged( oldState, _state );
		}
	}

	/// <summary>
	/// The current serve. In table tennis, the person who serves gets alternated evey 2 serves.
	/// </summary>
	[Net] private int currentServe { get; set; } = 0;
	
	/// <summary>
	/// The amount of serves that have happened in the current rotation.
	/// </summary>
	[Net] private int serveRotation { get; set; }

	public int CurrentServe => currentServe;

	public void AddServe()
	{
		currentServe++;
		serveRotation++;

		if ( serveRotation == 2 )
		{
			serveRotation = 0;
			ServingTeam = ServingTeam == BlueTeam ? RedTeam : BlueTeam;
		}
	}

	/// <summary>
	/// The current bounce. When this hits two, we can assume a point will be awarded.
	/// This is reset when a ball is hit.
	/// </summary>
	[Net] public int CurrentBounce { get; set; } = 0;

	// Teams
	[Net] public Team BlueTeam { get; set; }
	[Net] public Team RedTeam { get; set; }
	[Net] public Team ServingTeam { get; set; }

	[Net] public TimeSince TimeSinceScoredPoint { get; set; }

	public void OnScored( Team team )
	{
		var otherTeam = GetOppositeTeam( team );
		
		if ( team.CurrentScore >= MaxPoints )
		{
			HintWidget.AddMessage( To.Everyone, $"{team.Name} won the match!", "emoji_events", 20 );
			State = GameState.GameOver;

			GameServices.RecordEvent( team.Client, $"Scored a serve (bounce: {CurrentBounce}, time: {TimeSinceScoredPoint}) and won the game!", 1, otherTeam.Client );
			GameServices.RecordScore( team.Client.PlayerId, team.Client.IsBot, GameplayResult.Win, 1 );

			if ( otherTeam.Client != null )
			{
				GameServices.RecordEvent( team.Client, $"Lost the game.", -1, otherTeam.Client );
				GameServices.RecordScore( team.Client.PlayerId, team.Client.IsBot, GameplayResult.Lose, -1 );
			}

			GameServices.EndGame();
		}
		else
		{
			GameServices.RecordEvent( team.Client, $"Scored a serve (bounce: {CurrentBounce}, time: {TimeSinceScoredPoint})", 1, otherTeam.Client );
		}

		TimeSinceScoredPoint = 0;
	}
	public Team GetOppositeTeam( Team team )
	{
		if ( team == BlueTeam )
			return RedTeam;

		return BlueTeam;
	}

	public void ResetGame( bool force = false )
	{
		if ( force )
		{
			HintWidget.AddMessage( To.Everyone, $"The game was reset.", $"info" );
			GameServices.AbandonGame( false );
		}

		currentServe = 0;
		serveRotation = 0;
		ServingTeam = BlueTeam;
		CurrentBounce = 0;
		State = GameState.WaitingForPlayers;
		BlueTeam?.Reset();
		RedTeam?.Reset();
	}

	[ConCmd.Server( "tt_togglespectator" )]
	public static void ToggleSpectator()
	{
		var cl = ConsoleSystem.Caller;

		if ( cl.Pawn is PlayerPawn )
			Current.MakeSpectator( cl );
		else
			Current.CreatePawn( cl );
	}

	protected void CreatePawn( Client cl )
	{
		cl.Pawn?.Delete();
		cl.Pawn = null;

		cl.Pawn = new PlayerPawn();

		if ( !BlueTeam.TryAdd( cl ) )
		{
			if ( !RedTeam.TryAdd( cl ) )
			{
				MakeSpectator( cl );
			}
		}

		HintWidget.AddMessage( To.Everyone, $"{cl.Name} joined", $"avatar:{cl.PlayerId}" );

		if ( State == GameState.WaitingForPlayers && BlueTeam.IsOccupied() && RedTeam.IsOccupied() )
		{
			State = GameState.Serving;
		}
	}

	protected void MakeSpectator( Client cl )
	{
		var team = cl.GetTeam();
		team?.SetClient( null );

		cl.Pawn?.Delete();
		cl.Pawn = null;

		Log.Info( $"{cl.Name} (Vr: {cl.IsUsingVr}) joined as a spectator" );
		cl.Pawn = new SpectatorPawn();
	}

	public override void ClientJoined( Client cl )
	{
		// Set up client prefrences
		cl.Components.GetOrCreate<ClientPreferencesComponent>();
		// Set up rank component
		cl.Components.GetOrCreate<RankComponent>();
		
		if ( DebugNoFlow )
		{
			CreatePawn( cl );
			return;
		}

		// Non-VR players can't play Table Tennis - only watch.
		// Circumvented if Debug Mode is enabled
		// Turn this back on when the game's ripe and ready to go.
		if ( /* !cl.IsUsingVr */ false )
		{
			MakeSpectator( cl );
			return;
		}

		CreatePawn( cl );
	}

	public override void ClientDisconnect( Client cl, NetworkDisconnectionReason reason )
	{
		if ( cl.Pawn.IsValid() )
		{
			cl.Pawn.Delete();
			cl.Pawn = null;

			var team = cl.GetTeam();
			if ( team != null )
			{
				team.Reset();

				if ( State != GameState.WaitingForPlayers )
				{
					GameServices.RecordEvent( cl, "Left the game too early." );
					GameServices.AbandonGame( true );

					ResetGame();
				}
			}
		}

		HintWidget.AddMessage( To.Everyone, $"{cl.Name} left", $"avatar:{cl.PlayerId}" );
	}

	[Event.Tick.Server]
	protected void TickGameLoop()
	{
		if ( State == GameState.PointAwarded )
		{
			if ( GameStateChanged >= 3f )
			{
				State = GameState.Serving;
			}
		}
		else if ( State == GameState.GameOver )
		{
			if ( GameStateChanged >= 30f )
			{
				State = GameState.WaitingForPlayers;
			}
		}
	}

	[ConCmd.Server]
	public static void ServerBallBounce( int ballIdent, Vector3 hitPos )
	{
		var cl = ConsoleSystem.Caller;

		Current.OnBallBounce( hitPos );
		Current.RpcBallBounce( To.Everyone, hitPos );
	}

	[ClientRpc]
	public void RpcBallBounce( Vector3 hitPos )
	{
		OnBallBounce( hitPos );
	}

	public void OnBallBounce( Vector3 hitPos )
	{
		if ( IsServer )
		{
			// If the ball bounces while we're serving, the player threw the ball and didn't hit it
			if ( State == GameState.Serving )
			{
				HintWidget.AddMessage( To.Single( ServingTeam.Client ), "Hit the ball to serve, it must not bounce.", "sports_tennis" );
				ClientServingBall( To.Everyone, ServingTeam.Client );
				return;
			}

			// We only care about ball bounce events when we're in play.
			if ( State != GameState.Playing )
				return;

			CurrentBounce++;

			if ( CurrentBounce == 2f )
			{
				var winner = GetBounceWinner( hitPos );
				if ( winner != null )
				{
					State = GameState.PointAwarded;
					winner.ScorePoint();
				}
				else
				{
					State = GameState.Serving;
				}
			}
		}
	}

	[Net] public Team LastHitter { get; set; }
	public TimeSince SinceLastHit { get; set; }

	/// <summary>
	/// Called clientside by the person hitting the paddle.
	/// </summary>
	/// <param name="paddle"></param>
	/// <param name="hitPosition"></param>
	/// <param name="isLocal"></param>
	public void OnPaddleHit( Paddle paddle, Vector3 hitPosition, bool isLocal = false )
	{
		SinceLastHit = 0;

		if ( IsServer )
		{
			var pawn = paddle.Owner as PlayerPawn;
			LastHitter = pawn.GetTeam();

			// TODO - Second hit of the paddle needs to have bounced at least once, otherwise it's illegal
			if ( State == GameState.Serving )
			{
				State = GameState.Playing;
			}

			RpcPaddleHit( To.Everyone, paddle, hitPosition );
		}
		else
		{
			if ( isLocal ) return;
			Sound.FromWorld( "tabletennis.serve", hitPosition ).SetVolume( Ball.Velocity.Length / 50f );
		}
	}

	[ClientRpc]
	public void RpcPaddleHit( Paddle paddle, Vector3 hitPosition )
	{
		OnPaddleHit( paddle, hitPosition );
	}

	[ConCmd.Server]
	public static void ServerPaddleHit( int paddleIdent, Vector3 hitPosition )
	{
		var cl = ConsoleSystem.Caller;

		var paddle = Entity.FindByIndex( paddleIdent ) as Paddle;
		if ( !paddle.IsValid() ) return;

		Current.OnPaddleHit( paddle, hitPosition );
		Current.RpcPaddleHit( To.Everyone, paddle, hitPosition );
	}

	public Team GetBounceWinner( Vector3 hitPos )
	{
		// TODO - Do this better, support multiple tables in the future?
		var tableX = 0f;

		if ( hitPos.x < tableX - 1f )
			return RedTeam;
		else if ( hitPos.x > tableX + 1f )
			return BlueTeam;

		// No winner, maybe hit the net?
		return null;
	}

	public void StartGame()
	{
		foreach ( var cl in Client.All )
		{
			cl.Components.Get<RankComponent>()?.FetchStats();
		}

		State = GameState.Serving;
		GameServices.StartGame();
	}

	/// <summary>
	/// Called on server & client when the game state changes.
	/// </summary>
	/// <param name="oldState"></param>
	/// <param name="newState"></param>
	public void OnStateChanged( GameState oldState, GameState newState )
	{
		if ( IsServer )
		{
			GameStateChanged = 0;

			if ( newState == GameState.WaitingForPlayers )
			{
				if ( BlueTeam.IsOccupied() && RedTeam.IsOccupied() )
				{
					StartGame();
				}
			}

			if ( newState == GameState.Serving )
			{
				TimeSinceScoredPoint = 0;
				CurrentBounce = 0;
				LastHitter = null;

				ClientServingBall( To.Everyone, ServingTeam.Client );
			}
		}

		Event.Run( "tt.gamestatechanged", oldState, newState );
	}

	[Event.Tick.Server]
	protected void GameStateDebug()
	{
		if ( !DebugGameState )
			return;

		DebugOverlay.Text( $"Game State: {State}", Vector3.Zero );
		DebugOverlay.Text( $"Current bounce: {CurrentBounce}", Vector3.Zero + Vector3.Down * 8f );
		DebugOverlay.Text( $"Current serve: {CurrentServe}", Vector3.Zero + Vector3.Down * 16f );
		DebugOverlay.Text( $"Serving team: {ServingTeam?.Name ?? "nobody"}", Vector3.Zero + Vector3.Down * 24f );
		DebugOverlay.Text( $"Last ball hitter: {LastHitter?.Client?.Name ?? "nobody"}", Vector3.Zero + Vector3.Down * 32f );
	}
}
