using System;

namespace Glu.Localization.PortableObject
{
	public static class PortableObjectUtils
	{
		public static bool IsHeader(this Entry entry)
		{
			if (entry == null)
			{
				throw new ArgumentNullException("entry");
			}
			if (entry.IsValid && entry.Id == string.Empty && entry.Context == null)
			{
				return true;
			}
			return false;
		}
	}
}
