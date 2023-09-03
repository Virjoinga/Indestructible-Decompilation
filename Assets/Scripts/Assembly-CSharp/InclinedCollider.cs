using UnityEngine;

[RequireComponent(typeof(MeshCollider))]
[ExecuteInEditMode]
public class InclinedCollider : MonoBehaviour
{
	public Vector2 Size;

	public Vector2 Center;

	public float Slope;

	public bool SlopeLeft = true;

	public bool SlopeRight = true;

	private Mesh CreateMesh()
	{
		int[] triangles = new int[6] { 0, 1, 2, 2, 1, 3 };
		Vector3[] vertices = new Vector3[4];
		CalculateVertices(ref vertices);
		Mesh mesh = new Mesh();
		mesh.name = "Inclined";
		mesh.vertices = vertices;
		mesh.triangles = triangles;
		mesh.RecalculateBounds();
		mesh.RecalculateNormals();
		return mesh;
	}

	private void Init()
	{
		MeshCollider component = GetComponent<MeshCollider>();
		component.sharedMesh = CreateMesh();
	}

	private void Start()
	{
		Init();
	}

	private void CalculateVertices(ref Vector3[] vertices)
	{
		float num = Slope / 2f;
		float num2 = Size.x / 2f;
		float num3 = Size.y / 2f;
		vertices[0].x = 0f - num2 + Center.x;
		vertices[1].x = 0f - num2 + Center.x;
		vertices[2].x = num2 + Center.x;
		vertices[3].x = num2 + Center.x;
		if (SlopeLeft)
		{
			vertices[0].x -= num;
			vertices[1].x += num;
		}
		if (SlopeRight)
		{
			vertices[2].x -= num;
			vertices[3].x += num;
		}
		vertices[0].y = 0f - num3 + Center.y;
		vertices[1].y = num3 + Center.y;
		vertices[2].y = 0f - num3 + Center.y;
		vertices[3].y = num3 + Center.y;
	}

	private void Update()
	{
		if (Application.isEditor && !Application.isPlaying)
		{
			Init();
		}
	}
}
