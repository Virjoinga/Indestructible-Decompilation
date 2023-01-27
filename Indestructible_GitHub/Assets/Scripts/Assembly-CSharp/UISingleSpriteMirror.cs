using UnityEngine;

public class UISingleSpriteMirror : SpriteRootMirror
{
	public Vector2 textureScale;

	public override void Mirror(SpriteRoot s)
	{
		base.Mirror(s);
		textureScale = ((UISingleSprite)s).textureScale;
	}

	public override bool DidChange(SpriteRoot s)
	{
		if (base.DidChange(s))
		{
			return true;
		}
		if (((UISingleSprite)s).textureScale != textureScale)
		{
			s.uvsInitialized = false;
			return true;
		}
		return false;
	}
}
