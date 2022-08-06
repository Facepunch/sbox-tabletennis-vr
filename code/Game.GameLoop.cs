namespace TableTennis;

public partial class TableTennisGame
{
	public GameState State { get; set; } = GameState.WaitingForPlayers;

	public static float OutOfBoundsZ => 0f;

	/// <summary>
	/// A game of table tennis has a total of 11 points.
	/// A game must be won by two points.
	/// </summary>
	public static int MaxPoints => 11;

	/// <summary>
	/// The current serve. In table tennis, the person who serves gets alternated evey 2 serves.
	/// </summary>
	[Net] public int CurrentServe { get; set; } = 0;

	[Net] public Team BlueTeam { get; set; }
	[Net] public Team RedTeam { get; set; }

	public void ResetGame()
	{
		CurrentServe = 0;
		BlueTeam.Reset();
		RedTeam.Reset();
	}

	public void AddPlayerToTeam( Client cl )
	{
		if ( !BlueTeam.TryAdd( cl ) )
		{
			if ( !RedTeam.TryAdd( cl ) )
			{
				// TODO - Assign spectators
			}
		}
	}

	[Event.Tick.Server]
	public void TickGameLoop()
	{
		var ball = ActiveBall;
		if ( !ball.IsValid() )
			return;

		if ( ball.Position.z <= OutOfBoundsZ )
		{
			// TODO - The ball hit the floor, let's act on it.
		}
	}
}
