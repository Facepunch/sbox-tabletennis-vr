namespace TableTennis;

public class StepAttribute : Attribute
{
	public float Value;

	public StepAttribute( float value )
	{
		Value = value;
	}
}
