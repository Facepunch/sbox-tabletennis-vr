
public partial class HandPreset
{
	public static HandPreset Grip => new( 1.0f, 1.0f, 1.0f, 1.0f, 1.0f );
	public static HandPreset GripWithoutIndexFinger => new( -1.0f, 1.0f, 1.0f, 1.0f, 1.0f );
}
