namespace Glu
{
	public class ChartBoost
	{
		private static bool isInitialized;

		private static void ChartBoost_initialize(string appId, string appSignature)
		{
		}

		public static void Init(string appId, string appSignature)
		{
			if (!isInitialized)
			{
				ChartBoost_initialize(appId, appSignature);
				isInitialized = true;
			}
		}
	}
}
