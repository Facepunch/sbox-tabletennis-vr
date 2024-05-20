namespace TableTennis;

[Icon( "front_hand" )]
public partial class Hand
{
	/// <summary>
	/// Represents a controller to use when fetching skeletal data (finger curl/splay values)
	/// </summary>
	public enum Source
	{
		/// <summary>
		/// The left controller
		/// </summary>
		[Icon( "front_hand" )]
		Left,

		/// <summary>
		/// The right controller
		/// </summary>
		[Icon( "front_hand" )]
		Right
	}

	/// <summary>
	/// Which hand should we use to update the parameters?
	/// </summary>
	[Property, Group( "Pose System" )] public Source HandSource { get; set; } = Source.Left;
	
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
		// Get our controller inputs
		var source = Controller;

		Model.Set( "BasePose", 1 );
		Model.Set( "bGrab", true );
		Model.Set( "GrabMode", 1 );

		for ( FingerValue v = FingerValue.ThumbCurl; v <= FingerValue.PinkyCurl; ++v )
		{
			Model.Set( AnimGraphNames[(int)v], source?.GetFingerValue( v ) ?? 0 );
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

		if ( HeldObject.IsValid() )
		{
			ApplyHandPreset( HeldObject.HandPreset );
		}
		else
		{
			// Apply default preset
			ApplyHandPreset();
		}
	}
}
