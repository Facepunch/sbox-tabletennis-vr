namespace TableTennis;

public partial class HandMenuPointer : Component
{
	[Property] public Hand Hand { get; set; }
	[Property] public GameObject PointerOrigin { get; set; }

	/// <summary>
	/// The particle system for the laser line.
	/// </summary>
	[Property] public Sandbox.ParticleSystem LineParticle { get; set; }

	/// <summary>
	/// The particle system for the laser dot.
	/// </summary>
	[Property] public Sandbox.ParticleSystem DotParticle { get; set; }

	/// <summary>
	/// The color of the laser.
	/// </summary>
	[Property] public Color LaserColor { get; set; } = Color.White;

	/// <summary>
	/// Reference to the line particle system instance.
	/// </summary>
	LegacyParticleSystem LineParticleSystem { get; set; }

	/// <summary>
	/// Reference to the dot particle system instance.
	/// </summary>
	LegacyParticleSystem DotParticleSystem { get; set; }

	private Vector3 TraceEnd = Vector3.Zero;

	/// <summary>
	/// The control point setup for the line particle system.
	/// </summary>
	private List<ParticleControlPoint> LineCPs
	{
		get
		{
			return new()
			{
				new() { StringCP = "0", Value = ParticleControlPoint.ControlPointValueInput.Vector3, VectorValue = PointerOrigin.Transform.World.Position },
				new() { StringCP = "1", Value = ParticleControlPoint.ControlPointValueInput.Vector3, VectorValue = TraceEnd },
				new() { StringCP = "2", Value = ParticleControlPoint.ControlPointValueInput.Vector3, VectorValue = new( LaserColor.r, LaserColor.g, LaserColor.b )  },
			};
		}
	}

	/// <summary>
	/// The control point setup for the dot particle system.
	/// </summary>
	private List<ParticleControlPoint> DotCPs
	{
		get
		{
			return new()
			{
				new() { StringCP = "0", Value = ParticleControlPoint.ControlPointValueInput.Vector3, VectorValue = TraceEnd },
				new() { StringCP = "2", Value = ParticleControlPoint.ControlPointValueInput.Vector3, VectorValue = new( LaserColor.r, LaserColor.g, LaserColor.b )  },
			};
		}
	}

	private Sandbox.UI.WorldInput worldInput = new();

	protected override void OnEnabled()
	{
		worldInput.Enabled = Game.IsRunningInVR;

		LineParticleSystem?.Destroy();
		DotParticleSystem?.Destroy();

		if ( LineParticle is not null )
		{
			LineParticleSystem = Components.Create<LegacyParticleSystem>();
			LineParticleSystem.Flags = ComponentFlags.NotSaved | ComponentFlags.Hidden;
			LineParticleSystem.Particles = LineParticle;
			LineParticleSystem.ControlPoints = LineCPs;
		}

		if ( DotParticle is not null )
		{
			DotParticleSystem = Components.Create<LegacyParticleSystem>();
			DotParticleSystem.Flags = ComponentFlags.NotSaved | ComponentFlags.Hidden;
			DotParticleSystem.Particles = DotParticle;
			DotParticleSystem.ControlPoints = DotCPs;
		}
	}

	protected override void OnDisabled()
	{
		worldInput.Enabled = false;
	}

	protected override void OnUpdate()
	{
		var transform = PointerOrigin.Transform.World;
		float dist = 100000f;

		worldInput.Ray = new Ray( transform.Position, transform.Position + transform.Forward * dist );
		var rootPanel = worldInput.Hovered?.FindRootPanel();

		if ( rootPanel is not null )
		{
			// Fetch how far away the panel is in world-space so we can draw a line to it.
			rootPanel.RayToLocalPosition( worldInput.Ray, out _, out dist );
		}

		var tr = Scene.Trace.Ray( transform.Position, transform.Position + transform.Forward * dist )
			.IgnoreGameObject( GameObject )
			.Run();

		TraceEnd = tr.EndPosition;

		worldInput.MouseLeftPressed = Hand.InputState.IsTriggerDown;
		worldInput.MouseRightPressed = Hand.InputState.IsGripDown;

		if ( Hand.Controller is not null )
		{
			// Mouse wheel translation :0
			worldInput.MouseWheel = Hand.Controller.Joystick.Value;
		}

		// Make sure we update the control points of both particle systems.
		if ( LineParticleSystem.IsValid() )
			LineParticleSystem.ControlPoints = LineCPs;

		if ( DotParticleSystem.IsValid() )
			DotParticleSystem.ControlPoints = DotCPs;
	}
}
