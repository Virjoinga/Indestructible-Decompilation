namespace Glu.Localization.PortableObject
{
	public sealed class PortableObjectLogger : LoggerSingleton<PortableObjectLogger>
	{
		public PortableObjectLogger()
		{
			LoggerSingleton<PortableObjectLogger>.SetLoggerName("Package.Localization.PortableObject");
		}
	}
}
