namespace TableTennis;

public partial class GameManager
{
	/// <summary>
	/// Points to the ball prefab.
	/// </summary>
	[Property] public GameObject BallPrefab { get; set; }

	private Ball ball;
	private Ball GetOrCreateBall()
	{
		// Do we already have a valid ball?
		if ( ball.IsValid() ) return ball;

		var inst = BallPrefab.Clone();
		var x = inst.Components.Get<Ball>( FindMode.EnabledInSelfAndDescendants );
		ball = x;

		return x;
	}

	/// <summary>
	/// The ball in play.
	/// </summary>
	public Ball Ball
	{
		get => GetOrCreateBall();
	}
}
