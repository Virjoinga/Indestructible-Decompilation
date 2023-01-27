public class BootLogger : LoggerSingleton<BootLogger>
{
	public BootLogger()
	{
		LoggerSingleton<BootLogger>.SetLoggerName("App.Boot");
		LoggerSingleton<BootLogger>.SetLevel(10);
	}
}
