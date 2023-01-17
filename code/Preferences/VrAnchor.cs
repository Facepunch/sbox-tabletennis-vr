namespace TableTennis;

public class VrAnchor
{
	public Vector2 Range => new( -20, 20 );

	public float Right { get; set; }
	public float Forward { get; set; }

	public override string ToString()
	{
		return $"VrAnchor ({Right}, {Forward})";
	}

	public Transform GetOriginTransform()
	{
		return Game.LocalPawn.Transform;
	}
	
	public Transform GetTransform()
	{
		var tr = GetOriginTransform();
		return tr.WithPosition( tr.Position + (tr.Rotation.Right * Right) + (tr.Rotation.Forward * Forward) );
	}

	public void Set( Vector2 range )
	{
		Log.Info( $"Setting new range {range}" );
		Right = range.x;
		Forward = range.y;
	}
}

public class VrAnchorEditor
{
	public static bool IsEditing = false;
	public static Vector3 CurrentRange { get; set; } = Vector3.Zero;

	static Vector3 CachedPos;
	public static void Tick()
	{
		if ( !IsEditing ) return;

		var anchor = ClientPreferences.LocalSettings.Anchor;
		var tr = anchor.GetOriginTransform();

		var original = tr.Position;
		var wanted = original + (tr.Rotation.Right * CurrentRange.x) + (tr.Rotation.Forward * CurrentRange.y);

		DebugOverlay.Sphere( original, 1, Color.White, 0, false );
		DebugOverlay.Line( original, wanted, 0, false );
		DebugOverlay.Sphere( wanted, 1, Color.Green, 0, false );
		DebugOverlay.Box( original + new Vector3( -anchor.Range.x, -anchor.Range.y ), original + new Vector3( anchor.Range.x, anchor.Range.y ), Color.White );

		var lastPos = CachedPos;
		CachedPos = Input.VR.RightHand.Transform.Position;

		if ( Input.VR.RightHand.Grip.Value.AlmostEqual( 1f, 0.1f ) )
		{
			var delta = lastPos - CachedPos;
			CurrentRange = CurrentRange + (tr.Rotation.Right * delta.x) + (tr.Rotation.Forward * delta.y);
			CurrentRange = CurrentRange.Clamp( anchor.Range.x, anchor.Range.y );
		}
	}

	public static void Finish( bool save = true )
	{
		if ( save )
		{
			// Set the new anchor
			ClientPreferences.LocalSettings.Anchor.Set( CurrentRange );
			// Update to the server
			ClientPreferences.Save();
		}

		CurrentRange = Vector3.Zero;
	}
}
