using System.Collections;
using UnityEngine;

public class AwayAnimActivator : MonoBehaviour
{
	public AnimationClip IdleAnimation;

	public AnimationClip AwayAnimation;

	public float IdleDiffer = 0.5f;

	public float AwayDiffer = 0.5f;

	public Vector3 VisibleCenter = Vector3.zero;

	public int requiredQualityLevel = 1;

	private Animation[] _animations;

	private Vector3 _visiblePivot;

	private void Awake()
	{
		QualityManager instance = QualityManager.instance;
		instance.qualityLevelChangedEvent += QualityLevelChanged;
		QualityLevelChanged(0, instance.qualityLevel);
	}

	private void Start()
	{
		_visiblePivot = base.transform.TransformPoint(VisibleCenter);
		_animations = GetComponentsInChildren<Animation>();
		if (IdleAnimation != null && _animations != null)
		{
			Animation[] animations = _animations;
			foreach (Animation anim in animations)
			{
				float delay = Random.Range(0f, IdleDiffer * 100f) / 100f;
				StartCoroutine(StartIdleAnimation(anim, delay));
			}
		}
	}

	private void OnDestroy()
	{
		if (QualityManager.isExists)
		{
			QualityManager.instance.qualityLevelChangedEvent -= QualityLevelChanged;
		}
	}

	private void OnTriggerEnter(Collider collider)
	{
		VehiclesManager instance = VehiclesManager.instance;
		if (instance == null)
		{
			return;
		}
		Vehicle playerVehicle = instance.playerVehicle;
		if (!(playerVehicle != null) || !(playerVehicle.mainOwnerCollider == collider))
		{
			return;
		}
		if (AwayAnimation != null)
		{
			Animation[] animations = _animations;
			foreach (Animation value in animations)
			{
				StartCoroutine("StartActiveAnimation", value);
			}
		}
		StartCoroutine(CheckVisible());
	}

	private IEnumerator StartIdleAnimation(Animation anim, float delay)
	{
		yield return new WaitForSeconds(delay);
		string animName = IdleAnimation.name;
		anim.Play(animName);
	}

	private IEnumerator StartActiveAnimation(Animation anim)
	{
		float delay = Random.value * AwayDiffer;
		string animName = AwayAnimation.name;
		if (anim[animName].time == 0f)
		{
			yield return new WaitForSeconds(delay);
			anim[animName].layer = 1;
			anim[animName].wrapMode = WrapMode.ClampForever;
			anim[animName].weight = 1f;
			anim[animName].enabled = true;
			anim.Rewind(animName);
			anim.Play(animName);
		}
	}

	private IEnumerator CheckVisible()
	{
		while (true)
		{
			Camera cam = Camera.mainCamera;
			Vector3 vpP = cam.WorldToViewportPoint(_visiblePivot);
			if (vpP.x < -0.12f || vpP.x > 1.12f || vpP.y < -0.12f || vpP.y > 1.12f)
			{
				break;
			}
			yield return new WaitForSeconds(0.5f);
		}
		ResetAnims();
	}

	public void ResetAnims()
	{
		StopCoroutine("StartActiveAnimation");
		if (!(AwayAnimation == null))
		{
			string text = AwayAnimation.name;
			Animation[] animations = _animations;
			foreach (Animation animation in animations)
			{
				animation.Rewind(text);
				animation.Sample();
				animation[text].enabled = false;
			}
		}
	}

	private void QualityLevelChanged(int oldLevel, int newLevel)
	{
		if (oldLevel <= requiredQualityLevel)
		{
			if (requiredQualityLevel < newLevel)
			{
				base.gameObject.SetActiveRecursively(false);
			}
		}
		else if (newLevel <= requiredQualityLevel)
		{
			base.gameObject.SetActiveRecursively(true);
		}
	}

	public void OnDrawGizmos()
	{
		Gizmos.DrawSphere(base.transform.TransformPoint(VisibleCenter), 0.3f);
	}
}
