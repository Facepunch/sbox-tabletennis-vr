namespace TableTennis;

public enum Team
{
	None = 0,

	Blue,
	Red
}

public partial class TeamComponent : Component
{
	public Team Team { get; set; }

	/// <summary>
	/// What color is this team?
	/// </summary>
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
}

public static class TeamExtensions
{
	public static Team GetTeam( this GameObject gameObject )
	{
		var comp = gameObject.Components.Get<TeamComponent>();
		if ( !comp.IsValid() ) return Team.None;

		return comp.Team;
	}

	public static Color GetTeamColor( this GameObject gameObject )
	{
		var comp = gameObject.Components.Get<TeamComponent>();
		if ( !comp.IsValid() ) return Color.White;
		return comp.Color;
	}

	public static Team GetTeam( this Component component ) => component.GameObject.GetTeam();
	public static Color GetTeamColor( this Component component ) => component.GameObject.GetTeamColor();
}
