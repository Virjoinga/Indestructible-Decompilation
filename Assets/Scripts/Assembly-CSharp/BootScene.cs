using System.Collections;
using Glu.ABTesting;
using Glu.AssetBundles;
using Glu.DynamicContentPipeline;
using UnityEngine;

public class BootScene : MonoBehaviour
{
	public GameObject SplashCamera;

	public GameObject SplashManager;

	public UISingleSprite SplashScreen;

	public SpriteText DebugText;

	public InclinedProgressBar BootMeter;

	public UIAnimation BootMeterAnimation;

	public Transform BootMeterTransform;

	public Transform ContentTransform;

	private Logging.ILogger _logger;

	private BootLoggerHandler _handler;

	private string _previousBundleState = string.Empty;

	public static bool IsRunning;

	private void Awake()
	{
		AJavaTools.UI.StopIndeterminateProgress();
		IsRunning = true;
		_handler = new BootLoggerHandler(null);
		_logger = Logging.GetLogger("App.Boot");
		_logger.AddHandler(_handler);
		Input.multiTouchEnabled = false;
		MonoSingleton<GameController>.Instance.Create();
		MonoSingleton<GameController>.Instance.Reset();
		DestroyInstances();
	}

	private void OnDestroy()
	{
		_logger.RemoveHandler(_handler);
	}

	private void DestroyInstances()
	{
		MonoSingleton<Player>.DestroyInstance();
		MonoSingleton<ShopController>.DestroyInstance();
		MonoSingleton<NotificationsQueue>.DestroyInstance();
		MonoSingleton<DialogsQueue>.DestroyInstance();
	}

	private void LimitAlignedElementsHorizontalOffset()
	{
		Transform bootMeterTransform = BootMeterTransform;
		Vector3 position = bootMeterTransform.position;
		if (position.x < -95f)
		{
			position.x = -95f;
		}
		bootMeterTransform.position = position;
	}

	private void Start()
	{
		LimitAlignedElementsHorizontalOffset();
		BootMeter.Position = 0f;
		DownloadManager.ThreadCount = 3;
		DynamicContent.OnForcedBinariesUpdate = delegate
		{
			Dialogs.ApplicationGotForcedUpdateDialog();
		};
		DynamicContent.OnFail = delegate
		{
			Dialogs.ApplicationFailedToLoadAssetBundlesDialog();
		};
		DynamicContent.OnSuccess = delegate
		{
			OnAssetBundlesLoaded();
		};
		DynamicContent.Init(GameConstants.IDT_ASSET_BUNDLES_URL);
		DynamicContent.StartContentUpdate();
	}

	private string UpdateBundlesState()
	{
		string result = "Unknown state";
		Glu.ABTesting.Resolution aBTestingResolution = DynamicContent.ABTestingResolution;
		if (aBTestingResolution == null)
		{
			result = "Waiting for ABTesting...";
		}
		else if (!aBTestingResolution.Ready)
		{
			result = "ABTesting...";
		}
		else
		{
			IndexInfo assetBundlesIndexInfo = DynamicContent.AssetBundlesIndexInfo;
			if (assetBundlesIndexInfo != null)
			{
				result = "Checking for asset bundles...";
				if (assetBundlesIndexInfo.state == IndexInfo.State.InProgress)
				{
					if (assetBundlesIndexInfo.downloadInfo == null)
					{
						result = "Preparing to start to download index...";
					}
					else if (assetBundlesIndexInfo.downloadInfo.state != DownloadManager.Info.State.Downloaded)
					{
						result = "Loading index file...";
					}
					else if (assetBundlesIndexInfo.downloadInfo.state == DownloadManager.Info.State.Downloaded)
					{
						AssetBundleInfo[] assetBundleInfo = assetBundlesIndexInfo.assetBundleInfo;
						AssetBundleInfo[] array = assetBundleInfo;
						foreach (AssetBundleInfo assetBundleInfo2 in array)
						{
							if (assetBundleInfo2.downloadInfo.state == DownloadManager.Info.State.Receiving)
							{
								result = "Loading asset bundle " + assetBundleInfo2.Filename + "...";
							}
						}
					}
				}
				if (assetBundlesIndexInfo.state == IndexInfo.State.Failed)
				{
					result = "Cannot load asset bundles";
				}
				if (assetBundlesIndexInfo.state == IndexInfo.State.Succeeded)
				{
					result = "Asset bundles are loaded";
				}
			}
		}
		return result;
	}

	private void Update()
	{
		BootMeter.Position = DynamicContent.Progress;
		string text = UpdateBundlesState();
		if (_previousBundleState != text)
		{
			_previousBundleState = text;
		}
	}

	private void OnAssetBundlesLoaded()
	{
		AJavaTools.UI.StartIndeterminateProgress(85);
		MonoSingleton<GameController>.Instance.OnAssetBundlesReady();
		Application.LoadLevelAdditive("GarageScene");
		StartCoroutine(DisappearRoutine());
	}

	private IEnumerator DisappearRoutine()
	{
		ContentTransform.localPosition = new Vector3(0f, 0f, -90f);
		Object.Destroy(SplashManager);
		yield return null;
		Object.Destroy(SplashCamera);
		BootMeter.Position = 1f;
		BootMeterAnimation.Play();
		yield return new WaitForSeconds(0.5f);
		float time = 0f;
		float duration = 0.5f;
		while (time < duration)
		{
			Color color = Color.white;
			color.a = (duration - time) / duration;
			SplashScreen.SetColor(color);
			time += Time.deltaTime;
			yield return null;
		}
		SimpleSpriteUtils.UnloadTexture(SplashScreen);
		Object.Destroy(base.gameObject);
		IsRunning = false;
	}
}
