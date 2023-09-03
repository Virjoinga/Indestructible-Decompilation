using UnityEngine;

public abstract class BuffConf : ScriptableObject
{
	public float Duration = 5f;

	public abstract Buff CreateBuff();
}
