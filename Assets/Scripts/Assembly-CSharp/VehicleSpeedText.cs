using Glu;
using UnityEngine;

public class VehicleSpeedText : Glu.MonoBehaviour
{
	private float m_updatePeriod = 0.2f;

	private float m_updateTime;

	private SpriteText m_spriteText;

	private void Awake()
	{
		m_spriteText = GetComponent<SpriteText>();
		m_updateTime = m_updatePeriod;
	}

	private void Start()
	{
	}

	private void Update()
	{
		m_updateTime -= Time.deltaTime;
		if (m_updateTime <= 0f)
		{
			m_updateTime += m_updatePeriod;
			Vehicle playerVehicle = VehiclesManager.instance.playerVehicle;
			if (playerVehicle != null && playerVehicle.isActive)
			{
				m_spriteText.Text = string.Format("speed:{0:0.0}, Dmg:{1:0.0}\nDOT:{2:0.0}, ROF:{3:0.0}, SEC:{4:0.0}, QL:{5}", Mathf.Sqrt(playerVehicle.vehiclePhysics.sqrSpeed), playerVehicle.weapon.GetDamage(), (!(playerVehicle.weapon is IDOTWeapon)) ? 0f : (playerVehicle.weapon as IDOTWeapon).dotInterface.GetDamage(), 1f / playerVehicle.weapon.GetFireInterval(), playerVehicle.weapon.GetShotEnergyConsumption(), QualityManager.instance.qualityLevel);
			}
		}
	}
}
