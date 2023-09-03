using UnityEngine;

public class GarageInfluencePoints : MonoBehaviour
{
	public SpriteText PointsLabel;

	public SpriteText EloRateLabel;

	private int _cachedPoints = -1;

	private void Awake()
	{
		EloRateLabel.Text = string.Empty;
	}

	private void Update()
	{
		if (_cachedPoints != (int)MonoSingleton<Player>.Instance.InfluencePoints)
		{
			_cachedPoints = MonoSingleton<Player>.Instance.InfluencePoints;
			PointsLabel.Text = NumberFormat.Get(_cachedPoints);
		}
	}
}
