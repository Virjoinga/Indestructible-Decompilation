using System.Collections;
using UnityEngine;

public class CRLoadingArea : MonoBehaviour
{
	public GameObject UnloadingFX;

	public GameObject UnloadFinishFX;

	public SpriteText DissolvingTimer;

	private CRTeamGame _CRTeamGame;

	private void Start()
	{
		_CRTeamGame = IDTGame.Instance as CRTeamGame;
		StartParticlesAndDisableFX(UnloadingFX);
		StartParticlesAndDisableFX(UnloadFinishFX);
		if ((bool)DissolvingTimer)
		{
			DissolvingTimer.gameObject.SetActiveRecursively(false);
		}
	}

	private void StartParticlesAndDisableFX(GameObject fxObj)
	{
		if (!(fxObj != null))
		{
			return;
		}
		ParticleSystem[] componentsInChildren = fxObj.GetComponentsInChildren<ParticleSystem>();
		if (componentsInChildren != null)
		{
			ParticleSystem[] array = componentsInChildren;
			foreach (ParticleSystem particleSystem in array)
			{
				particleSystem.Play();
			}
		}
		fxObj.SetActiveRecursively(false);
	}

	private void OnTriggerEnter(Collider collider)
	{
		if (_CRTeamGame != null)
		{
			PlayerVehicle component = collider.GetComponent<PlayerVehicle>();
			if ((bool)component)
			{
				_CRTeamGame.PlayerEnterLoadingArea(component, this);
			}
		}
	}

	private void OnTriggerExit(Collider collider)
	{
		if (_CRTeamGame != null)
		{
			PlayerVehicle component = collider.GetComponent<PlayerVehicle>();
			if ((bool)component)
			{
				_CRTeamGame.PlayerLeaveLoadingArea(component, this);
			}
		}
	}

	public void OnChargeUnloadStart()
	{
		if ((bool)UnloadingFX)
		{
			UnloadingFX.SetActiveRecursively(true);
		}
	}

	public void OnChargeUnloadBreak()
	{
		if ((bool)UnloadingFX)
		{
			UnloadingFX.SetActiveRecursively(false);
		}
	}

	public void OnChargeUnloaded()
	{
	}

	public void OnChargeStartDissolving(float duration)
	{
		if ((bool)UnloadingFX)
		{
			UnloadingFX.SetActiveRecursively(true);
		}
		StartCoroutine("DissolvingIndicator", duration);
	}

	public void OnChargeDissolvingBreak()
	{
		if ((bool)UnloadingFX)
		{
			UnloadingFX.SetActiveRecursively(false);
		}
		if ((bool)UnloadFinishFX)
		{
			UnloadFinishFX.SetActiveRecursively(false);
		}
		if ((bool)DissolvingTimer)
		{
			DissolvingTimer.gameObject.SetActiveRecursively(false);
		}
		StopCoroutine("DissolvingIndicator");
	}

	public void OnChargeDissolved()
	{
		if ((bool)UnloadingFX)
		{
			UnloadingFX.SetActiveRecursively(false);
		}
		if ((bool)UnloadFinishFX)
		{
			UnloadFinishFX.SetActiveRecursively(true);
		}
		if ((bool)DissolvingTimer)
		{
			DissolvingTimer.gameObject.SetActiveRecursively(false);
		}
		StopCoroutine("DissolvingIndicator");
	}

	private IEnumerator DissolvingIndicator(float duration)
	{
		int count = (int)(duration + 0.5f);
		YieldInstruction delayYI = new WaitForSeconds(1f);
		if (count > 0 && DissolvingTimer != null)
		{
			DissolvingTimer.gameObject.SetActiveRecursively(true);
			for (int i = count; i > 0; i--)
			{
				DissolvingTimer.Text = i.ToString();
				yield return delayYI;
			}
		}
	}
}
