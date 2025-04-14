namespace VRTK;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// A multi-handed grabbable object.
/// </summary>
public partial class DualGrabObject : Component, IGrabbable
{
	[Property] public GrabInput GrabInput { get; set; } = GrabInput.Grip;
	[Property] public bool UseAveragePosition { get; set; } = false;
	[RequireComponent] public Rigidbody Rigidbody { get; set; }

	public HandPreset GetHandPreset( Hand hand )
	{
		return grabbingHands.FirstOrDefault( x => x.Hand == hand ).Reference.HandPreset;
	}

	public Hand Hand => grabbingHands.FirstOrDefault().Hand;

	private List<(Hand Hand, GrabReference Reference)> grabbingHands = new();

	private void OnStartGrabbing()
	{
		Rigidbody.MotionEnabled = false;
	}

	private void OnStopGrabbing()
	{
		Rigidbody.MotionEnabled = true;
	}

	public bool StartGrabbing( Hand hand )
	{
		var reference = GetClosestGrabReference( hand );
		if ( !reference.IsValid() ) return false;

		// Already being held. Fuck off.
		if ( grabbingHands.Any( x => x.Reference == reference ) ) return false;

		grabbingHands.Add( (hand, reference) );
		hand.AttachMeshTo( reference.GameObject );

		OnStartGrabbing();

		return true;
	}

	public bool StopGrabbing( Hand hand )
	{
		grabbingHands.RemoveAll( h => h.Hand == hand );

		hand.ReturnMesh();

		if ( grabbingHands.Count == 0 )
		{
			OnStopGrabbing();
		}

		return true;
	}

	protected override void OnUpdate()
	{
		if ( grabbingHands.Count == 0 ) return;

		if ( grabbingHands.Count == 1 )
		{
			HandleOneHandedGrab( grabbingHands[0] );
		}
		else
		{
			HandleMultiHandedGrab();
		}
	}

	private void HandleOneHandedGrab( (Hand hand, GrabReference reference) grabData )
	{
		var (hand, reference) = grabData;

		var localOffset = reference.LocalPosition + reference.GetOffset( hand.HandSource );
		var localRotationOffset = reference.GetRotationOffset( hand.HandSource );

		var worldOffset = hand.WorldRotation * localOffset;

		WorldPosition = hand.WorldPosition - worldOffset;
		WorldRotation = hand.WorldRotation * localRotationOffset;
	}

	protected Rotation GetHoldRotation()
	{
		var first = grabbingHands.First();
		var targetRotation = first.Reference.WorldRotation;

		// Are we holding from a secondary hold point as well?
		if ( grabbingHands.Count() > 1 && grabbingHands[1] is var second && second.Reference.IsValid() )
		{
			var direction = (second.Hand.WorldPosition - first.Reference.WorldPosition).Normal;
			targetRotation = Rotation.LookAt( direction, second.Reference.WorldRotation.Up );
		}

		return targetRotation;
	}

	private void HandleMultiHandedGrab()
	{
		if ( UseAveragePosition )
		{
			Vector3 averagePosition = Vector3.Zero;

			foreach ( var (hand, reference) in grabbingHands )
			{
				var pos = hand.WorldPosition - (hand.WorldRotation * (reference.LocalPosition + reference.GetOffset( hand.HandSource )));
				averagePosition += pos;
			}

			averagePosition /= grabbingHands.Count;

			WorldPosition = averagePosition;
		}
		else
		{
			var first = grabbingHands.FirstOrDefault();
			var hand = first.Hand;
			var reference = first.Reference;
			var pos = hand.WorldPosition - (hand.WorldRotation * (reference.LocalPosition + reference.GetOffset( hand.HandSource )));

			WorldPosition = pos;
		}

		WorldRotation = GetHoldRotation();
	}

	private GrabReference GetClosestGrabReference( Hand hand )
	{
		return GetComponentsInChildren<GrabReference>()
			.OrderBy( r => r.WorldPosition.Distance( hand.WorldPosition ) )
			.FirstOrDefault();
	}
}
