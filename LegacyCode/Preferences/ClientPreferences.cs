namespace TableTennis;

public partial class ClientPreferences
{
	public static Settings LocalSettings { get; protected set; }
	public static string PrefsCookie => "tt.clientprefs";
	
	public static void Load()
	{
		LocalSettings = Cookie.Get<Settings>( PrefsCookie, new() );
		SendToServer();
	}

	public static void Save()
	{
		Cookie.Set( PrefsCookie, LocalSettings );
		SendToServer();
	}

	[ConCmd.Server( "tt_prefs_send" )]
	public static void CmdSendPreferences( string blob )
	{
		var cl = ConsoleSystem.Caller;

		var settings = Settings.FromJson( blob );
		if ( settings is null ) return;

		var component = cl.Components.GetOrCreate<ClientPreferencesComponent>();
		if ( component is null ) return;

		component.Update( settings );
	}

	[ConCmd.Server( "tt_prefs_debug" )]
	public static void CmdDebug()
	{
		foreach ( var cl in Game.Clients )
		{
			Log.Info( $"> {cl.Name}" );
			var component = cl.Components.GetOrCreate<ClientPreferencesComponent>();
			component.Settings.DebugLog();
		}
	}

	public static void SendToServer()
	{
		var blob = LocalSettings.ToJson();
		CmdSendPreferences( blob );
	}

	//
	public class Settings
	{
		[Title( "Left-Handed" ), Description( "Flips the ball and paddle between the player's hands." )]
		public bool FlipHands { get; set; } = false;

		[Title( "Release Grip To Serve" ), Description( "Release the grip to serve the ball." )]
		public bool ReleaseGripToServe { get; set; } = true;

		[Title( "Show Paddle Hand" ), Description( "Decide whether you can see your paddle hand or not." )]
		public bool ShowPaddleHand { get; set; } = true;

		[Title( "Anchor" ), Description( "An offset applied on top of your original playspace position." )]
		public VrAnchor Anchor { get; set; } = new();

		[Title( "Paddle Angle" ), Description( "The paddle's angle." ), MinMax( 0, 90f ), Step( 1f )]
		public float PaddleAngle { get; set; } = 90f;

		public string ToJson()
		{
			return System.Text.Json.JsonSerializer.Serialize( this );
		}

		public static Settings FromJson( string json )
		{
			return Json.Deserialize<Settings>( json );
		}

		public void DebugLog()
		{
			Log.Info( $"Flip Hands: {FlipHands}" );
			Log.Info( $"Anchor Offset: {Anchor}" );
			Log.Info( $"Paddle Angle: {PaddleAngle}" );
		}
	}
}
