namespace TableTennis;

public partial class FocusWorldPanel : WorldPanel
{
	// These panels can define where they'll be relative to the player
	protected float Forward { get; set; } = 50f;
	protected float Right { get; set; } = 0;
	protected float Up { get; set; } = 10f;

	Transform head => Input.VR.Head;

	Vector3 GetWishPosition()
	{
		return head.Position
			+ head.Rotation.Forward * Forward
			+ head.Rotation.Right * Right
			+ head.Rotation.Up * Up;
	}

	Rotation GetWishRotation()
	{
		// Have the panel face the player's head at all times
		return Rotation.FromYaw( head.Rotation.Yaw() + 180f );
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
		Position = Position.LerpTo( focus.Position, Time.Delta * 5f );
		Rotation = Rotation.Slerp( Rotation, focus.Rotation, Time.Delta * 5f );

		//DebugOverlay.Line( Position, Position + Rotation.Forward * 10f, Color.White, 0.5f );
		//DebugOverlay.Sphere( Position, 1, Color.White, 0 );

		base.Tick();
	}
}
