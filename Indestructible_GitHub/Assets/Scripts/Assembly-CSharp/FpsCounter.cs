using UnityEngine;

public class FpsCounter : MonoBehaviour
{
	public enum Mode
	{
		Block = 0,
		Text = 1
	}

	public delegate string DebugInfoProc();

	public Mode mode = Mode.Text;

	public TextAnchor align = TextAnchor.UpperCenter;

	public float updateInterval = 0.25f;

	public Color color = new Color(0.7f, 0.7f, 0.7f);

	public int padding = 5;

	public bool showFrameTime = true;

	public bool showFullFpsInfo;

	public string numbersFormat = "0.";

	public float blockScale = 2f;

	public Color blockTextColor = Color.magenta;

	public Color blockBackgroundColor = Color.black;

	public bool showDebugInfo = true;

	public float debugInfoOffsetX = 30f;

	public float debugInfoOffsetY = 60f;

	public DebugInfoProc debugInfo { get; set; }

	public void Reset()
	{
	}

	public string GetText(bool full)
	{
		return "(disabled)";
	}
}
