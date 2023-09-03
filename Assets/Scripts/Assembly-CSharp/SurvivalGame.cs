public class SurvivalGame : SingleGame
{
	public delegate void WaveCompleteDelegate(int waveIndex, int rewardSC, int rewardXP);

	private int _rewardSC;

	private int _rewardXP;

	private int _lastCompleteWaveIndex = -1;

	public int lastCompleteWaveIndex
	{
		get
		{
			return _lastCompleteWaveIndex;
		}
	}

	public event WaveCompleteDelegate waveCompleteEvent;

	public void WaveComplete(int waveIndex, int rewardSC, int rewardXP)
	{
		_rewardSC += rewardSC;
		_rewardXP += rewardXP;
		_lastCompleteWaveIndex = waveIndex;
		if (this.waveCompleteEvent != null)
		{
			this.waveCompleteEvent((!base.IsBossFight) ? waveIndex : (-1), rewardSC, rewardXP);
		}
		if (base.IsBossFight)
		{
			GameOver(true);
		}
	}

	protected override void CalculateReward(bool hasWon, ref Reward reward)
	{
		base.CalculateReward(hasWon, ref reward);
		if (base.IsBossFight)
		{
			SetBossFightReward(hasWon, ref reward);
			return;
		}
		reward.MoneySoft = _rewardSC;
		reward.ExperiencePoints = _rewardXP;
	}
}
