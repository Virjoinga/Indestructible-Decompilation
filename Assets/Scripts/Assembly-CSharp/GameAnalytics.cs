using System;
using System.Collections.Generic;
using Glu.ABTesting;
using Glu.DynamicContentPipeline;
using Glu.Kontagent;
using UnityEngine;

public static class GameAnalytics
{
	public static void GameLogEvent(string id, Dictionary<string, object> data)
	{
		List<string> list = new List<string>();
		foreach (KeyValuePair<string, object> datum in data)
		{
			list.Add(datum.Key);
			list.Add(datum.Value.ToString());
		}
		AStats.Flurry.LogEvent(id, list.ToArray());
	}

	public static void RevenueTracking(int val)
	{
		if (Integrity.IsTrusted())
		{
			Kontagent.RevenueTracking(val);
		}
	}

	private static void MATTrackAction(string eventName)
	{
	}

	private static void MATTrackAction(string eventName, string itemName, float itemPrice, string currencyCode)
	{
	}

	public static string ABTestingVariantId()
	{
		Glu.ABTesting.Resolution aBTestingResolution = DynamicContent.ABTestingResolution;
		return (aBTestingResolution == null) ? null : aBTestingResolution.VariantId;
	}

	public static void EventIAPItemClicked(IAPShopItem item)
	{
		if (item is IAPShopItemSimple)
		{
			GameLogEvent("IAP_BUY_CLICKED", new Dictionary<string, object> { { "productId", item.productId } });
		}
		else if (item is IAPShopItemBoost)
		{
			GameLogEvent("BOOST_IAP_BUY_CLICKED", new Dictionary<string, object> { { "productId", item.productId } });
		}
		Kontagent.LogEvent(item.productId, "Economy", "IAP_CLICKED", ABTestingVariantId(), MonoSingleton<Player>.Instance.Level, (int)Math.Round(((!item.defaultPrice.HasValue) ? 0m : item.defaultPrice.Value) * 100.0m));
	}

	public static void EventIAPItemCancelled(IAPShopItem item)
	{
		if (item is IAPShopItemSimple)
		{
			GameLogEvent("IAP_PURCHASE_CANCELLED", new Dictionary<string, object> { { "productId", item.productId } });
		}
		else if (item is IAPShopItemBoost)
		{
			GameLogEvent("BOOST_IAP_PURCHASE_CANCELLED", new Dictionary<string, object> { { "productId", item.productId } });
		}
		Kontagent.LogEvent(item.productId, "Economy", "IAP_CANCELLED", ABTestingVariantId(), MonoSingleton<Player>.Instance.Level, (int)Math.Round(((!item.defaultPrice.HasValue) ? 0m : item.defaultPrice.Value) * 100.0m));
	}

	public static void EventIAPItemPurchased(IAPShopItem item)
	{
		if (item is IAPShopItemSimple)
		{
			IAPShopItemSimple iAPShopItemSimple = item as IAPShopItemSimple;
			GameLogEvent("IAP_PURCHASED", new Dictionary<string, object>
			{
				{ "productId", item.productId },
				{
					"productPrice",
					item.GetPrice()
				},
				{
					"level",
					MonoSingleton<Player>.Instance.Level
				}
			});
			Kontagent.LogEvent(iAPShopItemSimple.productId, "IAP_PURCHASES", iAPShopItemSimple.GetValueKind(), null, MonoSingleton<Player>.Instance.Level, (int)Math.Round(((!item.defaultPrice.HasValue) ? 0m : item.defaultPrice.Value) * 100.0m));
		}
		else if (item is IAPShopItemBoost)
		{
			GameLogEvent("BOOST_IAP_PURCHASED", new Dictionary<string, object>
			{
				{ "productId", item.productId },
				{
					"productPrice",
					item.GetPrice()
				},
				{
					"moneySoft",
					MonoSingleton<Player>.Instance.MoneySoft
				},
				{
					"experience",
					MonoSingleton<Player>.Instance.Experience
				},
				{
					"level",
					MonoSingleton<Player>.Instance.Level
				}
			});
			Kontagent.LogEvent(item.productId, "IAP_PURCHASES", "BOOST", null, MonoSingleton<Player>.Instance.Level, (int)Math.Round(((!item.defaultPrice.HasValue) ? 0m : item.defaultPrice.Value) * 100.0m));
		}
		Kontagent.LogEvent(item.productId, "Economy", "IAP_SUCCESS", ABTestingVariantId(), MonoSingleton<Player>.Instance.Level, (int)Math.Round(((!item.defaultPrice.HasValue) ? 0m : item.defaultPrice.Value) * 100.0m));
		Kontagent.LogEvent("monetization_transaction", "Monetization", "Transaction", null, MonoSingleton<Player>.Instance.Level, (int)Math.Round(((!item.defaultPrice.HasValue) ? 0m : item.defaultPrice.Value) * 100.0m));
		if (!Integrity.IsJailbroken())
		{
			Kontagent.RevenueTracking((int)Math.Round(((!item.defaultPrice.HasValue) ? 0m : item.defaultPrice.Value) * 100.0m));
			MATTrackAction("iap_purchased", item.productId, (float)((!item.defaultPrice.HasValue) ? 0m : item.defaultPrice.Value), "USD");
		}
		Debug.Log("Revenue tracking v = " + (int)Math.Round(((!item.defaultPrice.HasValue) ? 0m : item.defaultPrice.Value) * 100.0m));
	}

