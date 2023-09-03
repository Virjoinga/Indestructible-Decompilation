using System;
using System.Collections.Generic;
using Glu.Localization;

public static class PortableObject
{
	[Obsolete("Use Glu.Localization.Entry")]
	public class Entry : Glu.Localization.Entry
	{
		public Message Message
		{
			get
			{
				return new Message(this);
			}
		}
	}

	[Obsolete("Access Context, Id and String directly")]
	public class Message
	{
		private Entry entry;

		public string Context
		{
			get
			{
				return entry.Context;
			}
			set
			{
				entry.Context = value;
			}
		}

		public string ID
		{
			get
			{
				return entry.Id;
			}
			set
			{
				entry.Id = value;
			}
		}

		public string String
		{
			get
			{
				return entry.String;
			}
			set
			{
				entry.String = value;
			}
		}

		internal Message(Entry entry)
		{
			this.entry = entry;
		}
	}

	[Obsolete("Change method to IEnumerable<Entry> CollectEntries() or IEnumerable<Entry> CollectEntriesFromObject(UnityEngine.Object obj, GeneratorFromAssetsData data)")]
	public sealed class Writer
	{
		public IList<Glu.Localization.Entry> Entries { get; private set; }

		public Writer()
		{
			Entries = new List<Glu.Localization.Entry>();
		}

		public bool WriteEntry(ref Entry entry)
		{
			if (entry == null || !entry.IsValid)
			{
				return false;
			}
			Entries.Add(entry.Clone());
			return true;
		}
	}
}
