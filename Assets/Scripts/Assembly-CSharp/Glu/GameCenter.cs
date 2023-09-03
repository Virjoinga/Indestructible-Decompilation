using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Glu
{
	public static class GameCenter
	{
		public class Player : PlayerRecord
		{
			public bool underage { get; private set; }

			public bool authenticated
			{
				get
				{
					bool flag = IsAuthenticated();
					if (!flag)
					{
						localUser.Init(string.Empty, string.Empty, false);
					}
					return flag;
				}
			}

			public override void Init(string Name, string Id, bool Underage)
			{
				base.Init(Id, Name, false);
				underage = Underage;
			}

			public void Authenticate(Action<bool> callback)
			{
				authenticatedCallback(false);
				authenticatedCallback = callback ?? ((Action<bool>)delegate
				{
				});
				GameCenter.Authenticate();
			}

			public void LoadScore(string board, Action<ScoreRecord> callback)
			{
				callbacksCounter++;
				callbacksPool.Add(callbacksCounter.ToString(), callback ?? ((Action<ScoreRecord>)delegate
				{
				}));
				LoadScoreForLocalPlayer(callbacksCounter, board, false);
			}

			public void LoadScore(string board, bool friendsOnly, Action<ScoreRecord> callback)
			{
				callbacksCounter++;
				callbacksPool.Add(callbacksCounter.ToString(), callback ?? ((Action<ScoreRecord>)delegate
				{
				}));
				LoadScoreForLocalPlayer(callbacksCounter, board, friendsOnly);
			}
		}

		public class PlayerRecord
		{
			public string id { get; protected set; }

			public string userName { get; protected set; }

			public bool isFriend { get; protected set; }

			public virtual void Init(string Id, string UserName, bool IsFriend)
			{
				id = Id;
				userName = UserName;
				isFriend = IsFriend;
			}
		}

		public class HostedMatch
		{
			public void RegisterInvitationCallback(Action<string> callback)
			{
				invitationReceivedCallback = callback ?? ((Action<string>)delegate
				{
				});
			}

			public void Create(int minimumPlayers, int maximumPlayers, Action<bool> callback)
			{
				matchmakingCallback(false);
				matchmakingCallback = callback ?? ((Action<bool>)delegate
				{
				});
				CreateHostedMatch(minimumPlayers, maximumPlayers, 0u, 0u);
			}

			public void Create(int minimumPlayers, int maximumPlayers, uint playerGroup, uint playerMask, Action<bool> callback)
			{
				matchmakingCallback(false);
				matchmakingCallback = callback ?? ((Action<bool>)delegate
				{
				});
				CreateHostedMatch(minimumPlayers, maximumPlayers, playerGroup, playerMask);
			}

			public void ShowMatchmakerUI(Action<bool> callback)
			{
				matchmakingCallback(false);
				matchmakingCallback = callback ?? ((Action<bool>)delegate
				{
				});
				ShowMatchmaker();
			}

			public void DismissMatchmakerUI(bool valueToReturnToExistingCallback)
			{
				matchmakingCallback(valueToReturnToExistingCallback);
				matchmakingCallback = delegate
				{
				};
				DismissModalViewController();
			}

			public void SetPlayerStatus(string playerID, bool isConnected)
			{
				SetPlayerConnectedToHost(playerID, isConnected);
			}
		}

		public class Achievement
		{
			public string Identifier { get; private set; }

			public string Title { get; private set; }

			public string AchievedDescription { get; private set; }

			public string UnachievedDescription { get; private set; }

			public int MaximumPoints { get; private set; }

			public bool Hidden { get; private set; }

			public Achievement(string id, string title, string achieved, string unachieved, int points, bool hidden)
			{
				Identifier = id;
				Title = title;
				AchievedDescription = achieved;
				UnachievedDescription = unachieved;
				MaximumPoints = points;
				Hidden = hidden;
				MaximumPoints = points;
				Hidden = hidden;
			}
		}

		public class ScoreRecord
		{
			public string Board { get; private set; }

			public string PlayerID { get; private set; }

			public long Score { get; private set; }

			public string FormattedScore { get; private set; }

			public DateTime Date { get; private set; }

			public int Rank { get; private set; }

			public ScoreRecord(string board, string playerID, long Score, string formattedScore, DateTime date, int rank)
			{
				Board = board;
				PlayerID = playerID;
				this.Score = Score;
				FormattedScore = formattedScore;
				Date = date;
				Rank = rank;
			}
		}

		public class CatcherBase : MonoBehaviour
		{
			private void Authenticated(string data)
			{
				string[] array = data.Split('@');
				bool flag = Convert.ToBoolean(array[0]);
				if (flag)
				{
					localUser.Init(array[1], array[2], Convert.ToBoolean(array[3]));
				}
				Action<bool> authenticatedCallback = GameCenter.authenticatedCallback;
				GameCenter.authenticatedCallback = delegate
				{
				};
				authenticatedCallback(flag);
			}

			private void ScoreReported(string data)
			{
				string[] array = data.Split('@');
				bool obj = Convert.ToBoolean(array[1]);
				((Action<bool>)callbacksPool[array[0]])(obj);
				callbacksPool.Remove(array[0]);
			}

			private void ScoreLoaded(string data)
			{
				string[] array = data.Split('@');
				bool flag = Convert.ToBoolean(array[1]);
				List<ScoreRecord> list = new List<ScoreRecord>();
				if (flag)
				{
					int @int = PlayerPrefs.GetInt("GameCenterHighscoresCount");
					PlayerPrefs.DeleteKey("GameCenterHighscoresCount");
					for (int i = 0; i < @int; i++)
					{
						string key = "GameCenterHighscore" + i;
						string[] array2 = PlayerPrefs.GetString(key).Split('@');
						PlayerPrefs.DeleteKey(key);
						ScoreRecord item = new ScoreRecord(array2[0], array2[1], Convert.ToInt64(array2[2]), array2[3], Convert.ToDateTime(array2[4]), Convert.ToInt32(array2[5]));
						list.Add(item);
					}
				}
				((Action<ScoreRecord[]>)callbacksPool[array[0]])((!flag) ? null : list.ToArray());
				callbacksPool.Remove(array[0]);
			}

			private void LocalScoreLoaded(string data)
			{
				string[] array = data.Split('@');
				string key = array[0];
				ScoreRecord obj = null;
				if (Convert.ToBoolean(array[1]))
				{
					obj = ((Convert.ToInt32(array[7]) == 0) ? new ScoreRecord(array[2], localUser.id, 0L, string.Empty, new DateTime(0L), 0) : new ScoreRecord(array[2], array[3], Convert.ToInt64(array[4]), array[5], Convert.ToDateTime(array[6]), Convert.ToInt32(array[7])));
				}
				((Action<ScoreRecord>)callbacksPool[key])(obj);
				callbacksPool.Remove(key);
			}

			private void PlayersLoaded(string data)
			{
				string[] array = data.Split('@');
				bool flag = Convert.ToBoolean(array[1]);
				List<PlayerRecord> list = new List<PlayerRecord>();
				if (flag)
				{
					int @int = PlayerPrefs.GetInt("GameCenterPlayersCount");
					PlayerPrefs.DeleteKey("GameCenterPlayersCount");
					for (int i = 0; i < @int; i++)
					{
						string key = "GameCenterPlayer" + i;
						string[] array2 = PlayerPrefs.GetString(key).Split('@');
						PlayerPrefs.DeleteKey(key);
						PlayerRecord playerRecord = new PlayerRecord();
						playerRecord.Init(array2[0], array2[1], Convert.ToBoolean(array2[2]));
						list.Add(playerRecord);
					}
				}
				((Action<PlayerRecord[]>)callbacksPool[array[0]])((!flag) ? null : list.ToArray());
				callbacksPool.Remove(array[0]);
			}

			private void LeaderboardDismissed(string data)
			{
				Debug.Log("Leaderboard dismissed");
			}

			private void AchievementReported(string data)
			{
				string[] array = data.Split('@');
				bool obj = Convert.ToBoolean(array[1]);
				((Action<bool>)callbacksPool[array[0]])(obj);
				callbacksPool.Remove(array[0]);
			}

			private void AchievementsReseted(string data)
			{
				string[] array = data.Split('@');
				bool obj = Convert.ToBoolean(array[1]);
				((Action<bool>)callbacksPool[array[0]])(obj);
				callbacksPool.Remove(array[0]);
			}

			private void AchievementsDismissed(string data)
			{
				Debug.Log("Achievements dismissed");
			}

			private void AchievementsLoaded(string data)
			{
				achievementsLoaded = Convert.ToBoolean(data);
				foreach (KeyValuePair<string, Action<Achievement>> achievementDescriptionsCallback in achievementDescriptionsCallbacks)
				{
					if (achievementsLoaded)
					{
						GetAchievement(achievementDescriptionsCallback.Key, achievementDescriptionsCallback.Value);
					}
					else
					{
						achievementDescriptionsCallback.Value(null);
					}
				}
				achievementDescriptionsCallbacks.Clear();
			}

			private void MatchmakingFinished(string data)
			{
				string[] array = data.Split('@');
				bool flag = Convert.ToBoolean(array[0]);
				if (flag)
				{
					for (int i = 1; i < array.Length; i++)
					{
						Log("Player connected during matchmaking: " + array[i]);
					}
				}
				Action<bool> matchmakingCallback = GameCenter.matchmakingCallback;
				GameCenter.matchmakingCallback = delegate
				{
				};
				matchmakingCallback(flag);
			}

			private void InvitationReceived(string data)
			{
				invitationReceivedCallback(data);
			}
		}

		private static Player user;

		private static HostedMatch hMatch;

		private static bool achievementsLoaded;

		private static Dictionary<string, object> callbacksPool = new Dictionary<string, object>();

		private static uint callbacksCounter;

		private static Action<bool> authenticatedCallback = delegate
		{
		};

		private static Action<string> invitationReceivedCallback = delegate
		{
		};

		private static Action<bool> matchmakingCallback = delegate
		{
		};

		private static Dictionary<string, Action<Achievement>> achievementDescriptionsCallbacks = new Dictionary<string, Action<Achievement>>();

		public static Version Version
		{
			get
			{
				return new Version(1, 0, 2);
			}
		}

		public static Player localUser
		{
			get
			{
				if (user == null)
				{
					user = new Player();
					GameObject gameObject = GameObject.Find("GameCenterCatcher");
					if (gameObject == null)
					{
						gameObject = new GameObject("GameCenterCatcher");
						gameObject.AddComponent<GameCenterCatcher>();
						UnityEngine.Object.DontDestroyOnLoad(gameObject);
					}
				}
				return user;
			}
		}

		public static HostedMatch hostedMatch
		{
			get
			{
				if (hMatch == null)
				{
					hMatch = new HostedMatch();
				}
				return hMatch;
			}
		}

		public static void ReportScore(long score, string board, Action<bool> callback)
		{
			callbacksCounter++;
			callbacksPool.Add(callbacksCounter.ToString(), callback ?? ((Action<bool>)delegate
			{
			}));
			ReportScore(callbacksCounter, score, board);
		}

		public static void LoadScores(string board, bool friendsOnly, Action<ScoreRecord[]> callback)
		{
			callbacksCounter++;
			callbacksPool.Add(callbacksCounter.ToString(), callback ?? ((Action<ScoreRecord[]>)delegate
			{
			}));
			LoadScores(callbacksCounter, board, friendsOnly);
		}

		public static void LoadPlayerNames(string[] playerIds, Action<PlayerRecord[]> callback)
		{
			callbacksCounter++;
			callbacksPool.Add(callbacksCounter.ToString(), callback ?? ((Action<PlayerRecord[]>)delegate
			{
			}));
			LoadPlayersForIdentifiers(callbacksCounter, playerIds, playerIds.Length);
		}

		public static void ShowLeaderboardUI()
		{
			Log("Showing Leaderboard UI");
			ShowLeaderboard();
		}

		public static void ReportProgress(string achievement, float progress, Action<bool> callback)
		{
			callbacksCounter++;
			callbacksPool.Add(callbacksCounter.ToString(), callback ?? ((Action<bool>)delegate
			{
			}));
			ReportAchievement(callbacksCounter, achievement, progress);
		}

		public static void ResetAchievements(Action<bool> callback)
		{
			callbacksCounter++;
			callbacksPool.Add(callbacksCounter.ToString(), callback ?? ((Action<bool>)delegate
			{
			}));
			ResetAchievements(callbacksCounter);
		}

		public static void ShowAchievementsUI()
		{
			Log("Showing Achievement UI");
			ShowAchievements();
		}

		public static void GetAchievement(string achievementID, Action<Achievement> callback)
		{
			if (achievementsLoaded)
			{
				StringBuilder stringBuilder = new StringBuilder(1024);
				RetrieveAchievementInfo(achievementID, stringBuilder, stringBuilder.Capacity);
				string[] array = stringBuilder.ToString().Split('@');
				callback(new Achievement(array[0], array[1], array[2], array[3], Convert.ToInt32(array[4]), Convert.ToBoolean(array[5])));
			}
			else
			{
				if (achievementDescriptionsCallbacks.ContainsKey(achievementID))
				{
					callback(null);
				}
				else
				{
					achievementDescriptionsCallbacks.Add(achievementID, callback);
				}
				LoadAchievementDescriptions();
			}
		}

		private static void Log(string text)
		{
			Debug.Log("[GameCenter " + Version.ToString() + "]: " + text);
		}

		private static void Authenticate()
		{
			Log("Authentication works only on iOS devices.");
			Action<bool> action = authenticatedCallback;
			authenticatedCallback = delegate
			{
			};
			action(false);
		}

		private static bool IsAuthenticated()
		{
			return false;
		}

		private static void ShowLeaderboard()
		{
			Log("Leaderboard works only on iOS devices.");
		}

		private static void ReportScore(uint id, long score, string category)
		{
			Log("Score reporting works only on iOS devices.");
			((Action<bool>)callbacksPool[id.ToString()])(false);
			callbacksPool.Remove(id.ToString());
		}

		private static void LoadScores(uint id, string board, bool friendsOnly)
		{
			Log("Score retrieval works only on iOS devices.");
			((Action<ScoreRecord[]>)callbacksPool[id.ToString()])(null);
			callbacksPool.Remove(id.ToString());
		}

		private static void LoadScoreForLocalPlayer(uint id, string board, bool friendsOnly)
		{
			Log("Score retrieval works only on iOS devices.");
			((Action<ScoreRecord>)callbacksPool[id.ToString()])(null);
			callbacksPool.Remove(id.ToString());
		}

		private static void LoadPlayersForIdentifiers(uint id, string[] ids, int length)
		{
			Log("Players retrieval works only on iOS devices.");
			((Action<PlayerRecord[]>)callbacksPool[id.ToString()])(null);
			callbacksPool.Remove(id.ToString());
		}

		private static void ShowAchievements()
		{
			Log("Achievements UI works only on iOS devices.");
		}

		private static void ReportAchievement(uint id, string name, float progress)
		{
			Log("Achievements reporting works only on iOS devices.");
			((Action<bool>)callbacksPool[id.ToString()])(false);
			callbacksPool.Remove(id.ToString());
		}

		private static void ResetAchievements(uint id)
		{
			Log("Achievements resetting works only on iOS devices.");
			((Action<bool>)callbacksPool[id.ToString()])(false);
			callbacksPool.Remove(id.ToString());
		}

		private static void CreateHostedMatch(int minimumPlayers, int maximumPlayers, uint group, uint mask)
		{
			Log("Game Center matches works only on iOS devices.");
			Action<bool> action = matchmakingCallback;
			matchmakingCallback = delegate
			{
			};
			action(false);
		}

		private static void ShowMatchmaker()
		{
			Log("Game Center matches works only on iOS devices.");
			Action<bool> action = matchmakingCallback;
			matchmakingCallback = delegate
			{
			};
			action(false);
		}

		private static void SetPlayerConnectedToHost(string player, bool connected)
		{
			Log("Game Center player connection status works only on iOS devices.");
		}

		private static void DismissModalViewController()
		{
			Log("Game Center matches UI works only on iOS devices.");
		}

		private static void LoadAchievementDescriptions()
		{
			Log("Game Center achievements description is only available on iOS devices.");
			foreach (KeyValuePair<string, Action<Achievement>> achievementDescriptionsCallback in achievementDescriptionsCallbacks)
			{
				achievementDescriptionsCallback.Value(null);
			}
		}

		private static void RetrieveAchievementInfo(string id, StringBuilder sb, int size)
		{
			Log("Game Center achievement info is only available on iOS devices.");
		}
	}
}
