using System.IO;

namespace Glu.Localization.StreamGetters
{
	public sealed class FileStreamGetter : IStreamGetter
	{
		private string baseDir;

		public string BaseDir
		{
			get
			{
				return baseDir;
			}
			set
			{
				baseDir = value;
			}
		}

		public FileStreamGetter()
		{
			baseDir = string.Empty;
		}

		public Stream GetStream(string name)
		{
			string path = Path.Combine(baseDir, name);
			if (File.Exists(path))
			{
				return File.OpenRead(path);
			}
			return null;
		}
	}
}
