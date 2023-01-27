public class DialogIAPRestore : UIDialog
{
	protected override void Awake()
	{
		base.Awake();
		ICInAppPurchase.GetInstance().RestoreCompletedTransactions();
	}

	private void Update()
	{
		switch (ICInAppPurchase.GetInstance().GetRestoreStatus())
		{
		case ICInAppPurchase.RESTORE_STATE.FAILED:
			Dialogs.IAPRestoreFailed();
			Close();
			break;
		case ICInAppPurchase.RESTORE_STATE.SUCCESS_EMPTY:
			Dialogs.IAPRestoreEmpty();
			Close();
			break;
		case ICInAppPurchase.RESTORE_STATE.SUCCESS_DELIVERY:
			Close();
			break;
		}
	}
}
