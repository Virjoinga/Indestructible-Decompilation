using UnityEngine;

public static class MonoUtils
{
	public static void SetActive(GameObject o, bool active)
	{
		if (o != null)
		{
			o.SetActiveRecursively(active);
		}
	}

	public static void SetActive(Component c, bool active)
	{
		if ((bool)c)
		{
			SetActive(c.gameObject, active);
		}
	}

	public static T GetComponentAlsoInChildren<T>(GameObject o) where T : Component
	{
		T component = o.GetComponent<T>();
		if ((Object)component != (Object)null)
		{
			return component;
		}
		Transform component2 = o.GetComponent<Transform>();
		foreach (Transform item in component2)
		{
			T componentAlsoInChildren = GetComponentAlsoInChildren<T>(item.gameObject);
			if ((Object)componentAlsoInChildren != (Object)null)
			{
				return componentAlsoInChildren;
			}
		}
		return (T)null;
	}

	public static void DetachAndDestroy(Component c)
	{
		if (c != null)
		{
			DetachAndDestroy(c.gameObject);
		}
	}

	public static void DetachAndDestroy(GameObject o)
	{
		if (o != null)
		{
			o.GetComponent<Transform>().parent = null;
			Object.Destroy(o);
		}
	}
}
