using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter))]
public class UIPackable : MonoBehaviour, ISpriteAggregator, ISpriteAnimatable, ISpritePackable
{
	public float Width;

	public float Height;

	public UIAlign.Horizontal Horizontal = UIAlign.Horizontal.Center;

	public UIAlign.Vertical Vertical = UIAlign.Vertical.Center;

	protected Rect _uvs;

	protected Mesh _mesh;

	protected Vector2 _size;

	protected Vector3[] _vertices;

	protected Transform _transform;

	protected int _numVertices = 4;

	protected int _numTriangles = 2;

	protected Vector3 _center;

	private Texture2D[] AggregateTextures;

	private CSpriteFrame[] AggregateFrames;

	public TextureAnim[] TextureAnimations = new TextureAnim[0];

	public Color SpriteColor = Color.white;

	private int _loops;

	private float _time;

	private int _currentFrame;

	private TextureAnim _animation;

	private ISpriteAnimatable _prev;

	private ISpriteAnimatable _next;

	public Vector3 Center
	{
		get
		{
			return _center;
		}
	}

	public Rect DefaultUVs
	{
		get
		{
			Rect result = new Rect(0f, 0f, 1f, 1f);
			CSpriteFrame defaultFrame = DefaultFrame;
			if (defaultFrame != null)
			{
				return defaultFrame.uvs;
			}
			return result;
		}
	}

	public Texture2D[] SourceTextures
	{
		get
		{
			return AggregateTextures;
		}
	}

	public CSpriteFrame[] SpriteFrames
	{
		get
		{
			return AggregateFrames;
		}
	}

	public CSpriteFrame DefaultFrame
	{
		get
		{
			if (States.Length != 0 && States[0].spriteFrames.Length != 0)
			{
				return States[0].spriteFrames[0];
			}
			return null;
		}
	}

	public bool DoNotTrimImages
	{
		get
		{
			return true;
		}
	}

	public TextureAnim[] States
	{
		get
		{
			return TextureAnimations;
		}
		set
		{
			TextureAnimations = value;
		}
	}

	public Color Color
	{
		get
		{
			return SpriteColor;
		}
		set
		{
			SetColor(value);
		}
	}

	public bool SupportsArbitraryAnimations
	{
		get
		{
			return true;
		}
	}

	public SpriteRoot.ANCHOR_METHOD Anchor
	{
		get
		{
			return SpriteRoot.ANCHOR_METHOD.MIDDLE_CENTER;
		}
	}

	public ISpriteAnimatable prev
	{
		get
		{
			return _prev;
		}
		set
		{
			_prev = value;
		}
	}

	public ISpriteAnimatable next
	{
		get
		{
			return _next;
		}
		set
		{
			_next = value;
		}
	}

	protected virtual void Awake()
	{
		Init();
	}

	protected virtual void Start()
	{
	}

	protected virtual void Update()
	{
		if (Application.isEditor && !Application.isPlaying)
		{
			Init();
		}
	}

	protected virtual void Init()
	{
		_transform = GetComponent<Transform>();
		_uvs = DefaultUVs;
		_size = new Vector2(Width, Height);
		_mesh = CreateMesh();
		MeshFilter component = GetComponent<MeshFilter>();
		component.mesh = _mesh;
	}

	protected virtual Mesh CreateMesh()
	{
		int[] triangles = new int[_numTriangles * 3];
		Vector2[] array = new Vector2[_numVertices];
		Vector3[] array2 = new Vector3[_numVertices];
		Color[] colors = new Color[_numVertices];
		CalculateVertices(array2);
		CalculateTriangles(triangles);
		CalculateUVs(array2, array);
		_vertices = new Vector3[_numVertices];
		array2.CopyTo(_vertices, 0);
		UpdateAlign(array2);
		UpdateColors(colors, SpriteColor);
		Mesh mesh = new Mesh();
		mesh.vertices = array2;
		mesh.uv = array;
		mesh.triangles = triangles;
		mesh.colors = colors;
		UpdateCenter(array2);
		UpdateBounds(mesh);
		return mesh;
	}

