namespace TableTennis.UI;

internal class NameTagComponent : EntityComponent<PlayerPawn>
{
	NameTag NameTag;

	protected override void OnActivate()
	{
		var rankComponent = Entity.Client?.Components.Get<RankComponent>();
		NameTag = new NameTag( Entity.Client?.Name ?? Entity.Name, Entity.Client?.SteamId, rankComponent?.RankImage );
	}

	protected override void OnDeactivate()
	{
		NameTag?.Delete();
		NameTag = null;
	}

	/// <summary>
	/// Called for every tag, while it's active
	/// </summary>
	[Event.Client.Frame]
	public void FrameUpdate()
	{
		var tx = new Transform().WithPosition( Entity.HeadTransform.Position );
		tx.Position += Vector3.Up * 10.0f;
		
		if ( Global.IsRunningInVR )
			tx.Rotation = Rotation.LookAt( -Input.VR.Head.Rotation.Forward );
		else
			tx.Rotation = Rotation.LookAt( -Camera.Rotation.Forward );
		
		tx.Scale = 1f;
		
		NameTag.Transform = tx;

		var team = Entity.GetTeam();
		if ( team != null )
		{
			NameTag.SetClass( "red", team is Team.Red );
		}
	}

	[ConVar.Client( "tt_nametag_self" )]
	public static bool ShowOwnNametag { get; set; } = false;

	/// <summary>
	/// Called once per frame to manage component creation/deletion
	/// </summary>
	[Event.Client.Frame]
	public static void SystemUpdate()
	{
		foreach ( var player in Sandbox.Entity.All.OfType<PlayerPawn>() )
		{
			if ( player.IsLocalPawn && !ShowOwnNametag )
			{
				var c = player.Components.Get<NameTagComponent>();
				c?.Remove();
				continue;
			}

			var shouldRemove = player.Position.Distance( Camera.Position ) > 500;
			shouldRemove = shouldRemove || player.LifeState != LifeState.Alive;
			shouldRemove = shouldRemove || player.IsDormant;

			if ( shouldRemove )
			{
				var c = player.Components.Get<NameTagComponent>();
				c?.Remove();
				continue;
			}

			// Add a component if it doesn't have one
			player.Components.GetOrCreate<NameTagComponent>();
		}
	}
}

/// <summary>
/// A nametag panel in the world
/// </summary>
public class NameTag : WorldPanel
{
	public Panel Avatar;
	public Label NameLabel;
	public Panel Rank;

	public NameTag( string title, long? steamid, string rank = "rank_bronze" )
	{
		StyleSheet.Load( "UI/NameTag.scss" );

		if ( steamid != null )
		{
			Avatar = Add.Panel( "avatar" );
			Avatar.Style.SetBackgroundImage( $"avatar:{steamid}" );
		}

		NameLabel = Add.Label( title, "title" );
			
		Avatar = Add.Panel( "rank" );
		Avatar.Style.SetBackgroundImage( $"/ui/ranks/{rank}.png" );

		// this is the actual size and shape of the world panel
		PanelBounds = new Rect( -500, -100, 1000, 200 );
	}
}
