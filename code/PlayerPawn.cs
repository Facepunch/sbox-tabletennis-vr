namespace TableTennis;

public partial class PlayerPawn : Entity
{
	[Net] public Paddle Paddle { get; set; }

	public override void Spawn()
	{
		Paddle = new();
		Paddle.Owner = this;

		Transmit = TransmitType.Always;
		Predictable = true;
	}

	protected override void OnDestroy()
	{
		Paddle?.Delete();
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
