namespace TableTennis;

public partial class Paddle : ModelEntity
{
	public Transform ClientTransform { get; set; }
	[Net] public PaddleHand Hand { get; set; }

	[Net] public float PaddleAngle { get; set; } = 90f;

	public override void Spawn()
	{
		SetModel( "models/tabletennis.paddle.vmdl" );

		SetupPhysicsFromModel( PhysicsMotionType.Keyframed, false );

		EnableTraceAndQueries = true;
		Predictable = true;

		Tags.Add( "paddle" );

	}

	public Transform GetTransform()
	{
		var tr = Hand.Transform;
		tr.Rotation *= Rotation.FromAxis( Vector3.Right, 90f );
		tr.Position += tr.Rotation.Forward * 3f;

		return tr;
	}

	public override void Simulate( Client cl )
	{
		base.Simulate( cl );

		// cba doing predicted atm
		if ( !IsServer ) return;

		// This is solely just to sync with other players
		if ( cl.IsUsingVr )
		{
			Transform = GetTransform();
			Velocity = Hand.Velocity;
			AngularVelocity = Hand.AngularVelocity;

			Transform = Transform.WithRotation( Transform.Rotation * Rotation.FromPitch( 90f ) * Rotation.FromYaw( 180 + PaddleAngle ) );
			Transform = Transform.WithPosition( Transform.Position + Transform.Rotation.Down * 2.0f );
		}
	}

	public override void FrameSimulate( Client cl )
	{
		base.FrameSimulate( cl );

		var oldTransform = ClientTransform;

		if ( cl.IsUsingVr )
		{
			Transform = GetTransform();
			Velocity = Hand.Velocity;
			AngularVelocity = Hand.AngularVelocity;

			Transform = Transform.WithRotation( Transform.Rotation * Rotation.FromPitch( 90f ) * Rotation.FromYaw( 180 + PaddleAngle ) );
			Transform = Transform.WithPosition( Transform.Position + Transform.Rotation.Down * 2.0f );
		}

		// if ( !game.ServerBall.IsValid() ) return;

		/*if ( TableTennisGame.DebugBallPhysics )
		{
			// Position = game.ServerBall.Position.WithX( 62.0f ).WithZ( 35 );
			Position += Vector3.Left * 4.5f;

			var pitch = MathF.Sin( -20 + TableTennisGame.Current.LastSpawn * 10 ) * 70;
			
			Rotation = Rotation.FromRoll( 80 ) * Rotation.FromYaw( -10 ) * Rotation.FromPitch( pitch );
			AngularVelocity = new Angles( 1500, 0, 0 );
		}*/

		ClientTransform = Transform;
	}
}
