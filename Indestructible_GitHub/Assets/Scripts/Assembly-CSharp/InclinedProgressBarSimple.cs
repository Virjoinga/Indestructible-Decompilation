using UnityEngine;

public class InclinedProgressBarSimple : UIPackable
{
	public float SlopeWidth;

	public bool Simulate;

	private int _numLines;

	private int _numSections;

	private float _position = 1f;

	private float _t;

	public float Position
	{
		get
		{
			return _position;
		}
		set
		{
			SetPosition(value);
		}
	}

	protected override void Update()
	{
		base.Update();
		if (Application.isPlaying && Simulate)
		{
			_t += Time.deltaTime / 20f;
			if (_t > 1f)
			{
				_t = 0f;
			}
			Position = _t;
		}
	}

	protected virtual void SetPosition(float position)
	{
		position = Mathf.Clamp01(position);
		if (position != _position)
		{
			_position = position;
			int num = _vertices.Length;
			Vector3[] array = new Vector3[num];
			_vertices.CopyTo(array, 0);
			CalculateVertices(array);
			Vector2[] uv = _mesh.uv;
			UpdateUVs(array, uv);
			UpdateAlign(array);
			_mesh.vertices = array;
			_mesh.uv = uv;
			UpdateBounds(_mesh);
		}
	}

	protected override void CalculateVertices(Vector3[] vertices)
	{
		float num = Mathf.Abs(SlopeWidth);
		if (SlopeWidth > 0f)
		{
			vertices[0].x = 0f;
			vertices[1].x = num;
		}
		else
		{
			vertices[0].x = num;
			vertices[1].x = 0f;
		}
		vertices[3].x = vertices[1].x + (Width - num) * _position;
		vertices[2].x = vertices[0].x + (Width - num) * _position;
		for (int i = 0; i < _numLines; i++)
		{
			vertices[i * 2].y = 0f;
			vertices[i * 2 + 1].y = Height;
		}
	}

	protected override void CalculateUVs(Vector3[] vertices, Vector2[] uvs)
	{
		if (_size.x * _size.y == 0f)
		{
			return;
		}
		if (Horizontal == UIAlign.Horizontal.Right)
		{
			for (int i = 0; i < _numVertices; i++)
			{
				uvs[i].x = (_size.x - vertices[_numVertices - i - 1].x) / _size.x * _uvs.width + _uvs.x;
				uvs[i].y = vertices[i].y / _size.y * _uvs.height + _uvs.y;
			}
		}
		else
		{
			for (int j = 0; j < _numVertices; j++)
			{
				uvs[j].x = vertices[j].x / _size.x * _uvs.width + _uvs.x;
				uvs[j].y = vertices[j].y / _size.y * _uvs.height + _uvs.y;
			}
		}
	}

	protected override void UpdateUVs(Vector3[] vertices, Vector2[] uvs)
	{
		CalculateUVs(vertices, uvs);
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
		_numVertices = 4;
		_numTriangles = 2;
		_numLines = _numVertices / 2;
		_numSections = _numLines - 1;
		return base.CreateMesh();
	}
}
