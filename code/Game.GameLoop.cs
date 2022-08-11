namespace TableTennis;

public partial class TableTennisGame
{
	/// <summary>
	/// Z position that dictates that the ball is below the table, and thus out of bounds.
	/// </summary>
	public static float OutOfBoundsZ => 20f;

	/// <summary>
	/// A game of table tennis has a total of 11 points.
	/// A game must be won by two points.
	/// </summary>
	public static int MaxPoints => 11;
	
	/// <summary>
	/// The game's state. <see cref="GameState"/> for all available states of play.
	/// </summary>
	[Net, Change( "OnStateChanged" )] private GameState _state { get; set; }

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
	[Net] public int CurrentServe { get; set; } = 0;

	/// <summary>
	/// The current bounce. When this hits two, we can assume a point will be awarded.
	/// This is reset when a ball is hit.
	/// </summary>
	[Net] public int CurrentBounce { get; set; } = 0;

	// Teams
	[Net] public Team BlueTeam { get; set; }
	[Net] public Team RedTeam { get; set; }

	public Team GetOppositeTeam( Team team )
	{
		if ( team == BlueTeam )
			return RedTeam;

		return BlueTeam;
	}

	public Team ServingTeam { get; set; }

	public void ResetGame()
	{
		ServingTeam = null;
		CurrentServe = 0;
		CurrentBounce = 0;
		State = GameState.WaitingForPlayers;
		BlueTeam?.Reset();
		RedTeam?.Reset();

		HintWidget.AddMessage( To.Everyone, $"The game was reset.", $"info" );
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

		if ( BlueTeam.IsOccupied() && RedTeam.IsOccupied() )
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

	protected void SetupPlayer( Client cl )
	{
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

	[Event.Tick.Server]
	protected void TickGameLoop()
	{
		var ball = ServerBall;
		if ( !ball.IsValid() )
			return;

		if ( State == GameState.Playing )
		{
			if ( ball.Position.z <= OutOfBoundsZ )
			{
				if ( CurrentBounce == 1f && LastHitter != null )
				{
					LastHitter.ScorePoint();
				}
				else
				{
					GetOppositeTeam( LastHitter ).ScorePoint();
				}

				State = GameState.Serving;
			}
		}
	}

	[ConCmd.Server]
	public static void ServerBallBounce( int ballIdent, Vector3 hitPos )
	{
		var cl = ConsoleSystem.Caller;

		var ball = Entity.FindByIndex( ballIdent ) as Ball;
		if ( !ball.IsValid() ) return;

		Current.OnBallBounce( ball, hitPos );
		Current.RpcBallBounce( To.Everyone, ball, hitPos );
	}

	[ClientRpc]
	public void RpcBallBounce( Ball ball, Vector3 hitPos )
	{
		OnBallBounce( ball, hitPos );
	}

	public void OnBallBounce( Ball ball, Vector3 hitPos, Surface surface = null )
	{
		if ( !IsServer ) return;

		// If the ball bounces while we're serving, the player threw the ball and didn't hit it
		if ( State == GameState.Serving )
		{
			// GiveServingBall( ServingTeam.Client );
			return;
		}

		// We only care about ball bounce events when we're in play.
		if ( State != GameState.Playing )
			return;

		CurrentBounce++;

		if ( CurrentBounce == 2f )
		{
			var winner = GetBounceWinner( ball, hitPos );

			State = GameState.Serving;
			
			if ( winner != null )
			{
				winner.ScorePoint();
				State = GameState.Serving;
			}
		}
	}

	[Net] public Team LastHitter { get; set; }
	public TimeSince SinceLastHit { get; set; }

	/// <summary>
	/// Called clientside by the person hitting the paddle.
	/// </summary>
	/// <param name="paddle"></param>
	/// <param name="ball"></param>
	public void OnPaddleHit( Paddle paddle, Ball ball = null )
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

		}

		RpcPaddleHit( To.Everyone, paddle, ball );
	}

	[ClientRpc]
	public void RpcPaddleHit( Paddle paddle, Ball ball = null )
	{
		OnPaddleHit( paddle, ball );
	}

	[ConCmd.Server]
	public static void ServerPaddleHit( int paddleIdent )
	{
		var cl = ConsoleSystem.Caller;

		var paddle = Entity.FindByIndex( paddleIdent ) as Paddle;
		if ( !paddle.IsValid() ) return;

		Current.OnPaddleHit( paddle );
		Current.RpcPaddleHit( To.Everyone, paddle );
	}

	public Team GetBounceWinner( Ball ball, Vector3 hitPos )
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

	/// <summary>
	/// Called on server & client when the game state changes.
	/// </summary>
	/// <param name="oldState"></param>
	/// <param name="newState"></param>
	public void OnStateChanged( GameState oldState, GameState newState )
	{
		if ( IsServer )
		{
			if ( newState == GameState.WaitingForPlayers )
			{
				ServerBall?.Delete();
				
				if ( BlueTeam.IsOccupied() && RedTeam.IsOccupied() )
				{
					State = GameState.Serving;
				}
			}

			if ( newState == GameState.Serving )
			{
				CurrentBounce = 0;
				LastHitter = null;

				if ( CurrentServe % 2 == 0 )
				{
					SetServingTeam( ServingTeam == BlueTeam ? RedTeam : BlueTeam );
				}

				CurrentServe++;
			}
		}
	}

	internal string GetPaddleSound()
	{
		if ( LastHitter == null )
			return "tabletennis.serve";

		return "tabletennis.paddle";
	}

	protected void SetServingTeam( Team team )
	{
		if ( State != GameState.Serving ) return;

		ServingTeam = team;
		// GiveServingBall( team.Client );
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
