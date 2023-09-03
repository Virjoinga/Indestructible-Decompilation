using System;

namespace Glu.Localization
{
	public class Entry : ICloneable
	{
		public string TranslatorComment { get; set; }

		public string ExtractedComment { get; set; }

		public string References { get; set; }

		public string Context { get; set; }

		public string Id { get; set; }

		public string String { get; set; }

		public bool IsValid
		{
			get
			{
				if (Id == null || String == null)
				{
					return false;
				}
				if (Id == string.Empty && Context != null)
				{
					return false;
				}
				return true;
			}
		}

		public Entry()
		{
		}

		private Entry(Entry other)
		{
			TranslatorComment = other.TranslatorComment;
			ExtractedComment = other.ExtractedComment;
			References = other.References;
			Context = other.Context;
			Id = other.Id;
			String = other.String;
		}

		object ICloneable.Clone()
		{
			return Clone();
		}

		public Entry Clone()
		{
			return new Entry(this);
		}

		public override string ToString()
		{
			return string.Format("(Context = '{0}', Id = '{1}', String = '{2}')", Context ?? "null", Id ?? "null", String ?? "null");
		}
	}
}
