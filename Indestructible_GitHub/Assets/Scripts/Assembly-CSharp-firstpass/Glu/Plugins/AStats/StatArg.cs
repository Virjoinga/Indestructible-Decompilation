using System;

namespace Glu.Plugins.AStats
{
	internal struct StatArg
	{
		private string key;

		private object value;

		public string Key
		{
			get
			{
				return key;
			}
		}

		public object Value
		{
			get
			{
				return value;
			}
		}

		public Type Type
		{
			get
			{
				return (Value == null) ? typeof(string) : Value.GetType();
			}
		}

		public StatArg(string key, string value)
		{
			if (key == null)
			{
				throw new ArgumentNullException("key");
			}
			this.key = key;
			this.value = value;
		}

		public StatArg(string key, int value)
		{
			if (key == null)
			{
				throw new ArgumentNullException("key");
			}
			this.key = key;
			this.value = value;
		}

		public int GetInt()
		{
			return (int)Value;
		}

		public string GetString()
		{
			return (Value == null) ? null : Value.ToString();
		}
	}
}
