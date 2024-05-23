using System.Collections.Generic;

namespace Sandbox;

[Title( "Menu Input" ), Icon( "linear_scale" )]
public sealed class MenuInputManager : Component
{
	private Sandbox.UI.WorldInput worldInput = new();

	protected override void OnEnabled()
	{
		worldInput.Enabled = !Game.IsRunningInVR;
	}

	protected override void OnDisabled()
	{
		worldInput.Enabled = false;
	}

	protected override void OnUpdate()
	{
		if ( Game.IsRunningInVR )
		{
			// ideally shouldn't need to do anything here, MenuManager will spawn a MenuPlayer prefab,
			// with hands that can interact with UI
		}
		else
		{
			UpdateFlat();
		}
	}

	private void UpdateFlat()
	{
		var look = Input.AnalogLook;

		worldInput.Ray = new Ray( Scene.Camera.Transform.Position, Scene.Camera.Transform.Rotation.Forward );

		Scene.Camera.Transform.Rotation *= Rotation.From( look );

		worldInput.MouseLeftPressed = Input.Down( "Attack1" );
		worldInput.MouseRightPressed = Input.Down( "Attack2" );
		worldInput.MouseWheel = Input.MouseWheel;

		var rootPanel = worldInput.Hovered?.FindRootPanel();

		var dist = -1f;

		if ( rootPanel is not null )
		{
			// Fetch how far away the panel is in world-space so we can draw a line to it.
			rootPanel.RayToLocalPosition( worldInput.Ray, out _, out dist );
		}

		if ( dist == -1f ) return;

		var tr = Scene.Trace.Ray( Scene.Camera.Transform.Position, Scene.Camera.Transform.Position + Scene.Camera.Transform.Rotation.Forward * dist )
			.IgnoreGameObject( GameObject )
			.Run();

		Gizmo.Draw.LineSphere( tr.EndPosition, 1 );
	}
}
