using Sandbox;
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

		// We're in charge
		Network.AssignOwnership( hand.Network.Owner );

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
	// endof: IGrabbable

	[Rpc.Broadcast]
	private void Hit( Ball ball, Paddle paddle )
	{
		Sound.Play( BallHitSound, ball.WorldPosition );

		Scene.RunEvent<IGameEvents>( x => x.OnBallHit( this, paddle ) );
	}

	[Rpc.Broadcast]
	private void Bounce( Ball ball, bool tableHit )
	{
		Sound.Play( BallBounceSound, ball.WorldPosition );

		Scene.RunEvent<IGameEvents>( x => x.OnBallBounce( this, tableHit ) );
	}

	void ICollisionListener.OnCollisionStart( Sandbox.Collision collision )
	{
		// Whoever is in charge of the ball, is in charge of the collision
		if ( IsProxy ) return;

		// We don't care for light collisions (rolling)
		if ( collision.Contact.Speed.Length < MinContactSpeed ) return;

		if ( collision.Other.GameObject.Tags.Has( "paddle" ) )
		{
			var paddle = collision.Other.GameObject.Components.Get<Paddle>( FindMode.EnabledInSelfAndDescendants );
			Hit( this, paddle );
		}
		else
		{
			Bounce( this, collision.Other.GameObject.Tags.Has( "table" ) );
		}
	}

	protected override void OnUpdate()
	{
		if ( !Hand.IsValid() ) return;

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
