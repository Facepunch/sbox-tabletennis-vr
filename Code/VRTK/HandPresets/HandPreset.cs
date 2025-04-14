using System;

public sealed partial class HandPreset
{
	[Range( 0, 1 )]
	public float Thumb { get; set; } = 0f;

	[Range( 0, 1 )]
	public float Index { get; set; } = 0f;

	[Range( 0, 1 )]
	public float Middle { get; set; } = 0f;

	[Range( 0, 1 )]
	public float Ring { get; set; } = 0f;

	[Range( 0, 1 )]
	public float Pinky { get; set; } = 0f;

	public override string ToString()
	{
		return $"{Thumb}, {Index}, {Middle}, {Ring}, {Pinky}";
	}

	enum FingerCurl
	{
		Thumb,
		Index,
		Middle,
		Ring,
		Pinky
	}

	public HandPreset( float thumb, float index, float middle, float ring, float pinky )
	{
		Thumb = thumb;
		Index = index;
		Middle = middle;
		Ring = ring;
		Pinky = pinky;
	}

	public HandPreset() { }

	string GetFingerCurl( FingerCurl curl )
	{
		return curl switch
		{
			FingerCurl.Thumb => "FingerCurl_Thumb",
			FingerCurl.Index => "FingerCurl_Index",
			FingerCurl.Middle => "FingerCurl_Middle",
			FingerCurl.Ring => "FingerCurl_Ring",
			FingerCurl.Pinky => "FingerCurl_Pinky",
			_ => throw new NotImplementedException()
		};
	}

	float GetValue( FingerCurl curl )
	{
		return curl switch
		{
			FingerCurl.Thumb => Thumb,
			FingerCurl.Index => Index,
			FingerCurl.Middle => Middle,
			FingerCurl.Ring => Ring,
			FingerCurl.Pinky => Pinky,
			_ => 0f,
		};
	}

	/// <summary>
	/// Apply a hand preset onto a hand.
	/// </summary>
	/// <param name="renderer"></param>
	public void Apply( SkinnedModelRenderer renderer )
	{
		for ( FingerCurl v = FingerCurl.Thumb; v <= FingerCurl.Pinky; v++ )
		{
			var value = GetValue( v );
			// We use -1.0f to say we wanna use the player's input here.
			if ( value == -1.0f ) continue;

			renderer.Set( GetFingerCurl( v ), value );
		}
	}
}
