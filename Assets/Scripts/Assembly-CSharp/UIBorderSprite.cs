using UnityEngine;

public class UIBorderSprite : UIPackable
{
	public float BorderLeft;

	public float BorderRight;

	public float DesiredWidth;

	private int _numLines;

	private int _numSections;

	public void SetWidth(float width)
	{
		if (Width != width)
		{
			Width = width;
			int num = _vertices.Length;
			Vector3[] vertices = new Vector3[num];
			CalculateVertices(vertices);
			UpdateAlign(vertices);
			_mesh.vertices = vertices;
			UpdateCenter(vertices);
			UpdateBounds(_mesh);
		}
	}

	public void SetInternalWidth(float width)
	{
		width += BorderLeft + BorderRight;
		if (width < 0f)
		{
			width = 0f;
		}
		SetWidth(width);
	}

	protected override void CalculateVertices(Vector3[] vertices)
	{
		vertices[0].x = 0f;
		vertices[1].x = 0f;
		vertices[2].x = vertices[0].x + BorderLeft;
		vertices[3].x = vertices[1].x + BorderLeft;
		float num = Width - BorderLeft - BorderRight;
		vertices[4].x = vertices[2].x + num;
		vertices[5].x = vertices[3].x + num;
		vertices[6].x = vertices[4].x + BorderRight;
		vertices[7].x = vertices[5].x + BorderRight;
		for (int i = 0; i < _numLines; i++)
		{
			vertices[i * 2].y = 0f;
			vertices[i * 2 + 1].y = Height;
		}
	}

	protected override void CalculateTriangles(int[] triangles)
	{
		for (int i = 0; i < _numSections; i++)
		{
			int num = i * 3 * 2;
			int num2 = (triangles[num] = i * 2);
			triangles[num + 1] = num2 + 1;
			triangles[num + 2] = num2 + 2;
			triangles[num + 3] = num2 + 2;
			triangles[num + 4] = num2 + 1;
			triangles[num + 5] = num2 + 3;
		}
	}

	protected override Mesh CreateMesh()
	{
		_numVertices = 8;
		_numTriangles = 6;
		_numLines = _numVertices / 2;
		_numSections = _numLines - 1;
		return base.CreateMesh();
	}

	protected override void Awake()
	{
		base.Awake();
		if (Application.isPlaying)
		{
			SetWidth(DesiredWidth);
		}
	}
}
