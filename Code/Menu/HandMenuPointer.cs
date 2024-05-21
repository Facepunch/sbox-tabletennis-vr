namespace TableTennis;

public partial class HandMenuPointer : Component
{
	[Property] public Hand Hand { get; set; }
	[Property] public GameObject PointerOrigin { get; set; }
	
	private Sandbox.UI.WorldInput worldInput = new();

	protected override void OnEnabled()
	{
		worldInput.Enabled = Game.IsRunningInVR;
	}

	protected override void OnDisabled()
	{
		worldInput.Enabled = false;
	}

	protected override void OnUpdate()
	{
		var transform = PointerOrigin.Transform.World;
		float dist = 100000f;

		worldInput.Ray = new Ray( transform.Position, transform.Position + transform.Forward * dist );
		var rootPanel = worldInput.Hovered?.FindRootPanel();

		Gizmo.Draw.Color = Color.Red;
		if ( rootPanel is not null )
		{
			Gizmo.Draw.Color = Color.Green;
			// Fetch how far away the panel is in world-space so we can draw a line to it.
			rootPanel.RayToLocalPosition( worldInput.Ray, out _, out dist );
		}

		var tr = Scene.Trace.Ray( transform.Position, transform.Position + transform.Forward * dist )
			.IgnoreGameObject( GameObject )
			.Run();

		Gizmo.Draw.Line( tr.StartPosition, tr.EndPosition );

		worldInput.MouseLeftPressed = Hand.InputState.IsTriggerDown;
		worldInput.MouseRightPressed = Hand.InputState.IsGripDown;

		if ( Hand.Controller is not null )
		{
			// Mouse wheel translation :0
			worldInput.MouseWheel = Hand.Controller.Joystick.Value;
		}
	}
}
