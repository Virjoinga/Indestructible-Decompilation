using UnityEngine;

[ExecuteInEditMode]
public class UISingleSprite : SpriteRoot
{
	public Vector2 textureScale = new Vector2(1f, 1f);

	protected Vector2 pixelDimensions = Vector2.zero;

	protected bool nullCamera;

	private Vector2 GetPixelDimensions()
	{
		Renderer component = GetComponent<Renderer>();
		if (component == null)
		{
			return Vector2.zero;
		}
		Material sharedMaterial = component.sharedMaterial;
		if (sharedMaterial == null)
		{
			return Vector2.zero;
		}
		Texture mainTexture = sharedMaterial.mainTexture;
		if (mainTexture == null)
		{
			return Vector2.zero;
		}
		return new Vector2(mainTexture.width, mainTexture.height);
	}

	public override Vector2 GetDefaultPixelSize(PathFromGUIDDelegate guid2Path, AssetLoaderDelegate loader)
	{
		return pixelDimensions;
	}

	protected override void Awake()
	{
		base.Awake();
		Init();
	}

	protected override void Init()
	{
		nullCamera = renderCamera == null;
		pixelDimensions = GetPixelDimensions();
		base.Init();
	}

	public override void Start()
	{
		base.Start();
		if (UIManager.Exists() && nullCamera && UIManager.instance.uiCameras.Length > 0)
		{
			SetCamera(UIManager.instance.uiCameras[0].camera);
		}
	}

	public override void Clear()
	{
		base.Clear();
	}

	public override void Copy(SpriteRoot s)
	{
		base.Copy(s);
		if (s is UISingleSprite)
		{
			textureScale = ((UISingleSprite)s).textureScale;
			InitUVs();
			SetBleedCompensation(s.bleedCompensation);
			if (autoResize || pixelPerfect)
			{
				CalcSize();
			}
			else
			{
				SetSize(s.width, s.height);
			}
		}
	}

	public override void InitUVs()
	{
		uvRect.x = 0f;
		uvRect.y = 0f;
		uvRect.xMax = 1f / textureScale.x;
		uvRect.yMax = 1f / textureScale.y;
		frameInfo.uvs = uvRect;
		base.InitUVs();
	}

	public override int GetStateIndex(string stateName)
	{
		return -1;
	}

	public override void SetState(int index)
	{
	}

	public static UISingleSprite Create(string name, Vector3 pos)
	{
		GameObject gameObject = new GameObject(name);
		gameObject.transform.position = pos;
		return (UISingleSprite)gameObject.AddComponent(typeof(UISingleSprite));
	}

	public static UISingleSprite Create(string name, Vector3 pos, Quaternion rotation)
	{
		GameObject gameObject = new GameObject(name);
		gameObject.transform.position = pos;
		gameObject.transform.rotation = rotation;
		return (UISingleSprite)gameObject.AddComponent(typeof(UISingleSprite));
	}

	public override void DoMirror()
	{
		if (!Application.isPlaying)
		{
			if (screenSize.x == 0f || screenSize.y == 0f)
			{
				base.Start();
			}
			if (mirror == null)
			{
				mirror = new UISingleSpriteMirror();
				mirror.Mirror(this);
			}
			mirror.Validate(this);
			if (mirror.DidChange(this))
			{
				Init();
				mirror.Mirror(this);
			}
		}
	}
}
