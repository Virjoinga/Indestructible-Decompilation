using System.IO;

namespace Glu.Localization
{
	public interface IStreamGetter
	{
		Stream GetStream(string name);
	}
}
