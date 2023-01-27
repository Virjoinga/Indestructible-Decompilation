using System.Xml.Serialization;

public class LimitedTimeBundles
{
	private bool? _enabled;

	[XmlAttribute("enabled")]
	public bool Enabled
	{
		get
		{
			bool? enabled = _enabled;
			return !enabled.HasValue || enabled.Value;
		}
		private set
		{
			_enabled = value;
		}
	}
}
