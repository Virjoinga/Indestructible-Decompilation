using Glu;
using UnityEngine;

public class Weapon : Glu.MonoBehaviour, INetworkWeapon
{
	public delegate void MakeDamageDelegate(Destructible destructible, float damage);

	public LayerMask damageLayerMask;

	public float baseDamage = 20f;

	public DamageType weaponDamageType;

	private Collider _mainOwnerCollider;

	private int _damageLayers;

	private float _damage;

	private MatchPlayer _player;

	protected static Destructible[] _destructiblePool = new Destructible[16];

	public Collider mainOwnerCollider
	{
		get
		{
			return _mainOwnerCollider;
		}
	}

	public int damageLayers
	{
		get
		{
			return _damageLayers;
		}
	}

	public MatchPlayer player
	{
		get
		{
			return _player;
		}
	}

	public DamageType damageType
	{
		get
		{
			return weaponDamageType;
		}
	}

	public event MakeDamageDelegate makeDamageEvent;

	protected virtual void Awake()
	{
		if (_damageLayers == 0)
		{
			SetDamageLayers(damageLayerMask.value);
		}
		_damage = baseDamage;
	}

	public virtual float GetBaseDamage()
	{
		return baseDamage;
	}

	public float GetDamage()
	{
		return _damage;
	}

	public void SetDamage(float newDamage)
	{
		_damage = newDamage;
	}

	protected void SetMainOwnerCollider(Collider value)
	{
		_mainOwnerCollider = value;
	}

	public virtual void SetDamageLayers(int value)
	{
		_damageLayers = value;
	}

	public virtual void SetPlayer(MatchPlayer value)
	{
		_player = value;
	}

	public static DamageType GetBaseDamageType(DamageType value)
	{
		return value & (DamageType)(-17);
	}

	public static bool IsSecondaryDamageType(DamageType value)
	{
		return (value & DamageType.SecondaryGeneric) != 0;
	}

	public virtual GameObject GetGameObject(Collider collider)
	{
		return collider.transform.root.gameObject;
	}

	public virtual Destructible GetDestructible(Collider collider)
	{
		return collider.transform.root.GetComponent<Destructible>();
	}

	public virtual Destructible GetDestructible(Collider collider, GameObject gameObject)
	{
		return gameObject.GetComponent<Destructible>();
	}

	public bool IsFoe(Collider collider)
	{
		return collider != _mainOwnerCollider;
	}

	protected virtual bool CheckFoeDamageAbilty(Collider collider, out Destructible destructible)
	{
		if (((1 << collider.gameObject.layer) & damageLayers) != 0)
		{
			destructible = GetDestructible(collider);
			if (destructible != null && destructible.isMine)
			{
				return true;
			}
		}
		destructible = null;
		return false;
	}

	protected virtual bool CheckDamageAbilty(Collider collider, out Destructible destructible)
	{
		if (IsFoe(collider))
		{
			return CheckFoeDamageAbilty(collider, out destructible);
		}
		destructible = null;
		return false;
	}

	protected bool HasFoeCollider(Collider[] colliders, out Collider foeCollider)
	{
		int i = 0;
		for (int num = colliders.Length; i != num; i++)
		{
			Collider collider = colliders[i];
			if (IsFoe(collider))
			{
				foeCollider = collider;
				return true;
			}
		}
		foeCollider = null;
		return false;
	}

	protected Collider CheckColliders(Collider[] colliders, out Destructible destructible)
	{
		Collider result = null;
		int i = 0;
		for (int num = colliders.Length; i != num; i++)
		{
			Collider collider = colliders[i];
			if (IsFoe(collider))
			{
				if (CheckFoeDamageAbilty(collider, out destructible))
				{
					return collider;
				}
				result = collider;
			}
		}
		destructible = null;
		return result;
	}

	protected int FilterColliders(Collider[] colliders, out Destructible[] destructibles)
	{
		destructibles = _destructiblePool;
		int num = colliders.Length;
		if (num != 0)
		{
			int num2 = num;
			if (_destructiblePool.Length < num)
			{
				_destructiblePool = new Destructible[num];
			}
			int num3 = 0;
			do
			{
				Collider collider = colliders[num3];
				Destructible destructible;
				if (!IsFoe(collider))
				{
					colliders[num3] = colliders[--num2];
					colliders[num2] = null;
					num--;
				}
				else if (CheckFoeDamageAbilty(collider, out destructible))
				{
					_destructiblePool[num3++] = destructible;
				}
				else
				{
					colliders[num3] = colliders[--num];
					colliders[num] = collider;
				}
			}
			while (num3 != num);
		}
		return num;
	}

	protected DamageResult Damage(Destructible destructible)
	{
		return Damage(destructible, GetDamage());
	}

	protected virtual DamageResult Damage(Destructible destructible, float damage)
	{
		DamageResult damageResult = destructible.Damage(damage, this);
		if (damageResult != 0 && this.makeDamageEvent != null)
		{
			this.makeDamageEvent(destructible, damage);
		}
		return damageResult;
	}

	protected virtual int Damage(Vector3 position, float radius, float constDamageFactor, float sqrDamageFactor, float explosionForce)
	{
		Collider[] array = Physics.OverlapSphere(position, radius, damageLayers);
		Destructible[] destructibles;
		int num = FilterColliders(array, out destructibles);
		if (num != 0)
		{
			float num2 = GetDamage() * (constDamageFactor + sqrDamageFactor);
			float num3 = radius * radius;
			float num4 = GetDamage() * sqrDamageFactor / num3;
			int num5 = 0;
			do
			{
				float sqrMagnitude = (array[num5].ClosestPointOnBounds(position) - position).sqrMagnitude;
				if (!(sqrMagnitude < num3))
				{
					continue;
				}
				Destructible destructible = destructibles[num5];
				Damage(destructible, num2 - num4 * sqrMagnitude);
				if (0f < explosionForce)
				{
					Rigidbody rigidbody = destructible.rigidbody;
					if (rigidbody != null)
					{
						rigidbody.AddExplosionForce(explosionForce, position, radius);
					}
				}
			}
			while (++num5 != num);
		}
		return num;
	}
}
