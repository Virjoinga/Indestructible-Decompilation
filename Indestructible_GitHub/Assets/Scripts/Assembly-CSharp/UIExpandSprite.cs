using UnityEngine;

public class UIExpandSprite : UIPackable
{
	public float Border;

	public float DesiredWidth;

	public float DesiredHeight;

	private int _numLines;

	private int _numSections;

	public void SetSize(float width, float height)
	{
		if (Width != width || Height != height)
		{
			Width = width;
			Height = height;
			int num = _vertices.Length;
			Vector3[] vertices = new Vector3[num];
			CalculateVertices(vertices);
			UpdateAlign(vertices);
			_mesh.vertices = vertices;
			UpdateBounds(_mesh);
		}
	}

	public void SetWidth(float width)
	{
		SetSize(width, Height);
	}

	public void SetHeight(float height)
	{
		SetSize(Width, height);
	}

	public void SetInternalWidth(float width)
	{
		SetWidth(width + Border * 2f);
	}

	public void SetInternalHeight(float height)
	{
		SetHeight(height + Border * 2f);
	}

	protected override void CalculateVertices(Vector3[] vertices)
	{
		for (int i = 0; i < 4; i++)
		{
			vertices[i].x = 0f;
			vertices[i + 4].x = Border;
			vertices[i + 8].x = Width - Border;
			vertices[i + 12].x = Width;
			int num = i * 4;
			vertices[num].y = 0f;
			vertices[num + 1].y = Border;
			vertices[num + 2].y = Height - Border;
			vertices[num + 3].y = Height;
		}
	}

	protected override void CalculateTriangles(int[] triangles)
	{
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < _numTriangles / 2; i++)
		{
			num2 = i + i / 3;
			triangles[num++] = num2;
			triangles[num++] = num2 + 1;
			triangles[num++] = num2 + 4;
			triangles[num++] = num2 + 4;
			triangles[num++] = num2 + 1;
			triangles[num++] = num2 + 5;
		}
	}

	protected override Mesh CreateMesh()
	{
		_numVertices = 16;
		_numTriangles = 18;
		return base.CreateMesh();
	}

	protected override void Awake()
	{
		base.Awake();
		if (Application.isPlaying)
		{
			SetSize(DesiredWidth, DesiredHeight);
		}
	}
}
