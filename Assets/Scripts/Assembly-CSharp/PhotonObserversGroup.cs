using System;
using UnityEngine;

public class PhotonObserversGroup : MonoBehaviour
{
	public Component[] observers;

	private void Awake()
	{
		if (PhotonNetwork.room == null)
		{
			UnityEngine.Object.Destroy(this);
		}
	}

	public IPhotonObserver GetObserver(int index)
	{
		if (observers.Length <= index)
		{
			return null;
		}
		return observers[index] as IPhotonObserver;
	}

	public void SetObserver(int index, IPhotonObserver observer)
	{
		int num = observers.Length;
		if (num <= index)
		{
			Array.Resize(ref observers, index + 1);
		}
		observers[index] = observer as Component;
	}

	private void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		int i = 0;
		for (int num = observers.Length; i != num; i++)
		{
			IPhotonObserver photonObserver = observers[i] as IPhotonObserver;
			if (photonObserver != null)
			{
				photonObserver.OnPhotonSerializeView(stream, info);
			}
		}
	}
}
