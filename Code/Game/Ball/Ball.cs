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

			var paddle = collision.Other.GameObject.Components.Get<Paddle>( FindMode.EnabledInSelfAndDescendants );
			GameManager.Instance?.OnBallHit( new( this, paddle, collision ) );
		}
		if ( collision.Other.GameObject.Tags.Has( "table" ) )
		{
			Sound.Play( BallBounceSound, collision.Contact.Point );
			GameManager.Instance?.OnBallBounced( new( this, collision ) );
		}
	}
}
