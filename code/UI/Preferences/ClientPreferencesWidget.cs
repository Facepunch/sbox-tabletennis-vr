using System.Reflection;

namespace TableTennis;

public partial class PreferenceRow : Panel
{
	public Label Label { get; }
	public Panel ValueArea { get; }

	public PreferenceRow( object target, PropertyDescription property ) : this()
	{
		var currentValue = property.GetValue( target );
		Label.Text = DisplayInfo.For( property ).Name;

		if ( property.PropertyType == typeof( bool ) )
		{
			var button = ValueArea.Add.Button( string.Empty, "toggle" );
			button.SetClass( "active", (bool)property.GetValue( target ) );
			button.AddEventListener( "onmousedown", () =>
			{
				button.SetClass( "active", !button.HasClass( "active" ) );
				property.SetValue( target, button.HasClass( "active" ) );
			} );
		}
		else if ( property.PropertyType == typeof( string ) )
		{
			var value = (string)property.GetValue( target );
			var textentry = ValueArea.Add.TextEntry( value );
			textentry.AddEventListener( "value.changed", () =>
			{
				property.SetValue( target, textentry.Text );
			} );
		}
		else if ( property.PropertyType.IsEnum )
		{
			var value = property.GetValue( target ).ToString();
			var dropdown = new DropDown( ValueArea );
			dropdown.SetPropertyObject( "value", property.GetValue( target ) );
			dropdown.AddEventListener( "value.changed", () =>
			{
				property.SetValue( target, dropdown.Value );
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
		}
		else if ( property.PropertyType == typeof( VrAnchor ) )
		{
			var value = (VrAnchor)property.GetValue( target );
			var editButton = ValueArea.Add.Button( "Edit", "press", () => ClientPreferencesWidget.Current.ToggleEditingAnchor() );
		}
	}

	public PreferenceRow()
	{
		Label = Add.Label( "Label" );
		Add.Panel().Style.FlexGrow = 1;
		ValueArea = Add.Panel( "value-area" );
	}
}

[UseTemplate]
public partial class ClientPreferencesWidget : MenuPageWidget
{
	// @singleton
	public static ClientPreferencesWidget Current { get; set; }

	// @ref
	public Panel Canvas { get; set; }

	public ClientPreferencesWidget()
	{
		Current = this;
	}

	public void ToggleEditingAnchor()
	{
		VrAnchorEditor.IsEditing = !VrAnchorEditor.IsEditing;
		
		if ( !VrAnchorEditor.IsEditing )
		{
			VrAnchorEditor.Finish( false );
		}
	}

	public void AddProperties( object obj )
	{
		var properties = TypeLibrary.GetPropertyDescriptions( obj );

		foreach ( var prop in properties )
			Canvas.AddChild( new PreferenceRow( obj, prop ) );
	}

	Vector2 Size => new( 800, 750f );
	protected override void PostTemplateApplied()
	{
		base.PostTemplateApplied();
		Initialize();
	}

	public override void OnDeleted()
	{
		ClientPreferences.Save(); VrAnchorEditor.Finish( true );

		base.OnDeleted();
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
	
		var pawn = Game.LocalPawn as PlayerPawn;
		var hand = pawn.ServeHand;

		if ( Game.IsRunningInVR )
			Rotation = Rotation.LookAt( -Input.VR.Head.Rotation.Forward );
		else
			Rotation = Rotation.LookAt( -Camera.Rotation.Forward );

		Position = hand.Position + Vector3.Up * 5.6f;
		PanelBounds = new( -Size.x / 2f, -Size.y, Size.x, Size.y );
		WorldScale = 0.4f;
		Scale = 2.0f;

		//if ( hand.InMenu )
		//{
		//	SetVisible( !Visible );
		//}

		VrAnchorEditor.Tick();
	}

	public override bool RayToLocalPosition( Ray ray, out Vector2 position, out float distance )
	{
		var ret = base.RayToLocalPosition( ray, out position, out distance );

		return ret;
	}
}
