namespace Glu.Localization.Internal
{
	public struct MessageKey
	{
		private string context;

		private string id;

		public string Context
		{
			get
			{
				return context;
			}
		}

		public string Id
		{
			get
			{
				return id;
			}
		}

		public MessageKey(string context, string id)
		{
			this.context = context;
			this.id = id;
		}

		public bool Equals(MessageKey other)
		{
			return context == other.context && id == other.id;
		}

		public override bool Equals(object obj)
		{
			if (!(obj is MessageKey))
			{
				return false;
			}
			return Equals((MessageKey)obj);
		}

		public override int GetHashCode()
		{
			int num = ((context != null) ? context.GetHashCode() : 0);
			int num2 = ((id != null) ? id.GetHashCode() : 0);
			return num ^ num2;
		}

		public static bool operator ==(MessageKey a, MessageKey b)
		{
			return a.Equals(b);
		}

		public static bool operator !=(MessageKey a, MessageKey b)
		{
			return !a.Equals(b);
		}
	}
}
