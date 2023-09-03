using System;
using UnityEngine;

namespace Glu
{
	public static class Facebook
	{
		private static class platform
		{
			public static bool Init(string appId)
			{
				Debug.Log("Not implemented");
				return false;
			}

			public static bool IsLoggedIn()
			{
				Debug.Log("Not implemented");
				return false;
			}

			public static void Login()
			{
				Debug.Log("Not implemented");
			}

			public static void Logout()
			{
				Debug.Log("Not implemented");
			}

			public static void GetUserInfo()
			{
				Debug.Log("Not implemented");
				if (userInfoCallback != null)
				{
					userInfoCallback(null, null);
				}
			}

			public static void SendRequestToFriend(string requestText, string notificationText, bool loginIfNeeded)
			{
				Debug.Log("Not implemented");
			}

			public static void PostMessageToWall(string messageText, bool loginIfNeeded)
			{
				Debug.Log("Not implemented");
			}

			public static void PostLinkToWall(string linkUrl, string pictureUrl, string name, string caption, string descriptionText, bool loginIfNeeded)
			{
				Debug.Log("Not implemented");
			}
		}

		private static bool _initialized;

		private static Action<string, string> userInfoCallback;

		public static Version Version
		{
			get
			{
				return new Version(1, 0, 1);
			}
		}

		public static void Init(string appId)
		{
			if (!_initialized)
			{
				_initialized = platform.Init(appId);
			}
		}

		public static bool IsLoggedIn()
		{
			return _initialized && platform.IsLoggedIn();
		}

		public static void Login()
		{
			if (_initialized)
			{
				platform.Login();
			}
		}

		public static void Logout()
		{
			if (_initialized && IsLoggedIn())
			{
				platform.Logout();
			}
		}

		public static void GetUserInfo(Action<string, string> callback)
		{
			if (_initialized && IsLoggedIn())
			{
				userInfoCallback = callback;
				platform.GetUserInfo();
			}
		}

		public static void UserInfoCallback(string info)
		{
			if (userInfoCallback != null)
			{
				string[] array = info.Split('@');
				userInfoCallback(array[0], array[1]);
			}
			userInfoCallback = null;
		}

		public static void SendRequestToFriend(string requestText, string notificationText, bool loginIfNeeded)
		{
			if (_initialized)
			{
				if (requestText == null)
				{
					throw new ArgumentNullException("Request text must not be null.");
				}
				if (requestText.Equals(string.Empty))
				{
					throw new ArgumentException("Request text must not be empty.");
				}
				platform.SendRequestToFriend(requestText, notificationText, loginIfNeeded);
			}
		}

		public static void SendRequestToFriend(string requestText, string notificationText)
		{
			SendRequestToFriend(requestText, notificationText, false);
		}

		public static void PostMessageToWall(string messageText, bool loginIfNeeded)
		{
			if (_initialized)
			{
				if (messageText == null)
				{
					throw new ArgumentNullException("Message text must not be null.");
				}
				if (messageText.Equals(string.Empty))
				{
					throw new ArgumentException("Message text must not be empty.");
				}
				platform.PostMessageToWall(messageText, loginIfNeeded);
			}
		}

		public static void PostMessageToWall(string messageText)
		{
			PostMessageToWall(messageText, false);
		}

		public static void PostLinkToWall(string linkUrl, string pictureUrl, string name, string caption, string descriptionText, bool loginIfNeeded)
		{
			if (_initialized)
			{
				if (linkUrl == null)
				{
					throw new ArgumentNullException("Link URL must not be null.");
				}
				if (linkUrl.Equals(string.Empty))
				{
					throw new ArgumentException("Link URL must not be empty.");
				}
				platform.PostLinkToWall(linkUrl, pictureUrl, name, caption, descriptionText, loginIfNeeded);
			}
		}

		public static void PostLinkToWall(string linkUrl, string pictureUrl, string name, string caption, string descriptionText)
		{
			PostLinkToWall(linkUrl, pictureUrl, name, caption, descriptionText, false);
		}
	}
}
