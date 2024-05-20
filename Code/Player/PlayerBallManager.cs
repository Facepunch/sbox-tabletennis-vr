namespace TableTennis;

public sealed class PlayerBallManager : Component
{
	[RequireComponent] Hand Hand { get; set; }
	[Property] public GameObject BallPrefab { get; set; }

	public IHoldableObject BallInstance { get; private set; }

	IHoldableObject GetOrCreateBall()
	{
		if ( BallInstance.IsValid() )
			return BallInstance;

		var instance = BallPrefab.Clone();
		instance.Transform.Position = Transform.Position;

		var holdable = instance.Components.Get<IHoldableObject>( FindMode.EnabledInSelfAndDescendants );
		BallInstance = holdable;

		return BallInstance;
	}

	protected override void OnUpdate()
	{
		if ( IsProxy ) return;

		if ( ( Hand.Controller?.ButtonA.WasPressed ?? false ) || Input.Down( "Jump" ) )
		{
			var ball = GetOrCreateBall();
			Hand.StartHolding( ball );
		}
	}
}
