namespace TableTennis;

/// <summary>
/// A component that turns off its GameObject if we're running in VR.
/// </summary>
[Icon( "toggle_off" )]
public sealed class DisableIfInVR : Component
{
	protected override void OnStart()
	{
		if ( Game.IsRunningInVR )
		{
			GameObject.Enabled = false;
		}
	}
}
