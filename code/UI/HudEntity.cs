namespace TableTennis;

public class HudEntity : HudEntity<RootPanel>
{
	public ScoreWidget Scores { get; set; }
	public HintWidget Hints { get; set; }

	public HudEntity()
	{
		if ( !IsClient )
			return;

		Scores = new ScoreWidget();
		Hints = new HintWidget();
	}
}