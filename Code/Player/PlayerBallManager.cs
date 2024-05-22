namespace TableTennis;

public sealed class PlayerBallManager : Component
{
	[RequireComponent] Hand Hand { get; set; }

	protected override void OnUpdate()
	{
		if ( IsProxy ) return;

		if ( ( Hand.Controller?.ButtonA.WasPressed ?? false ) || Input.Down( "Jump" ) )
		{
			// Fetch the ball
			var ball = GameManager.Instance.Ball;
			ball.GameObject.Transform.Position = Transform.Position;
			Hand.StartHolding( ball );
		}
	}
}