	protected virtual void CalculateVertices(Vector3[] vertices)
	{
	}

	protected virtual void CalculateTriangles(int[] triangles)
	{
	}

	protected virtual void CalculateUVs(Vector3[] vertices, Vector2[] uvs)
	{
		if (_size.x * _size.y != 0f)
		{
			for (int i = 0; i < _numVertices; i++)
			{
				uvs[i].x = vertices[i].x / _size.x * _uvs.width + _uvs.x;
				uvs[i].y = vertices[i].y / _size.y * _uvs.height + _uvs.y;
			}
		}
	}

	protected virtual void UpdateUVs(Vector3[] vertices, Vector2[] uvs)
	{
		CalculateUVs(vertices, uvs);
	}

	protected virtual void UpdateCenter(Vector3[] vertices)
	{
		Vector3 min;
		Vector3 max;
		GetMinMax(vertices, out min, out max);
		_center = (min + max) / 2f;
	}

	private void GetMinMax(Vector3[] vertices, out Vector3 min, out Vector3 max)
	{
		min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
		max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
		for (int i = 0; i < vertices.Length; i++)
		{
			Vector3 vector = vertices[i];
			if (vector.x < min.x)
			{
				min.x = vector.x;
			}
			if (vector.x > max.x)
			{
				max.x = vector.x;
			}
			if (vector.y < min.y)
			{
				min.y = vector.y;
			}
			if (vector.y > max.y)
			{
				max.y = vector.y;
			}
			if (vector.z < min.z)
			{
				min.z = vector.z;
			}
			if (vector.z > max.z)
			{
				max.z = vector.z;
			}
		}
	}

	protected virtual void UpdateBounds(Mesh mesh)
	{
		mesh.RecalculateBounds();
		mesh.RecalculateNormals();
	}

	protected virtual void UpdateAlign(Vector3[] vertices)
	{
		Vector3 min;
		Vector3 max;
		GetMinMax(vertices, out min, out max);
		Vector2 zero = Vector2.zero;
		switch (Horizontal)
		{
		case UIAlign.Horizontal.Left:
			zero.x = 0f - min.x;
			break;
		case UIAlign.Horizontal.Right:
			zero.x = 0f - max.x;
			break;
		case UIAlign.Horizontal.Center:
			zero.x = (0f - (min.x + max.x)) / 2f;
			break;
		}
		switch (Vertical)
		{
		case UIAlign.Vertical.Top:
			zero.y = 0f - max.y;
			break;
		case UIAlign.Vertical.Bottom:
			zero.y = 0f - min.y;
			break;
		case UIAlign.Vertical.Center:
			zero.y = (0f - (min.y + max.y)) / 2f;
			break;
		}
		for (int i = 0; i < vertices.Length; i++)
		{
			vertices[i].x += zero.x;
			vertices[i].y += zero.y;
		}
	}

	private void UpdateColors(Color[] colors, Color color)
	{
		for (int i = 0; i < colors.Length; i++)
		{
			colors[i] = color;
		}
		SpriteColor = color;
	}

	private void SetColor(Color color)
	{
		Color[] colors = _mesh.colors;
		UpdateColors(colors, color);
		_mesh.colors = colors;
	}

	public void Aggregate(PathFromGUIDDelegate guid2Path, LoadAssetDelegate load, GUIDFromPathDelegate path2Guid)
	{
		List<Texture2D> list = new List<Texture2D>();
		List<CSpriteFrame> list2 = new List<CSpriteFrame>();
		TextureAnim[] states = States;
		foreach (TextureAnim textureAnim in states)
		{
			textureAnim.Allocate();
			if (textureAnim.frameGUIDs.Length >= textureAnim.framePaths.Length)
			{
				for (int j = 0; j < textureAnim.frameGUIDs.Length; j++)
				{
					string path = guid2Path(textureAnim.frameGUIDs[j]);
					Object @object = load(path, typeof(Texture2D));
					list.Add(@object as Texture2D);
					list2.Add(textureAnim.spriteFrames[j]);
				}
				textureAnim.framePaths = new string[0];
				continue;
			}
			textureAnim.frameGUIDs = new string[textureAnim.framePaths.Length];
			textureAnim.spriteFrames = new CSpriteFrame[textureAnim.framePaths.Length];
			for (int k = 0; k < textureAnim.spriteFrames.Length; k++)
			{
				textureAnim.spriteFrames[k] = new CSpriteFrame();
			}
			for (int l = 0; l < textureAnim.framePaths.Length; l++)
			{
				textureAnim.frameGUIDs[l] = path2Guid(textureAnim.framePaths[l]);
				Object object2 = load(textureAnim.framePaths[l], typeof(Texture2D));
				list.Add(object2 as Texture2D);
				list2.Add(textureAnim.spriteFrames[l]);
			}
		}
		AggregateTextures = list.ToArray();
		AggregateFrames = list2.ToArray();
	}

