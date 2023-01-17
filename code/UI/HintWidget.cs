namespace TableTennis;

internal class HintWidgetEntry : Panel
{
	public HintWidgetEntry( string text, string icon = null, float lifetime = 5f, string className = null )
	{
		if ( !string.IsNullOrEmpty( icon ) )
		{
			if ( icon.StartsWith( "avatar:" ) )
				Add.Image( icon, "avatar" );
			else
				Add.Label( icon, "icon" );
		}

		if ( !string.IsNullOrEmpty( className ) )
			SetClass( className, true );

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
		PanelBounds = new( -Size.x / 2f, -Size.y / 2f, Size.x, Size.y );
	}

	public void AddEntry( string text, string icon = null, float lifetime = 5f, string className = null )
	{
		Canvas.AddChild( new HintWidgetEntry( text, icon, lifetime, className ) );
	}

	public override void Tick()
	{
		base.Tick();

		var game = TableTennisGame.Current;
		if ( !game.IsValid() )
			return;

		var myTeam = Game.LocalClient?.GetTeam();
		if ( myTeam == null )
			return;

		Position = myTeam.UIAnchor.Position + Vector3.Up * 17f;
		Rotation = myTeam.UIAnchor.Rotation;
	}

	[ConCmd.Client( "tt_addmessage", CanBeCalledFromServer = true )]
	public static void AddMessage( string message, string icon = null, float lifetime = 5f, string className = null )
	{
		Current.AddEntry( message, icon, lifetime, className );

		if ( !Game.IsListenServer )
		{
			Log.Info( $"{message}" );
		}
	}

	[ConCmd.Client( "tt_hinttest" )]
	public static void HintTest()
	{
		var a = async () =>
		{
			HintWidget.AddMessage( $"You scored a point!", $"thumb_up", 3f, "win" );
			await GameTask.DelaySeconds( 3f );
			HintWidget.AddMessage( $"You lost a point.", $"thumb_down", 3f, "lose" );
		};
		_ = a();
	}
}
