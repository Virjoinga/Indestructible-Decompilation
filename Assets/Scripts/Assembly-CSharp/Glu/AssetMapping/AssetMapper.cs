using System;
using System.Collections.Generic;
using UnityEngine;

namespace Glu.AssetMapping
{
	public class AssetMapper
	{
		private class ObjectLoader
		{
			private IAssetGroupLoader loader;

			private int index;

			public ObjectLoader(IAssetGroupLoader loader, int index)
			{
				this.loader = loader;
				this.index = index;
			}

			public UnityEngine.Object Load()
			{
				AssetGroup assetGroup = loader.Load();
				if (assetGroup == null)
				{
					throw new NotFoundException("Failed to load group");
				}
				return assetGroup.Assets[index].Asset;
			}
		}

		private static AssetMapper instance;

		private AssetFactory factory;

		private AssetDb db;

		private IList<IAssetGroupLoader> unprocessedLoaders;

		public static AssetMapper Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new AssetMapper();
				}
				return instance;
			}
		}

		private AssetMapper()
		{
			factory = new AssetFactory();
			db = new AssetDb();
			unprocessedLoaders = new List<IAssetGroupLoader>();
		}

		public void TagAsset(UnityEngine.Object obj, string id)
		{
			if (obj == null)
			{
				throw new ArgumentNullException("obj");
			}
			if (id == null)
			{
				throw new ArgumentNullException("id");
			}
			db.Add(obj, id);
		}

		public void TagGroup(AssetGroup group)
		{
			if (group == null)
			{
				throw new ArgumentNullException("group");
			}
			Validate(group);
			IList<IdAssetPair> assets = group.Assets;
			int count = assets.Count;
			for (int i = 0; i < count; i++)
			{
				IdAssetPair idAssetPair = assets[i];
				if (idAssetPair.Asset != null)
				{
					db.Add(idAssetPair.Asset, idAssetPair.Id);
				}
			}
		}

		public void AddAsset(string id, Func<UnityEngine.Object> ctor)
		{
			if (id == null)
			{
				throw new ArgumentNullException("id");
			}
			if (ctor == null)
			{
				throw new ArgumentNullException("ctor");
			}
			factory.Add(id, ctor);
		}

		public void AddGroup(IAssetGroupLoader groupLoader)
		{
			if (groupLoader == null)
			{
				throw new ArgumentNullException("groupLoader");
			}
			unprocessedLoaders.Add(groupLoader);
		}

		public T MapObject<T>(T obj) where T : UnityEngine.Object
		{
			if ((UnityEngine.Object)obj == (UnityEngine.Object)null)
			{
				throw new ArgumentNullException("obj");
			}
			string id = db.GetId(obj);
			return Get<T>(id);
		}

		public T MapObjectOrDefault<T>(T obj, T defaultValue) where T : UnityEngine.Object
		{
			if ((UnityEngine.Object)obj == (UnityEngine.Object)null)
			{
				return defaultValue;
			}
			if (!db.Contains(obj))
			{
				return defaultValue;
			}
			string id = db.GetId(obj);
			if (Contains(id))
			{
				return factory.Create<T>(id);
			}
			return defaultValue;
		}

		public bool Contains(string id)
		{
			if (id == null)
			{
				throw new ArgumentNullException("id");
			}
			ProcessLoaders();
			return factory.Contains(id);
		}

		public T Get<T>(string id) where T : UnityEngine.Object
		{
			if (id == null)
			{
				throw new ArgumentNullException("id");
			}
			ProcessLoaders();
			try
			{
				return factory.Create<T>(id);
			}
			catch (KeyNotFoundException inner)
			{
				throw new NotFoundException(string.Format("Object with ID={0} not found", id), inner);
			}
		}

		public void Reset()
		{
			ResetTags();
			ResetAssets();
		}

		public void ResetTags()
		{
			db.Reset();
		}

		public void ResetAssets()
		{
			factory.Reset();
			unprocessedLoaders.Clear();
		}

		private static void Validate(AssetGroup group)
		{
			HashSet<string> hashSet = new HashSet<string>();
			IList<IdAssetPair> assets = group.Assets;
			int count = assets.Count;
			for (int i = 0; i < count; i++)
			{
				IdAssetPair idAssetPair = assets[i];
				if (string.IsNullOrEmpty(idAssetPair.Id))
				{
					throw new InvalidDataException("Empty ID");
				}
				if (hashSet.Contains(idAssetPair.Id))
				{
					throw new InvalidDataException(string.Format("Duplicate ID {0}", idAssetPair.Id));
				}
				hashSet.Add(idAssetPair.Id);
			}
		}

		private void ProcessLoaders()
		{
			if (unprocessedLoaders.Count > 0)
			{
				AddLoaders();
				unprocessedLoaders.Clear();
			}
		}

		private void AddLoaders()
		{
			int count = unprocessedLoaders.Count;
			for (int i = 0; i < count; i++)
			{
				IAssetGroupLoader assetGroupLoader = unprocessedLoaders[i];
				AssetGroup assetGroup = assetGroupLoader.Load();
				if (assetGroup == null)
				{
					throw new NotFoundException("Failed to load group");
				}
				Validate(assetGroup);
				IList<IdAssetPair> assets = assetGroup.Assets;
				int count2 = assets.Count;
				for (int j = 0; j < count2; j++)
				{
					IdAssetPair idAssetPair = assets[j];
					if (!(idAssetPair.Asset == null))
					{
						ObjectLoader @object = new ObjectLoader(assetGroupLoader, j);
						factory.Add(idAssetPair.Id, @object.Load);
					}
				}
			}
		}
	}
}
