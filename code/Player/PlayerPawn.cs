namespace TableTennis;

public partial class PlayerPawn : Entity
{
	[Net] public Paddle Paddle { get; set; }

	protected ModelEntity HeadModel { get; set; }
	
	[Net, Predicted] public ServeHand ServeHand { get; set; }

	public override void Spawn()
	{
		Paddle = new();
		Paddle.Owner = this;

		Transmit = TransmitType.Always;
		Predictable = true;

		ServeHand = new()
		{
			Owner = this
		};
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

	protected Transform GetHandTransform( Client cl )
	{
		if ( cl.IsUsingVr )
			return Input.VR.LeftHand.Transform;
		else
			return Transform.WithPosition( EyePosition + EyeRotation.Down * 8f + EyeRotation.Forward * 10f + EyeRotation.Left * 12.5f );
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

		if ( ServeHand.IsValid() )
		{
			ServeHand.Transform = GetHandTransform( cl );
			ServeHand.Simulate( cl );
		}

		Paddle?.Simulate( cl );
	}

	public override void FrameSimulate( Client cl )
	{
		base.FrameSimulate( cl );
		Paddle?.FrameSimulate( cl );

		if ( ServeHand.IsValid() )
		{
			ServeHand.FrameSimulate( cl );
		}
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
}
