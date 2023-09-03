public class BootLoggerHandler : Logging.HandlerBase
{
	private SpriteText _text;

	public BootLoggerHandler(SpriteText text)
	{
		_text = text;
	}

	public override void Emit(ref Logging.LogRecord record)
	{
		if (_text != null)
		{
			string text = Format(ref record);
			SpriteText text2 = _text;
			text2.Text = text2.Text + '\n' + text;
		}
	}
}
