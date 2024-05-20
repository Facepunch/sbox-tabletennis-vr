using System.Text.Json.Serialization;

namespace TableTennis;

[Title( "Holdable" ), Icon( "handshake" )]
public partial class BaseHoldable : Component, IHoldableObject
{
	/// <summary>
	/// The hold input.
	/// </summary>
	[Property, Group( "Input" )] public virtual HoldInput HoldInput { get; set; }

	/// <summary>
	/// The hold input type.
	/// </summary>
	[Property, Group( "Input" )] public virtual HoldType HoldType { get; set; }

	/// <summary>
	/// The holdable's rigidbody.
	/// </summary>
	[Property, Group( "Setup" )] public Rigidbody Rigidbody { get; set; }

	/// <summary>
	/// The hand preset for when the hand is grabbing this object.
	/// </summary>
	[Property, Group( "Setup" )] public HandPreset HandPreset { get; private set; } = HandPreset.Grip;

	/// <summary>
	/// The hand that is holding this object.
	/// </summary>
	[Property, ReadOnly, Group( "Runtime" ), JsonIgnore] public Hand HeldHand { get; private set; }

	/// <summary>
	/// The hold transform for this holdable object.
	/// </summary>
	public Transform HoldTransform => HeldHand.Transform.World;

	/// <summary>
	/// Is this holdable being held right now?
	/// </summary>
	public bool IsHeld => HeldHand.IsValid();

	/// <summary>
	/// Can we start holding this holdable object?
	/// </summary>
	/// <param name="hand"></param>
	/// <returns></returns>
	protected virtual bool CanHoldObject( Hand hand )
	{
		// Is already being held
		if ( HeldHand.IsValid() ) return false;
		if ( !hand.CanHoldObject( this ) ) return false;

		return true;
	}

	protected virtual bool CanStopHoldObject()
	{
		// Not being held
		if ( !HeldHand.IsValid() ) return false;
		if ( !HeldHand.CanStopHoldObject( this ) ) return false;

		return true;
	}

	public virtual bool HoldObject( Hand hand )
	{
		if ( !CanHoldObject( hand ) )
			return false;

		Log.Info( $"Start holding {this} with {hand}" );

		HeldHand = hand;
		HeldHand.OnStartHoldingObject( this );

		// Turn off rigidbody's gravity while we're holding the object.
		Rigidbody.MotionEnabled = false;

		Log.Info( Rigidbody.MotionEnabled );

		// Do stuff
		return true;
	}

	bool IHoldableObject.StopHoldObject()
	{
		if ( !CanStopHoldObject() )
			return false;

		Log.Info( $"Stop holding {this} with {HeldHand}" );

		HeldHand?.OnStopHoldingObject( this );
		HeldHand = null;

		// Turn back on the rigidbody's gravity once we're done with it.
		Rigidbody.MotionEnabled = true;

		return true;
	}

	/// <summary>
	/// Updates the holdable's transform while we're being held by someone's hand.
	/// </summary>
	protected virtual void UpdateTransform()
	{
		var velocity = Rigidbody.Velocity;
		Vector3.SmoothDamp( Rigidbody.Transform.Position, HoldTransform.Position, ref velocity, 0.1f, Time.Delta );
		Rigidbody.Velocity = velocity;

		var angularVelocity = Rigidbody.AngularVelocity;
		Rotation.SmoothDamp( Rigidbody.Transform.Rotation, HoldTransform.Rotation, ref angularVelocity, 0.1f, Time.Delta );
		Rigidbody.AngularVelocity = angularVelocity;
	}

	protected override void OnUpdate()
	{
		if ( IsHeld )
		{
			UpdateTransform();
		}
	}
}
