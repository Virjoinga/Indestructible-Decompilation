using System;
using System.Diagnostics;
using Glu.Localization;

[Obsolete("Use Glu.Localization.Strings")]
public static class Localization
{
	[Serializable]
	[Obsolete("Use Glu.Localization.Strings")]
	public class String : LocalizedString
	{
		[Obsolete("Use Id")]
		public string ID
		{
			get
			{
				return base.Id;
			}
		}

		public String()
		{
		}

		public String(string id)
			: base(id)
		{
		}

		public String(string context, string id)
			: base(context, id)
		{
		}
	}

	public static string Locale
	{
		get
		{
			return Strings.Locale;
		}
		set
		{
			Strings.Locale = value;
		}
	}

	public static LocalizationManager Manager
	{
		get
		{
			return Strings.Manager;
		}
		set
		{
			Strings.Manager = value;
		}
	}

	public static string GetString(string id)
	{
		return Strings.GetString(id);
	}

	public static string GetParticularString(string context, string id)
	{
		return Strings.GetParticularString(context, id);
	}

	public static string GetStringFormat(string id, params object[] args)
	{
		return Strings.GetStringFormat(id, args);
	}

	public static string GetParticularStringFormat(string context, string id, params object[] args)
	{
		return Strings.GetParticularStringFormat(context, id, args);
	}

	[Conditional("o_O")]
	public static void MarkString(string id)
	{
	}

	[Conditional("o_O")]
	public static void MarkParticularString(string context, string id)
	{
	}

	public static void Initialize()
	{
		Strings.Initialize();
	}

	public static void Destroy()
	{
		Strings.Destroy();
	}

	public static void RegisterLocaleListener(ILocaleListener listener)
	{
		Strings.RegisterLocaleListener(listener);
	}

	public static void UnregisterLocaleListener(ILocaleListener listener)
	{
		Strings.UnregisterLocaleListener(listener);
	}
}
