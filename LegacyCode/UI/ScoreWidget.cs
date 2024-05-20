namespace TableTennis.UI;

public partial class ScoreWidget
{
	[ConVar.Client( "tt_debug_showscores" )]
	public static bool AlwaysShowScores { get; set; } = false;
}
