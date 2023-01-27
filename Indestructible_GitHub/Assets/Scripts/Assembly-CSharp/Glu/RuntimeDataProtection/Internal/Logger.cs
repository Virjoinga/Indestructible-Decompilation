namespace Glu.RuntimeDataProtection.Internal
{
	internal sealed class Logger : LoggerSingleton<Logger>
	{
		public Logger()
		{
			LoggerSingleton<Logger>.SetLoggerName("Package.RuntimeDataProtection");
		}
	}
}
