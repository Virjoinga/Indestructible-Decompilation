using UnityEngine;

public class LavaEffect : MonoBehaviour
{
	public Material LavaMaterial;

	public float LavaSpeed = 0.1f;

	private float _offset;

	private void Start()
	{
		_offset = 0f;
	}

	private void Update()
	{
		if (LavaMaterial != null)
		{
			_offset += Time.deltaTime * LavaSpeed;
			LavaMaterial.SetFloat("_UVOffset", _offset);
		}
	}
}
