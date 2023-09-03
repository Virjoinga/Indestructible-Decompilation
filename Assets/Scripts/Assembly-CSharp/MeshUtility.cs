using System;
using UnityEngine;

public class MeshUtility
{
	private struct MeshInfo
	{
		public Mesh mesh;

		public Transform meshTransform;

		public Transform bone;

		public int boneIndex;
	}

	public static Transform[] CombineAndSkin(Transform rootTransform, Mesh resultMesh, Material filterMaterial, string staticTagPrefix, int maxBoneCount, bool destroySourceFiltersAndRenderers)
	{
		MeshFilter[] componentsInChildren = rootTransform.GetComponentsInChildren<MeshFilter>();
		int num = componentsInChildren.Length;
		MeshInfo[] array = new MeshInfo[num];
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		int num5 = 0;
		for (int i = 0; i != num; i++)
		{
			MeshFilter meshFilter = componentsInChildren[i];
			MeshRenderer component = meshFilter.GetComponent<MeshRenderer>();
			if (!(component != null) || !(component.sharedMaterial == filterMaterial))
			{
				continue;
			}
			Mesh mesh = meshFilter.mesh;
			num4 += mesh.vertexCount;
			num5 += mesh.GetTriangles(0).Length;
			array[num2].mesh = mesh;
			Transform transform = meshFilter.transform;
			Transform transform2 = transform;
			array[num2].meshTransform = transform;
			int num6 = -1;
			if (destroySourceFiltersAndRenderers)
			{
				UnityEngine.Object.Destroy(component);
				UnityEngine.Object.Destroy(meshFilter);
			}
			if (meshFilter.gameObject.tag.StartsWith(staticTagPrefix))
			{
				Transform parent = transform2.parent;
				while (parent != null)
				{
					for (int num7 = num2 - 1; num7 != -1; num7--)
					{
						if (parent == array[num7].bone)
						{
							transform2 = parent;
							num6 = (array[num2].boneIndex = array[num7].boneIndex);
							break;
						}
					}
					if (num6 != -1)
					{
						break;
					}
					transform2 = parent;
					parent = parent.parent;
				}
			}
			array[num2].bone = transform2;
			if (num6 == -1)
			{
				array[num2].boneIndex = num3;
				if (++num3 == maxBoneCount)
				{
					num2++;
					break;
				}
			}
			num2++;
		}
		Transform[] array2 = new Transform[num3];
		Vector3[] array3 = new Vector3[num4];
		Vector3[] array4 = new Vector3[num4];
		Vector2[] array5 = new Vector2[num4];
		Vector2[] array6 = new Vector2[num4];
		int[] array7 = new int[num5];
		int j = 0;
		int num8 = 0;
		int num9 = 0;
		for (; j != num2; j++)
		{
			Mesh mesh2 = array[j].mesh;
			Vector3[] vertices = mesh2.vertices;
			Vector3[] normals = mesh2.normals;
			int vertexCount = mesh2.vertexCount;
			int num10 = num8 + vertexCount;
			Transform meshTransform = array[j].meshTransform;
			Transform bone = array[j].bone;
			int boneIndex = array[j].boneIndex;
			array2[boneIndex] = bone;
			if (meshTransform != bone)
			{
				int num11 = 0;
				int num12 = num8;
				while (num11 != vertexCount)
				{
					array3[num12] = bone.InverseTransformPoint(meshTransform.TransformPoint(vertices[num11]));
					array4[num12] = bone.InverseTransformDirection(meshTransform.TransformDirection(normals[num11]));
					array6[num12].x = boneIndex;
					num11++;
					num12++;
				}
			}
			else
			{
				Array.Copy(mesh2.vertices, 0, array3, num8, vertexCount);
				Array.Copy(mesh2.normals, 0, array4, num8, vertexCount);
				for (int k = num8; k != num10; k++)
				{
					array6[k].x = boneIndex;
				}
			}
			Array.Copy(mesh2.uv, 0, array5, num8, vertexCount);
			int[] triangles = mesh2.GetTriangles(0);
			int num13 = 0;
			int num14 = triangles.Length;
			while (num13 != num14)
			{
				array7[num9] = triangles[num13] + num8;
				num13++;
				num9++;
			}
			num8 = num10;
		}
		resultMesh.Clear();
		resultMesh.vertices = array3;
		resultMesh.normals = array4;
		resultMesh.uv = array5;
		resultMesh.uv2 = array6;
		resultMesh.triangles = array7;
		resultMesh.Optimize();
		return array2;
	}
}
