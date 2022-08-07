namespace TableTennis;

public partial class TableTennisGame
{
	TimeSince LastSpawn = 0;
	Paddle DebugPaddle;

	[ConVar.Replicated( "tt_debug_ballspawn" )]
	public static bool BallSpawnDebug { get; set; } = false;

	[ConVar.Replicated( "tt_debug_ballspawnthrow" )]
	public static int BallSpawnThrowResponse { get; set; } = 1;

	[ConVar.Replicated( "tt_debug_ballspawntime" )]
	public static int BallSpawnTime { get; set; } = 3;

	[Event.Tick.Server]
	public void BallTimer()
	{
		if ( !BallSpawnDebug ) return;

		// Don't worry about this code getting ugly as shit

		if ( DebugPaddle.IsValid() && LastSpawn > 0.95f && LastSpawn < 1.2f )
		{
			var pitch = MathF.Sin( LastSpawn * 20 ) * 90;
			
			if ( BallSpawnThrowResponse > 0f /* 1 */ )
			{
				DebugPaddle.Rotation = Rotation.FromRoll( 80 ) * Rotation.FromYaw( 30 ) * Rotation.FromPitch( pitch );
			}
			else /* 0, or anything else that isn't specifically stated */
			{
				DebugPaddle.Rotation = Rotation.FromRoll( 90 ) * Rotation.FromYaw( -20 ) * Rotation.FromPitch( pitch );
			}
		}

		if ( LastSpawn < BallSpawnTime ) return;

		SpawnBall();
		ActiveBall.Position = new Vector3( -72.0f, Rand.Float( -28.0f, 28.0f ), 56.0f );
		ActiveBall.Velocity = Vector3.Forward * Rand.Float( 160.0f, 180.0f );
		LastSpawn = 0;
	}
}
