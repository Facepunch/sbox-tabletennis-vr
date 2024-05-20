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
	public static int BallSpawnTime { get; set; } = 5;

	[ConVar.Server( "tt_debug_gamestate" )]
	public static bool DebugGameState { get; set; } = false;

	[ConVar.Server( "tt_debug_slowmo" )]
	public static bool DebugSlowMo { get; set; } = false;

	[Event.Tick.Server]
	public void BallTimer()
	{
		if ( !DebugBallPhysics ) return;

		if ( LastSpawn < BallSpawnTime ) return;
		
		LastHitter = null;

		var position = new Vector3( 72.0f, Game.Random.Float( -28.0f, 28.0f ), 56.0f );
		var velocity = Vector3.Backward * Game.Random.Float( 360.0f, 380.0f );

		LocalSetBall( To.Everyone, position, velocity );

		LastSpawn = 0;
	}

	[ClientRpc]
	public void LocalSetBall( Vector3 position, Vector3 velocity )
	{
		// if ( Ball.IsValid() ) Ball.Delete();
		// Ball = new Ball();
		Ball.Position = position;
		Ball.Velocity = velocity;
	}

	private void DebugSimulate( IClient cl )
	{
		if ( DebugSpawnBallAlways )
		{
			var spawnButtonPressed = Input.VR.LeftHand.ButtonA.WasPressed || Input.Pressed( InputButton.Jump );
			if ( spawnButtonPressed )
				ClientServingBall( To.Everyone, cl );
		}
	}
}
