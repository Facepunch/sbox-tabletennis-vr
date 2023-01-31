namespace TableTennis.UI;

public partial class HintWidget
{
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
