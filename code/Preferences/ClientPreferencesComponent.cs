namespace TableTennis;

public partial class ClientPreferencesComponent : EntityComponent, ISingletonComponent
{
	public ClientPreferences.Settings Settings { get; set; }

	protected override void OnActivate()
	{
		Settings = new();
	}

	public void Update( ClientPreferences.Settings newSettings )
	{
		Settings = newSettings;

		var cl = Entity as IClient;
		var pawn = cl.Pawn as PlayerPawn;

		if ( Settings.FlipHands )
		{
			pawn.ServeHand.HandType = VrPlayerHand.VrHandType.Right;
			pawn.PaddleHand.HandType = VrPlayerHand.VrHandType.Left;
		}
		else
		{
			pawn.ServeHand.HandType = VrPlayerHand.VrHandType.Left;
			pawn.PaddleHand.HandType = VrPlayerHand.VrHandType.Right;
		}

		pawn.Paddle.PaddleAngle = Settings.PaddleAngle;
		pawn.PaddleHand.VisibleHand = Settings.ShowPaddleHand;
	}
}
