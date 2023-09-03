using System;
using System.IO;
using ExitGames.Client.Photon;
using UnityEngine;

internal static class CustomTypes
{
	internal static void Register()
	{
		PhotonPeer.RegisterType(typeof(Vector2), 87, SerializeVector2, DeserializeVector2);
		PhotonPeer.RegisterType(typeof(Vector3), 86, SerializeVector3, DeserializeVector3);
		PhotonPeer.RegisterType(typeof(Transform), 84, SerializeTransform, DeserializeTransform);
		PhotonPeer.RegisterType(typeof(Quaternion), 81, SerializeQuaternion, DeserializeQuaternion);
		PhotonPeer.RegisterType(typeof(PhotonPlayer), 80, SerializePhotonPlayer, DeserializePhotonPlayer);
		PhotonPeer.RegisterType(typeof(PhotonViewID), 73, SerializePhotonViewID, DeserializePhotonViewID);
	}

	private static byte[] SerializeTransform(object customobject)
	{
		Transform transform = (Transform)customobject;
		return Protocol.Serialize(new Vector3[2] { transform.position, transform.eulerAngles });
	}

	private static object DeserializeTransform(byte[] serializedcustomobject)
	{
		return Protocol.Deserialize(serializedcustomobject);
	}

	private static byte[] SerializeVector3(object customobject)
	{
		Vector3 vector = (Vector3)customobject;
		int targetOffset = 0;
		byte[] array = new byte[12];
		Protocol.Serialize(vector.x, array, ref targetOffset);
		Protocol.Serialize(vector.y, array, ref targetOffset);
		Protocol.Serialize(vector.z, array, ref targetOffset);
		return array;
	}

	private static object DeserializeVector3(byte[] bytes)
	{
		Vector3 vector = default(Vector3);
		int offset = 0;
		Protocol.Deserialize(out vector.x, bytes, ref offset);
		Protocol.Deserialize(out vector.y, bytes, ref offset);
		Protocol.Deserialize(out vector.z, bytes, ref offset);
		return vector;
	}

	private static byte[] SerializeVector2(object customobject)
	{
		Vector2 vector = (Vector2)customobject;
		MemoryStream memoryStream = new MemoryStream(8);
		memoryStream.Write(BitConverter.GetBytes(vector.x), 0, 4);
		memoryStream.Write(BitConverter.GetBytes(vector.y), 0, 4);
		return memoryStream.ToArray();
	}

	private static object DeserializeVector2(byte[] bytes)
	{
		Vector2 vector = default(Vector2);
		vector.x = BitConverter.ToSingle(bytes, 0);
		vector.y = BitConverter.ToSingle(bytes, 4);
		return vector;
	}

	private static byte[] SerializeQuaternion(object obj)
	{
		Quaternion quaternion = (Quaternion)obj;
		MemoryStream memoryStream = new MemoryStream(12);
		memoryStream.Write(BitConverter.GetBytes(quaternion.w), 0, 4);
		memoryStream.Write(BitConverter.GetBytes(quaternion.x), 0, 4);
		memoryStream.Write(BitConverter.GetBytes(quaternion.y), 0, 4);
		memoryStream.Write(BitConverter.GetBytes(quaternion.z), 0, 4);
		return memoryStream.ToArray();
	}

	private static object DeserializeQuaternion(byte[] bytes)
	{
		Quaternion quaternion = default(Quaternion);
		quaternion.w = BitConverter.ToSingle(bytes, 0);
		quaternion.x = BitConverter.ToSingle(bytes, 4);
		quaternion.y = BitConverter.ToSingle(bytes, 8);
		quaternion.z = BitConverter.ToSingle(bytes, 12);
		return quaternion;
	}

	private static byte[] SerializePhotonPlayer(object customobject)
	{
		int iD = ((PhotonPlayer)customobject).ID;
		return BitConverter.GetBytes(iD);
	}

	private static object DeserializePhotonPlayer(byte[] bytes)
	{
		int key = BitConverter.ToInt32(bytes, 0);
		if (PhotonNetwork.networkingPeer.mActors.ContainsKey(key))
		{
			return PhotonNetwork.networkingPeer.mActors[key];
		}
		return null;
	}

	private static byte[] SerializePhotonViewID(object customobject)
	{
		int iD = ((PhotonViewID)customobject).ID;
		return BitConverter.GetBytes(iD);
	}

	private static object DeserializePhotonViewID(byte[] bytes)
	{
		int num = BitConverter.ToInt32(bytes, 0);
		int iD = num % PhotonNetwork.MAX_VIEW_IDS;
		int num2 = num / PhotonNetwork.MAX_VIEW_IDS;
		PhotonPlayer owner = null;
		if (num2 > 0)
		{
			owner = PhotonPlayer.Find(num2);
		}
		return new PhotonViewID(iD, owner);
	}
}
