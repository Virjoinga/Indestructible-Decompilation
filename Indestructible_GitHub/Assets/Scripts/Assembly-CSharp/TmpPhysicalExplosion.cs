using UnityEngine;

public class TmpPhysicalExplosion : CachedTmpObject
{
	private struct Part
	{
		public Vector3 startLocalPosition;

		public Quaternion startLocalRotation;

		public Transform transform;

		public Rigidbody rigidBody;
	}

	public float explosionForce = 10000f;

	public float explosionRadius = 300f;

	private Part[] _parts;

	protected override void Awake()
	{
		Transform component = GetComponent<Transform>();
		_parts = new Part[component.childCount];
		int num = 0;
		foreach (Transform item in component)
		{
			_parts[num].startLocalPosition = item.localPosition;
			_parts[num].startLocalRotation = item.localRotation;
			_parts[num].transform = item;
			Rigidbody component2 = item.GetComponent<Rigidbody>();
			component2.isKinematic = true;
			_parts[num].rigidBody = component2;
			num++;
		}
		base.Awake();
	}

	public override void Activate(Vector3 pos, Quaternion rot)
	{
		Activate(pos, rot, Vector3.zero);
	}

	public void Activate(Vector3 pos, Quaternion rot, Vector3 velocity)
	{
		base.Activate(pos, rot);
		int i = 0;
		for (int num = _parts.Length; i != num; i++)
		{
			Transform transform = _parts[i].transform;
			transform.gameObject.SetActiveRecursively(true);
			transform.localPosition = _parts[i].startLocalPosition;
			transform.localRotation = _parts[i].startLocalRotation;
			Rigidbody rigidBody = _parts[i].rigidBody;
			rigidBody.isKinematic = false;
			rigidBody.velocity = velocity;
			rigidBody.AddExplosionForce(explosionForce, pos, explosionRadius);
		}
	}

	private void OnDisable()
	{
		int i = 0;
		for (int num = _parts.Length; i != num; i++)
		{
			Rigidbody rigidBody = _parts[i].rigidBody;
			if (rigidBody != null)
			{
				_parts[i].rigidBody.isKinematic = true;
				_parts[i].transform.gameObject.SetActiveRecursively(false);
			}
		}
	}
}
