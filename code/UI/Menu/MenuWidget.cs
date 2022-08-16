namespace TableTennis;

public class MenuTeamCard : Panel
{
	public Team Team { get; set; }
	public Label TeamScore { get; set; }

	public Label PlayerName { get; set; }
	public Image AvatarImage { get; set; }

	public string TeamId { get; set; } = "blue";

	public void SetTeam( Team team )
	{
		if ( !team.IsValid() ) return;

		DeleteChildren( true );

		SetClass( "red", team is Team.Red );

		Team = team;

		AvatarImage = Add.Image( "/ui/facepunch.png", "playeravatar" );
		PlayerName = Add.Label( team.Client?.Name ?? "N/A", "playername" );

		Add.Panel( "stretch" );
		TeamScore = Add.Label( $"{team.CurrentScore}", "teamscore" );
	}

	public override void SetProperty( string name, string value )
	{
		if ( name == "team" )
		{
			TeamId = value;
		}
	}

	public override void Tick()
	{
		base.Tick();

		if ( !Team.IsValid() )
		{
			var game = TableTennisGame.Current;
			var team = TeamId == "red" ? game.RedTeam : game.BlueTeam;
			SetTeam( team );

			return;
		}

		TeamScore.Text = $"{Team.CurrentScore}";

		if ( Team.Client.IsValid() )
		{
			PlayerName.Text = Team.Client.Name;
			AvatarImage.SetTexture( $"avatar:{Team.Client.PlayerId}" );
		}
	}
}

public class MenuPageWidget : WorldPanel
{
	//
}

[UseTemplate]
public class MenuWidget : WorldPanel
{
	public bool Enabled { get; set; } = false;
	public MenuPageWidget Page { get; set; }
	Vector2 Size => new( 500, 420f );

	public MenuTeamCard BlueTeamCard { get; set; }
	public MenuTeamCard RedTeamCard { get; set; }

	public MenuWidget()
	{
		BindClass( "focused", () => Page is null );
	}

	public void SetPage( MenuPageWidget page )
	{
		if ( Page != null ) Page.Delete( true );

		Page = page;

		SetClass( "inpage", page != null );
	}

	public void OpenPreferences()
	{
		SetPage( new ClientPreferencesWidget() );
	}

	public void Return() { SetPage( null ); }

	public void SetEnabled( bool enabled )
	{
		Enabled = enabled;
		SetClass( "enabled", enabled );
	}

	public override void Tick()
	{
		var pawn = Local.Pawn as PlayerPawn;
		if ( !pawn.IsValid() ) 
			return;
		
		var hand = pawn.ServeHand;
		if ( !hand.IsValid() ) 
			return;

		Position = hand.Position + Vector3.Up * 3f;

		if ( Global.IsRunningInVR )
			Rotation = Rotation.LookAt( -Input.VR.Head.Rotation.Forward );
		else
			Rotation = Rotation.LookAt( -CurrentView.Rotation.Forward );

		PanelBounds = new( -Size.x / 2f, -Size.y, Size.x, Size.y );
		WorldScale = 0.4f;
		Scale = 2.0f;
	}
}
