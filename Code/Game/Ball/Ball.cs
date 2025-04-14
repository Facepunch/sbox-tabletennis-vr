using VRTK;

namespace TableTennis;

public partial class Ball : Component, IGrabbable, Component.ICollisionListener
{
	[Property, Group( "Sounds" )] public SoundEvent BallHitSound { get; set; }
	[Property, Group( "Sounds" )] public SoundEvent BallBounceSound { get; set; }
	[Property, Group( "Sounds" )] public float MinContactSpeed { get; set; } = 70f;

	/// <summary>
	/// The hand that is holding this object ( can be null )
	/// </summary>
	public Hand Hand { get; set; }

	/// <summary>
	/// The grab reference for this paddle
	/// </summary>
	[Property]
	public GrabReference GrabReference { get; set; }

	// IGrabbable 

	Hand IGrabbable.Hand => Hand;

	GrabInput IGrabbable.GrabInput => GrabInput.None;

	HandPreset IGrabbable.GetHandPreset( Hand hand ) => GrabReference.HandPreset;

	bool IGrabbable.StartGrabbing( Hand hand )
	{
		return false;
	}

	bool IGrabbable.StopGrabbing( Hand hand )
	{
		return false;
	}

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
