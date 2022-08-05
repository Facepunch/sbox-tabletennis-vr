namespace TableTennis;

public partial class Team : BaseNetworkable
{
	// Networkable data
	[Net] public Client Client { get; set; }
	[Net] public int CurrentScore { get; set; }

	/// <summary>
	/// The team's color, used for UI elements mainly.
	/// </summary>
	public Color Color { get; set; }

	/// <summary>
	/// A friendly name for the team.
	/// </summary>
	public string Name { get; set; }

	/// <summary>
	/// A static location relative to the active table, to dictate where 
	/// the player will be anchored to while in play.
	/// </summary>
	public Transform Anchor { get; set; }

	/// <summary>
	/// A static location relative to the active table, to dictate where 
	/// the Scores and Hint widgets will be placed.
	/// </summary>
	public Transform UIAnchor { get; set; }


	/// <summary>
	/// Resets the player's anchor position, and anything else we might want to reset.
	/// </summary>
	public void Reset()
	{
		if ( !Client.IsValid() )
			return;
	
		var player = Client.Pawn as PlayerPawn;
		if ( !player.IsValid() )
			return;
	
		player.Transform = Anchor;
	}

	public bool IsOccupied() => Client.IsValid();

	protected void SetClient( Client cl )
	{
		Client = cl;
		Reset();

		Log.Info( $"Added {cl.Name} to {Name}" );
	}

	public bool TryAdd( Client cl )
	{
		if ( !IsOccupied() )
		{
			SetClient( cl );
			return true;
		}

		return false;
	}

	//
	
	public class Red : Team
	{
		public Red()
		{
			Color = Color.Parse( "#D71920" ) ?? default;
			Name = "Red Team";
			Anchor = new( new Vector3( 76.0f, 0, 0 ), Rotation.FromYaw( 180 ) );
			UIAnchor = new( new Vector3( -1.2f, 0f, 33f ), Rotation.FromYaw( 180f ) );
		}
	}

	public class Blue : Team
	{
		public Blue()
		{
			Color = Color.Parse( "#08B2E3" ) ?? default;
			Name = "Blue Team";
			Anchor = new( new Vector3( -76.0f, 0, 0 ), Rotation.FromYaw( 0 ) );
			UIAnchor = new( new Vector3( -1.2f, 0f, 33f ), Rotation.FromYaw( 0f ) );
		}
	}
}

public static class ClientExtensions
{
	public static Team GetTeam( this Client cl )
	{
		var game = TableTennisGame.Current;

		if ( game.BlueTeam.Client == cl )
			return game.BlueTeam;

		return game.RedTeam;
	}
}
