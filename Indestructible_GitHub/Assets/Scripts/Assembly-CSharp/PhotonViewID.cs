using System;

public class PhotonViewID
{
	private PhotonPlayer internalOwner;

	private int internalID = -1;

	public int ID
	{
		get
		{
			if (internalOwner == null)
			{
				return internalID;
			}
			return internalOwner.ID * PhotonNetwork.MAX_VIEW_IDS + internalID;
		}
	}

	public bool isMine
	{
		get
		{
			return owner.isLocal;
		}
	}

	public PhotonPlayer owner
	{
		get
		{
			int iD = ID / PhotonNetwork.MAX_VIEW_IDS;
			return PhotonPlayer.Find(iD);
		}
	}

	[Obsolete("Used for compatibility with Unity networking only.")]
	public static PhotonViewID unassigned
	{
		get
		{
			return new PhotonViewID(-1, null);
		}
	}

	public PhotonViewID(int ID, PhotonPlayer owner)
	{
		internalID = ID;
		internalOwner = owner;
	}

	public override string ToString()
	{
		return ID.ToString();
	}

	public override bool Equals(object p)
	{
		PhotonViewID photonViewID = p as PhotonViewID;
		return photonViewID != null && ID == photonViewID.ID;
	}

	public override int GetHashCode()
	{
		return ID;
	}
}
