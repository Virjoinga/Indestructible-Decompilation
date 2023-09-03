using UnityEngine;

[AddComponentMenu("Indestructible/Utils/SkelController")]
public class SkelController : MonoBehaviour
{
	private int _lastFrame = -1;

	private Transform[] _bones;

	private Matrix4x4[] _boneWorldMatrices;

	private Material[] _materials;

	private bool _isD3D;

	private static string[] _boneMatrixNames = new string[16]
	{
		"_Bones_MVP0", "_Bones_MVP1", "_Bones_MVP2", "_Bones_MVP3", "_Bones_MVP4", "_Bones_MVP5", "_Bones_MVP6", "_Bones_MVP7", "_Bones_MVP8", "_Bones_MVP9",
		"_Bones_MVP10", "_Bones_MVP11", "_Bones_MVP12", "_Bones_MVP13", "_Bones_MVP14", "_Bones_MVP15"
	};

	public Material[] materials
	{
		get
		{
			return _materials;
		}
		set
		{
			_materials = value;
		}
	}

	public Renderer CombineAndSkinChildren(Material filterMaterial, string staticTagPrefix)
	{
		Mesh mesh = new Mesh();
		mesh.hideFlags = HideFlags.HideAndDontSave;
		_bones = MeshUtility.CombineAndSkin(base.transform, mesh, filterMaterial, staticTagPrefix, 16, true);
		Debug.Log("CombineAndSkinChildren: Bones count = " + _bones.Length);
		_boneWorldMatrices = new Matrix4x4[_bones.Length];
		base.gameObject.AddComponent<MeshFilter>().sharedMesh = mesh;
		_isD3D = 0 <= SystemInfo.graphicsDeviceVersion.IndexOf("Direct3D");
		Renderer renderer = base.renderer;
		_materials = renderer.materials;
		return renderer;
	}

	private void OnWillRenderObject()
	{
		int num = _materials.Length;
		int frameCount = Time.frameCount;
		Camera current = Camera.current;
		Matrix4x4 projectionMatrix = current.projectionMatrix;
		if (_isD3D)
		{
			if (current.targetTexture != null)
			{
				for (int i = 0; i != 4; i++)
				{
					projectionMatrix[1, i] = 0f - projectionMatrix[1, i];
				}
			}
			for (int j = 0; j != 4; j++)
			{
				projectionMatrix[2, j] = projectionMatrix[2, j] * 0.5f + projectionMatrix[3, j] * 0.5f;
			}
		}
		projectionMatrix *= current.worldToCameraMatrix;
		int k = 0;
		for (int num2 = _bones.Length; k != num2; k++)
		{
			if (_lastFrame != frameCount)
			{
				_boneWorldMatrices[k] = _bones[k].localToWorldMatrix;
			}
			Matrix4x4 matrix = projectionMatrix * _boneWorldMatrices[k];
			for (int l = 0; l != num; l++)
			{
				_materials[l].SetMatrix(_boneMatrixNames[k], matrix);
			}
		}
		_lastFrame = frameCount;
	}
}
