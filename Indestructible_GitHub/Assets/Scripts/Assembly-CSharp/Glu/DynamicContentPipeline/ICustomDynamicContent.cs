using Glu.ABTesting;

namespace Glu.DynamicContentPipeline
{
	public interface ICustomDynamicContent
	{
		float Progress { get; }

		bool IsInProgress { get; }

		bool Result { get; }

		long LastUpdateSize { get; }

		void StartContentUpdate(Resolution resolution);

		void CheckForUpdates(Resolution resolution);

		void InvalidateLastCheckReport();
	}
}
