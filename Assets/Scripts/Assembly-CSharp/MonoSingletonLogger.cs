public class MonoSingletonLogger : LoggerSingleton<MonoSingletonLogger>
{
	public MonoSingletonLogger()
	{
		LoggerSingleton<MonoSingletonLogger>.SetLoggerName("App.MonoSingleton");
	}
}
