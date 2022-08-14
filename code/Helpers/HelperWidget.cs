namespace TableTennis;

[UseTemplate]
public partial class HelperWidget : WorldPanel
{
	public string HelperText { get; set; }
	public string Icon { get; set; } = "help_outline";

	public Entity FollowEntity { get; set; }

	public HelperWidget( string message, string icon = null, float lifetime = 2f )
	{
		HelperText = message;

		if ( !string.IsNullOrEmpty( icon ) )
			Icon = icon;

		DeleteAsync( lifetime );
	}

	public HelperWidget( string message, Vector3 worldPosition, string icon = null, float lifetime = 2f ) : this( message, icon, lifetime )
	{
		Position = worldPosition + Vector3.Up * 1f;
	}

	public HelperWidget( string message, Entity followEntity, string icon = null, float lifetime = 2f ) : this( message, icon, lifetime )
	{
		FollowEntity = followEntity;
	}

	public async void DeleteAsync( float time )
	{
		await GameTask.DelaySeconds( time );
		Delete();
	}

	Vector2 Size => new( 800, 650f );
	
	public override void Tick()
	{
		base.Tick();

		Rotation = Rotation.LookAt( -Input.VR.Head.Rotation.Forward );
		PanelBounds = new( -Size.x / 2f, -Size.y, Size.x, Size.y );
		WorldScale = 0.4f;
		Scale = 2.0f;

		if ( FollowEntity.IsValid() )
		{
			Position = FollowEntity.Position + Vector3.Up * 1f;
		}
	}
}
