namespace TableTennis;

public partial class FocusWorldPanel : WorldPanel
{
	float forward => 50f;
	Transform head => Input.VR.Head;

	Vector3 GetWishPosition()
	{
		return head.Position + head.Rotation.Forward * forward;
	}

	Rotation GetWishRotation()
	{
		return head.Rotation.RotateAroundAxis( Vector3.Right, 180f );
	}

	Transform GetFocus()
	{
		var transform = new Transform();

		// TODO - Figure out moving only when needed, as to not be annoying as fuck to look at

		transform.Position = GetWishPosition();
		transform.Rotation = GetWishRotation();

		return transform;
	}

	public override void Tick()
	{
		var focus = GetFocus();
		Position = focus.Position;
		Rotation = focus.Rotation;

//		DebugOverlay.Line( Position, Position + Rotation.Forward * 10f, Color.White, 0f );
//		DebugOverlay.Sphere( Position, 1, Color.White, 0 );

		base.Tick();
	}
}
