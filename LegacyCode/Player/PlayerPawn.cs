namespace TableTennis;

public partial class PlayerPawn : Entity
{
	protected ModelEntity HeadModel { get; set; }
	
	[Net, Predicted] public ServeHand ServeHand { get; set; }
	[Net, Predicted] public PaddleHand PaddleHand { get; set; }

	// Accessor for the paddle
	public Paddle Paddle => PaddleHand.Paddle;

	public Transform HeadTransform { get; private set; }

	public override void Spawn()
	{
		Transmit = TransmitType.Always;
		Predictable = true;

		ServeHand = new()
		{
			Owner = this
		};

		PaddleHand = new()
		{
			Owner = this
		};
		
		PaddleHand.Paddle.Owner = this;
	}

	public override void ClientSpawn()
	{
		base.ClientSpawn();

		HeadModel = new( "models/vrhead/vrhead.vmdl", this );
	}

	protected override void OnDestroy()
	{
		if ( Game.IsServer )
		{
			Paddle?.Delete();
			ServeHand?.Delete();
		}
		else
		{
			HeadModel?.Delete();
		}
	}

	public override void Simulate( IClient cl )
	{
		base.Simulate( cl );

		if ( cl.IsUsingVr )
		{
			HeadTransform = Input.VR.Head;
		}
		else
		{
			HeadTransform = Transform.WithPosition( Position + Vector3.Up * 50f );
		}

		ServeHand?.Simulate( cl );
		PaddleHand?.Simulate( cl );
	}

	public override void FrameSimulate( IClient cl )
	{
		base.FrameSimulate( cl );

		ServeHand?.FrameSimulate( cl );
		PaddleHand?.FrameSimulate( cl );
	}

	[Event.Client.Frame]
	protected void UpdateHeadSpot()
	{
		if ( HeadModel.IsValid() )
		{
			HeadModel.Transform = HeadTransform;

			var team = Client.GetTeam();
			if ( team != null )
			{
				HeadModel.SceneObject.Attributes.Set( "PlayerColor", team.Color );
			}

			HeadModel.EnableDrawing = Camera.FirstPersonViewer != Client;
		}
	}

	public Team GetTeam() => Client.GetTeam();

	WorldInput WorldInput = new();
	public override void BuildInput()
	{
		if ( !Game.IsRunningInVR )
		{
			WorldInput.Ray = new Ray( Camera.Position, Camera.Rotation.Forward );
			WorldInput.MouseLeftPressed = Input.Down( InputButton.PrimaryAttack );
		}
		else
		{
			WorldInput.Ray = new Ray( PaddleHand.Position, PaddleHand.Rotation.Forward );
			WorldInput.MouseLeftPressed = PaddleHand.InTrigger;
		}
	}
}
