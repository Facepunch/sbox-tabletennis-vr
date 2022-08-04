namespace TableTennis;

internal class HintWidgetEntry : Panel
{
	public HintWidgetEntry( string text, float lifetime = 5f, string icon = null )
	{
		if ( !string.IsNullOrEmpty( icon ) )
		{
			if ( icon.StartsWith( "avatar:" ) )
				Add.Image( icon, "avatar" );
			else
				Add.Label( icon, "icon" );
		}

		Add.Label( text, "message" );
		_ = DeleteAsync( lifetime );
	}

	protected async Task DeleteAsync( float time )
	{
		await GameTask.DelayRealtimeSeconds( time );
		Delete();
	}
}

[UseTemplate]
public partial class HintWidget : WorldPanel
{
	public static HintWidget Current;

	// @ref
	public Panel Canvas { get; set; }
	
	Vector2 Size => new( 1200, 500f );
	public HintWidget()
	{
		Current = this;
	}

	public void AddMessage( string text, float lifetime = 5f, string icon = null )
	{
		Canvas.AddChild( new HintWidgetEntry( text, lifetime, icon ) );
	}

	public override void Tick()
	{
		base.Tick();

		Position = new Vector3( -1.2f, 0f, 50f );
		Rotation = Rotation.FromYaw( 180f );
		PanelBounds = new( -Size.x / 2f, -Size.y / 2f, Size.x, Size.y );
	}

	[ConCmd.Client( "tt_hinttest" )]
	public static void HintTest()
	{
		Current.AddMessage( "DevulTj joined the game", 5f, "login" );
		var a = async () =>
		{

			await GameTask.DelaySeconds( 4f );
			Current.AddMessage( "matt left the game", 5f, "logout" );
			
		};
		_ = a();
	}
}
