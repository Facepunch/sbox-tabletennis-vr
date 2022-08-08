namespace TableTennis.UI;

internal class NameTagComponent : EntityComponent<PlayerPawn>
{
	NameTag NameTag;

	protected override void OnActivate()
	{
		NameTag = new NameTag( Entity.Client?.Name ?? Entity.Name, Entity.Client?.PlayerId );
	}

	protected override void OnDeactivate()
	{
		NameTag?.Delete();
		NameTag = null;
	}

	/// <summary>
	/// Called for every tag, while it's active
	/// </summary>
	[Event.Frame]
	public void FrameUpdate()
	{
		var tx = new Transform().WithPosition( Entity.EyePosition );
		tx.Position += Vector3.Up * 10.0f;
		tx.Rotation = Rotation.LookAt( -CurrentView.Rotation.Forward );
		tx.Scale = 1f;
		
		NameTag.Transform = tx;

		var team = Entity.GetTeam();
		if ( team != null )
		{
			NameTag.SetClass( "red", team is Team.Red );
		}
	}

	/// <summary>
	/// Called once per frame to manage component creation/deletion
	/// </summary>
	[Event.Frame]
	public static void SystemUpdate()
	{
		foreach ( var player in Sandbox.Entity.All.OfType<PlayerPawn>() )
		{
			if ( player.IsLocalPawn )
			{
				var c = player.Components.Get<NameTagComponent>();
				c?.Remove();
				continue;
			}

			var shouldRemove = player.Position.Distance( CurrentView.Position ) > 500;
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

	public NameTag( string title, long? steamid )
	{
		StyleSheet.Load( "UI/NameTag.scss" );

		if ( steamid != null )
		{
			Avatar = Add.Panel( "avatar" );
			Avatar.Style.SetBackgroundImage( $"avatar:{steamid}" );
		}

		NameLabel = Add.Label( title, "title" );

		// this is the actual size and shape of the world panel
		PanelBounds = new Rect( -500, -100, 1000, 200 );
	}
}
