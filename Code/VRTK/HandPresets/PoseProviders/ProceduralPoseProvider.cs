using Sandbox.VR;

public partial class ProceduralPoseProvider : IPoseProvider
{
	private readonly FingerPoser[] fingerPosers = new FingerPoser[5];

	public ProceduralPoseProvider()
	{
		fingerPosers =
		[
			new( FingerValue.ThumbCurl ),
			new( FingerValue.IndexCurl ),
			new( FingerValue.MiddleCurl ),
			new( FingerValue.RingCurl ),
			new( FingerValue.PinkyCurl )
		];
	}

	public void Update( Hand hand )
	{
		foreach ( var fingerPoser in fingerPosers )
		{
			fingerPoser.Update( hand );
		}
	}
}
