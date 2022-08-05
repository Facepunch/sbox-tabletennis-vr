namespace TableTennis;

[UseTemplate]
public partial class ScoreWidget : WorldPanel
{
	Vector2 Size => new( 1200, 64 );
	public ScoreWidget()
	{
	}

	public override void Tick()
	{
		base.Tick();

		// TODO - Decide where this is based on what team you're on
		Position = new Vector3( -1.2f, 0f, 33f );
		Rotation = Rotation.FromYaw( 180f );
		PanelBounds = new( -Size.x / 2f, -Size.y / 2f, Size.x, Size.y );
	}
}
