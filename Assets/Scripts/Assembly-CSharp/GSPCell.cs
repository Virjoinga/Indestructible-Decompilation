using System.Collections.Generic;
using UnityEngine;

public class GSPCell
{
	public const int BaseCapacity = 32;

	private List<GameObject> m_Objects = new List<GameObject>(32);

	public void AddObject(GameObject Obj)
	{
		if (m_Objects.Count == m_Objects.Capacity)
		{
			m_Objects.Capacity += 32;
		}
		m_Objects.Add(Obj);
	}

	public void RemoveObject(GameObject Obj)
	{
		m_Objects.Remove(Obj);
		if (m_Objects.Count <= 32)
		{
			m_Objects.Capacity = 32;
		}
	}

	public bool PointCheck(Bounds Box, ref List<GameObject> ObjList)
	{
		bool result = false;
		foreach (GameObject @object in m_Objects)
		{
			if (Box.Contains(@object.transform.position))
			{
				ObjList.Add(@object);
				result = true;
			}
		}
		return result;
	}
}
