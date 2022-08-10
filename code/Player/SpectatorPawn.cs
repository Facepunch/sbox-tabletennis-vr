namespace TableTennis;

public partial class SpectatorPawn : Entity
{
	public override void Spawn()
	{
		base.Spawn();

		Components.Add( new SpectatorCamera() );
	}
}
