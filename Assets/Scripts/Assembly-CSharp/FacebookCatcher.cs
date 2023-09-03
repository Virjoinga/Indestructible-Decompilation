using Glu;
using UnityEngine;

public class FacebookCatcher : UnityEngine.MonoBehaviour
{
	private void UserInfoAcquired(string data)
	{
		Facebook.UserInfoCallback(data);
	}
}
