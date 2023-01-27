using System;
using UnityEngine;

namespace Glu.AssetMapping
{
	[Serializable]
	public class IdAssetPair
	{
		[SerializeField]
		private string id;

		[SerializeField]
		private UnityEngine.Object asset;

		public string Id
		{
			get
			{
				return id;
			}
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException("value");
				}
				id = value;
			}
		}

		public UnityEngine.Object Asset
		{
			get
			{
				return asset;
			}
			set
			{
				asset = value;
			}
		}

		public IdAssetPair()
		{
			id = string.Empty;
			asset = null;
		}
	}
}
