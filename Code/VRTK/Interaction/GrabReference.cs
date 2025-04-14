public sealed class GrabReference : Component, Component.ExecuteInEditor
{
	private static Model HumanHandLeft = Model.Load( "models/hands/human_hand_left.vmdl" );
	private static Model HumanHandRight = Model.Load( "models/hands/human_hand_right.vmdl" );

	[Property, Group( "Debug" ), MakeDirty] public Hand.HandSources VisualHand { get; set; }
	[Property, InlineEditor] public HandPreset HandPreset { get; set; } = new();

	private SkinnedModelRenderer _hintRenderer;

	public Vector3 GetOffset( Hand.HandSources hand )
	{
		if ( hand == Hand.HandSources.Left ) return Vector3.Zero;
		return new Vector3( 0, -LocalPosition.y, 0 );
	}

	public Rotation GetRotationOffset( Hand.HandSources hand )
	{
		if ( hand == Hand.HandSources.Left )
			return Rotation.Identity; // No change needed for left hand

		// Reflect the rotation across the YZ plane (negate X and Z)
		return new Rotation( -LocalRotation.x, LocalRotation.y, -LocalRotation.z, -LocalRotation.w );

	}

	private void CreateHint()
	{
		if ( Game.IsPlaying )
			return;

		if ( _hintRenderer.IsValid() )
			ClearHint();

		var go = new GameObject( GameObject );
		go.Flags |= GameObjectFlags.Hidden | GameObjectFlags.NotSaved;

		_hintRenderer = go.AddComponent<SkinnedModelRenderer>();
		_hintRenderer.Tint = _hintRenderer.Tint.WithAlpha( 0.4f );
		UpdateHint();
	}

	private void UpdateHint()
	{
		if ( _hintRenderer.IsValid() )
		{
			_hintRenderer.Model = VisualHand == Hand.HandSources.Left ? HumanHandLeft : HumanHandRight;
			_hintRenderer.LocalPosition = GetOffset( VisualHand );
			_hintRenderer.LocalRotation = GetRotationOffset( VisualHand );
		}
	}

	private void ClearHint()
	{
		if ( _hintRenderer.IsValid() )
			_hintRenderer.DestroyGameObject();

		_hintRenderer = null;
	}

	protected override void OnDirty()
	{
		UpdateHint();
	}

	protected override void OnEnabled()
	{
		CreateHint();
	}

	protected override void OnDisabled()
	{
		ClearHint();
	}

	protected override void OnUpdate()
	{
		if ( _hintRenderer.IsValid() )
		{
			_hintRenderer.Set( "BasePose", 1 );
			_hintRenderer.Set( "bGrab", true );
			_hintRenderer.Set( "GrabMode", 1 );

			HandPreset.Apply( _hintRenderer );
		}
	}
}
