using UnityEngine;

public class CTFFlagIndicatorController : MonoBehaviour
{
	public GameObject IndicatorPrefab;

	private GameObject _indicator;

	private void OnDestroy()
	{
		if (_indicator != null)
		{
			Object.Destroy(_indicator);
		}
	}

	private void Start()
	{
		PhotonView component = GetComponent<PhotonView>();
		if (component == null)
		{
			base.enabled = false;
		}
		if (!(IDTGame.Instance is CTFGame))
		{
			base.enabled = false;
		}
		if (base.enabled)
		{
			_indicator = (GameObject)Object.Instantiate(IndicatorPrefab);
			CTFFlagIndicator component2 = _indicator.GetComponent<CTFFlagIndicator>();
			component2.FlagTransform = GetComponent<Transform>();
			FlagItem component3 = GetComponent<FlagItem>();
			Color teamColor = MonoSingleton<Player>.Instance.GetTeamColor(component3.TeamID);
			_indicator.GetComponent<PackedSprite>().SetColor(teamColor);
		}
	}
}
