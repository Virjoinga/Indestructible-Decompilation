using UnityEngine;

public class InclinedProgressBar : UIPackable
{
	public float SlopeWidth;

	public float BorderWidth;

	public bool Simulate;

	private int _numLines;

	private int _numSections;

	private float _position = 1f;

	private float _t;

	private float _rightPosition;

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

	public float GetRightPosition()
	{
		return _rightPosition;
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

	private void SetPosition(float position)
	{
		position = Mathf.Clamp01(position);
		if (position != _position)
		{
			_position = position;
			int num = _vertices.Length;
			Vector3[] array = new Vector3[num];
			_vertices.CopyTo(array, 0);
			CalculateVertices(array);
			UpdateAlign(array);
			_mesh.vertices = array;
			UpdateBounds(_mesh);
		}
	}

	protected override void CalculateVertices(Vector3[] vertices)
	{
		float num = BorderWidth;
		float num2 = Mathf.Abs(SlopeWidth);
		float num3 = (Width - num2) * _position;
		if (num3 < BorderWidth * 2f)
		{
			num = num3 / 2f;
		}
		if (SlopeWidth > 0f)
		{
			vertices[0].x = 0f;
			vertices[1].x = num2;
		}
		else
		{
			vertices[0].x = num2;
			vertices[1].x = 0f;
		}
		vertices[7].x = vertices[1].x + (Width - num2) * _position;
		vertices[6].x = vertices[0].x + (Width - num2) * _position;
		vertices[2].x = vertices[0].x + num;
		vertices[3].x = vertices[1].x + num;
		vertices[5].x = vertices[7].x - num;
		vertices[4].x = vertices[6].x - num;
		_rightPosition = (vertices[7].x + vertices[6].x) / 2f;
		_rightPosition += _transform.position.x;
		for (int i = 0; i < _numLines; i++)
		{
			vertices[i * 2].y = 0f;
			vertices[i * 2 + 1].y = Height;
		}
	}

	protected override void UpdateUVs(Vector3[] vertices, Vector2[] uvs)
	{
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
}
