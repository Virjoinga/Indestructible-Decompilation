using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using UnityEngine;

namespace Glu.Localization
{
	[Serializable]
	public class LocalizedString : IXmlSerializable
	{
		[SerializeField]
		private string id;

		[SerializeField]
		private string context;

		[SerializeField]
		private string comment;

		public string Context
		{
			get
			{
				return StringOrNull(context);
			}
			set
			{
				context = value;
			}
		}

		public string Id
		{
			get
			{
				return StringOrNull(id);
			}
			set
			{
				id = value;
			}
		}

		public string Comment
		{
			get
			{
				return StringOrNull(comment);
			}
			set
			{
				comment = value;
			}
		}

		public LocalizedString()
		{
			context = null;
			id = null;
		}

		public LocalizedString(string id)
		{
			context = null;
			this.id = id;
		}

		public LocalizedString(string context, string id)
		{
			this.context = context;
			this.id = id;
		}

		public override string ToString()
		{
			if (Context == null)
			{
				return Strings.GetString(Id);
			}
			return Strings.GetParticularString(Context, Id);
		}

		private static string StringOrNull(string s)
		{
			return (!(s != string.Empty)) ? null : s;
		}

		public void ReadXml(XmlReader reader)
		{
			string text = null;
			string text2 = null;
			string text3 = null;
			if (reader.MoveToFirstAttribute())
			{
				do
				{
					if (reader.Name == "context")
					{
						text = text ?? reader.Value;
					}
					else if (reader.Name == "id")
					{
						text2 = text2 ?? reader.Value;
					}
					else if (reader.Name == "comment")
					{
						text3 = text3 ?? reader.Value;
					}
				}
				while (reader.MoveToNextAttribute());
			}
			bool isEmptyElement = reader.IsEmptyElement;
			reader.ReadStartElement();
			if (isEmptyElement)
			{
				return;
			}
			reader.MoveToContent();
			if (reader.NodeType == XmlNodeType.Text)
			{
				text2 = text2 ?? reader.Value;
				reader.Read();
			}
			else
			{
				while (reader.NodeType == XmlNodeType.Element)
				{
					if (reader.Name == "context")
					{
						string text4 = reader.ReadElementContentAsString();
						text = text ?? text4;
					}
					else if (reader.Name == "id")
					{
						string text5 = reader.ReadElementContentAsString();
						text2 = text2 ?? text5;
					}
					else if (reader.Name == "comment")
					{
						string text6 = reader.ReadElementContentAsString();
						text3 = text3 ?? text6;
					}
					else
					{
						reader.Skip();
					}
					reader.MoveToContent();
				}
			}
			reader.ReadEndElement();
			Context = text;
			Id = text2;
			Comment = text3;
		}

		public void WriteXml(XmlWriter writer)
		{
			if (!string.IsNullOrEmpty(Context))
			{
				writer.WriteElementString("context", Context);
			}
			if (!string.IsNullOrEmpty(Id))
			{
				writer.WriteElementString("id", Id);
			}
			if (!string.IsNullOrEmpty(Comment))
			{
				writer.WriteElementString("comment", Comment);
			}
		}

		public XmlSchema GetSchema()
		{
			return null;
		}

		public static implicit operator string(LocalizedString s)
		{
			return (s == null) ? string.Empty : s.ToString();
		}
	}
}
