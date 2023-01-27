using System;
using System.ComponentModel;
using System.Xml;

public static class XmlUtils
{
	public static T GetAttribute<T>(XmlNode element, string name)
	{
		return GetAttribute(element, name, default(T));
	}

	public static T GetAttribute<T>(XmlNode element, string name, T value)
	{
		//Discarded unreachable code: IL_0042
		if (element != null)
		{
			XmlNode namedItem = element.Attributes.GetNamedItem(name);
			if (namedItem == null)
			{
				return value;
			}
			try
			{
				TypeConverter converter = TypeDescriptor.GetConverter(typeof(T));
				return (T)converter.ConvertFromString(namedItem.Value);
			}
			catch (Exception)
			{
				return value;
			}
		}
		return value;
	}

	public static void SetAttribute(XmlElement element, string name, object value)
	{
		if (element != null && value != null && name != null)
		{
			element.SetAttribute(name, value.ToString());
		}
	}
}
