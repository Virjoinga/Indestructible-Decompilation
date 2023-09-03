using System.Collections.Generic;
using System.IO;
using System.Linq;
using Glu.Localization.PortableObject;
using Glu.Localization.Utils;

namespace Glu.Localization.Internal
{
	public sealed class StringManager
	{
		private Dictionary<string, string> strings;

		private Dictionary<MessageKey, string> contextStrings;

		public StringManager()
		{
			strings = new Dictionary<string, string>();
			contextStrings = new Dictionary<MessageKey, string>();
		}

		public string GetString(string id)
		{
			string value;
			if (id != null && strings.TryGetValue(id, out value))
			{
				return value;
			}
			return null;
		}

		public string GetParticularString(string context, string id)
		{
			string value;
			contextStrings.TryGetValue(new MessageKey(context, id), out value);
			return value;
		}

		public void Reset()
		{
			strings.Clear();
			contextStrings.Clear();
		}

		public bool Load(Stream stream)
		{
			IEnumerable<Entry> enumerable;
			try
			{
				IEnumerable<Entry> source = Parser.Parse(stream);
				enumerable = source.ToList();
			}
			catch (InvalidDataException)
			{
				return false;
			}
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			Dictionary<MessageKey, string> dictionary2 = new Dictionary<MessageKey, string>();
			foreach (Entry item in enumerable)
			{
				if (item.Context == null)
				{
					string value;
					if (!LoggerSingleton<LocalizationLogger>.IsEnabledFor(30) || !dictionary.TryGetValue(item.Id, out value) || value != item.String)
					{
					}
					dictionary[item.Id] = item.String;
					continue;
				}
				MessageKey key = new MessageKey(item.Context, item.Id);
				string value2;
				if (!LoggerSingleton<LocalizationLogger>.IsEnabledFor(30) || !dictionary2.TryGetValue(key, out value2) || value2 != item.String)
				{
				}
				dictionary2[key] = item.String;
			}
			strings = dictionary;
			contextStrings = dictionary2;
			return true;
		}
	}
}
