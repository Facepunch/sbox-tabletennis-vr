using TableTennis;

public partial class VrSimulator : Component
{
	public Hand LeftHand => Player.LeftHand;
	public Hand RightHand => Player.RightHand;

	[Property]
	public Player Player { get; set; }

	public Hand CurrentHand { get; set; }

	// Rotation sensitivity controls
	private const float RotationSensitivity = 2.0f;

	// Store the accumulated angles for absolute control
	private float accumulatedYaw = 0f;
	private float accumulatedPitch = 0f;
	private float accumulatedRoll = 0f;

	protected override void OnUpdate()
	{
		if ( !Game.IsEditor ) return;

		if ( !Player.IsValid() )
		{
			var pl = Scene.GetAll<Player>().FirstOrDefault();
			if ( pl.IsValid() ) Player = pl;
		}

		Output();

		if ( Input.Released( "Score" ) )
		{
			SelectHand();

			if ( CurrentHand.IsValid() )
			{
				// When selecting a hand, initialize our accumulated angles to match its current orientation
				var angles = CurrentHand.WorldRotation.Angles();
				accumulatedYaw = angles.yaw;
				accumulatedPitch = angles.pitch;
				accumulatedRoll = angles.roll;
			}
		}

		if ( CurrentHand.IsValid() )
		{
			Gizmo.Draw.Color = Color.White.WithAlpha( 0.2f );
			Gizmo.Draw.SolidSphere( CurrentHand.WorldPosition, 2, 16, 16 );

			Gizmo.Draw.Color = Color.Red.WithAlpha( 0.2f );
			Gizmo.Draw.Arrow( CurrentHand.WorldPosition, CurrentHand.WorldPosition + CurrentHand.WorldRotation.Forward * 8f, 4f, 1f );

			TryMoveHand();
		}
		else
		{
			Player.Head.WorldRotation *= Input.AnalogLook;
			Player.Head.WorldRotation = Player.Head.WorldRotation.Angles().WithRoll( 0f );
		}

		TryMovePlayer();
	}

	private Vector2 debugPos;

	void Line( string text )
	{
		Gizmo.Draw.ScreenText( text, debugPos, "Roboto", 24 );
		debugPos += new Vector2( 0f, 32f );
	}

	void Output()
	{
		debugPos = new( 16, 16 );
		Line( $"Player" );
		Line( $"[WASD]: Move Player" );
		Line( "" );
		Line( "Hands" );
		Line( $"[TAB]: Switch Hands" );
		Line( $"[Mouse Move]: Move Hand L/R/F/B" );
		Line( $"[Mouse Scroll]: Move Hand U/D" );
		Line( $"[Right Mouse Button]: Rotate Hand" );
		Line( $"[E/Q]: Roll Hand" );
	}

	void TryMovePlayer()
	{
		var inputDir = Input.AnalogMove.Normal;
		var headRot = Player.Head.WorldRotation;
		headRot *= Rotation.From( 0, 90, 0 );
		var fwd = new Vector3( inputDir.y, -inputDir.x, 0 ) * headRot;
		Move( fwd );
	}

	void Move( Vector3 direction )
	{
		var spd = 200f;
		direction = direction.WithZ( 0 );
		var velocity = direction * spd * Time.Delta;
		GameObject.WorldPosition += velocity;
	}

	void TryMoveHand()
	{
		var look = Input.AnalogLook;
		var scr = Input.MouseWheel;

		if ( Input.Down( "Attack2" ) )
		{
			accumulatedYaw += look.yaw * RotationSensitivity;
			accumulatedPitch += look.pitch * RotationSensitivity;

			var targetRotation = Rotation.From( accumulatedPitch, accumulatedYaw, accumulatedRoll );
			CurrentHand.WorldRotation = targetRotation;
		}
		else
		{
			var scrollSpd = 2f;
			var moveDelta = new Vector3( -look.pitch, look.yaw, scr.y * scrollSpd );
			var overallSpd = 0.25f;

			// TODO: Make a player controller and just PlayerController.Move( direction );
			CurrentHand.WorldPosition += moveDelta * overallSpd;
		}

		// Roll controls
		float rollSpeed = 180f * Time.Delta; // 180 degrees per second

		if ( Input.Down( "Menu" ) )
		{
			// Roll left (counter-clockwise)
			accumulatedRoll -= rollSpeed;
			var targetRotation = Rotation.From( accumulatedPitch, accumulatedYaw, accumulatedRoll );
			CurrentHand.WorldRotation = targetRotation;
		}
		else if ( Input.Down( "Use" ) )
		{
			// Roll right (clockwise)
			accumulatedRoll += rollSpeed;
			var targetRotation = Rotation.From( accumulatedPitch, accumulatedYaw, accumulatedRoll );
			CurrentHand.WorldRotation = targetRotation;
		}
	}

	private void SetCurrentHand( Hand hand )
	{
		if ( CurrentHand.IsValid() ) CurrentHand.WantsKeyboardInput = false;

		CurrentHand = hand;

		if ( CurrentHand.IsValid() ) CurrentHand.WantsKeyboardInput = true;
	}

	/// <summary>
	/// Selects a hand to be translated by the simulator
	/// </summary>
	void SelectHand()
	{
		// Select the left hand by default
		if ( !CurrentHand.IsValid() )
		{
			SetCurrentHand( LeftHand );
			return;
		}

		// Protection
		if ( !CurrentHand.IsValid() )
			return;

		// Swap to right
		if ( CurrentHand == LeftHand )
		{
			SetCurrentHand( RightHand );
		}
		// Swap to none
		else
		{
			SetCurrentHand( null );
		}
	}
}
