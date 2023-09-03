using System.Collections.Generic;
using UnityEngine;

public class GridSpacePartition : MonoBehaviour
{
	public float CellSize = 100f;

	public int Width = 2;

	public int Height = 2;

	private GSPCell[,] m_Cells = new GSPCell[2, 2];

	private bool m_bInited;

	private float m_StartX;

	private float m_StartY;

	private void Awake()
	{
		m_StartX = base.gameObject.transform.position.x;
		m_StartY = base.gameObject.transform.position.z;
		Init();
	}

	public void Init()
	{
		GSPCell[,] array = new GSPCell[Width, Height];
		int num = Mathf.Min(m_Cells.GetLength(0), Width);
		int num2 = Mathf.Min(m_Cells.GetLength(1), Height);
		if (m_bInited)
		{
			for (int i = 0; i < num; i++)
			{
				for (int j = 0; j < num2; j++)
				{
					array[i, j] = m_Cells[i, j];
				}
			}
		}
		else
		{
			m_bInited = true;
		}
		m_Cells = array;
		for (int k = 0; k < m_Cells.GetLength(0); k++)
		{
			for (int l = 0; l < m_Cells.GetLength(1); l++)
			{
				if (m_Cells[k, l] == null)
				{
					m_Cells[k, l] = new GSPCell();
				}
			}
		}
	}

	private int GetCellX(Vector3 Point)
	{
		return Mathf.FloorToInt(Point.x / CellSize);
	}

	private int GetCellY(Vector3 Point)
	{
		return Mathf.FloorToInt(Point.z / CellSize);
	}

	private int GetCellX(Vector2 Point)
	{
		return Mathf.FloorToInt(Point.x / CellSize);
	}

	private int GetCellY(Vector2 Point)
	{
		return Mathf.FloorToInt(Point.y / CellSize);
	}

	public void AddObject(GameObject Obj)
	{
		Vector2 point = new Vector2(Obj.transform.position.x, Obj.transform.position.z);
		if (point.x < m_StartX || point.y < m_StartY)
		{
			Debug.LogError("Object coords are out of PSP");
			Debug.Break();
		}
		int cellX = GetCellX(point);
		int cellY = GetCellY(point);
		if (cellX >= Width || cellY >= Height)
		{
			Debug.LogError("Object coords are out of PSP");
			Debug.Break();
		}
		m_Cells[cellX, cellY].AddObject(Obj);
	}

	public void RemoveObject(GameObject Obj)
	{
		Vector2 point = new Vector2(Obj.transform.position.x, Obj.transform.position.z);
		if (point.x < m_StartX || point.y < m_StartY)
		{
			Debug.LogError("Object coords are out of PSP");
			Debug.Break();
		}
		int cellX = GetCellX(point);
		int cellY = GetCellY(point);
		if (cellX >= Width || cellY >= Height)
		{
			Debug.LogError("Object coords are out of PSP");
			Debug.Break();
		}
		m_Cells[cellX, cellY].RemoveObject(Obj);
	}

	public bool PointCheck(Bounds Box, ref List<GameObject> ObjList)
	{
		int num = Mathf.Max(GetCellX(Box.min), 0);
		int num2 = Mathf.Min(GetCellX(Box.max), Width - 1);
		int num3 = Mathf.Max(GetCellY(Box.min), 0);
		int num4 = Mathf.Min(GetCellY(Box.max), Height - 1);
		bool flag = false;
		for (int i = num; i <= num2; i++)
		{
			for (int j = num3; j <= num4; j++)
			{
				flag |= m_Cells[i, j].PointCheck(Box, ref ObjList);
			}
		}
		return flag;
	}
}
