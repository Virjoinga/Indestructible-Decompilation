using UnityEngine;

namespace Glu
{
	public class MonoBehaviour : UnityEngine.MonoBehaviour
	{
		public I GetExistingComponentIface<I>() where I : class
		{
			return GetComponent(typeof(I)) as I;
		}

		public static I GetExistingComponentIface<I>(GameObject obj) where I : class
		{
			return obj.GetComponent(typeof(I)) as I;
		}

		public I GetComponentIface<I>() where I : class
		{
			UnityEngine.MonoBehaviour[] components = GetComponents<UnityEngine.MonoBehaviour>();
			UnityEngine.MonoBehaviour[] array = components;
			foreach (UnityEngine.MonoBehaviour monoBehaviour in array)
			{
				if (monoBehaviour is I)
				{
					return monoBehaviour as I;
				}
			}
			return (I)null;
		}

		public static I GetComponentIface<I>(GameObject obj) where I : class
		{
			UnityEngine.MonoBehaviour[] components = obj.GetComponents<UnityEngine.MonoBehaviour>();
			UnityEngine.MonoBehaviour[] array = components;
			foreach (UnityEngine.MonoBehaviour monoBehaviour in array)
			{
				if (monoBehaviour is I)
				{
					return monoBehaviour as I;
				}
			}
			return (I)null;
		}

		public I GetComponentIfaceInChildren<I>() where I : class
		{
			I componentIface = GetComponentIface<I>();
			if (componentIface != null)
			{
				return componentIface;
			}
			return GetComponentIfaceInChildrenOnly<I>(base.transform);
		}

		public static I GetComponentIfaceInChildren<I>(GameObject obj) where I : class
		{
			I componentIface = GetComponentIface<I>(obj);
			if (componentIface != null)
			{
				return componentIface;
			}
			return GetComponentIfaceInChildrenOnly<I>(obj.transform);
		}

		public static I GetComponentIfaceInChildrenOnly<I>(Transform transform) where I : class
		{
			foreach (Transform item in transform)
			{
				I componentIface = GetComponentIface<I>(item.gameObject);
				if (componentIface != null)
				{
					return componentIface;
				}
			}
			foreach (Transform item2 in transform)
			{
				I componentIfaceInChildrenOnly = GetComponentIfaceInChildrenOnly<I>(item2);
				if (componentIfaceInChildrenOnly != null)
				{
					return componentIfaceInChildrenOnly;
				}
			}
			return (I)null;
		}

		public int GetComponentIfacesInChildren<I>(GameObject obj, out UnityEngine.MonoBehaviour[] components) where I : class
		{
			components = obj.GetComponentsInChildren<UnityEngine.MonoBehaviour>();
			int num = components.Length;
			if (num != 0)
			{
				int num2 = 0;
				do
				{
					UnityEngine.MonoBehaviour monoBehaviour = components[num2];
					if (monoBehaviour is I)
					{
						num2++;
					}
					else
					{
						components[num2] = components[--num];
					}
				}
				while (num2 != num);
			}
			return num;
		}
	}
}
