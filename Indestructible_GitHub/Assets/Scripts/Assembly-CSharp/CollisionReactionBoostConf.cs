public class CollisionReactionBoostConf : BuffConf
{
	public bool DamageAbsoluteValue = true;

	public float DamageBoost;

	public float FragilityBoost;

	public override Buff CreateBuff()
	{
		return new CollisionReactionBoostBuff(this);
	}
}
