using UnityEngine;

public interface ISteeringControl
{
	float steerAngle { get; }

	float targetSteerAngle { get; set; }

	Vector2 direction { set; }

	float directionAngle { get; set; }
}
