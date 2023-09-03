using System.Collections;
using UnityEngine;

public class UIAnimatedPanel : MonoBehaviour
{
	protected UIPanel _panel;

	private int _transitionIndex;

	private UIPanelManager.SHOW_MODE[] _transitions = new UIPanelManager.SHOW_MODE[1];

	protected virtual void Start()
	{
		_panel = GetComponent<UIPanel>();
		StartCoroutine(Tick());
	}

	protected virtual IEnumerator Tick()
	{
		while (true)
		{
			if (!_panel.IsTransitioning)
			{
				if (_transitionIndex == _transitions.Length)
				{
					break;
				}
				_panel.StartTransition(_transitions[_transitionIndex]);
				_transitionIndex++;
			}
			yield return null;
		}
	}
}
