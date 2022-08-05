namespace TableTennis;

public partial class PlayerPawn : Entity
{
	[Net] public Paddle Paddle { get; set; }

	protected ModelEntity HeadModel { get; set; }

	public override void Spawn()
	{
		Paddle = new();
		Paddle.Owner = this;

		Transmit = TransmitType.Always;
		Predictable = true;
	}

	public override void ClientSpawn()
	{
		base.ClientSpawn();

		HeadModel = new( "models/vrhead/vrhead.vmdl", this );
	}

	protected override void OnDestroy()
	{
		Paddle?.Delete();
	}

	public override void Simulate( Client cl )
	{
		base.Simulate( cl );

		if ( cl.IsUsingVr )
			EyePosition = Input.VR.Head.Position;
		else
			EyePosition = Position + Vector3.Up * 50f;
	
		Paddle?.Simulate( cl );
	}

	public override void FrameSimulate( Client cl )
	{
		base.FrameSimulate( cl );
		Paddle?.FrameSimulate( cl );
	}

	[Event.Frame]
	protected void UpdateHeadSpot()
	{
		if ( HeadModel.IsValid() )
		{
			HeadModel.Position = EyePosition;
			HeadModel.Rotation = EyeRotation;
		}
	}
}
