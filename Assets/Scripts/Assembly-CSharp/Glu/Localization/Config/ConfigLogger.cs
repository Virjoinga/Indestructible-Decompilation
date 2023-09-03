namespace Glu.Localization.Config
{
	public sealed class ConfigLogger : LoggerSingleton<ConfigLogger>
	{
		public ConfigLogger()
		{
			LoggerSingleton<ConfigLogger>.SetLoggerName("Package.Localization.Config");
		}
	}
}
