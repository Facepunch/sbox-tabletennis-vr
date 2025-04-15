namespace TableTennis;

public static class GameSettings
{
	/// <summary>
	/// Are we in free play mode (No game loop)
	/// </summary>
	[ConVar( "tt.freeplay", ConVarFlags.GameSetting )]
	public static bool FreePlay { get; set; } = false;
}
