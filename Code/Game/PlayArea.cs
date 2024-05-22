using TableTennis;

namespace Sandbox;

public sealed class PlayArea : Component
{
	[Property, Group( "Areas" )] public BBox FirstArea { get; set; }
	[Property, Group( "Areas" )] public BBox SecondArea { get; set; }

	private void DrawArea( string identifier, BBox box, out BBox newBox, Color color = default )
	{
		newBox = box;

		bool selected = Gizmo.IsSelected;
		if ( selected )
		{
			Gizmo.Control.BoundingBox( identifier, box, out newBox );

			Gizmo.Draw.Color = color.WithAlpha( 0.2f );
			Gizmo.Draw.SolidBox( newBox );
		}

		Gizmo.Draw.Color = color.WithAlpha( selected ? 0.8f : 0.2f );
		Gizmo.Draw.LineBBox( newBox );
	}

	protected override void DrawGizmos()
	{
		Gizmo.Hitbox.BBox( FirstArea.AddBBox( SecondArea ) );

		{
			DrawArea( "FirstArea", FirstArea, out var box, Color.Blue );
			FirstArea = box;
		}

		{
			DrawArea( "SecondArea", SecondArea, out var box, Color.Red );
			SecondArea = box;
		}
	}

	/// <summary>
	/// Which team owns this area?
	/// </summary>
	/// <param name="point"></param>
	/// <returns></returns>
	public Team GetTeamForArea( Vector3 point )
	{
		if ( FirstArea.Contains( point ) ) return Team.Blue;
		if ( SecondArea.Contains( point ) ) return Team.Red;

		// Didn't fall within zone
		return Team.None;
	}
}
