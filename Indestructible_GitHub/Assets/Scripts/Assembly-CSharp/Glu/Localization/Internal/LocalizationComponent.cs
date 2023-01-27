using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using UnityEngine;

namespace Glu.Localization.Internal
{
	public class LocalizationComponent : UnityEngine.MonoBehaviour, ILocaleListener
	{
		[SerializeField]
		private string context;

		[SerializeField]
		private string id;

		[SerializeField]
		private string comment;

		private LocalizationManager manager;

		private Action<string> stringSetter;

		private object[] args;

		private bool started;

		public LocalizationManager Manager
		{
			get
			{
				return manager;
			}
			set
			{
				UnregisterThis();
				manager = value;
				RegisterThis();
				UpdateText();
			}
		}

		public Action<string> TextSetter
		{
			get
			{
				return stringSetter;
			}
			set
			{
				stringSetter = value;
				UpdateText();
			}
		}

		public string Context
		{
			get
			{
				return context;
			}
		}

		public string Id
		{
			get
			{
				return id;
			}
		}

		public string Comment
		{
			get
			{
				return comment;
			}
			set
			{
				comment = value;
			}
		}

		public IList<object> Args
		{
			get
			{
				return new ReadOnlyCollection<object>(args);
			}
		}

		private void Awake()
		{
			RegisterThis();
		}

		private void Start()
		{
			if (!started)
			{
				if (stringSetter == null)
				{
					AutodetectTextSetter();
				}
				started = true;
				UpdateText();
			}
		}

		private void OnDestroy()
		{
			UnregisterThis();
		}

		public void HandleLocaleChanged(string locale)
		{
			UpdateText();
		}

		public void SetString(string id)
		{
			context = null;
			this.id = id;
			args = null;
			UpdateText();
		}

		public void SetStringFormat(string id, params object[] args)
		{
			context = null;
			this.id = id;
			this.args = args;
			UpdateText();
		}

		public void SetParticularString(string context, string id)
		{
			this.context = context;
			this.id = id;
			args = null;
			UpdateText();
		}

		public void SetParticularStringFormat(string context, string id, params object[] args)
		{
			this.context = context;
			this.id = id;
			this.args = args;
			UpdateText();
		}

		public void UpdateText()
		{
			if (!started)
			{
				Start();
			}
			else if (stringSetter != null)
			{
				LocalizationManager localizationManager = manager ?? Strings.Manager;
				string text = ((!string.IsNullOrEmpty(context)) ? localizationManager.GetParticularString(context, id) : localizationManager.GetString(id));
				if (args != null)
				{
					text = string.Format(text, args);
				}
				stringSetter(text);
			}
		}

		private void RegisterThis()
		{
			if (manager == null)
			{
				Strings.RegisterLocaleListener(this);
			}
		}

		private void UnregisterThis()
		{
			if (manager == null)
			{
				Strings.UnregisterLocaleListener(this);
			}
		}

		private void AutodetectTextSetter()
		{
			UnityEngine.MonoBehaviour[] components = base.gameObject.GetComponents<UnityEngine.MonoBehaviour>();
			Type[] emptyTypes = Type.EmptyTypes;
			int num = components.Length;
			for (int i = 0; i < num; i++)
			{
				UnityEngine.MonoBehaviour mb = components[i];
				PropertyInfo textProperty = mb.GetType().GetProperty("Text", BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetField, null, typeof(string), emptyTypes, null);
				if (textProperty != null)
				{
					stringSetter = delegate(string text)
					{
						textProperty.SetValue(mb, text, null);
					};
					if (string.IsNullOrEmpty(id) && string.IsNullOrEmpty(context) && textProperty.CanRead)
					{
						context = null;
						id = (string)textProperty.GetValue(mb, null);
					}
					break;
				}
			}
		}
	}
}
