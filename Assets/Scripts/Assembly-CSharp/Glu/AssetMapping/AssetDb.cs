using System;
using System.Collections.Generic;
using UnityEngine;

namespace Glu.AssetMapping
{
	public class AssetDb
	{
		private IDictionary<string, string> tags;

		public AssetDb()
		{
			tags = new Dictionary<string, string>();
		}

		public bool Contains(UnityEngine.Object obj)
		{
			if (obj == null)
			{
				throw new ArgumentNullException("obj");
			}
			string assetId = GetAssetId(obj);
			return tags.ContainsKey(assetId);
		}

		public string GetId(UnityEngine.Object obj)
		{
			if (obj == null)
			{
				throw new ArgumentNullException("obj");
			}
			string assetId = GetAssetId(obj);
			string value;
			if (!tags.TryGetValue(assetId, out value))
			{
				throw new NotFoundException(string.Format("Unknown object {0}", obj));
			}
			return value;
		}

		public void Add(UnityEngine.Object obj, string id)
		{
			if (obj == null)
			{
				throw new ArgumentNullException("obj");
			}
			if (id == null)
			{
				throw new ArgumentNullException("id");
			}
			string assetId = GetAssetId(obj);
			tags[assetId] = id;
		}

		public void Reset()
		{
			tags.Clear();
		}

		private static string GetAssetId(UnityEngine.Object obj)
		{
			return string.Format("{0} {1}", obj.name, obj.GetType().FullName);
		}
	}
}
