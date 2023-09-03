using System.Collections.Generic;
using System.IO;

namespace Glu.Localization.StreamGetters
{
	public sealed class GroupStreamGetter : IStreamGetter
	{
		private IList<IStreamGetter> children;

		public GroupStreamGetter()
		{
			children = new List<IStreamGetter>();
		}

		public int GetChildCount()
		{
			return children.Count;
		}

		public IStreamGetter GetChild(int childIndex)
		{
			return children[childIndex];
		}

		public void AddChild(IStreamGetter streamGetter)
		{
			if (!object.ReferenceEquals(streamGetter, null) && !children.Contains(streamGetter))
			{
				children.Add(streamGetter);
			}
		}

		public void RemoveChild(IStreamGetter streamGetter)
		{
			children.Remove(streamGetter);
		}

		public Stream GetStream(string name)
		{
			int count = children.Count;
			for (int i = 0; i < count; i++)
			{
				Stream stream = children[i].GetStream(name);
				if (stream != null)
				{
					return stream;
				}
			}
			return null;
		}
	}
}
