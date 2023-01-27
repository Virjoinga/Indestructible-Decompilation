using System.Xml;

public class DC_KillWithEachWeapon : DailyChallenges.DailyChallenge
{
	private MultiplayerGame _mpGame;

	private int _targetDamageTypeBits;

	private int _usedDamageTypeBits;

	public DC_KillWithEachWeapon(string id)
		: base(id)
	{
		_goal = 3;
		_targetDamageTypeBits = 22;
	}

	public override void OnGameStarted()
	{
		base.OnGameStarted();
		_mpGame = IDTGame.Instance as MultiplayerGame;
		if (_mpGame != null)
		{
			_mpGame.playerKillEnemyEvent += OnPlayerKillEnemy;
		}
	}

	private void OnPlayerKillEnemy(GamePlayer player, GamePlayer enemy, DamageType damageType)
	{
		if (player == _mpGame.localPlayer && enemy != _mpGame.localPlayer)
		{
			int num = 1 << (int)Weapon.GetBaseDamageType(damageType);
			if ((_targetDamageTypeBits & num) != 0 && (_usedDamageTypeBits & num) == 0)
			{
				_value++;
			}
			_usedDamageTypeBits |= num;
		}
	}

	public override void SaveXml(XmlDocument document, XmlElement root)
	{
		base.SaveXml(document, root);
		XmlUtils.SetAttribute(root, "usedDamageTypeBits", _usedDamageTypeBits);
	}

	public override void LoadXml(XmlElement root)
	{
		base.LoadXml(root);
		_usedDamageTypeBits = XmlUtils.GetAttribute<int>(root, "usedDamageTypeBits");
	}
}
