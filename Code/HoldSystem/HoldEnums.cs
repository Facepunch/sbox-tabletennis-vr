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

/// <summary>
/// The hold input type.
/// </summary>
public enum HoldType
{
	/// <summary>
	/// Called continuously, will release the object when you release the hold input.
	/// </summary>
	[Icon( "pan_tool" )]
	Continuous,

	/// <summary>
	/// Called when pressing the input button, will toggle the holding state.
	/// </summary>
	[Icon( "pan_tool" )]
	OnRelease,
}
