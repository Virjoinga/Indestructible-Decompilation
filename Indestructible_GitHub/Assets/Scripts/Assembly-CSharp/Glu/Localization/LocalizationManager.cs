using System;
using System.IO;
using Glu.Localization.Internal;

namespace Glu.Localization
{
	public sealed class LocalizationManager : ILocaleListener
	{
		private StringManager stringManager;

		private Func<string, string, string, string> stringDecorator;

		private Func<string, string, string> untranslatedStringHandler;

		public IStreamGetter StreamGetter { get; set; }

		public string Locale { get; private set; }

		public Func<string, string, string, string> StringDecorator
		{
			get
			{
				return stringDecorator;
			}
			set
			{
				stringDecorator = value ?? new Func<string, string, string, string>(DefaultStringDecorator);
			}
		}

		public Func<string, string, string> UntranslatedStringHandler
		{
			get
			{
				return untranslatedStringHandler;
			}
			set
			{
				untranslatedStringHandler = value ?? new Func<string, string, string>(DefaultUntranslatedStringHandler);
			}
		}

		public LocalizationManager()
		{
			Locale = string.Empty;
			stringManager = new StringManager();
			StringDecorator = DefaultStringDecorator;
			UntranslatedStringHandler = DefaultUntranslatedStringHandler;
		}

		public static string GetFileName(string locale)
		{
			return string.Format("{0}.txt", locale);
		}

		public string GetString(string id)
		{
			string text = stringManager.GetString(id);
			if (text == null)
			{
				text = UntranslatedStringHandler(null, id);
			}
			return StringDecorator(null, id, text);
		}

		public string GetParticularString(string context, string id)
		{
			string text = stringManager.GetParticularString(context, id);
			if (text == null)
			{
				text = UntranslatedStringHandler(context, id);
			}
			return StringDecorator(context, id, text);
		}

		public bool Load(string locale)
		{
			string fileName = GetFileName(locale);
			if (LoadStrings(stringManager, StreamGetter, fileName))
			{
				Locale = locale;
				return true;
			}
			if (locale != "en")
			{
				fileName = GetFileName("en");
				if (LoadStrings(stringManager, StreamGetter, fileName))
				{
					Locale = locale;
					return true;
				}
			}
			return false;
		}

		public void ResetStrings()
		{
			stringManager.Reset();
			Locale = string.Empty;
		}

		public void HandleLocaleChanged(string locale)
		{
			if (!Load(locale))
			{
			}
		}

		private static bool LoadStrings(StringManager stringManager, IStreamGetter streamGetter, string name)
		{
			using (Stream stream = streamGetter.GetStream(name))
			{
				if (stream != null)
				{
					return stringManager.Load(stream);
				}
			}
			return false;
		}

		private static string DefaultStringDecorator(string context, string id, string str)
		{
			return str;
		}

		private static string DefaultUntranslatedStringHandler(string context, string id)
		{
			string text = "ID \"{0}\" is not found";
			if (context != null)
			{
				text = "ID \"{0}\" with context \"{1}\" is not found";
			}
			return id;
		}
	}
}
