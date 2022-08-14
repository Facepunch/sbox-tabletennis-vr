namespace TableTennis;

[UseTemplate]
public partial class ScoreWidget : WorldPanel
{
	public int BlueScore { get; set; } = 0;
	public int RedScore { get; set; } = 0;

	public string RedName { get; set; } = "Red Team";
	public string BlueName { get; set; } = "Red Team";

	[ConVar.Client( "tt_debug_showscores" )]
	public static bool AlwaysShowScores { get; set; } = false;

	public TimeSince SinceChanged = 0;

	Vector2 Size => new( 1200, 900 );
	public ScoreWidget()
	{
		PanelBounds = new( -Size.x / 2f, -Size.y / 2f, Size.x, Size.y );

		BindClass( "visible", () => AlwaysShowScores || SinceChanged < 5f );
	}

	public override void Tick()
	{
		base.Tick();

		var game = TableTennisGame.Current;
		if ( !game.IsValid() )
			return;

		var myTeam = Local.Client?.GetTeam();
		if ( myTeam == null ) 
			return;

		//var anchor = new Transform( new Vector3( -30.2f, 0f, 30.5f ), Rotation.FromPitch( 90f ) * Rotation.FromYaw( 180f ) );
		var anchor = myTeam.ScoreAnchor;

		Position = anchor.Position;
		Rotation = anchor.Rotation;

		DebugOverlay.Sphere( Position, 1f, Color.White, 0, false );

		var blueTeam = game.BlueTeam;
		BlueScore = blueTeam.CurrentScore;
		
		var redTeam = game.RedTeam;
		RedScore = redTeam.CurrentScore;

		RedName = redTeam.Client?.Name ?? "N/A";
		BlueName = blueTeam.Client?.Name ?? "N/A";
	}

	[Event( "tt.gamestatechanged")]
	public void GameStateChanged( GameState oldState, GameState newState )
	{
		SinceChanged = 0;
	}
}
