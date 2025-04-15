using VRTK;

namespace TableTennis;

public partial class Ball : Component, IGrabbable, Component.ICollisionListener
{
	[Property, Group( "Sounds" )] public SoundEvent BallHitSound { get; set; }
	[Property, Group( "Sounds" )] public SoundEvent BallBounceSound { get; set; }
	[Property, Group( "Sounds" )] public float MinContactSpeed { get; set; } = 70f;
	[Property] public Rigidbody Rigidbody { get; set; }

	/// <summary>
	/// The grab reference for this paddle
	/// </summary>
	[Property] public GrabReference GrabReference { get; set; }

	/// <summary>
	/// The hand that is holding this object ( can be null )
	/// </summary>
	public Hand Hand { get; set; }

	// IGrabbable 
	Hand IGrabbable.Hand => Hand;
	GrabInput IGrabbable.GrabInput => GrabInput.Grip;
	HandPreset IGrabbable.GetHandPreset( Hand hand ) => GrabReference.HandPreset;

	bool IGrabbable.StartGrabbing( Hand hand )
	{
		Hand = hand;
		Rigidbody.MotionEnabled = false;
		return true;
	}

	bool IGrabbable.StopGrabbing( Hand hand )
	{
		Rigidbody.MotionEnabled = true;

		if ( Hand.IsValid() )
		{
			Rigidbody.Velocity = Hand.Velocity;
		}

		Hand = null;
		return true;
	}

	void ICollisionListener.OnCollisionStart( Sandbox.Collision collision )
	{
		if ( collision.Contact.Speed.Length < MinContactSpeed ) return;

		if ( collision.Other.GameObject.Tags.Has( "paddle" ) )
		{
			Sound.Play( BallHitSound, collision.Contact.Point );

			var paddle = collision.Other.GameObject.Components.Get<Paddle>( FindMode.EnabledInSelfAndDescendants );
			Scene.RunEvent<IGameEvents>( x => x.OnBallHit( this, paddle, collision ) );
		}
		else
		{
			Sound.Play( BallBounceSound, collision.Contact.Point );
			Scene.RunEvent<IGameEvents>( x => x.OnBallBounce( this, collision, collision.Other.GameObject.Tags.Has( "table" ) ) );
		}
	}

	protected override void OnUpdate()
	{
		if ( !Hand.IsValid() )
			return;

		if ( Hand.IsValid() )
		{
			var reference = GetComponentInChildren<GrabReference>();
			var offset = reference.LocalPosition + reference.GetOffset( Hand.HandSource );
			var rotatedOffset = Hand.WorldRotation * offset;

			WorldPosition = Hand.WorldPosition - rotatedOffset;
			WorldRotation = Hand.WorldRotation;
		}
	}
}
