using Glu.ABTesting;
using Glu.AssetBundles;
using Glu.DynamicContentPipeline;

public class DialogSecret : UIDialog
{
	public SpriteText AboutText;

	public UIExpandSprite Background;

	protected override void Start()
	{
		base.Start();
		string text = "BUILD TAG: " + BuildTag.Get() + "\n";
		IndexInfo assetBundlesIndexInfo = DynamicContent.AssetBundlesIndexInfo;
		text = text + "BUNDLES URL: " + GameConstants.IDT_ASSET_BUNDLES_URL + "\n";
		if (assetBundlesIndexInfo != null)
		{
			text = text + "BUNDLES TAG: " + assetBundlesIndexInfo.BuildTag + "\n";
		}
		text = text + "PHOTON APP ID: " + NetworkManager.instance.appID + "\n";
		Resolution aBTestingResolution = DynamicContent.ABTestingResolution;
		if (aBTestingResolution != null)
		{
			text = text + "\nAB GROUP: " + aBTestingResolution.VariantId + "\n";
		}
		text = text + "\nFLURRY API KEY: " + AJavaTools.Properties.GetFlurryKey() + "\n";
		text = text + "\nPLAYHAVEN TOKEN: " + AJavaTools.Properties.GetPlayHavenToken() + "\n";
		text = text + "PLAYHAVEN SECRET: " + AJavaTools.Properties.GetPlayHavenSecret() + "\n";
		AboutText.Text = text;
		float num = AboutText.TopLeft.y - AboutText.BottomRight.y;
		Background.SetHeight(num + 10f);
	}

	private void OnCloseButtonTap()
	{
		MonoSingleton<UISounds>.Instance.Play(UISounds.Type.Click);
		Close();
	}
}
