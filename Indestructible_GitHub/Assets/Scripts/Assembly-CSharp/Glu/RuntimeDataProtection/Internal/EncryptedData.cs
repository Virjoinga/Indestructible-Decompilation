using System;

namespace Glu.RuntimeDataProtection.Internal
{
	internal sealed class EncryptedData : ICloneable
	{
		private byte[] data;

		private uint key;

		public int DataLength
		{
			get
			{
				return (data != null) ? data.Length : 0;
			}
		}

		public EncryptedData()
		{
		}

		private EncryptedData(EncryptedData other)
		{
			if (other.data != null)
			{
				byte[] array = (byte[])other.data.Clone();
				Encryption.Encrypt(array, 0, array.Length, other.key);
				TransferUnencryptedData(array);
			}
		}

		object ICloneable.Clone()
		{
			return Clone();
		}

		public void Set(byte[] arr, int offset, int length)
		{
			if (length == 0)
			{
				Reset();
				return;
			}
			byte[] destinationArray = new byte[length];
			Array.Copy(arr, offset, destinationArray, 0, length);
			TransferUnencryptedData(destinationArray);
		}

		public int Get(byte[] arr, int offset, int length)
		{
			if (data == null)
			{
				return 0;
			}
			int num = Math.Min(length, data.Length);
			Array.Copy(data, 0, arr, offset, num);
			Encryption.Encrypt(arr, offset, num, key);
			return num;
		}

		public void Reset()
		{
			data = null;
			key = 0u;
		}

		public EncryptedData Clone()
		{
			return new EncryptedData(this);
		}

		private void TransferUnencryptedData(byte[] data)
		{
			uint num = StaticData.GenerateUInt();
			Encryption.Encrypt(data, 0, data.Length, num);
			this.data = data;
			key = num;
		}
	}
}
