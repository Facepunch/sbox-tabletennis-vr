namespace TableTennis;

public partial class Ball : BaseHoldable, Component.ICollisionListener
{
	[Property, Group( "Sounds" )] public SoundEvent BallHitSound { get; set; }
	[Property, Group( "Sounds" )] public SoundEvent BallBounceSound { get; set; }
	[Property, Group( "Sounds" )] public float MinContactSpeed { get; set; } = 70f;

	void ICollisionListener.OnCollisionStart( Sandbox.Collision collision )
	{
		if ( collision.Contact.Speed.Length < MinContactSpeed ) return;

		if ( collision.Other.GameObject.Tags.Has( "paddle" ) )
		{
			Sound.Play( BallHitSound, collision.Contact.Point );
		}
		if ( collision.Other.GameObject.Tags.Has( "table" ) )
		{
			Sound.Play( BallBounceSound, collision.Contact.Point );
		}
	}
}