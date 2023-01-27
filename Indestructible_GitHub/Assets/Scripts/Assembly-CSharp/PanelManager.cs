using UnityEngine;

public class PanelManager : MonoBehaviour
{
	public GameObject[] _panels;

	protected PanelManagerPanel _activePanel;

	public virtual PanelManagerPanel ActivatePanel(string name)
	{
		return ActivatePanel(name, true);
	}

	public virtual void ActivatePreviousPanel()
	{
		if (_activePanel != null)
		{
			ActivatePanel(_activePanel.PreviousPanel, false);
		}
	}

	public virtual PanelManagerPanel ActivateDirectly(string name)
	{
		return ActivatePanel(name, false);
	}

	private PanelManagerPanel ActivatePanel(string name, bool forward)
	{
		if (_activePanel != null && _activePanel.Name == name)
		{
			return _activePanel;
		}
		GameObject[] panels = _panels;
		foreach (GameObject gameObject in panels)
		{
			if (!(gameObject.name == name))
			{
				continue;
			}
			PanelManagerPanel component = gameObject.GetComponent<PanelManagerPanel>();
			if (_activePanel != null)
			{
				if (forward)
				{
					component.PreviousPanel = _activePanel.Name;
				}
				_activePanel.gameObject.SetActiveRecursively(false);
				_activePanel.OnDeactivate();
				_activePanel = null;
			}
			component.gameObject.SetActiveRecursively(true);
			component.OnActivate();
			_activePanel = component;
		}
		return _activePanel;
	}

	protected virtual void Awake()
	{
		GameObject[] panels = _panels;
		foreach (GameObject gameObject in panels)
		{
			gameObject.SetActiveRecursively(false);
		}
	}

	protected virtual void Start()
	{
	}

	public PanelManagerPanel GetActivePanel()
	{
		return _activePanel;
	}
}
