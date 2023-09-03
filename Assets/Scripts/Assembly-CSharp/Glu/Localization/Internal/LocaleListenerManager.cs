using System;
using System.Collections.Generic;

namespace Glu.Localization.Internal
{
	public sealed class LocaleListenerManager
	{
		private IList<WeakReference> listeners;

		public LocaleListenerManager()
		{
			listeners = new List<WeakReference>();
		}

		public void RegisterListener(ILocaleListener listener)
		{
			if (listener != null && FindListener(listener) < 0)
			{
				listeners.Add(new WeakReference(listener));
			}
		}

		public void UnregisterListener(ILocaleListener listener)
		{
			if (listener != null)
			{
				int num = FindListener(listener);
				if (num >= 0)
				{
					listeners.RemoveAt(num);
				}
			}
		}

		public int FindListener(ILocaleListener listener)
		{
			if (listener == null)
			{
				return -1;
			}
			int count = listeners.Count;
			for (int i = 0; i < count; i++)
			{
				ILocaleListener localeListener = (ILocaleListener)listeners[i].Target;
				if (listener == localeListener)
				{
					return i;
				}
			}
			return -1;
		}

		public void NotifyListeners(string locale)
		{
			for (int num = listeners.Count - 1; num >= 0; num--)
			{
				ILocaleListener localeListener = (ILocaleListener)listeners[num].Target;
				if (localeListener != null)
				{
					localeListener.HandleLocaleChanged(locale);
				}
				else
				{
					listeners.RemoveAt(num);
				}
			}
		}

		public void GC()
		{
			for (int num = listeners.Count - 1; num >= 0; num--)
			{
				if (listeners[num].Target == null)
				{
					listeners.RemoveAt(num);
				}
			}
		}
	}
}
