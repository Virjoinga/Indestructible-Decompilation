using UnityEngine;

public class PanelManagerPanel : MonoBehaviour
{
	public PanelManager Owner;

	public string PreviousPanel;

	public string Name
	{
		get
		{
			return base.gameObject.name;
		}
	}

	public virtual void OnActivate()
	{
	}

	public virtual void OnDeactivate()
	{
	}
}
