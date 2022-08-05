namespace TableTennis;

[UseTemplate]
public partial class ScoreWidget : WorldPanel
{
	public int BlueScore { get; set; } = 0;
	public int RedScore { get; set; } = 0;

	Vector2 Size => new( 1200, 64 );
	public ScoreWidget()
	{
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

		Position = myTeam.UIAnchor.Position;
		Rotation = myTeam.UIAnchor.Rotation;
		PanelBounds = new( -Size.x / 2f, -Size.y / 2f, Size.x, Size.y );

		var blueTeam = game.BlueTeam;
		BlueScore = blueTeam.CurrentScore;
		
		var redTeam = game.RedTeam;
		RedScore = redTeam.CurrentScore;
	}
}
