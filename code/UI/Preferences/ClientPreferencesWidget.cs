using System.Reflection;

namespace TableTennis;

public partial class PreferenceRow : Panel
{
	public Label Label { get; }
	public Panel ValueArea { get; }

	public PreferenceRow( object target, PropertyInfo property ) : this()
	{
		var info = DisplayInfo.ForProperty( property );
		Label.Text = info.Name;

		if ( property.PropertyType == typeof( bool ) )
		{
			var button = ValueArea.Add.Button( string.Empty, "toggle" );
			button.SetClass( "active", (bool)property.GetValue( target ) );
			button.AddEventListener( "onmousedown", () =>
			{
				button.SetClass( "active", !button.HasClass( "active" ) );
				property.SetValue( target, button.HasClass( "active" ) );
				CreateEvent( "save" );
			} );
		}
		else if ( property.PropertyType == typeof( string ) )
		{
			var value = (string)property.GetValue( target );
			var textentry = ValueArea.Add.TextEntry( value );
			textentry.AddEventListener( "value.changed", () =>
			{
				property.SetValue( target, textentry.Text );
				Update();
			} );
		}
		else if ( property.PropertyType.IsEnum )
		{
			var value = property.GetValue( target ).ToString();
			var dropdown = new DropDown( ValueArea );
			dropdown.SetPropertyObject( "value", property.GetValue( target ) );
			dropdown.AddEventListener( "value.changed", () =>
			{
				Enum.TryParse( property.PropertyType, dropdown.Value, out var newval );
				property.SetValue( target, newval );
				Update();
			} );
		}
		else if ( property.PropertyType == typeof( float ) )
		{
			var value = (float)property.GetValue( target );
			var minmax = property.GetCustomAttribute<MinMaxAttribute>();
			var min = minmax?.MinValue ?? 0f;
			var max = minmax?.MaxValue ?? 1000f;
			var step = property.GetCustomAttribute<StepAttribute>()?.Value ?? .1f;
			var slider = ValueArea.Add.SliderWithEntry( min, max, step );
			slider.Bind( "value", target, property.Name );
			slider.AddEventListener( "value.changed", Update );
		}
	}

	public void Update()
	{
		// TODO - update the prop
	}

	public PreferenceRow()
	{
		Label = Add.Label( "Label" );
		Add.Panel().Style.FlexGrow = 1;
		ValueArea = Add.Panel( "value-area" );
	}
}

[UseTemplate]
public partial class ClientPreferencesWidget : WorldPanel
{
	// @ref
	public Panel Canvas { get; set; }

	public bool Visible { get; set; } = false;

	public void AddProperties( object obj )
	{
		var properties = obj.GetType().GetProperties( BindingFlags.Public | BindingFlags.Instance );

		foreach ( var prop in properties )
			Canvas.AddChild( new PreferenceRow( obj, prop ) );

		Canvas.Add.Label( "Preferences", "title" );
	}

	Vector2 Size => new( 800, 450f );
	protected override void PostTemplateApplied()
	{
		base.PostTemplateApplied();

		Initialize();
	}

	[Event.Hotload]
	public void Initialize()
	{
		Canvas.DeleteChildren( true );

		AddProperties( ClientPreferences.LocalSettings );
	}

	public override void Tick()
	{
		base.Tick();
	
		var pawn = Local.Pawn as PlayerPawn;
		var hand = pawn.ServeHand;

		Position = hand.Position + Vector3.Up * 2f;
		Rotation = Rotation.LookAt( -Input.VR.Head.Rotation.Forward );
		PanelBounds = new( -Size.x / 2f, -Size.y, Size.x, Size.y );
		WorldScale = 0.2f;
		Scale = 2.0f;

		if ( hand.InMenu )
		{
			Visible ^= true;
			SetClass( "visible", Visible );
		}
	}

	public override bool RayToLocalPosition( Ray ray, out Vector2 position, out float distance )
	{
		var ret = base.RayToLocalPosition( ray, out position, out distance );

		// TODO - sexy particle
		if ( Visible )
		{
			DebugOverlay.Line( ray.Origin, ray.Origin + ray.Direction * distance, Color.Red, 0, true );
		}

		return ret;
	}
}
