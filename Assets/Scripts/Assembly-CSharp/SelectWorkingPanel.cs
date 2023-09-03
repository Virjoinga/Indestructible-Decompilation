public class SelectWorkingPanel : PanelManagerPanel
{
	public SpriteText Text;

	public UIButton CancelButton;

	public override void OnActivate()
	{
		base.OnActivate();
		CancelButton.gameObject.SetActiveRecursively(false);
	}
}
