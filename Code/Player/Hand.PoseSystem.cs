namespace TableTennis;

public partial class Hand
{
	/// <summary>
	/// Represents a controller to use when fetching skeletal data (finger curl/splay values)
	/// </summary>
	public enum HandSources
	{
		/// <summary>
		/// The left controller
		/// </summary>
		Left,

		/// <summary>
		/// The right controller
		/// </summary>
		Right
	}

	/// <summary>
	/// Which hand should we use to update the parameters?
	/// </summary>
	[Property] public HandSources HandSource { get; set; } = HandSources.Left;

	/// <summary>
	/// A preset pose. This is fucking shit, but I don't think it matters for this game.
	/// </summary>
	public enum PresetPose
	{
		None,
		Grip,
		GripNoIndex,
		HoldItem,
		Clamp
	}
	
	private static List<string> AnimGraphNames = new()
	{
		"FingerCurl_Thumb",
		"FingerCurl_Index",
		"FingerCurl_Middle",
		"FingerCurl_Ring",
		"FingerCurl_Pinky"
	};

	/// <summary>
	/// Applies a hand preset for this hand.
	/// </summary>
	/// <param name="preset"></param>
	public void ApplyHandPreset( HandPreset preset = null )
	{
		if ( !Game.IsRunningInVR ) return;

		// Get our controller inputs
		var source = GetController();

		Model.Set( "BasePose", 1 );
		Model.Set( "bGrab", true );
		Model.Set( "GrabMode", 1 );

		for ( FingerValue v = FingerValue.ThumbCurl; v <= FingerValue.PinkyCurl; ++v )
		{
			Model.Set( AnimGraphNames[(int)v], source.GetFingerValue( v ) );
		}

		if ( preset is not null )
		{
			preset.Apply( Model );
		}
	}

	/// <summary>
	/// Designed to run every Update, will update the pose of the hand.
	/// </summary>
	private void UpdatePose()
	{
		if ( !Model.IsValid() )
			return;

		// Apply default preset
		ApplyHandPreset();
	}
}
