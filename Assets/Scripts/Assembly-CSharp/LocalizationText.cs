using Glu.Localization;
using UnityEngine;

public class LocalizationText : MonoBehaviour
{
	public string Id;

	private void Awake()
	{
		SpriteText component = GetComponent<SpriteText>();
		component.Text = Strings.GetString(Id);
		Object.Destroy(this);
	}
}
