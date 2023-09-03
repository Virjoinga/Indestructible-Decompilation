using System;
using System.Collections.Generic;
using UnityEngine;

namespace Glu.AssetMapping
{
	public class AssetFactory
	{
		private IDictionary<string, Func<UnityEngine.Object>> ctors;

		public AssetFactory()
		{
			ctors = new Dictionary<string, Func<UnityEngine.Object>>();
		}

		public bool Contains(string id)
		{
			if (id == null)
			{
				throw new ArgumentNullException("id");
			}
			return ctors.ContainsKey(id);
		}

		public T Create<T>(string id) where T : UnityEngine.Object
		{
			Func<UnityEngine.Object> ctor = GetCtor<T>(id);
			return (T)ctor();
		}

		public Func<UnityEngine.Object> GetCtor<T>(string id)
		{
			if (id == null)
			{
				throw new ArgumentNullException("id");
			}
			return ctors[id];
		}

		public void Add(string id, Func<UnityEngine.Object> ctor)
		{
			if (id == null)
			{
				throw new ArgumentNullException("id");
			}
			if (ctor == null)
			{
				throw new ArgumentNullException("ctor");
			}
			ctors[id] = ctor;
		}

		public void Reset()
		{
			ctors.Clear();
		}
	}
}
