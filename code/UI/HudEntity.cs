namespace TableTennis.UI;

public class HudEntity : HudEntity<RootPanel>
{
	public ScoreWidget Scores { get; set; }
	public HintWidget Hints { get; set; }
	public MenuWidget Menu { get; set; }

	public HudEntity()
	{
		if ( !Game.IsClient )
			return;

		Scores = new ScoreWidget();
		Hints = new HintWidget();
		Menu = new MenuWidget();
	}
}
