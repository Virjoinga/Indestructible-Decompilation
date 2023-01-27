public interface IAdvancedSteeringControl : ISteeringControl
{
	float steerFactor { get; }

	float leftRightRatio { get; }
}
