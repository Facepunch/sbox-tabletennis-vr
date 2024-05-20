namespace TableTennis.UI;

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
		PlayerName = Add.Label( team.Player?.Name ?? "N/A", "playername" );

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

		if ( Team.Player.IsValid() )
		{
			PlayerName.Text = Team.Player.Name;
			AvatarImage.SetTexture( $"avatar:{Team.Player.SteamId}" );
		}
	}
}

public class MenuPageWidget : WorldPanel
{
	//
}
