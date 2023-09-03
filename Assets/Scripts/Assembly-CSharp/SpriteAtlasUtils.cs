using Glu.Localization;
using UnityEngine;

public static class SpriteAtlasUtils
{
	private static string _pathMaterials = "Sprite Atlases/Materials/";

	private static string _pathTextures = "Sprite Atlases/Textures/";

	public static void UnloadMaterial(string name)
	{
	}

	public static void LoadMaterial(string name)
	{
		Object @object = Resources.Load(_pathMaterials + name, typeof(Material));
		Material material = @object as Material;
		if (!material || !material.mainTexture)
		{
			SimpleSpriteUtils.ChangeTexture(@object as Material, _pathTextures + name);
		}
	}

	public static void SetLocalizationMaterial()
	{
		string text = "LocalizationMaterial-" + Strings.Locale;
		string path = _pathMaterials + "LocalizationMaterial";
		text = _pathTextures + text;
		Object @object = Resources.Load(path, typeof(Material));
		SimpleSpriteUtils.ChangeTexture(@object as Material, text);
	}

	public static void SetDefaultLocalizationMaterial()
	{
		string text = "LocalizationMaterial";
		string path = _pathMaterials + "LocalizationMaterial";
		text = _pathTextures + text;
		Object @object = Resources.Load(path, typeof(Material));
		SimpleSpriteUtils.ChangeTexture(@object as Material, text);
	}

	public static void UnloadAll()
	{
		UnloadMaterial("GarageMaterial");
		UnloadMaterial("ProfileMaterial");
		UnloadMaterial("TalentsMaterial");
		UnloadMaterial("CustomizationMaterial");
		UnloadMaterial("DialogsMaterial2");
		UnloadMaterial("ShopMaterial");
	}
}
