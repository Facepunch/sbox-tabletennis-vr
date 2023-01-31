namespace TableTennis;

public partial class Team : Entity
{
	public Team()
	{
		Transmit = TransmitType.Always;
	}

	// Networkable data
	[Net] public IClient Player { get; set; }
	[Net] public int CurrentScore { get; set; }

	/// <summary>
	/// The team's color, used for UI elements mainly.
	/// </summary>
	public Color Color { get; set; }

	/// <summary>
	/// A friendly name for the team.
	/// </summary>
	public new string Name { get; set; }

	/// <summary>
	/// A static location relative to the active table, to dictate where 
	/// the player will be anchored to while in play.
	/// </summary>
	public Transform Anchor { get; set; }

	/// <summary>
	/// A static location relative to the active table, to dictate where 
	/// the Hint widget will be placed.
	/// </summary>
	public Transform UIAnchor { get; set; }

	/// <summary>
	/// A static location relative to the active table, to dictate where 
	/// the Scores widget will be placed.
	/// </summary>
	public Transform ScoreAnchor { get; set; }

	/// <summary>
	/// Is this my team?
	/// </summary>
	public bool IsMine => Player == Game.LocalClient;

	/// <summary>
	/// Resets the player's anchor position, and anything else we might want to reset.
	/// </summary>
	public void Reset()
	{
		CurrentScore = 0;

		if ( !Player.IsValid() )
			return;
	
		var player = Player.Pawn as PlayerPawn;
		if ( !player.IsValid() )
			return;
	
		player.Transform = Anchor;

		if ( Game.IsRunningInVR )
			VR.Anchor = ClientPreferences.LocalSettings.Anchor.GetTransform();
	}

	public bool IsOccupied() => Player.IsValid();

	public void SetClient( IClient cl = null )
	{
		Player = cl;
		Reset();

		if ( Player.IsValid() )
		{
			Log.Info( $"Added {cl.Name} to {Name}" );
		}
	}

	public bool TryAdd( IClient cl )
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
			UIAnchor = new( new Vector3( 1.2f, 0f, 33f ), Rotation.FromYaw( 0f ) );
			ScoreAnchor = new( new Vector3( 30.2f, 0f, 30.5f ), Rotation.FromPitch( 90f ) * Rotation.FromYaw( 180f ) * Rotation.FromRoll( 180f ) );
		}
	}

	public class Blue : Team
	{
		public Blue()
		{
			Color = Color.Parse( "#08B2E3" ) ?? default;
			Name = "Blue Team";
			Anchor = new( new Vector3( -76.0f, 0, 0 ), Rotation.FromYaw( 0 ) );
			UIAnchor = new( new Vector3( -1.2f, 0f, 33f ), Rotation.FromYaw( 180f ) );
			ScoreAnchor = new( new Vector3( -30.2f, 0f, 30.5f ), Rotation.FromPitch( 90f ) * Rotation.FromYaw( 180f ) );
		}
	}
}

public static class ClientExtensions
{
	public static Team GetTeam( this IClient cl )
	{
		var game = TableTennisGame.Current;

		if ( game.BlueTeam.Player == cl )
			return game.BlueTeam;

		return game.RedTeam;
	}
}
