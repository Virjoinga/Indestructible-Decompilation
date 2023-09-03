using System.Collections;
using ExitGames.Client.Photon;
using UnityEngine;

public static class Extensions
{
	public static bool AlmostEquals(this Vector3 target, Vector3 second, float sqrMagniturePrecision)
	{
		return (target - second).sqrMagnitude < sqrMagniturePrecision;
	}

	public static bool AlmostEquals(this Vector2 target, Vector2 second, float sqrMagniturePrecision)
	{
		return (target - second).sqrMagnitude < sqrMagniturePrecision;
	}

	public static bool AlmostEquals(this Quaternion target, Quaternion second, float maxAngle)
	{
		return Quaternion.Angle(target, second) < maxAngle;
	}

	public static bool AlmostEquals(this float target, float second, float floatDiff)
	{
		return Mathf.Abs(target - second) < floatDiff;
	}

	public static void Merge(this IDictionary target, IDictionary addHash)
	{
		if (addHash == null || target.Equals(addHash))
		{
			return;
		}
		foreach (object key in addHash.Keys)
		{
			target[key] = addHash[key];
		}
	}

	public static void MergeStringKeys(this IDictionary target, IDictionary addHash)
	{
		if (addHash == null || target.Equals(addHash))
		{
			return;
		}
		foreach (object key in addHash.Keys)
		{
			if (key is string)
			{
				target[key] = addHash[key];
			}
		}
	}

	public static string ToStringFull(this IDictionary origin)
	{
		return SupportClass.DictionaryToString(origin, false);
	}

	public static Hashtable StripToStringKeys(this IDictionary original)
	{
		Hashtable hashtable = new Hashtable();
		foreach (DictionaryEntry item in original)
		{
			if (item.Key is string)
			{
				hashtable[item.Key] = item.Value;
			}
		}
		return hashtable;
	}

	public static void StripKeysWithNullValues(this IDictionary original)
	{
		object[] array = new object[original.Count];
		original.Keys.CopyTo(array, 0);
		foreach (object key in array)
		{
			if (original[key] == null)
			{
				original.Remove(key);
			}
		}
	}

	public static bool Contains(this int[] target, int nr)
	{
		for (int i = 0; i < target.Length; i++)
		{
			if (target[i] == nr)
			{
				return true;
			}
		}
		return false;
	}
}
