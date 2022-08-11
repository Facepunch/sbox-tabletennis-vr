namespace TableTennis;

public partial class TableTennisGame
{
	[Net]
	public TimeSince LastSpawn { get; set; } = 0;

	/// <summary>
	/// Debug Mode disables the default game flow.
	/// Only use it if you wish to fuck about and test game features.
	/// </summary>
	public static bool DebugNoFlow => false;

	[ConVar.Replicated( "tt_debug_physics_test" )]
	public static bool DebugBallPhysics { get; set; } = false;

	[ConVar.Replicated( "tt_debug_spawn_ball" )]
	public static bool DebugSpawnBallAlways { get; set; } = false;

	[ConVar.Server( "tt_debug_ballspawntime" )]
	public static int BallSpawnTime { get; set; } = 3;

	[ConVar.Server( "tt_debug_gamestate" )]
	public static bool DebugGameState { get; set; } = false;

	[ConVar.Server( "tt_debug_slowmo" )]
	public static bool DebugSlowMo { get; set; } = false;

	[Event.Tick.Server]
	public void BallTimer()
	{
		if ( !DebugBallPhysics ) return;

		// Don't worry about this code getting ugly as shit

		if ( DebugSlowMo )
		{
			Global.TimeScale = (LastSpawn > 0.95f && LastSpawn < 1.1f) ? 0.05f : 1.0f;
		}

		if ( LastSpawn < BallSpawnTime ) return;

		LastHitter = null;
		// SpawnBall();
		// ServerBall.Position = new Vector3( -72.0f, Rand.Float( -28.0f, 28.0f ), 56.0f );
		// ServerBall.Velocity = Vector3.Forward * Rand.Float( 160.0f, 180.0f );

		LastSpawn = 0;
	}
}
