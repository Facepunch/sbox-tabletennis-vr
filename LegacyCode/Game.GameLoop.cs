using TableTennis.UI;

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

			Log.Info( $"Game state has changed from {oldState} to {_state}" );

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

	/// <summary>
	/// Incremented every time the ball gets hit
	/// </summary>
	[Net] public int RallyCount { get; set; } = 0;

	// Teams
	[Net] public Team BlueTeam { get; set; }
	[Net] public Team RedTeam { get; set; }
	[Net] public Team ServingTeam { get; set; }
	[Net] public TimeSince TimeSinceScoredPoint { get; set; }

	public void EndGame( Team winner )
	{
		var loser = GetOppositeTeam( winner );
		
		HintWidget.AddMessage( To.Everyone, $"{winner.Name} won the match!", "emoji_events", 20 );
		State = GameState.GameOver;

		/* GameServices.RecordEvent( winner.Client, $"Scored a serve (bounce: {CurrentBounce}, time: {TimeSinceScoredPoint}) and won the game!", 1, loser.Client );
		winner.Client.SetGameResult( GameplayResult.Win, 1 );

		if ( loser.Client != null )
		{
			GameServices.RecordEvent( loser.Client, $"Lost the game.", -1, winner.Client );
			loser.Client.SetGameResult( GameplayResult.Lose, -1 );
		}

		GameServices.EndGame(); */
	}

	public void OnScored( Team team )
	{
		team.CurrentScore++;

		if ( team.CurrentScore >= MaxPoints )
			EndGame( team );
		else
		{
			// GameServices.RecordEvent( team.Client, $"Scored a serve (bounce: {CurrentBounce}, time: {TimeSinceScoredPoint})", 1, GetOppositeTeam( team )?.Client );
			RpcScoredPoint( To.Everyone, team );
		}

		TimeSinceScoredPoint = 0;
	}

	[ClientRpc]
	public void RpcScoredPoint( Team team )
	{
		bool isMine = team.IsMine;
		if ( isMine )
			HintWidget.AddMessage( $"Point scored!", $"sports_score", 5f, "win" );
		else
			HintWidget.AddMessage( $"Point lost.", $"sports_score", 5f, "lose" );
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
		//	GameServices.AbandonGame( false );
		}

		currentServe = 0;
		serveRotation = 0;
		ServingTeam = BlueTeam;
		CurrentBounce = 0;
		RallyCount = 0;
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

	protected void CreatePawn( IClient cl )
	{
		cl.Pawn?.Delete();
		cl.Pawn = null;

		var pawn = new PlayerPawn();
		cl.Pawn = pawn;

		if ( !BlueTeam.TryAdd( cl ) )
		{
			if ( !RedTeam.TryAdd( cl ) )
				MakeSpectator( cl );
		}

		HintWidget.AddMessage( To.Everyone, $"{cl.Name} joined", $"avatar:{cl.SteamId}" );

		if ( State == GameState.WaitingForPlayers && BlueTeam.IsOccupied() && RedTeam.IsOccupied() )
			State = GameState.Serving;
	}

	protected void MakeSpectator( IClient cl )
	{
		var team = cl.GetTeam();
		team?.SetClient( null );

		cl.Pawn?.Delete();
		cl.Pawn = null;

		Log.Info( $"{cl.Name} (Vr: {cl.IsUsingVr}) joined as a spectator" );
		cl.Pawn = new SpectatorPawn();
	}

	public override void ClientJoined( IClient cl )
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
		// Unless we're in tools mode.
		if ( !Game.IsEditor && !cl.IsUsingVr )
		{
			MakeSpectator( cl );
			return;
		}

		CreatePawn( cl );
	}

	public override void ClientDisconnect( IClient cl, NetworkDisconnectionReason reason )
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
				//	GameServices.RecordEvent( cl, "Left the game too early." );
				//	GameServices.AbandonGame( true );

					ResetGame();
				}
			}
		}

		HintWidget.AddMessage( To.Everyone, $"{cl.Name} left", $"avatar:{cl.SteamId}" );
	}

	[Event.Tick.Server]
	protected void TickGameLoop()
	{
		if ( State == GameState.FailedServe )
		{
			if ( GameStateChanged >= 1.5f )
			{
				State = GameState.Serving;
			}
		}
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
				ResetGame();
			}
		}
	}

	[ConCmd.Server]
	public static void ServerBallBounce( int ballIdent, Vector3 hitPos )
	{
		var cl = ConsoleSystem.Caller;

		if ( Current.IsOnSide( cl, hitPos ) || Current.HasBotTeam() )
		{
			Current.OnBallBounce( hitPos );
			Current.RpcBallBounce( To.Everyone, hitPos );
		}
	}

	[ClientRpc]
	public void RpcBallBounce( Vector3 hitPos )
	{
		OnBallBounce( hitPos );
	}
	
	public bool HasBotTeam()
	{
		if ( !BlueTeam.Player.IsValid() || !RedTeam.Player.IsValid() ) return true;

		return BlueTeam.Player.IsBot || RedTeam.Player.IsBot;
	}

	public bool IsOnSide( IClient cl, Vector3 hitPos )
	{
		// Blue -x Red +x
		if ( BlueTeam.Player == cl )
		{
			return hitPos.x < 0;
		}

		if ( RedTeam.Player == cl )
		{
			return hitPos.x > 0;
		}

		return false;
	}

	public bool IsFloor( Vector3 pos )
	{
		return pos.z < 40f;
	}

	public void OnServingBounce( PlayerPawn player, TraceResult tr )
	{
		if ( !Game.IsServer ) return;

		// If the ball bounces somewhere while we're serving, assume that the paddle was not hit once.
		// The state gets set to Playing if the paddle is hit.

		State = GameState.FailedServe;
		
		HintWidget.AddMessage( To.Everyone, $"{ServingTeam.Player.Name} messed up their serve.", $"avatar:{ServingTeam.Player.SteamId}", 2f );
		Helpers.TryDisplay( To.Single( ServingTeam.Player ), "serve_failure", "Make sure to hit the paddle when serving.", player.PaddleHand.NetworkIdent, 5, "sports_tennis" );
	}

	public void WinRound( Team winner )
	{
		if ( winner != null )
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

	public void OnPlayBounce( PlayerPawn player, TraceResult tr )
	{
		CurrentBounce++;

		// The bounce winner is the person who wins based on where the ball landed.
		var bounceWinner = GetBounceWinner( tr.StartPosition );

		// If we didn't hit the table, assume it was the floor.
		// TODO - What about the net?
		bool isTable = tr.Surface?.ResourceName == "wood";
		if ( !isTable )
		{
			if ( CurrentBounce == 3 )
			{
				WinRound( ServingTeam );
			}
			else
			{
				WinRound( GetOppositeTeam( ServingTeam ) );
			}
		}
		// RallyCount == 1 assumes that the player just served, but hit the ball once.
		// Let's behave differently here.
		else if ( RallyCount == 1 )
		{
			if ( CurrentBounce == 1 )
			{
				// The ball must hit the serving team's side, otherwise it's illegal play.
				if ( !IsOnSide( ServingTeam.Player, tr.StartPosition ) )
				{
					WinRound( GetOppositeTeam( ServingTeam ) );
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

	public void OnBallBounce( Vector3 hitPos )
	{
		var player = ServingTeam.Player.Pawn as PlayerPawn;

		var tr = Trace.Ray( hitPos, hitPos + Vector3.Down * 10f )
			.WorldOnly()
			.Run();

		switch ( State )
		{
			case GameState.Serving: 
				OnServingBounce( player, tr );
				break;
			case GameState.Playing:
				OnPlayBounce( player, tr );
				break;
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

		if ( Game.IsServer )
		{
			var pawn = paddle.Owner as PlayerPawn;
			LastHitter = pawn.GetTeam();

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
		State = GameState.Serving;
	}

	/// <summary>
	/// Called on server & client when the game state changes.
	/// </summary>
	/// <param name="oldState"></param>
	/// <param name="newState"></param>
	public void OnStateChanged( GameState oldState, GameState newState )
	{
		if ( Game.IsServer )
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
				RallyCount = 0;
				TimeSinceScoredPoint = 0;
				CurrentBounce = 0;
				LastHitter = null;

				ClientServingBall( To.Everyone, ServingTeam.Player );
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
		DebugOverlay.Text( $"Last ball hitter: {LastHitter?.Player?.Name ?? "nobody"}", Vector3.Zero + Vector3.Down * 32f );
	}
}
