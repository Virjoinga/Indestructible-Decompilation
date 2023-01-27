using System.Xml.Serialization;

public class VehicleFueling
{
	private bool? _freezeForever;

	[XmlAttribute("freezeForever")]
	public bool FreezeForever
	{
		get
		{
			bool? freezeForever = _freezeForever;
			return freezeForever.HasValue && freezeForever.Value;
		}
		private set
		{
			_freezeForever = value;
		}
	}
}
