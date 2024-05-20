namespace TableTennis;

public class InputState
{
	const float flDeadzone = 0.25f;

	public struct Frame
	{
		public bool IsTriggerDown { get; set; }
		public bool IsGripDown { get; set; }
	}

	private Frame Previous;
	private Frame Current;

	/// <summary>
	/// Is the trigger down?
	/// </summary>
	public bool IsTriggerDown => Current.IsTriggerDown;

	/// <summary>
	/// Was the trigger released this frame?
	/// </summary>
	public bool WasTriggerReleased => IsTriggerDown && !Previous.IsTriggerDown;

	/// <summary>
	/// Is the grip down?
	/// </summary>
	public bool IsGripDown => Current.IsGripDown;

	/// <summary>
	/// Was the grip released this frame?
	/// </summary>
	public bool WasGripReleased => IsGripDown && !Previous.IsGripDown;

	public void Update( VRController controller )
	{
		Previous = Current;
		Current = new()
		{
			IsGripDown = controller?.Grip.Value > flDeadzone,
			IsTriggerDown = controller?.Trigger.Value > flDeadzone,
		};
	}
}
