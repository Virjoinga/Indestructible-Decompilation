using System.Collections.Generic;
using UnityEngine;

namespace Glu.Localization.Utils
{
	public static class LanguageTable
	{
		public const string English = "en";

		private static IDictionary<SystemLanguage, string> dictLanguageToIso639;

		private static IDictionary<string, SystemLanguage> dictIso639ToLanguage;

		public static string GetIso639FromLanguage(SystemLanguage language)
		{
			InitializeLanguageTables();
			string value;
			if (dictLanguageToIso639.TryGetValue(language, out value))
			{
				return value;
			}
			return string.Empty;
		}

		public static SystemLanguage GetLanguageFromIso639(string locale)
		{
			InitializeLanguageTables();
			SystemLanguage value;
			if (locale != null && dictIso639ToLanguage.TryGetValue(locale, out value))
			{
				return value;
			}
			return SystemLanguage.Unknown;
		}

		private static void InitializeLanguageTables()
		{
			if (dictLanguageToIso639 != null && dictIso639ToLanguage != null)
			{
				return;
			}
			Dictionary<string, SystemLanguage> dictionary = new Dictionary<string, SystemLanguage>();
			dictionary["af"] = SystemLanguage.Afrikaans;
			dictionary["ar"] = SystemLanguage.Arabic;
			dictionary["eu"] = SystemLanguage.Basque;
			dictionary["be"] = SystemLanguage.Belarusian;
			dictionary["bg"] = SystemLanguage.Bulgarian;
			dictionary["ca"] = SystemLanguage.Catalan;
			dictionary["zh"] = SystemLanguage.Chinese;
			dictionary["cs"] = SystemLanguage.Czech;
			dictionary["da"] = SystemLanguage.Danish;
			dictionary["nl"] = SystemLanguage.Dutch;
			dictionary["en"] = SystemLanguage.English;
			dictionary["et"] = SystemLanguage.Estonian;
			dictionary["fo"] = SystemLanguage.Faroese;
			dictionary["fi"] = SystemLanguage.Finnish;
			dictionary["fr"] = SystemLanguage.French;
			dictionary["de"] = SystemLanguage.German;
			dictionary["el"] = SystemLanguage.Greek;
			dictionary["he"] = SystemLanguage.Hebrew;
			dictionary["hu"] = SystemLanguage.Hugarian;
			dictionary["is"] = SystemLanguage.Icelandic;
			dictionary["id"] = SystemLanguage.Indonesian;
			dictionary["it"] = SystemLanguage.Italian;
			dictionary["ja"] = SystemLanguage.Japanese;
			dictionary["ko"] = SystemLanguage.Korean;
			dictionary["lv"] = SystemLanguage.Latvian;
			dictionary["lt"] = SystemLanguage.Lithuanian;
			dictionary["no"] = SystemLanguage.Norwegian;
			dictionary["pl"] = SystemLanguage.Polish;
			dictionary["pt"] = SystemLanguage.Portuguese;
			dictionary["ro"] = SystemLanguage.Romanian;
			dictionary["ru"] = SystemLanguage.Russian;
			dictionary["sr"] = SystemLanguage.SerboCroatian;
			dictionary["sk"] = SystemLanguage.Slovak;
			dictionary["sl"] = SystemLanguage.Slovenian;
			dictionary["es"] = SystemLanguage.Spanish;
			dictionary["sv"] = SystemLanguage.Swedish;
			dictionary["th"] = SystemLanguage.Thai;
			dictionary["tr"] = SystemLanguage.Turkish;
			dictionary["uk"] = SystemLanguage.Ukrainian;
			dictionary["vi"] = SystemLanguage.Vietnamese;
			Dictionary<SystemLanguage, string> dictionary2 = new Dictionary<SystemLanguage, string>();
			foreach (KeyValuePair<string, SystemLanguage> item in dictionary)
			{
				dictionary2[item.Value] = item.Key;
			}
			dictionary2[SystemLanguage.Unknown] = string.Empty;
			dictIso639ToLanguage = dictionary;
			dictLanguageToIso639 = dictionary2;
		}
	}
}
