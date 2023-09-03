namespace Glu.Localization.Utils
{
	public sealed class LocalizationLogger : LoggerSingleton<LocalizationLogger>
	{
		public LocalizationLogger()
		{
			LoggerSingleton<LocalizationLogger>.SetLoggerName("Package.Localization");
		}
	}
}