	public static void EventIAPItemFailed(IAPShopItem item, string status)
	{
		if (item is IAPShopItemSimple)
		{
			GameLogEvent("IAP_FAILED", new Dictionary<string, object>
			{
				{ "productId", item.productId },
				{ "status", status }
			});
		}
		else if (item is IAPShopItemBoost)
		{
			GameLogEvent("BOOST_IAP_FAILED", new Dictionary<string, object>
			{
				{ "productId", item.productId },
				{ "status", status }
			});
		}
		Kontagent.LogEvent(item.productId, "Economy", "IAP_FAILED", ABTestingVariantId(), MonoSingleton<Player>.Instance.Level, (int)Math.Round(((!item.defaultPrice.HasValue) ? 0m : item.defaultPrice.Value) * 100.0m));
	}

	public static void EventResetTalents(ShopItemPrice item)
	{
		Kontagent.LogEvent(item.id, "HC_VIRTUAL_GOOD", "TALENT_RESET", ABTestingVariantId(), MonoSingleton<Player>.Instance.Level, item.GetPrice(ShopItemCurrency.Hard));
	}

	public static void EventLoseHard(int price)
	{
		Kontagent.LogEvent("Sink", "SINK_SOURCE", "HC", ABTestingVariantId(), MonoSingleton<Player>.Instance.Level, -price);
	}

	public static void EventLoseSoft(int price)
	{
		Kontagent.LogEvent("Sink", "SINK_SOURCE", "SC", ABTestingVariantId(), MonoSingleton<Player>.Instance.Level, -price);
	}

	public static void EventEarnedSoft(int delta)
	{
		GameLogEvent("SOFT_CURRENCY_EARNED", new Dictionary<string, object>
		{
			{
				"current",
				MonoSingleton<Player>.Instance.MoneySoft
			},
			{ "delta", delta },
			{
				"level",
				MonoSingleton<Player>.Instance.Level
			},
			{
				"league",
				MonoSingleton<Player>.Instance.League
			}
		});
		Kontagent.LogEvent("Source", "SINK_SOURCE", "SC", ABTestingVariantId(), MonoSingleton<Player>.Instance.Level, delta);
	}

	public static void EventEarnedHard(int delta, string source)
	{
		if (MonoSingleton<Player>.Exists())
		{
			GameLogEvent("HARD_CURRENCY_EARNED", new Dictionary<string, object>
			{
				{
					"current",
					MonoSingleton<Player>.Instance.MoneyHard
				},
				{ "delta", delta },
				{
					"level",
					MonoSingleton<Player>.Instance.Level
				},
				{
					"league",
					MonoSingleton<Player>.Instance.League
				}
			});
			Kontagent.LogEvent("Source", "SINK_SOURCE", "HC " + source, ABTestingVariantId(), MonoSingleton<Player>.Instance.Level, delta);
		}
	}

	public static void EventEarnedInfluencePoints(int delta)
	{
		GameLogEvent("INFLUENCE_POINTS_CHANGED", new Dictionary<string, object>
		{
			{
				"current",
				MonoSingleton<Player>.Instance.InfluencePoints
			},
			{ "delta", delta },
			{
				"level",
				MonoSingleton<Player>.Instance.Level
			},
			{
				"league",
				MonoSingleton<Player>.Instance.League
			}
		});
		Kontagent.LogEvent("Influence Changed", "INFLUENCE_CHANGED", ABTestingVariantId(), null, MonoSingleton<Player>.Instance.InfluencePoints, delta);
	}

	public static void EventEarnedEloRate(int delta)
	{
		GameLogEvent("ELO_RATE_CHANGED", new Dictionary<string, object>
		{
			{
				"current",
				MonoSingleton<Player>.Instance.EloRate
			},
			{ "delta", delta },
			{
				"level",
				MonoSingleton<Player>.Instance.Level
			},
			{
				"league",
				MonoSingleton<Player>.Instance.League
			}
		});
		Kontagent.LogEvent("Elo Changed", "ELO_RATING_CHANGED", ABTestingVariantId(), null, MonoSingleton<Player>.Instance.EloRate, delta);
	}

	public static void EventTalentBought(PlayerTalent talent)
	{
		GameLogEvent("TALENT_LEARNED", new Dictionary<string, object>
		{
			{
				"talentId",
				talent.Item.id
			},
			{ "talentLevel", talent.Level },
			{
				"level",
				MonoSingleton<Player>.Instance.Level
			},
			{
				"league",
				MonoSingleton<Player>.Instance.League
			}
		});
		Kontagent.LogEvent(talent.Item.id, "SKILL_LEARNED", ABTestingVariantId(), null, MonoSingleton<Player>.Instance.Level, talent.Level);
	}

	public static void EventResetTalents(int pointsSpent)
	{
		GameLogEvent("RESET_TALENTS", new Dictionary<string, object>
		{
			{ "pointsSpent", pointsSpent },
			{
				"influencePoints",
				MonoSingleton<Player>.Instance.InfluencePoints
			},
			{
				"level",
				MonoSingleton<Player>.Instance.Level
			},
			{
				"league",
				MonoSingleton<Player>.Instance.League
			}
		});
	}

	public static void EventLevelUp()
	{
		GameLogEvent("LEVEL_UP", new Dictionary<string, object>
		{
			{
				"level",
				MonoSingleton<Player>.Instance.Level
			},
			{
				"moneySoft",
				MonoSingleton<Player>.Instance.MoneySoft
			},
			{
				"moneyHard",
				MonoSingleton<Player>.Instance.MoneyHard
			}
		});
		Kontagent.LogEvent(MonoSingleton<Player>.Instance.Level.ToString(), "LEVEL_UP", ABTestingVariantId(), null, MonoSingleton<Player>.Instance.Level, MonoSingleton<Player>.Instance.Statistics.MultiplayerGamesPlayed);
	}

