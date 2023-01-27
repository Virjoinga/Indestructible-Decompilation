using System.Collections;
using UnityEngine;

public class GarageCustomizeButton : MonoBehaviour
{
	public GameObject HintPrefab;

	private UIButton _button;

	private GameObject _hint;

	private void InputDelegate(ref POINTER_INFO ptr)
	{
		POINTER_INFO.INPUT_EVENT evt = ptr.evt;
		if (evt == POINTER_INFO.INPUT_EVENT.TAP)
		{
			if (!MonoSingleton<Player>.Instance.Tutorial.IsCustomizeTap())
			{
				MonoSingleton<Player>.Instance.Tutorial.SetCustomizeTap();
				MonoSingleton<Player>.Instance.Save();
				StopAllCoroutines();
			}
			if (_hint != null)
			{
				Object.Destroy(_hint);
				_hint = null;
			}
		}
	}

	private void Awake()
	{
		if (!MonoSingleton<Player>.Instance.Tutorial.IsCustomizeTap())
		{
			_button = GetComponent<UIButton>();
			_button.AddInputDelegate(InputDelegate);
		}
	}

	public void OnActivate()
	{
		if (!MonoSingleton<Player>.Instance.Tutorial.IsCustomizeTap() && (int)MonoSingleton<Player>.Instance.Level > 1 && _hint == null)
		{
			StartCoroutine(TutorialCustomizeTap());
		}
	}

	public void OnDeactivate()
	{
		StopAllCoroutines();
	}

	private IEnumerator TutorialCustomizeTap()
	{
		do
		{
			yield return new WaitForSeconds(3f);
		}
		while (!MonoSingleton<DialogsQueue>.Instance.IsEmpty());
		_hint = (GameObject)Object.Instantiate(HintPrefab);
		Transform t = _hint.GetComponent<Transform>();
		t.parent = GetComponent<Transform>();
		t.localPosition = new Vector3(-80f, 0f, -2f);
	}
}
