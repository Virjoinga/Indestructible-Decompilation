using UnityEngine;

public class UINotification : MonoBehaviour
{
	protected virtual void Awake()
	{
	}

	protected virtual void Start()
	{
	}

	public virtual void Activate()
	{
		MonoUtils.SetActive(this, true);
	}
}
