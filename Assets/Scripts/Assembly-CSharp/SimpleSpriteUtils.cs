using System.Collections.Generic;
using UnityEngine;

public class SimpleSpriteUtils
{
	private static Dictionary<string, int> _textureLoadingCounter = new Dictionary<string, int>();

	public static bool ChangeTexture(UISingleSprite sprite, Texture2D texture)
	{
		MeshRenderer component = sprite.GetComponent<MeshRenderer>();
		component.material.SetTexture("_MainTex", texture);
		if (_textureLoadingCounter.ContainsKey(texture.name))
		{
			_textureLoadingCounter[texture.name] = _textureLoadingCounter[texture.name] + 1;
		}
		else
		{
			_textureLoadingCounter[texture.name] = 1;
		}
		return true;
	}

	public static bool ChangeTexture(UISingleSprite sprite, string path)
	{
		Texture2D texture2D = Resources.Load(path) as Texture2D;
		if (texture2D == null)
		{
			return false;
		}
		return ChangeTexture(sprite, texture2D);
	}

	public static bool ChangeTexture(GameObject o, string path)
	{
		Renderer component = o.GetComponent<Renderer>();
		if (component == null)
		{
			return false;
		}
		if (component.material == null)
		{
			return false;
		}
		return ChangeTexture(component.material, path);
	}

	public static bool ChangeTexture(Material material, string path)
	{
		Texture2D texture2D = Resources.Load(path) as Texture2D;
		if (texture2D == null)
		{
			return false;
		}
		UnloadTexture(material);
		material.SetTexture("_MainTex", texture2D);
		if (_textureLoadingCounter.ContainsKey(texture2D.name))
		{
			_textureLoadingCounter[texture2D.name] = _textureLoadingCounter[texture2D.name] + 1;
		}
		else
		{
			_textureLoadingCounter[texture2D.name] = 1;
		}
		return true;
	}

	public static void UnloadTexture(Material material)
	{
		if (!(material != null))
		{
			return;
		}
		Texture mainTexture = material.mainTexture;
		if (!(mainTexture != null))
		{
			return;
		}
		bool flag = true;
		if (_textureLoadingCounter.ContainsKey(mainTexture.name))
		{
			int num = _textureLoadingCounter[mainTexture.name];
			num--;
			if (num > 0)
			{
				_textureLoadingCounter[mainTexture.name] = num;
				flag = false;
			}
			else
			{
				_textureLoadingCounter.Remove(mainTexture.name);
			}
		}
		if (flag)
		{
			material.mainTexture = null;
			Resources.UnloadAsset(mainTexture);
		}
	}

	public static void UnloadTexture(UISingleSprite sprite)
	{
		if (sprite != null)
		{
			UnloadTexture(sprite.GetComponent<Renderer>().material);
		}
	}
}
