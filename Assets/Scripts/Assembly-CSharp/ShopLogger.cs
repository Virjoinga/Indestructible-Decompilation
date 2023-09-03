public class ShopLogger : LoggerSingleton<ShopLogger>
{
	public ShopLogger()
	{
		LoggerSingleton<ShopLogger>.SetLoggerName("App.Shop");
	}
}
