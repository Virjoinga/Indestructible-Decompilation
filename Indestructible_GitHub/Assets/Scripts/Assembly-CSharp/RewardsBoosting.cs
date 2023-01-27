using System.Xml.Serialization;

public class RewardsBoosting
{
	private bool? _boostForever;

	[XmlAttribute("boostForever")]
	public bool BoostForever
	{
		get
		{
			bool? boostForever = _boostForever;
			return boostForever.HasValue && boostForever.Value;
		}
		private set
		{
			_boostForever = value;
		}
	}
}
