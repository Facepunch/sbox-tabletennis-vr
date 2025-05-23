using System.Text.Json.Serialization;

namespace TableTennis;

public enum Team
{
	None = 0,

	Blue,
	Red
}

/// <summary>
/// Dictates what team something belongs to
/// </summary>
public partial class TeamComponent : Component
{
	public Action<Team> OnTeamChanged { get; set; } 

	private Team team;
	[Property, Group( "Setup" )] public Team Team
	{
		get => team;
		set
		{
			if ( team == value ) return;

			team = value;
			TeamChanged( team );
		}
	}

	/// <summary>
	/// What color is this team?
	/// </summary>
	[Property, ReadOnly, JsonIgnore, Group( "Data" )]
	public Color Color
	{
		get
		{
			return Team switch
			{
				Team.Blue => Color.Parse( "#08B2E3" ) ?? default,
				Team.Red => Color.Parse( "#D71920" ) ?? default,
				_ => Color.White
			};
		}
	}

	private void TeamChanged( Team after )
	{
		OnTeamChanged?.Invoke( after );
	}
}

public static class TeamExtensions
{
	public static Team GetTeam( this GameObject gameObject )
	{
		var comp = gameObject.Components.Get<TeamComponent>( FindMode.EverythingInSelfAndAncestors );
		if ( !comp.IsValid() ) return Team.None;

		return comp.Team;
	}

	public static Color GetTeamColor( this GameObject gameObject )
	{
		var comp = gameObject.Components.Get<TeamComponent>( FindMode.EverythingInSelfAndAncestors );
		if ( !comp.IsValid() ) return Color.White;
		return comp.Color;
	}

	/// <summary>
	/// Crude method to get a good team
	/// </summary>
	/// <returns></returns>
	public static Team GetBestTeam( this TeamComponent team )
	{
		var teams = Game.ActiveScene.GetAll<TeamComponent>();
		var reds = teams.Count( x => x.Team == Team.Red );
		var blues = teams.Count( x => x.Team == Team.Blue );

		if ( reds > blues ) return Team.Blue;

		return Team.Red;
	}

	public static Team GetTeam( this Component component ) => component.GameObject?.GetTeam() ?? Team.None;
	public static Color GetTeamColor( this Component component ) => component.GameObject?.GetTeamColor() ?? Color.White;
}
