using Sandbox.VR;

public partial class ProceduralPoseProvider
{
	private class FingerPoser
	{
		private static bool ShowTraces => true;

		public static float LerpRate => 5f;

		private float _value;
		private FingerValue _fingerValue;

		public FingerPoser( FingerValue finger )
		{
			_fingerValue = finger;

			_value = 0;
		}

		private float GetValue( Hand hand )
		{
			float proceduralAngle = GetAngleForFinger( hand, _fingerValue );
			float userAngle = hand.VRController?.GetFingerValue( _fingerValue ) ?? 0;

			float angle = userAngle.LerpTo( proceduralAngle, 1.0f );
			if ( proceduralAngle < 0.05f )
				angle = userAngle;

			_value = _value.LerpTo( angle, LerpRate * Time.Delta );
			return _value;
		}

		public void Update( Hand hand )
		{
			var animGraphName = Hand.AnimGraphNames[(int)_fingerValue];
			var value = GetValue( hand );

			hand.Renderer.Set( animGraphName, value );
		}

		private static float GetAngleForFinger( Hand targetHand, FingerValue targetFinger )
		{
			const float angleMax = 75f;
			const float angleMin = 10f;
			const float step = 0.5f;

			var angleRange = angleMax - angleMin;
			var scene = targetHand.Scene;

			var isLeft = targetHand.HandSource == Hand.HandSources.Left;

			var jointBoneName = targetFinger switch
			{
				FingerValue.ThumbCurl => "finger_thumb_1",
				FingerValue.IndexCurl => "finger_index_0",
				FingerValue.MiddleCurl => "finger_middle_0",
				FingerValue.RingCurl => "finger_ring_0",
				FingerValue.PinkyCurl => "finger_pinky_0",
				_ => "root"
			};

			var palmBoneName = "root";

			//
			// Left/right suffix
			//
			jointBoneName += (isLeft ? "_L" : "_R");

			// Lengths proportional to finger sizes
			var length = targetFinger switch
			{
				FingerValue.ThumbCurl => 3f,
				FingerValue.IndexCurl => 4f,
				FingerValue.MiddleCurl => 4.5f,
				FingerValue.RingCurl => 4f,
				FingerValue.PinkyCurl => 3f,
				_ => 4f
			};

			if ( !targetHand.Renderer.TryGetBoneTransform( jointBoneName, out var jointTransform ) )
				return 0;

			if ( !targetHand.Renderer.TryGetBoneTransform( palmBoneName, out var palmTransform ) )
				return 0;

			var factor = 0f;

			{
				var direction = isLeft ? palmTransform.Right : palmTransform.Left;

				var tr = scene.Trace
							  .Ray( palmTransform.Position, palmTransform.Position + direction * 2f )
							  .IgnoreGameObjectHierarchy( targetHand.GameObject )
							  .Run();

				if ( ShowTraces )
					DebugOverlay.Line( tr.StartPosition, tr.EndPosition );

				if ( ShowTraces )
				{
					DebugOverlay.Sphere( new Sphere( tr.EndPosition, 0.25f ) );
				}

				factor = 1.0f - tr.Fraction;
			}

			for ( float angleDelta = angleMin; angleDelta < angleMax; angleDelta += step )
			{
				var rotation = Rotation.FromYaw( angleDelta );

				if ( isLeft )
					rotation = Rotation.FromRoll( 90f ) * rotation;
				else
					rotation = Rotation.FromRoll( -90f ) * rotation;

				rotation = jointTransform.Rotation * rotation;

				var tr = scene.Trace
							  .Ray( jointTransform.Position, jointTransform.Position + rotation.Forward * length )
							  .IgnoreGameObjectHierarchy( targetHand.GameObject )
							  .Run();

				if ( tr.Hit )
				{
					var resultValue = angleDelta / angleRange;
					resultValue *= factor;

					if ( ShowTraces )
					{
						DebugOverlay.Sphere( new Sphere( tr.EndPosition, 0.25f ), Color.White.WithAlpha( factor ) );
					}

					return resultValue;
				}
			}

			return 0;
		}
	}
}
