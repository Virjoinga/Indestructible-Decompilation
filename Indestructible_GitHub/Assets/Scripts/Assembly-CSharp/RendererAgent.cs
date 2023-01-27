using System;
using Glu;
using UnityEngine;

[AddComponentMenu("Indestructible/Generic/Renderer Agent")]
public class RendererAgent : Glu.MonoBehaviour
{
	public delegate void VisibilityChangedDelegate(bool isVisible);

	private VisibilityChangedDelegate _visibilityChangedEvent;

	private bool _isVisible;

	public bool isVisible
	{
		get
		{
			return _isVisible;
		}
	}

	protected virtual void OnDisable()
	{
		_isVisible = false;
	}

	public void SubscriveVisibilityChange(VisibilityChangedDelegate visibilityChangedDelegate)
	{
		_visibilityChangedEvent = (VisibilityChangedDelegate)Delegate.Combine(_visibilityChangedEvent, visibilityChangedDelegate);
		visibilityChangedDelegate(_isVisible);
	}

	public void UnsubscriveFromVisibilityChange(VisibilityChangedDelegate visibilityChangedDelegate)
	{
		_visibilityChangedEvent = (VisibilityChangedDelegate)Delegate.Remove(_visibilityChangedEvent, visibilityChangedDelegate);
	}

	protected virtual void OnBecameVisible()
	{
		if (!_isVisible)
		{
			_isVisible = true;
			if (_visibilityChangedEvent != null)
			{
				_visibilityChangedEvent(true);
			}
		}
	}

	protected virtual void OnBecameInvisible()
	{
		_isVisible = false;
		if (_visibilityChangedEvent != null)
		{
			_visibilityChangedEvent(false);
		}
	}
}
