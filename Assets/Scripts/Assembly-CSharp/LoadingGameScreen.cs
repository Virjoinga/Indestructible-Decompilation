using System;
using Glu.Localization;
using UnityEngine;

public class LoadingGameScreen : MonoBehaviour
{
	private const float ImageAspect = 2.764992f;

	public UISingleSprite ScreenImage;

	public SpriteText[] ScreenTexts;

	public SpriteText ScreenTip;

	public Transform LoadingTextRoot;

	public SpriteText LoadingText;

	public UIAnimation LoadingTextAnimation;

	public SpriteText TapToStartText;

	public UIAnimation TapToStartTextAnimation;

	public UIAnimation TapToStartTextAnimation2;

	public void SetAlpha(float a)
	{
		LoadingScene.SetRootAlpha(ScreenImage, a);
		LoadingScene.SetTextAlpha(ScreenTip, a);
		LoadingScene.SetTextAlpha(LoadingText, a);
		LoadingScene.SetTextAlpha(TapToStartText, a);
		SpriteText[] screenTexts = ScreenTexts;
		foreach (SpriteText text in screenTexts)
		{
			LoadingScene.SetTextAlpha(text, a);
		}
	}

	private void SetGameScreen(IDTGame game)
	{
	}

	private void ShowImage(string path)
	{
		if (!string.IsNullOrEmpty(path))
		{
			Vector2 screenSize = UITools.GetScreenSize();
			float num = screenSize.x / 2.764992f;
			SimpleSpriteUtils.ChangeTexture(ScreenImage, path);
			ScreenImage.SetSize(screenSize.x, num);
			Vector3 localPosition = LoadingTextRoot.localPosition;
			LoadingTextRoot.localPosition = new Vector3(0f, num / 2f + 7f, localPosition.z);
		}
		else
		{
			MonoUtils.SetActive(ScreenImage, false);
		}
	}

	private void ShowRandomTip(bool show)
	{
		if (show)
		{
			System.Random random = new System.Random();
			string text = "IDS_LOADING_SCREEN_TIP_";
			text += random.Next(1, 18);
			Vector2 screenSize = UITools.GetScreenSize();
			ScreenTip.maxWidth = screenSize.x - 10f;
			ScreenTip.Text = Strings.GetString(text);
			Transform component = ScreenTip.GetComponent<Transform>();
			Vector3 localPosition = component.localPosition;
			localPosition.y = (0f - screenSize.x) / 2.764992f / 2f - 5f;
			component.localPosition = localPosition;
		}
		else
		{
			MonoUtils.SetActive(ScreenTip, false);
		}
	}

	private void ShowTripleTips(string textId)
	{
		if (!string.IsNullOrEmpty(textId))
		{
			float num = 8f;
			int num2 = ScreenTexts.Length;
			Vector2 screenSize = UITools.GetScreenSize();
			float num3 = screenSize.x / 2.764992f;
			float num4 = (screenSize.x - (float)(num2 + 1) * num) / (float)num2;
			for (int i = 0; i < num2; i++)
			{
				SpriteText spriteText = ScreenTexts[i];
				spriteText.maxWidth = num4;
				spriteText.Text = Strings.GetString(textId + (i + 1));
				Transform component = spriteText.GetComponent<Transform>();
				Vector3 localPosition = component.localPosition;
				localPosition.y = (0f - num3) / 2f - 5f;
				localPosition.x = ((float)i + 0.5f) * num4 + (float)(i + 1) * num;
				localPosition.x -= screenSize.x / 2f;
				component.localPosition = localPosition;
			}
		}
		else
		{
			SpriteText[] screenTexts = ScreenTexts;
			foreach (SpriteText c in screenTexts)
			{
				MonoUtils.SetActive(c, false);
			}
		}
	}

	public void TapToStart()
	{
		LoadingTextAnimation.Play();
		TapToStartTextAnimation.Play();
		TapToStartTextAnimation2.Play();
	}

	public void StopAnimations()
	{
		TapToStartTextAnimation.Pause();
		TapToStartTextAnimation2.Pause();
	}

	public void Activate(LoadingScene loading)
	{
		string path = null;
		string textId = null;
		bool show = false;
		if (LoadingScene.ContentType == LoadingScene.Content.Game)
		{
			if (MonoSingleton<Player>.Instance.LastPlayedType == "multiplayer")
			{
				if (MonoSingleton<Player>.Instance.LastPlayedGame == "CTFConf")
				{
					path = "Loading/LoadingScreenCTF";
					textId = "IDS_LOADING_SCREEN_CTF_";
				}
				else if (MonoSingleton<Player>.Instance.LastPlayedGame == "CRConf")
				{
					path = "Loading/LoadingScreenCRS";
					textId = "IDS_LOADING_SCREEN_CRS_";
				}
				else if (MonoSingleton<Player>.Instance.LastPlayedGame == "DeathmatchConf")
				{
					path = "Loading/LoadingScreenDM";
					textId = "IDS_LOADING_SCREEN_DM_";
				}
				else if (MonoSingleton<Player>.Instance.LastPlayedGame == "TeamDeathmatchConf")
				{
					path = "Loading/LoadingScreenTDM";
					textId = "IDS_LOADING_SCREEN_TDM_";
				}
			}
			else
			{
				string lastPlayedMap = MonoSingleton<Player>.Instance.LastPlayedMap;
				if (MonoSingleton<Player>.Instance.LastPlayedMode == "boss")
				{
					path = "Loading/LoadingScreenCampaign";
					show = true;
				}
				else
				{
					switch (lastPlayedMap)
					{
					case "ctf_aircrash":
						path = "Loading/LoadingScreenSingleAircrash";
						show = true;
						break;
					case "koh_iceberg":
						path = "Loading/LoadingScreenSingleIceberg";
						show = true;
						break;
					case "dtb_rocketbase":
						path = "Loading/LoadingScreenSingleRocketBase";
						show = true;
						break;
					}
				}
			}
		}
		ShowImage(path);
		ShowTripleTips(textId);
		ShowRandomTip(show);
	}

	private void OnDestroy()
	{
		SimpleSpriteUtils.UnloadTexture(ScreenImage);
	}
}
