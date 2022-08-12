namespace TableTennis;

public class UnitAttribute : Attribute
{
	public string Value { get; set; }

	public UnitAttribute( string value )
	{
		Value = value;
	}
}
