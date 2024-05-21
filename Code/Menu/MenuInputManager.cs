namespace Sandbox;

[Title( "Menu Input" ), Icon( "linear_scale" )]
public sealed class MenuInputManager : Component
{
	private Sandbox.UI.WorldInput worldInput = new();

	protected override void OnEnabled()
	{
		worldInput.Enabled = true;
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
	}
}
