using TableTennis;

namespace Sandbox;

public sealed class BallShooter : Component
{
	[Property]
	public float Power { get; set; } = 100000;

	public Ball Ball => Scene.GetAll<Ball>().FirstOrDefault();

	bool shot = false;

	[Button( "Reset Ball" )]
	public void ResetBall()
	{
		shot = false;
	}

	private void Shoot()
	{
		Ball.WorldPosition = WorldPosition;
		Ball.Rigidbody.Velocity = Vector3.Zero;
		Ball.Rigidbody.AngularVelocity = Vector3.Zero;
		Ball.Rigidbody.ApplyForce( WorldRotation.Forward * Power );
	}

	protected override void OnUpdate()
	{
		if ( !shot )
		{
			if ( Ball.IsValid() )
			{
				Shoot();
				shot = true;
			}
		}
	}
}
