namespace TableTennis;

public class VrAnchor
{
	public float Right { get; set; }
	public float Up { get; set; }

	public override string ToString()
	{
		return $"VrAnchor <{Right}, {Up}>";
	}

	public Transform GetTransform()
	{
		var tr = Local.Pawn.Transform;
		return tr.WithPosition( tr.Position + (Vector3.Right * Right) + (Vector3.Up * Up) );
	}
}
