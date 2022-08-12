namespace TableTennis;

public partial class PlayerPawn : Entity
{
	protected ModelEntity HeadModel { get; set; }
	
	[Net, Predicted] public ServeHand ServeHand { get; set; }
	[Net, Predicted] public PaddleHand PaddleHand { get; set; }

	// Accessor for the paddle
	public Paddle Paddle => PaddleHand.Paddle;

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
		if ( IsServer )
		{
			Paddle?.Delete();
			ServeHand?.Delete();
		}
		else
		{
			HeadModel?.Delete();
		}
	}

	public override void Simulate( Client cl )
	{
		base.Simulate( cl );

		if ( cl.IsUsingVr )
		{
			EyePosition = Input.VR.Head.Position;
			EyeRotation = Input.VR.Head.Rotation;
		}
		else
			EyePosition = Position + Vector3.Up * 50f;

		ServeHand?.Simulate( cl );
		PaddleHand?.Simulate( cl );
	}

	public override void FrameSimulate( Client cl )
	{
		base.FrameSimulate( cl );

		ServeHand?.FrameSimulate( cl );
		PaddleHand?.FrameSimulate( cl );
	}

	[Event.Frame]
	protected void UpdateHeadSpot()
	{
		if ( HeadModel.IsValid() )
		{
			HeadModel.Position = EyePosition;
			HeadModel.Rotation = EyeRotation;

			var team = Client.GetTeam();
			if ( team != null )
			{
				HeadModel.SceneObject.Attributes.Set( "PlayerColor", team.Color );
			}

			HeadModel.EnableDrawing = CurrentView.Viewer != Client;
		}
	}

	public Team GetTeam() => Client.GetTeam();

	WorldInput WorldInput = new();
	public override void BuildInput( InputBuilder input )
	{
		base.BuildInput( input );

		if ( !Input.VR.IsActive ) return;

		WorldInput.Ray = new Ray( PaddleHand.Position, PaddleHand.Rotation.Forward );
		WorldInput.MouseLeftPressed = PaddleHand.InTrigger;
	}
}
