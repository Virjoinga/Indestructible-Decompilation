public interface IPhotonObserver
{
	void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info);
}
