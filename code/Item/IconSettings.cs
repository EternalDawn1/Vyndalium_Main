namespace GeneralGame;

public struct IconSettings : IEquatable<IconSettings>
{
	public string Model { get; set; }
	public string MaterialGroup { get; set; }
	public string MaterialOverride { get; set; }
	
	public float LightBrightness { get; set; }
	public float LightRadius { get; set; }
	public Color LightColour { get; set; }
	public float DirectionalLightBrightness { get; set; }
	public Angles DirectionalLightRotation { get; set; }
	public Color DirectionalLightColour { get; set; }
	

	public Color Colour { get; set; }
	public Vector3 Position { get; set; }
	public Rotation Rotation { get; set; }
	public Guid Guid { get; set; }

	[Hide, JsonIgnore]
	public string Path => $"ui/icons/{Guid}.png";

	public override int GetHashCode()
	{
		return HashCode.Combine( Guid );
	}

	public override bool Equals( object obj )
	{
		return obj is IconSettings other
			&& Equals( other );
	}

	public bool Equals( IconSettings other )
	{
		return other.Guid == Guid;
	}
}
