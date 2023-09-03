using UnityEngine;

public class MenuScript : MonoBehaviour
{
	private void Start()
	{
	}

	private void OnDestroy()
	{
	}

	public void PlayAircrash()
	{
		PlaySingle("ctf_aircrash");
	}

	public void PlayIceberg()
	{
		PlaySingle("koh_iceberg");
	}

	public void PlayRocketbase()
	{
		PlaySingle("dtb_rocketbase");
	}

	public void PlayIsland()
	{
		PlaySingle("island");
	}

	public void PlaySingle(string sceneName)
	{
		MonoSingleton<Player>.Instance.StartMatchLevel(sceneName, null);
	}

	public void PlayGarage()
	{
		MonoSingleton<Player>.Instance.StartLevel("GarageScene");
	}
}
