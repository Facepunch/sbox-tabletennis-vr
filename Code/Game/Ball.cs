namespace TableTennis;

public partial class Ball : Component, Component.ICollisionListener
{
	[Property] public SoundEvent BallHitSound { get; set; }
	[Property] public SoundEvent BallBounceSound { get; set; }

	void ICollisionListener.OnCollisionStart( Sandbox.Collision collision )
	{
		if ( collision.Contact.Speed.Length < 70 ) return;

		if ( collision.Other.GameObject.Tags.Has( "paddle" ) )
		{
			Sound.Play( BallHitSound, collision.Contact.Point );
		}
		else
		{
			Sound.Play( BallBounceSound, collision.Contact.Point );
		}
	}
}
