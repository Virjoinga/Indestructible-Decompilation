using System;

namespace Glu.Plugins.AMiscUtils
{
	public static class ExceptionUtils
	{
		public static event EventHandler<ExceptionEventArgs> ExceptionRethrowing;

		public static event EventHandler<ExceptionEventArgs> ExceptionThrowing;

		public static event EventHandler<ExceptionEventArgs> ExceptionWrapping;

		public static event EventHandler<ExceptionEventArgs> ExceptionIgnored;

		public static void ArgumentNotNull<T>(this T arg, string name, object sender = null) where T : class
		{
			if (arg == null)
			{
				throw new ArgumentNullException(name).Throw(sender);
			}
		}

		public static Exception Throw(this Exception ex, object sender = null)
		{
			ExceptionUtils.ExceptionThrowing.Raise(sender, new ExceptionEventArgs(ex, ExceptionStatus.Throwing));
			return ex;
		}

		public static Exception Wrap(this Exception ex, object sender = null)
		{
			ExceptionUtils.ExceptionWrapping.Raise(sender, new ExceptionEventArgs(ex, ExceptionStatus.Ignored));
			return ex;
		}

		public static void Rethrow(this Exception ex, object sender = null)
		{
			ExceptionUtils.ExceptionRethrowing.Raise(sender, new ExceptionEventArgs(ex, ExceptionStatus.Rethrowing));
		}

		public static void Ignore(this Exception ex, object sender = null)
		{
			ExceptionUtils.ExceptionIgnored.Raise(sender, new ExceptionEventArgs(ex, ExceptionStatus.Ignored));
		}
	}
}
