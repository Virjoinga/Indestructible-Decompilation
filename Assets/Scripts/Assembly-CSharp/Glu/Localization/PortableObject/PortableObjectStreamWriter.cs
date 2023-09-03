using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Glu.Localization.PortableObject
{
	public sealed class PortableObjectStreamWriter : IDisposable
	{
		private TextWriter textWriter;

		public PortableObjectStreamWriter(Stream stream)
		{
			textWriter = new StreamWriter(stream, new UTF8Encoding(false));
			Entry entry = new Entry
			{
				Id = string.Empty,
				String = "Content-Type: text/plain; charset=UTF-8\n"
			};
			DoWrite(entry);
		}

		public void Flush()
		{
			textWriter.Flush();
		}

		public void Close()
		{
			textWriter.Close();
		}

		public void Write(Entry entry)
		{
			if (entry == null)
			{
				throw new ArgumentNullException("entry");
			}
			if (!entry.IsHeader())
			{
				if (!entry.IsValid)
				{
					string message = string.Format("Entry {0} is invalid", entry);
					throw new ArgumentException(message, "entry");
				}
				DoWrite(entry);
			}
		}

		public void Write(IEnumerable<Entry> entries)
		{
			if (entries == null)
			{
				throw new ArgumentNullException("entries");
			}
			foreach (Entry entry in entries)
			{
				Write(entry);
			}
		}

		public void Dispose()
		{
			textWriter.Dispose();
		}

		private void DoWrite(Entry entry)
		{
			WriteComment(string.Empty, entry.TranslatorComment);
			WriteComment(".", entry.ExtractedComment);
			WriteComment(":", entry.References);
			WriteElement("msgctxt", entry.Context);
			WriteElement("msgid", entry.Id);
			WriteElement("msgstr", entry.String);
			textWriter.WriteLine();
		}

		private void WriteComment(string prefix, string s)
		{
			if (s != null)
			{
				int num = 0;
				int num2;
				do
				{
					num2 = s.IndexOf('\n', num);
					textWriter.Write('#');
					textWriter.Write(prefix);
					textWriter.Write(' ');
					string value = ((num2 < 0) ? s.Substring(num) : s.Substring(num, num2 - num));
					textWriter.WriteLine(value);
					num = num2 + 1;
				}
				while (num2 >= 0);
			}
		}

		private void WriteElement(string identifier, string s)
		{
			if (s == null)
			{
				return;
			}
			textWriter.Write(identifier);
			textWriter.Write(' ');
			IEnumerable<string> enumerable = SplitLines(s);
			int num = enumerable.Count();
			if (num != 1)
			{
				WriteString(string.Empty);
				textWriter.WriteLine();
			}
			foreach (string item in enumerable)
			{
				WriteString(item);
				textWriter.WriteLine();
			}
		}

		private static IEnumerable<string> SplitLines(string s)
		{
			int prev = 0;
			while (true)
			{
				int pos2 = s.IndexOf('\n', prev);
				if (pos2 == -1)
				{
					break;
				}
				pos2++;
				yield return s.Substring(prev, pos2 - prev);
				prev = pos2;
			}
			if (prev < s.Length)
			{
				yield return s.Substring(prev);
			}
		}

		private void WriteString(string s)
		{
			textWriter.Write('"');
			int length = s.Length;
			for (int i = 0; i < length; i++)
			{
				char c = s[i];
				switch (c)
				{
				case '\n':
					textWriter.Write("\\n");
					break;
				case '\r':
					textWriter.Write("\\r");
					break;
				case '\t':
					textWriter.Write("\\t");
					break;
				case '"':
					textWriter.Write("\\\"");
					break;
				case '\\':
					textWriter.Write("\\\\");
					break;
				default:
					textWriter.Write(c);
					break;
				}
			}
			textWriter.Write('"');
		}
	}
}
