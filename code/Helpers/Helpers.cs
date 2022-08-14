namespace TableTennis;

public partial class Helpers
{
	public static Dictionary<string, int> Storage { get; protected set; } = new();

	public static string StorageCookie => "tt.helpers";

	public static void Load()
	{
		Storage = Cookie.Get( StorageCookie, new Dictionary<string, int>() );
	}

	public static void Save()
	{
		Cookie.Set( StorageCookie, Storage );
	}

	public static bool ShouldShow( string identifier, int amountUntilStop = 0 )
	{
		var value = 0;
		Storage.TryGetValue( identifier, out value );

		return amountUntilStop == 0 || value < amountUntilStop;
	}

	public static void Add( string identifier )
	{
		if ( Storage.ContainsKey( identifier ) )
		{
			Storage[identifier]++;
		}
		else
		{
			Storage.Add( identifier, 1 );
		}

		Save();
	}

	public static void Display( string identifier, string message, Vector3 worldPosition, string icon = null, float lifetime = 2f )
	{
		Host.AssertClient();

		_ = new HelperWidget( message, worldPosition, icon, lifetime );
		Add( identifier );
	}

	public static void Display( string identifier, string message, Entity followEntity, string icon = null, float lifetime = 2f )
	{
		Host.AssertClient();

		_ = new HelperWidget( message, followEntity, icon, lifetime );
		Add( identifier );
	}

	[ConCmd.Client( "tt_helper_send", CanBeCalledFromServer = true )]
	public static void TryDisplay( string identifier, string message, Vector3 worldPosition, int amountUntilStop = 0, string icon = null, float lifetime = 2f )
	{
		if ( ShouldShow( identifier, amountUntilStop ) )
		{
			Display( identifier, message, worldPosition, icon, lifetime );
		}
	}

	[ConCmd.Client( "tt_helper_send_entity", CanBeCalledFromServer = true )]
	public static void TryDisplay( string identifier, string message, int networkIdent, int amountUntilStop = 0, string icon = null, float lifetime = 2f )
	{
		TryDisplay( identifier, message, Entity.FindByIndex( networkIdent ), amountUntilStop, icon, lifetime );
	}
		
	public static void TryDisplay( string identifier, string message, Entity followEntity, int amountUntilStop = 0, string icon = null, float lifetime = 2f )
	{
		Host.AssertClient();

		if ( ShouldShow( identifier, amountUntilStop ) )
		{
			Display( identifier, message, followEntity, icon, lifetime );
		}
	}
}
