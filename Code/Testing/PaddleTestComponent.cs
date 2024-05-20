using Sandbox;

public sealed class PaddleTestComponent : Component
{
	private Rotation InitialRotation = Rotation.Identity;

	[Property] public GameObject BallOrigin { get; set; }
	[Property] public GameObject Ball { get; set; }

	private RealTimeUntil TimeUntilRespawnBall = 1.5f;

	protected override void OnStart()
	{
		InitialRotation = Transform.Rotation;
	}

	protected override void OnUpdate()
	{
		var degrees = 45f;
		var spd = 4f;

		var sin = MathF.Sin( Time.Now * spd ) * degrees;

		Transform.Rotation = InitialRotation;
		Transform.Rotation *= Rotation.From( 0, sin, 0 );

		if ( TimeUntilRespawnBall )
		{
			TimeUntilRespawnBall = 1.5f;
			Ball.Transform.Position = BallOrigin.Transform.Position;
			Ball.Transform.Rotation = BallOrigin.Transform.Rotation;

			var rb = Ball.Components.Get<Rigidbody>( FindMode.EnabledInSelfAndDescendants );
			rb.Velocity = 0;
			rb.AngularVelocity = 0;
		}
	}
}
