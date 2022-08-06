namespace TableTennis;

public partial class TableTennisGame
{
	/// <summary>
	/// Z position that dictates that the ball is below the table, and thus out of bounds.
	/// </summary>
	public static float OutOfBoundsZ => 0f;

	/// <summary>
	/// A game of table tennis has a total of 11 points.
	/// A game must be won by two points.
	/// </summary>
	public static int MaxPoints => 11;
	
	/// <summary>
	/// The game's state. <see cref="GameState"/> for all available states of play.
	/// </summary>
	[Net] public GameState State { get; set; }

	/// <summary>
	/// The current serve. In table tennis, the person who serves gets alternated evey 2 serves.
	/// </summary>
	[Net] public int CurrentServe { get; set; } = 0;

	// Teams
	[Net] public Team BlueTeam { get; set; }
	[Net] public Team RedTeam { get; set; }

	public void ResetGame()
	{
		CurrentServe = 0;
		BlueTeam.Reset();
		RedTeam.Reset();
	}

	protected void AddPlayerToTeam( Client cl )
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
	protected void TickGameLoop()
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
