using System.Xml.Serialization;

public class VehicleAbilities
{
	[XmlElement("active")]
	public VehicleAbilityActive Active = new VehicleAbilityActive();

	[XmlElement("passive")]
	public VehicleAbilityPassive Passive = new VehicleAbilityPassive();

	public void Override(VehicleAbilities other)
	{
		Passive.Override(other.Passive);
		Active.Override(other.Active);
	}
}
