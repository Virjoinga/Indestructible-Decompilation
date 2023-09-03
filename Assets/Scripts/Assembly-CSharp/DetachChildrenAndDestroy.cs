using System.Collections.Generic;
using UnityEngine;

public class DetachChildrenAndDestroy : MonoBehaviour
{
	private void Start()
	{
		if (base.transform.parent == null)
		{
			base.transform.DetachChildren();
		}
		else
		{
			List<Transform> list = new List<Transform>();
			foreach (Transform item in base.transform)
			{
				list.Add(item);
			}
			foreach (Transform item2 in list)
			{
				item2.parent = base.transform.parent;
			}
		}
		Object.Destroy(base.gameObject);
	}
}
