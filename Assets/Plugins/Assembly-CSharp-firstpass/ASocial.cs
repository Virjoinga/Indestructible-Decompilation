using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class ASocial
{
	public static class Facebook
	{
		public static void Init(string gameObjectName = "")
		{
			Facebook_Init(gameObjectName, AJavaTools.Properties.GetFacebookAppID());
		}

		public static bool IsLoggedIn()
		{
			return Facebook_IsLoggedIn();
		}

		public static void Login(params string[] permissions)
		{
			Facebook_Login(permissions);
		}

		public static void ExtendAccess()
		{
			Facebook_ExtendAccess();
		}

		public static void Post(params string[] parameters)
		{
			Facebook_Post(parameters);
		}

		public static void PostPhoto(string path, string caption = "")
		{
			Facebook_PostPhoto(path, caption);
		}

		public static void Request(params string[] parameters)
		{
			Facebook_Request(parameters);
		}

		public static string GetID()
		{
			return GetUserInfo("id");
		}

		public static string GetName()
		{
			return GetUserInfo("name");
		}

		public static string GetUsername()
		{
			return GetUserInfo("username");
		}

		public static string GetUserInfo(string key)
		{
			return Facebook_GetUserInfo(key);
		}

		public static List<Dictionary<string, string>> GetFriendsWithApp()
		{
			string text = FQLQuery("SELECT name, uid FROM user WHERE is_app_user=1 and uid IN (SELECT uid2 FROM friend WHERE uid1=me())");
			text = text.Trim("[{}]".ToCharArray());
			string[] array = Regex.Split(text, "},{");
			List<Dictionary<string, string>> list = new List<Dictionary<string, string>>();
			for (int i = 0; i < array.Length; i++)
			{
				Match match = Regex.Match(array[i], "^\"name\":\"(.+)\",\"uid\":([0-9]+)$", RegexOptions.IgnoreCase);
				if (match.Success)
				{
					Dictionary<string, string> dictionary = new Dictionary<string, string>();
					dictionary.Add("name", match.Groups[1].Value);
					dictionary.Add("uid", match.Groups[2].Value);
					list.Add(dictionary);
				}
			}
			return list;
		}

		public static string FQLQuery(string query)
		{
			return Facebook_FQLQuery(query);
		}

		public static void Logout()
		{
			Facebook_Logout();
		}
	}

	public static class Amazon
	{
		public const string CONFLICT_STRATEGY_PLAYER_SELECT = "PLAYER_SELECT";

		public const string CONFLICT_STRATEGY_AUTO_TO_CLOUD = "AUTO_RESOLVE_TO_CLOUD";

		public const string CONFLICT_STRATEGY_AUTO_TO_IGNORE = "AUTO_RESOLVE_TO_IGNORE";

		public static void Init()
		{
			Amazon_Init();
		}

		public static void Sync(string description, string conflictStrategy = "")
		{
			Amazon_Sync(description, conflictStrategy);
		}

		public static void SyncOnExit()
		{
			Amazon_Sync(string.Empty, "AUTO_RESOLVE_TO_IGNORE");
		}

		public static void RequestRevert()
		{
			Amazon_RequestRevert();
		}
	}

	private static AndroidJavaClass _asocial;

	private static AndroidJavaClass _facebook;

	private static AndroidJavaClass _amazon;

	public static AndroidJavaClass asocial
	{
		get
		{
			if (_asocial == null)
			{
				_asocial = new AndroidJavaClass("com.glu.plugins.ASocial");
			}
			return _asocial;
		}
	}

	public static AndroidJavaClass facebook
	{
		get
		{
			if (_facebook == null)
			{
				_facebook = new AndroidJavaClass("com.glu.plugins.FacebookGlu");
			}
			return _facebook;
		}
	}

	public static AndroidJavaClass amazon
	{
		get
		{
			if (_amazon == null)
			{
				_amazon = new AndroidJavaClass("com.glu.plugins.AmazonGameCircleGlu");
			}
			return _amazon;
		}
	}

	public static void Init()
	{
		ASocial_Init(Debug.isDebugBuild);
	}

	private static void ASocial_Init(bool debug)
	{
		asocial.CallStatic("Init", debug);
	}

	private static void Facebook_Init(string gameObjectName, string app_id)
	{
		facebook.CallStatic("Init", gameObjectName, app_id);
	}

	private static void Facebook_Login(string[] permissions)
	{
		facebook.CallStatic("Login", false, permissions);
	}

	private static bool Facebook_IsLoggedIn()
	{
		return facebook.CallStatic<bool>("IsLoggedIn", new object[0]);
	}

	private static void Facebook_ExtendAccess()
	{
		facebook.CallStatic("ExtendAccess");
	}

	private static void Facebook_Post(string[] parameters)
	{
		facebook.CallStatic("Post", false, parameters);
	}

	private static void Facebook_PostPhoto(string path, string caption)
	{
		facebook.CallStatic("PostPhoto", path, caption);
	}

	private static void Facebook_Request(string[] parameters)
	{
		facebook.CallStatic("Request", false, parameters);
	}

	private static string Facebook_FQLQuery(string query)
	{
		return facebook.CallStatic<string>("FQLQuery", new object[1] { query });
	}

	private static string Facebook_GetUserInfo(string key)
	{
		return facebook.CallStatic<string>("GetUserInfo", new object[1] { key });
	}

	private static void Facebook_Logout()
	{
		facebook.CallStatic("Logout");
	}

	private static void Amazon_Init()
	{
		amazon.CallStatic("Init");
	}

	private static void Amazon_Sync(string description, string conflictStrategy)
	{
		amazon.CallStatic("Sync", description, conflictStrategy);
	}

	private static void Amazon_RequestRevert()
	{
		amazon.CallStatic("RequestRevert");
	}
}
