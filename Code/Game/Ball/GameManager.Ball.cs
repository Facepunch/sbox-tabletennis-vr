namespace TableTennis;

public interface IGameEvents : ISceneEvent<IGameEvents>
{
	public void OnBallBounce( Ball ball, bool isTableHit );
	public void OnBallHit( Ball ball, Paddle paddle );
}

public partial class GameManager
{
	/// <summary>
	/// Points to the ball prefab.
	/// </summary>
	[Property] public GameObject BallPrefab { get; set; }

	[Sync( SyncFlags.FromHost )]
	private Ball _ball { get; set; }

	private Ball GetOrCreateBall()
	{
		// Do we already have a valid ball?
		if ( _ball.IsValid() ) return _ball;

		var inst = BallPrefab.Clone();
		var x = inst.Components.Get<Ball>( FindMode.EnabledInSelfAndDescendants );
		_ball = x;
		inst.NetworkSpawn();

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
