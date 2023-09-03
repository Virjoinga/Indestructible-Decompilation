using UnityEngine;

public class PanelTalents : PanelAtlasController
{
	public TalentsTalents Talents;

	public void OnBackButtonTap()
	{
		MonoSingleton<UISounds>.Instance.Play(UISounds.Type.Click);
		Owner.ActivatePreviousPanel();
	}

	public void OnPlayButtonTap()
	{
		MonoSingleton<UISounds>.Instance.Play(UISounds.Type.Click);
		GarageManager garageManager = Owner as GarageManager;
		garageManager.ActivatePanel("PanelGarage");
	}

	public override void OnActivate()
	{
		base.OnActivate();
		Talents.UpdateTalents();
	}

	public void OnResetTalentsButtonTap()
	{
		MonoSingleton<UISounds>.Instance.Play(UISounds.Type.Click);
		Dialogs.ResetTalentsConfirmation(Talents);
	}

	public void OnLearnTalentButtonTap()
	{
		MonoSingleton<UISounds>.Instance.Play(UISounds.Type.TalentLearned);
		Talents.LearnTalent();
	}

	private void Update()
	{
		if (Input.GetKeyUp(KeyCode.Escape) && MonoSingleton<DialogsQueue>.Instance.IsEmpty())
		{
			OnBackButtonTap();
		}
	}
}