	public Material GetPackedMaterial(out string errString)
	{
		errString = "Sprite \"" + base.name + "\" has not been assigned a material!";
		Renderer component = GetComponent<Renderer>();
		return component.sharedMaterial;
	}

	public void SetUVs(Rect uvs)
	{
		Init();
	}

	public void Play(TextureAnim a)
	{
		if (a != null && a != _animation)
		{
			_currentFrame = -1;
			_animation = a;
			_time = 0f;
			_loops = 0;
			Stop();
			Resume();
			SetFrame(a, 0);
		}
	}

	public void Play(string name)
	{
		TextureAnim[] textureAnimations = TextureAnimations;
		foreach (TextureAnim textureAnim in textureAnimations)
		{
			if (textureAnim.name == name)
			{
				Play(textureAnim);
				break;
			}
		}
	}

	public void Play(int index)
	{
		TextureAnim a = TextureAnimations[index];
		Play(a);
	}

	public void Play()
	{
		Play(0);
	}

	public void Resume()
	{
		if (_animation != null)
		{
			SpriteAnimationPump.Add(this);
		}
	}

	public void Stop()
	{
		SpriteAnimationPump.Remove(this);
	}

	public void Rewind()
	{
		SetFrame(_animation, 0);
		Stop();
	}

	private void Finish()
	{
		switch (_animation.onAnimEnd)
		{
		case UVAnimation.ANIM_END_ACTION.Do_Nothing:
			Stop();
			break;
		case UVAnimation.ANIM_END_ACTION.Deactivate:
			Stop();
			base.gameObject.active = false;
			break;
		case UVAnimation.ANIM_END_ACTION.Destroy:
			Stop();
			Object.Destroy(base.gameObject);
			break;
		case UVAnimation.ANIM_END_ACTION.Hide:
		{
			Stop();
			MeshRenderer component = GetComponent<MeshRenderer>();
			if (component != null)
			{
				component.enabled = false;
			}
			break;
		}
		case UVAnimation.ANIM_END_ACTION.Play_Default_Anim:
			Play();
			break;
		case UVAnimation.ANIM_END_ACTION.Revert_To_Static:
			Rewind();
			break;
		}
	}

	private void SetFrame(TextureAnim a, int index)
	{
		if (_currentFrame != index)
		{
			_currentFrame = index;
			Vector2[] uv = _mesh.uv;
			_uvs = a.spriteFrames[index].uvs;
			CalculateUVs(_vertices, uv);
			_mesh.uv = uv;
		}
	}

	public bool StepAnim(float dt)
	{
		if (_animation != null)
		{
			_time += dt;
			float f = _time * _animation.framerate;
			int num = Mathf.FloorToInt(f);
			if (num < _animation.spriteFrames.Length)
			{
				SetFrame(_animation, num);
				return true;
			}
			if (_animation.loopCycles > 0)
			{
				_loops++;
				if (_animation.loopCycles == _loops)
				{
					Finish();
				}
			}
			_time -= (float)_animation.spriteFrames.Length / _animation.framerate;
		}
		return false;
	}

	/*virtual GameObject ISpriteAggregator.get_gameObject()
	{
		return base.gameObject;
	}

	virtual GameObject ISpritePackable.get_gameObject()
	{
		return base.gameObject;
	}*/
}
