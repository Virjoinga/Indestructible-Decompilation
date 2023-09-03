using System;
using System.IO;
using System.Text;

namespace Glu.Localization.PortableObject
{
	public sealed class Lexer
	{
		public const int TokenInvalid = -2;

		public const int TokenEndOfFile = -1;

		public const int TokenEndOfLine = 10;

		public const int TokenIdentifier = 256;

		public const int TokenNumber = 257;

		public const int TokenString = 258;

		public const int TokenMsgCtxt = 259;

		public const int TokenMsgId = 260;

		public const int TokenMsgStr = 261;

		public const int TokenComment = 262;

		private TextReader textReader;

		private int currentChar;

		private int line;

		private int column;

		private int numberValue;

		private string stringValue;

		private StringBuilder stringBuilder;

		public int Line
		{
			get
			{
				return line;
			}
		}

		public int Column
		{
			get
			{
				return column;
			}
		}

		public int Number
		{
			get
			{
				return numberValue;
			}
		}

		public string String
		{
			get
			{
				return stringValue;
			}
		}

		public Lexer(Stream stream)
		{
			if (stream == null)
			{
				throw new ArgumentNullException("stream");
			}
			line = 1;
			column = 0;
			textReader = null;
			currentChar = -2;
			numberValue = 0;
			stringValue = null;
			stringBuilder = new StringBuilder();
			textReader = new StreamReader(stream, Encoding.UTF8);
			ReadChar();
		}

		public int ReadToken()
		{
			while (true)
			{
				switch (currentChar)
				{
				case -1:
					return -1;
				case 9:
				case 32:
					break;
				case 10:
				case 13:
				{
					int num4 = column;
					int num5 = currentChar;
					int num6 = ReadChar();
					if ((num6 == 10 || num6 == 13) && num5 != num6)
					{
						ReadChar();
					}
					line++;
					column = 1;
					return 10;
				}
				case 35:
					stringValue = null;
					return ReadComment();
				case 34:
				{
					int num3 = column;
					stringValue = null;
					return ReadString();
				}
				default:
				{
					char c = (char)currentChar;
					int num = column;
					if (char.IsDigit(c))
					{
						return ReadNumber();
					}
					if (char.IsLetter(c))
					{
						int num2 = ReadIdentifier();
						if (LoggerSingleton<PortableObjectLogger>.IsEnabledFor(10))
						{
							switch (num2)
							{
							}
						}
						return num2;
					}
					if (c >= '\u0080')
					{
						return -2;
					}
					return currentChar;
				}
				}
				ReadChar();
			}
		}

		private int ReadString()
		{
			StringBuilder emptyStringBuilder = GetEmptyStringBuilder();
			while (true)
			{
				int num = ReadChar();
				switch (num)
				{
				case 34:
					ReadChar();
					stringValue = emptyStringBuilder.ToString();
					return 258;
				case 10:
				case 13:
					stringValue = string.Format("Invalid token EOL in string \"{0}\"", emptyStringBuilder.ToString());
					return -2;
				case -1:
					stringValue = string.Format("Invalid token EOF in string \"{0}\"", emptyStringBuilder.ToString());
					return -2;
				case 92:
					num = ReadChar();
					if (!IsEol(num))
					{
						switch (num)
						{
						case 110:
							num = 10;
							break;
						case 114:
							num = 13;
							break;
						case 116:
							num = 9;
							break;
						}
						emptyStringBuilder.Append((char)num);
					}
					break;
				default:
					emptyStringBuilder.Append((char)num);
					break;
				}
			}
		}

		private int ReadNumber()
		{
			StringBuilder emptyStringBuilder = GetEmptyStringBuilder();
			char value = (char)currentChar;
			emptyStringBuilder.Append(value);
			bool flag = false;
			ReadChar();
			while (currentChar >= 0)
			{
				value = (char)currentChar;
				if (char.IsLetter(value) || value == '.')
				{
					flag = true;
				}
				else if (!char.IsDigit(value))
				{
					break;
				}
				emptyStringBuilder.Append(value);
				ReadChar();
			}
			if (flag)
			{
				stringValue = string.Format("Invalid token {0}", emptyStringBuilder.ToString());
				return -2;
			}
			int.TryParse(emptyStringBuilder.ToString(), out numberValue);
			return 257;
		}

		private int ReadIdentifier()
		{
			StringBuilder emptyStringBuilder = GetEmptyStringBuilder();
			char value = (char)currentChar;
			emptyStringBuilder.Append(value);
			ReadChar();
			while (currentChar >= 0)
			{
				value = (char)currentChar;
				if (char.IsLetterOrDigit(value))
				{
					emptyStringBuilder.Append(value);
					ReadChar();
					continue;
				}
				break;
			}
			string text = emptyStringBuilder.ToString();
			switch (text)
			{
			case "msgctxt":
				return 259;
			case "msgid":
				return 260;
			case "msgstr":
				return 261;
			default:
				stringValue = text;
				return 256;
			}
		}

		private int ReadComment()
		{
			StringBuilder emptyStringBuilder = GetEmptyStringBuilder();
			int num = ReadChar();
			while (!IsEol(num))
			{
				emptyStringBuilder.Append((char)num);
				num = ReadChar();
			}
			stringValue = emptyStringBuilder.ToString();
			return 262;
		}

		private int ReadChar()
		{
			currentChar = textReader.Read();
			column++;
			return currentChar;
		}

		private StringBuilder GetEmptyStringBuilder()
		{
			stringBuilder.Remove(0, stringBuilder.Length);
			return stringBuilder;
		}

		private static bool IsEol(int t)
		{
			return t == -1 || t == 10 || t == 13;
		}
	}
}
