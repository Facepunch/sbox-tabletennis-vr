namespace TableTennis;

public class PlayerCamera : CameraMode
{
	Angles LookAngles;
	Vector3 MoveInput;

	Vector3 TargetPos;
	Rotation TargetRot;

	float MoveSpeed;
	float LerpMode = 0;

	public PlayerCamera()
	{
		TargetPos = CurrentView.Position;
		TargetRot = CurrentView.Rotation;

		Position = TargetPos;
		Rotation = TargetRot;
		LookAngles = Rotation.Angles();
	}

	float FlySpeed => 150f;

	public override void Update()
	{
		var player = Local.Client;
		if ( !player.IsValid() ) 
			return;

		Viewer = null;

		FreeMove();
	}

	public override void BuildInput( InputBuilder input )
	{
		MoveInput = input.AnalogMove;

		MoveSpeed = 1;
		if ( input.Down( InputButton.Run ) ) MoveSpeed = 5;
		if ( input.Down( InputButton.Duck ) ) MoveSpeed = 0.2f;

		if ( input.Down( InputButton.Slot1 ) ) LerpMode = 0.0f;
		if ( input.Down( InputButton.Slot2 ) ) LerpMode = 0.5f;
		if ( input.Down( InputButton.Slot3 ) ) LerpMode = 0.9f;

		LookAngles += input.AnalogLook;
		LookAngles.roll = 0;
	}

	void FreeMove()
	{
		var mv = MoveInput.Normal * FlySpeed * RealTime.Delta * Rotation * MoveSpeed;

		TargetRot = Rotation.From( LookAngles );
		TargetPos += mv;

		Position = Vector3.Lerp( Position, TargetPos, 10 * RealTime.Delta * (1 - LerpMode) );
		Rotation = Rotation.Slerp( Rotation, TargetRot, 10 * RealTime.Delta * (1 - LerpMode) );
	}
}
