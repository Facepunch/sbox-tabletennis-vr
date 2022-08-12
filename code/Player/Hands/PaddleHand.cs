namespace TableTennis;

public partial class PaddleHand : VrPlayerHand
{
	[Net] public Paddle Paddle { get; set; }

	public override void Spawn()
	{
		Paddle = new();
		Paddle.Owner = this;

		base.Spawn();

		HandType = VrHandType.Right;
		VisibleHand = false;
	}

	public override void Simulate( Client cl )
	{
		base.Simulate( cl );
		Paddle?.Simulate( cl );
	}

	public override void FrameSimulate( Client cl )
	{
		base.FrameSimulate( cl );
		Paddle?.FrameSimulate( cl );
	}
}
