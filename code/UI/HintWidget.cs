namespace TableTennis;

internal class HintWidgetEntry : Panel
{
	public HintWidgetEntry( string text, string icon = null, float lifetime = 5f )
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

	public void AddEntry( string text, string icon = null, float lifetime = 5f )
	{
		Canvas.AddChild( new HintWidgetEntry( text, icon, lifetime ) );
	}

	public override void Tick()
	{
		base.Tick();

		// TODO - Decide where this is based on what team you're on
		Position = new Vector3( -1.2f, 0f, 50f );
		Rotation = Rotation.FromYaw( 180f );
		PanelBounds = new( -Size.x / 2f, -Size.y / 2f, Size.x, Size.y );
	}

	[ConCmd.Client( "tt_addmessage", CanBeCalledFromServer = true )]
	public static void AddMessage( string message, string icon = null, float lifetime = 5f )
	{
		Current.AddEntry( message, icon, lifetime );

		if ( !Global.IsListenServer )
		{
			Log.Info( $"{message}" );
		}
	}

	[ConCmd.Client( "tt_hinttest" )]
	public static void HintTest()
	{
		Current.AddEntry( "DevulTj joined", "avatar:76561197973858781" );

		var a = async () =>
		{
			await GameTask.DelaySeconds( 3f );
			Current.AddEntry( "Blue scored 10 points", "recommend" );
			await GameTask.DelaySeconds( 4f );
			Current.AddEntry( "matt left", "avatar:76561197996859119" );
			
		};
		_ = a();
	}
}
