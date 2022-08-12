namespace TableTennis;

public partial class ClientPreferencesComponent : EntityComponent, ISingletonComponent
{
	public ClientPreferences.Settings Settings { get; set; }

	protected override void OnActivate()
	{
		Settings = new();
	}
}
