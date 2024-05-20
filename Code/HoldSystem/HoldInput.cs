namespace TableTennis;

/// <summary>
/// Hold input.
/// </summary>
public enum HoldInput
{
	/// <summary>
	/// Will only start holding the object when the VR controller's grip button is pressed down.
	/// </summary>
	[Icon( "pan_tool" )]
	GripButton,

	/// <summary>
	/// Will only start holding the object when the VR controller's trigger button is pressed down.
	/// </summary>
	[Icon( "touch_app" )]
	TriggerButton,

	/// <summary>
	/// Will hold the object regardless of any input.
	/// </summary>
	[Icon( "sensors" )]
	Nothing
}