	public static void EventTryToBuyDurableSoft(ShopItem item, int price)
	{
		GameLogEvent("SHOP_SC_ITEM_BUY_BUTTON_TAP", new Dictionary<string, object>
		{
			{ "itemId", item.id },
			{ "price", price }
		});
		Kontagent.LogEvent(item.id, "BUTTON_TAP_ITEM_BUY", "SC", item.ItemType.ToString().ToUpper(), MonoSingleton<Player>.Instance.Level, price);
	}

	public static void EventTryToBuyDurableHard(ShopItem item, int price)
	{
		GameLogEvent("SHOP_HC_ITEM_BUY_BUTTON_TAP", new Dictionary<string, object>
		{
			{ "itemId", item.id },
			{ "price", price }
		});
		Kontagent.LogEvent(item.id, "BUTTON_TAP_ITEM_BUY", "HC", item.ItemType.ToString().ToUpper(), MonoSingleton<Player>.Instance.Level, price);
	}

	public static void EventPurchaseDurableSoft(ShopItem item, int price)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary.Add("itemId", item.id);
		dictionary.Add("price", price);
		dictionary.Add("type", item.ItemType);
		dictionary.Add("stats", item.GetParameters());
		dictionary.Add("level", MonoSingleton<Player>.Instance.Level);
		dictionary.Add("league", MonoSingleton<Player>.Instance.League);
		Dictionary<string, object> dictionary2 = dictionary;
		string text = "SHOP_SC_DURABLE_PURCHASED";
		GameLogEvent(text, new Dictionary<string, object>(dictionary2));
		text = text + "_" + item.id.ToUpper();
		GameLogEvent(text, dictionary2);
		Kontagent.LogEvent(item.id, "SC_VIRTUAL_GOOD", item.ItemType.ToString().ToUpper(), ABTestingVariantId(), MonoSingleton<Player>.Instance.Level, price);
	}

	public static void EventPurchaseDurableHard(ShopItem item, int price)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary.Add("itemId", item.id);
		dictionary.Add("price", price);
		dictionary.Add("type", item.ItemType);
		dictionary.Add("stats", item.GetParameters());
		dictionary.Add("level", MonoSingleton<Player>.Instance.Level);
		dictionary.Add("league", MonoSingleton<Player>.Instance.League);
		Dictionary<string, object> dictionary2 = dictionary;
		string text = "SHOP_HC_DURABLE_PURCHASED";
		GameLogEvent(text, new Dictionary<string, object>(dictionary2));
		text = text + "_" + item.id.ToUpper();
		GameLogEvent(text, dictionary2);
		Kontagent.LogEvent(item.id, "HC_VIRTUAL_GOOD", item.ItemType.ToString().ToUpper(), ABTestingVariantId(), MonoSingleton<Player>.Instance.Level, price);
	}

	public static void EventItemDetailsDialog(ShopItem item)
	{
		GameLogEvent("SHOP_ITEM_DETAILS", new Dictionary<string, object>
		{
			{ "itemId", item.id },
			{
				"price",
				item.GetPrice(ShopItemCurrency.None)
			},
			{
				"currency",
				item.GetPriceKind()
			},
			{
				"type",
				item.ItemType.ToString().ToUpper()
			}
		});
		Kontagent.LogEvent(item.id, "BUTTON_TAP_ITEM_INFO", item.GetPriceKind(), item.ItemType.ToString().ToUpper(), MonoSingleton<Player>.Instance.Level, item.GetPrice(ShopItemCurrency.None));
	}

	public static void EventAddMoreCurrency(string kind)
	{
		UnityEngine.Object @object = UnityEngine.Object.FindObjectOfType(typeof(PanelManager));
		PanelManager panelManager = @object as PanelManager;
		if (!(panelManager == null))
		{
			PanelManagerPanel activePanel = panelManager.GetActivePanel();
			if (!(activePanel == null))
			{
				GameLogEvent("ADD_MORE_CURRENCY", new Dictionary<string, object>
				{
					{ "panel", activePanel.Name },
					{ "group", kind },
					{
						"level",
						MonoSingleton<Player>.Instance.Level
					},
					{
						"league",
						MonoSingleton<Player>.Instance.League
					}
				});
				Kontagent.LogEvent("ADD_MORE_CLICKED", "BUTTON_TAP_ADD_MORE", kind, activePanel.Name, MonoSingleton<Player>.Instance.Level, null);
			}
		}
	}

	public static void EventSingleStart(SelectManager manager)
	{
		GameLogEvent("SINGLE_STARTED", new Dictionary<string, object>
		{
			{ "mode", manager.SelectedMode },
			{ "game", manager.SelectedGame },
			{ "map", manager.SelectedMap },
			{
				"level",
				MonoSingleton<Player>.Instance.Level
			}
		});
		GarageVehicle selectedVehicle = MonoSingleton<Player>.Instance.SelectedVehicle;
		if (selectedVehicle != null)
		{
			Kontagent.LogEvent(selectedVehicle.Vehicle.id, "SINGLEPLAYER_STARTED", manager.SelectedMap, ABTestingVariantId(), MonoSingleton<Player>.Instance.Level, null);
		}
	}

	public static void EventSingleFinish(IDTGame game, string reason)
	{
		if (game == null)
		{
			return;
		}
		SingleGame singleGame = game as SingleGame;
		if (!(singleGame == null))
		{
			GameLogEvent("SINGLE_FINISHED", new Dictionary<string, object>
			{
				{ "kills", singleGame.killCount },
				{
					"game",
					MonoSingleton<Player>.Instance.LastPlayedGame
				},
				{
					"map",
					MonoSingleton<Player>.Instance.LastPlayedMap
				},
				{
					"level",
					MonoSingleton<Player>.Instance.Level
				},
				{ "reason", reason }
			});
			GarageVehicle selectedVehicle = MonoSingleton<Player>.Instance.SelectedVehicle;
			if (selectedVehicle != null)
			{
				Kontagent.LogEvent(selectedVehicle.Vehicle.id, "SINGLEPLAYER_QUIT", MonoSingleton<Player>.Instance.LastPlayedMap, ABTestingVariantId(), MonoSingleton<Player>.Instance.Level, singleGame.killCount);
				Kontagent.LogEvent(selectedVehicle.Vehicle.id, "SINGLEPLAYER_COMPLETED", MonoSingleton<Player>.Instance.LastPlayedMap, ABTestingVariantId(), MonoSingleton<Player>.Instance.Level, singleGame.killCount);
			}
		}
	}

	public static void EventMultiplayerStart()
	{
		Player instance = MonoSingleton<Player>.Instance;
		GameLogEvent("MULTIPLAYER_STARTED", new Dictionary<string, object>
		{
			{ "mode", instance.LastPlayedMode },
			{ "game", instance.LastPlayedGame },
			{ "map", instance.LastPlayedMap },
			{ "players", instance.LastPlayedPlayers },
			{ "level", instance.Level }
		});
		GarageVehicle selectedVehicle = MonoSingleton<Player>.Instance.SelectedVehicle;
		if (selectedVehicle != null)
		{
			Kontagent.LogEvent(selectedVehicle.Vehicle.id, "MULTIPLAYER_STARTED", instance.LastPlayedMode, ABTestingVariantId(), MonoSingleton<Player>.Instance.Level, null);
		}
	}

	public static void EventMultiplayerFinish(ref IDTGame.Reward reward)
	{
		MultiplayerGame multiplayerGame = IDTGame.Instance as MultiplayerGame;
		if (multiplayerGame == null)
		{
			return;
		}
		int num = 0;
		foreach (GamePlayer player in multiplayerGame.players)
		{
			num++;
		}
		GameLogEvent("MULTIPLAYER_FINISHED", new Dictionary<string, object>
		{
			{
				"mode",
				MonoSingleton<Player>.Instance.LastPlayedMode
			},
			{
				"game",
				MonoSingleton<Player>.Instance.LastPlayedGame
			},
			{
				"map",
				MonoSingleton<Player>.Instance.LastPlayedMap
			},
			{
				"players",
				MonoSingleton<Player>.Instance.LastPlayedPlayers
			},
			{ "playersLeft", num }
		});
		Kontagent.LogEvent((!reward.Victory) ? "LOSE" : "WIN", "MULTIPLAYER_COMPLETED", MonoSingleton<Player>.Instance.LastPlayedMode, ABTestingVariantId(), MonoSingleton<Player>.Instance.Level, multiplayerGame.localPlayer.killCount);
	}

	public static void EventMultiplayerDisconnect()
	{
		GameLogEvent("MULTIPLAYER_DISCONNECT", new Dictionary<string, object>
		{
			{
				"mode",
				MonoSingleton<Player>.Instance.LastPlayedMode
			},
			{
				"game",
				MonoSingleton<Player>.Instance.LastPlayedGame
			},
			{
				"map",
				MonoSingleton<Player>.Instance.LastPlayedMap
			},
			{
				"players",
				MonoSingleton<Player>.Instance.LastPlayedPlayers
			},
			{
				"level",
				MonoSingleton<Player>.Instance.Level
			}
		});
		GarageVehicle selectedVehicle = MonoSingleton<Player>.Instance.SelectedVehicle;
		if (selectedVehicle != null)
		{
			Kontagent.LogEvent(selectedVehicle.Vehicle.id, "MULTIPLAYER_DISCONNECT", MonoSingleton<Player>.Instance.LastPlayedMode, ABTestingVariantId(), MonoSingleton<Player>.Instance.Level, null);
		}
	}

	public static void EventMultiplayerKillsInGame()
	{
		GameLogEvent("MULTIPLAYER_KILLS_GAME", new Dictionary<string, object>
		{
			{
				"kills",
				MonoSingleton<Player>.Instance.Statistics.MultiplayerLastPlayedKills
			},
			{
				"mode",
				MonoSingleton<Player>.Instance.LastPlayedMode
			},
			{
				"game",
				MonoSingleton<Player>.Instance.LastPlayedGame
			},
			{
				"map",
				MonoSingleton<Player>.Instance.LastPlayedMap
			},
			{
				"players",
				MonoSingleton<Player>.Instance.LastPlayedPlayers
			},
			{
				"level",
				MonoSingleton<Player>.Instance.Level
			},
			{
				"league",
				MonoSingleton<Player>.Instance.League
			}
		});
	}

	public static void EventMultiplayerFlagsInGame()
	{
		GameLogEvent("MULTIPLAYER_FLAGS_CAPTURED_GAME", new Dictionary<string, object>
		{
			{
				"flags",
				MonoSingleton<Player>.Instance.Statistics.MultiplayerLastPlayedFlags
			},
			{
				"mode",
				MonoSingleton<Player>.Instance.LastPlayedMode
			},
			{
				"map",
				MonoSingleton<Player>.Instance.LastPlayedMap
			},
			{
				"players",
				MonoSingleton<Player>.Instance.LastPlayedPlayers
			},
			{
				"level",
				MonoSingleton<Player>.Instance.Level
			},
			{
				"league",
				MonoSingleton<Player>.Instance.League
			}
		});
	}

	public static void EventMultiplayerGameLeft(IDTGame game)
	{
		if (game == null)
		{
			return;
		}
		MultiplayerGame multiplayerGame = game as MultiplayerGame;
		if (!(multiplayerGame == null))
		{
			GamePlayer localPlayer = multiplayerGame.localPlayer;
			GameLogEvent("MULTIPLAYER_GAME_LEFT", new Dictionary<string, object>
			{
				{
					"mode",
					MonoSingleton<Player>.Instance.LastPlayedMode
				},
				{
					"game",
					MonoSingleton<Player>.Instance.LastPlayedGame
				},
				{
					"map",
					MonoSingleton<Player>.Instance.LastPlayedMap
				},
				{
					"players",
					MonoSingleton<Player>.Instance.LastPlayedPlayers
				},
				{
					"level",
					MonoSingleton<Player>.Instance.Level
				},
				{ "kills", localPlayer.killCount },
				{ "deaths", localPlayer.deathCount }
			});
			GarageVehicle selectedVehicle = MonoSingleton<Player>.Instance.SelectedVehicle;
			if (selectedVehicle != null)
			{
				Kontagent.LogEvent(selectedVehicle.Vehicle.id, "MULTIPLAYER_QUIT", MonoSingleton<Player>.Instance.LastPlayedMode, ABTestingVariantId(), MonoSingleton<Player>.Instance.Level, localPlayer.killCount);
			}
		}
	}

	public static void EventRoomCreated()
	{
		GameLogEvent("ROOM_CREATED", new Dictionary<string, object>
		{
			{
				"mode",
				MonoSingleton<Player>.Instance.LastPlayedMode
			},
			{
				"game",
				MonoSingleton<Player>.Instance.LastPlayedGame
			},
			{
				"map",
				MonoSingleton<Player>.Instance.LastPlayedMap
			},
			{
				"players",
				MonoSingleton<Player>.Instance.LastPlayedPlayers
			},
			{
				"level",
				MonoSingleton<Player>.Instance.Level
			},
			{
				"league",
				MonoSingleton<Player>.Instance.League
			}
		});
		Kontagent.LogEvent(MonoSingleton<Player>.Instance.LastPlayedPlayers.ToString(), "ROOM_CREATED", MonoSingleton<Player>.Instance.LastPlayedMode, ABTestingVariantId(), MonoSingleton<Player>.Instance.Level, MonoSingleton<Player>.Instance.League);
	}

	public static void EventRoomJoined()
	{
		GameLogEvent("ROOM_JOINED", new Dictionary<string, object>
		{
			{
				"mode",
				MonoSingleton<Player>.Instance.LastPlayedMode
			},
			{
				"game",
				MonoSingleton<Player>.Instance.LastPlayedGame
			},
			{
				"map",
				MonoSingleton<Player>.Instance.LastPlayedMap
			},
			{
				"players",
				MonoSingleton<Player>.Instance.LastPlayedPlayers
			},
			{
				"level",
				MonoSingleton<Player>.Instance.Level
			},
			{
				"league",
				MonoSingleton<Player>.Instance.League
			}
		});
		Kontagent.LogEvent(MonoSingleton<Player>.Instance.LastPlayedPlayers.ToString(), "ROOM_JOINED", MonoSingleton<Player>.Instance.LastPlayedMode, ABTestingVariantId(), MonoSingleton<Player>.Instance.Level, MonoSingleton<Player>.Instance.League);
	}

	public static void EventOpenCount(int count, int level)
	{
		GameLogEvent("OPEN_COUNT", new Dictionary<string, object>
		{
			{ "count", count },
			{ "level", level }
		});
		Kontagent.LogEvent("Count", "AppCount", ABTestingVariantId(), null, level, count);
	}

	public static void EventMatchmakingFailed(string reason)
	{
		GameLogEvent("MATCHMAKING_FAILED", new Dictionary<string, object> { { "reason", reason } });
		Kontagent.LogEvent(reason, "MATCHMAKING_FAILED", null, null, null, null);
	}

	public static void EventPlayerVehicle(GarageVehicle vehicle)
	{
		if (vehicle == null)
		{
			return;
		}
		string value = ((vehicle.Vehicle == null) ? string.Empty : vehicle.Vehicle.id);
		string value2 = ((vehicle.Weapon == null) ? string.Empty : vehicle.Weapon.id);
		string value3 = ((vehicle.Armor == null) ? string.Empty : vehicle.Armor.id);
		string value4 = ((vehicle.Body == null) ? string.Empty : vehicle.Body.id);
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary.Add("vehicleId", value);
		dictionary.Add("weaponId", value2);
		dictionary.Add("armorId", value3);
		dictionary.Add("bodyId", value4);
		Dictionary<string, object> dictionary2 = dictionary;
		for (int i = 0; i < vehicle.Components.Length; i++)
		{
			string value5 = string.Empty;
			if (vehicle.Components[i] != null)
			{
				value5 = vehicle.Components[i].id;
			}
			string key = "componentId" + i;
			dictionary2.Add(key, value5);
		}
		GameLogEvent("PLAYER_VEHICLE", dictionary2);
	}

	public static void EventCTFFinish(CTFGame game, ref IDTGame.Reward reward)
	{
		if (!(game == null))
		{
			GameLogEvent("MULTIPLAYER_CTF_FINISH", new Dictionary<string, object>
			{
				{
					"teamId",
					game.localPlayer.teamID
				},
				{ "victory", reward.Victory }
			});
		}
	}

	public static void EventTapJoyButtonPressed(string source)
	{
		GameLogEvent("TAPJOY_BUTTON_PRESSED", new Dictionary<string, object>
		{
			{ "source", source },
			{
				"level",
				MonoSingleton<Player>.Instance.Level
			}
		});
		Kontagent.LogEvent("TAPJOY_CLICKED", "BUTTON_TAP_TAPJOY", null, null, MonoSingleton<Player>.Instance.Level, null);
	}

	public static void EventTapJoyPointsReceived(int points)
	{
		GameLogEvent("TAPJOY_POINTS_RECEIVED", new Dictionary<string, object>
		{
			{ "points", points },
			{
				"level",
				MonoSingleton<Player>.Instance.Level
			}
		});
	}

	public static void EventProfileButtonTap()
	{
		GameLogEvent("PROFILE_BUTTON_TAP", new Dictionary<string, object>());
		Kontagent.LogEvent("PROFILE_CLICKED", "BUTTON_TAP_PROFILE", null, null, MonoSingleton<Player>.Instance.Level, null);
	}

	public static void EventBuyNewVehiclesButtonTap()
	{
		GameLogEvent("BUY_NEW_VEHICLES_BUTTON_TAP", new Dictionary<string, object>());
		GarageVehicle selectedVehicle = MonoSingleton<Player>.Instance.SelectedVehicle;
		if (selectedVehicle != null)
		{
			Kontagent.LogEvent(selectedVehicle.Vehicle.id, "BUTTON_TAP_BUY_NEW_VEHICLES", null, null, MonoSingleton<Player>.Instance.Level, null);
		}
	}

	public static void EventPlayButtonTap(string panel)
	{
		GameLogEvent("PLAY_BUTTON_TAP", new Dictionary<string, object> { { "panel", panel } });
		Kontagent.LogEvent("PLAY_CLICKED", "BUTTON_TAP_PLAY", panel, null, MonoSingleton<Player>.Instance.Level, null);
	}

	public static void EventPaintJobsButtonTap()
	{
		GarageVehicle selectedVehicle = MonoSingleton<Player>.Instance.SelectedVehicle;
		if (selectedVehicle != null)
		{
			GameLogEvent("PAINT_JOBS_BUTTON_TAP", new Dictionary<string, object>
			{
				{
					"vehicleId",
					selectedVehicle.Vehicle.id
				},
				{
					"bodyId",
					selectedVehicle.Body.id
				}
			});
			Kontagent.LogEvent(selectedVehicle.Body.id, "BUTTON_TAP_PAINT_JOBS", selectedVehicle.Vehicle.id, null, MonoSingleton<Player>.Instance.Level, null);
		}
	}

	public static void EventCustomizeButtonTap(string source)
	{
		GameLogEvent("CUSTOMIZE_BUTTON_TAP", new Dictionary<string, object>
		{
			{
				"level",
				MonoSingleton<Player>.Instance.Level
			},
			{
				"moneySoft",
				MonoSingleton<Player>.Instance.MoneySoft
			},
			{
				"moneyHard",
				MonoSingleton<Player>.Instance.MoneyHard
			},
			{ "source", source }
		});
		Kontagent.LogEvent("CUSTOMIZE_CLICKED", "BUTTON_TAP_CUSTOMIZE", source, null, MonoSingleton<Player>.Instance.Level, null);
	}

	public static void EventLeagueChanged(int delta)
	{
		int league = MonoSingleton<Player>.Instance.League;
		GameLogEvent("LEAGUE_CHANGED", new Dictionary<string, object>
		{
			{ "league", league },
			{
				"leaguePrevious",
				league - delta
			},
			{
				"level",
				MonoSingleton<Player>.Instance.Level
			},
			{
				"eloRate",
				MonoSingleton<Player>.Instance.EloRate
			},
			{
				"moneySoft",
				MonoSingleton<Player>.Instance.MoneySoft
			},
			{
				"moneyHard",
				MonoSingleton<Player>.Instance.MoneyHard
			}
		});
		Kontagent.LogEvent(league.ToString(), "LEAGUE_CHANGED", ABTestingVariantId(), (league - delta).ToString(), null, null);
	}

	public static void EventGallonPurchased(ShopItemPrice item)
	{
		int price = item.GetPrice(ShopItemCurrency.None);
		string value = price + " " + item.GetPriceKind();
		int num = MonoSingleton<Player>.Instance.BoughtItemsCount(ShopItemType.Vehicle);
		GarageVehicle selectedVehicle = MonoSingleton<Player>.Instance.SelectedVehicle;
		if (selectedVehicle != null)
		{
			GameLogEvent("FUEL_PURCHASED_GALLON", new Dictionary<string, object>
			{
				{
					"bodyId",
					selectedVehicle.Body.id
				},
				{
					"gallons",
					selectedVehicle.Fuel.GetLevel()
				},
				{
					"moneySoft",
					MonoSingleton<Player>.Instance.MoneySoft
				},
				{
					"moneyHard",
					MonoSingleton<Player>.Instance.MoneyHard
				},
				{ "vehicles", num },
				{ "price", value }
			});
			Kontagent.LogEvent(item.id, "HC_VIRTUAL_GOOD", "FUEL_GALLON", ABTestingVariantId(), MonoSingleton<Player>.Instance.Level, price);
		}
	}

	public static void EventDayTankPurchased(ShopItemPrice item)
	{
		int price = item.GetPrice(ShopItemCurrency.None);
		string value = price + " " + item.GetPriceKind();
		int num = MonoSingleton<Player>.Instance.BoughtItemsCount(ShopItemType.Vehicle);
		GarageVehicle selectedVehicle = MonoSingleton<Player>.Instance.SelectedVehicle;
		if (selectedVehicle != null)
		{
			GameLogEvent("FUEL_PURCHASED_DAY_TANK", new Dictionary<string, object>
			{
				{
					"bodyId",
					selectedVehicle.Body.id
				},
				{
					"gallons",
					selectedVehicle.Fuel.GetLevel()
				},
				{
					"moneySoft",
					MonoSingleton<Player>.Instance.MoneySoft
				},
				{
					"moneyHard",
					MonoSingleton<Player>.Instance.MoneyHard
				},
				{ "vehicles", num },
				{ "price", value }
			});
			Kontagent.LogEvent(item.id, "HC_VIRTUAL_GOOD", "FUEL_DAY_TANK", ABTestingVariantId(), MonoSingleton<Player>.Instance.Level, price);
		}
	}

	public static void EventDialogLevelUpAction(string action)
	{
		GameLogEvent("LEVEL_UP_DIALOG_ACTION", new Dictionary<string, object>
		{
			{
				"level",
				MonoSingleton<Player>.Instance.Level
			},
			{ "action", action }
		});
		Kontagent.LogEvent(action, "LEVEL_UP_ACTION", ABTestingVariantId(), MonoSingleton<Player>.Instance.Level.ToString(), MonoSingleton<Player>.Instance.Level, null);
	}

	public static void EventAchievementReceived(PlayerAchievements.Achievement a)
	{
		if (a != null)
		{
			GarageVehicle selectedVehicle = MonoSingleton<Player>.Instance.SelectedVehicle;
			if (selectedVehicle != null)
			{
				string value = ((selectedVehicle.Vehicle == null) ? string.Empty : selectedVehicle.Vehicle.id);
				string value2 = ((selectedVehicle.Weapon == null) ? string.Empty : selectedVehicle.Weapon.id);
				GameLogEvent("ACHIEVEMENT_RECEIVED", new Dictionary<string, object>
				{
					{ "id", a.Id },
					{
						"level",
						MonoSingleton<Player>.Instance.Level
					},
					{ "vehicleId", value },
					{ "weaponId", value2 }
				});
				Kontagent.LogEvent(a.Id, "ACHIEVEMENT_UNLOCKED", ABTestingVariantId(), null, MonoSingleton<Player>.Instance.Level, MonoSingleton<Player>.Instance.Statistics.MultiplayerGamesPlayed);
			}
		}
	}

	public static void EventTutorialGameMode(string mode)
	{
		GameLogEvent("TUTORIAL_GAME_MODE", new Dictionary<string, object> { { "mode", mode } });
	}

	public static void EventMatchmakerConnectionFinished(NetworkManager.Result result, float time, int preferredServer, int currentServer)
	{
		GameLogEvent("MATCHMAKER_CONNECTION_FINISHED", new Dictionary<string, object>
		{
			{ "result", result },
			{ "time", time },
			{ "preferredServer", preferredServer },
			{ "currentServer", currentServer }
		});
		Kontagent.LogEvent(result.ToString().ToUpper(), "MATCHMAKING_CONNECTION_FINISHED", null, null, null, null);
	}

	public static void EventMatchmakerFoundMatch(float time, int playerCount, int selectionCount, int maxLeaguesDiff, int maxRatingDiff)
	{
		GameLogEvent("MATCHMAKER_FOUND_MATCH", new Dictionary<string, object>
		{
			{ "time", time },
			{ "playerCount", playerCount },
			{ "selectionCount", selectionCount },
			{ "maxLeaguesDiff", maxLeaguesDiff },
			{ "maxRatingDiff", maxRatingDiff }
		});
		Kontagent.LogEvent("Match Found", "MATCHMAKING_MATCH_FOUND", null, null, (int)time, selectionCount);
	}

	public static void EventFacebookButtonTap(string source)
	{
		GameLogEvent("FACEBOOK_LOGIN_BUTTON_TAP", new Dictionary<string, object> { { "source", source } });
		Kontagent.LogEvent("FACEBOOK_LOGIN_CLICKED", "FACEBOOK_LOGIN_CLICKED", source, null, MonoSingleton<Player>.Instance.Level, null);
	}

	public static void EventFacebookLoggedIn(string source, bool rewarded)
	{
		GameLogEvent("FACEBOOK_LOGGED_IN", new Dictionary<string, object>
		{
			{ "source", source },
			{
				"reward",
				!rewarded
			},
			{
				"level",
				MonoSingleton<Player>.Instance.Level
			}
		});
		Kontagent.LogEvent("FACEBOOK_LOGGED_IN", "FACEBOOK_LOGGED_IN", source, rewarded ? "REWARD NO" : "REWARD YES", MonoSingleton<Player>.Instance.Level, null);
	}

	public static void EventFacebookLoggedOut()
	{
		GameLogEvent("FACEBOOK_LOGGED_OUT", new Dictionary<string, object> { 
		{
			"level",
			MonoSingleton<Player>.Instance.Level
		} });
		Kontagent.LogEvent("FACEBOOK_LOGED_OUT", "FACEBOOK_LOGED_OUT", null, null, MonoSingleton<Player>.Instance.Level, null);
	}

	public static void EventFirstVehicleSelected(string selectedVehicleId, string freeVehicleId)
	{
		GameLogEvent("FIRST_VEHICLE_SELECTED", new Dictionary<string, object>
		{
			{ "selected", selectedVehicleId },
			{ "free", freeVehicleId }
		});
		Kontagent.LogEvent(selectedVehicleId, "TUTORIAL_VEHICLE_SELECTED", ABTestingVariantId(), freeVehicleId, null, null);
	}

	public static void EventTutorialMatchStarted()
	{
		GameLogEvent("TUTORIAL_MATCH_STARTED", new Dictionary<string, object>());
		GarageVehicle selectedVehicle = MonoSingleton<Player>.Instance.SelectedVehicle;
		if (selectedVehicle != null)
		{
			Kontagent.LogEvent("TUTORIAL_STARTED", "TUTORIAL_STARTED", selectedVehicle.Vehicle.id, ABTestingVariantId(), null, null);
		}
	}

	public static void EventTutorialMatchFinished(string reason)
	{
		TeamDeathmatchGame teamDeathmatchGame = IDTGame.Instance as TeamDeathmatchGame;
		if (!(teamDeathmatchGame == null) && teamDeathmatchGame.IsTutorial && !(teamDeathmatchGame.match == null) && !teamDeathmatchGame.match.isOnline)
		{
			GameLogEvent("TUTORIAL_MATCH_FINISHED", new Dictionary<string, object> { { "reason", reason } });
			GarageVehicle selectedVehicle = MonoSingleton<Player>.Instance.SelectedVehicle;
			if (selectedVehicle != null)
			{
				Kontagent.LogEvent(reason, "TUTORIAL_FINISHED", selectedVehicle.Vehicle.id, ABTestingVariantId(), null, null);
				MATTrackAction("tutorial_complete");
			}
		}
	}

	public static void EventDailyChallengeCompleted(string challengeId, int difficultyGroup)
	{
		GameLogEvent("DAILY_CHALLENGE_COMPLETED", new Dictionary<string, object>
		{
			{ "challenge_id", challengeId },
			{ "group", difficultyGroup },
			{
				"level",
				MonoSingleton<Player>.Instance.Level.Get()
			}
		});
		Kontagent.LogEvent(challengeId, "DAILY_CHALLENGE_COMPLETED", difficultyGroup.ToString(), null, MonoSingleton<Player>.Instance.Level.Get(), null);
	}

	public static void EventDailyChallengeAllCompleted()
	{
		int num = MonoSingleton<Player>.Instance.Level.Get();
		List<DailyChallenges.DailyChallenge> completed = MonoSingleton<Player>.Instance.Challenges.GetCompleted();
		string[] array = new string[3]
		{
			string.Empty,
			string.Empty,
			string.Empty
		};
		foreach (DailyChallenges.DailyChallenge item in completed)
		{
			int group = item.Group;
			if (group < array.Length)
			{
				array[group] = item.Id;
			}
		}
		GameLogEvent("DAILY_CHALLENGE_ALL_COMPLETED", new Dictionary<string, object>
		{
			{ "level", num },
			{
				"challenge_1",
				array[0]
			},
			{
				"challenge_2",
				array[1]
			},
			{
				"challenge_3",
				array[2]
			}
		});
		Kontagent.LogEvent("CHALLENGE_ALL_COMPLETED", "DAILY_CHALLENGE_ALL_COMPLETED", null, null, num, 10, new Dictionary<string, string>
		{
			{
				"challenge_1",
				array[0]
			},
			{
				"challenge_2",
				array[1]
			},
			{
				"challenge_3",
				array[2]
			}
		});
	}

	public static void EventCampaignBattleResult(bool won)
	{
		int num = -1;
		num = ((!won) ? (MonoSingleton<Player>.Instance.LastWonBossFight + 1 + 1) : (MonoSingleton<Player>.Instance.LastWonBossFight + 1));
		int num2 = MonoSingleton<Player>.Instance.Level;
		GarageVehicle selectedVehicle = MonoSingleton<Player>.Instance.SelectedVehicle;
		string id = selectedVehicle.Vehicle.id;
		string id2 = selectedVehicle.Weapon.id;
		string value = ((selectedVehicle.Ammunition == null) ? string.Empty : selectedVehicle.Ammunition.id);
		string lastPlayedMap = MonoSingleton<Player>.Instance.LastPlayedMap;
		string[] array = new string[3]
		{
			string.Empty,
			string.Empty,
			string.Empty
		};
		int num3 = 0;
		ShopItemComponent[] components = selectedVehicle.Components;
		foreach (ShopItemComponent shopItemComponent in components)
		{
			if (shopItemComponent != null)
			{
				array[num3] = shopItemComponent.id;
			}
			num3++;
			if (num3 >= array.Length)
			{
				break;
			}
		}
		GameLogEvent((!won) ? "CAMPAIGN_BATTLE_LOST" : "CAMPAIGN_BATTLE_WON", new Dictionary<string, object>
		{
			{ "battle_number", num },
			{ "level", num2 },
			{ "map", lastPlayedMap }
		});
		if (won)
		{
			GameLogEvent("CAMPAIGN_BATTLE_WON_" + num, new Dictionary<string, object>
			{
				{ "level", num2 },
				{ "vehicle", id },
				{ "weapon", id2 },
				{
					"component_1",
					array[0]
				},
				{
					"component_2",
					array[1]
				},
				{
					"component_3",
					array[2]
				},
				{ "ammunition", value },
				{ "map", lastPlayedMap }
			});
		}
		Kontagent.LogEvent("Battle_" + num, "CAMPAIGN_BATTLE", (!won) ? "LOST" : "WON", null, num2, null, new Dictionary<string, string>
		{
			{
				"level",
				num2.ToString()
			},
			{ "vehicle", id },
			{ "weapon", id2 },
			{
				"component_1",
				array[0]
			},
			{
				"component_2",
				array[1]
			},
			{
				"component_3",
				array[2]
			},
			{ "ammunition", value },
			{ "map", lastPlayedMap }
		});
	}

	public static void EventNameEnterPrompted()
	{
		GameLogEvent("NAME_ENTER_PROMPTED", new Dictionary<string, object> { 
		{
			"level",
			MonoSingleton<Player>.Instance.Level.Get()
		} });
		Kontagent.LogEvent("name_enter_prompted", "NAME_ENTER", null, null, MonoSingleton<Player>.Instance.Level.Get(), null);
	}

	public static void EventNameEntered(string name)
	{
		GameLogEvent("NAME_ENTERED", new Dictionary<string, object>
		{
			{ "name", name },
			{
				"level",
				MonoSingleton<Player>.Instance.Level.Get()
			}
		});
		Kontagent.LogEvent(name, "NAME_ENTER", null, null, MonoSingleton<Player>.Instance.Level.Get(), null);
	}
}
