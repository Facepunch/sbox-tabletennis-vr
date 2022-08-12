namespace TableTennis;

public partial class PaddleHand : VrPlayerHand
{
	[Net] public Paddle Paddle { get; set; }

	public override void Spawn()
	{
		Paddle = new();
		Paddle.Owner = Owner;
		
		base.Spawn();

		HandType = VrHandType.Right;
		VisibleHand = false;
		Paddle.Hand = this;
		Transmit = TransmitType.Always;
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
