using UnityEngine;

namespace UnityTracking
{
	public interface ITrackingManager
	{
		int TargetScreenWidth { get; }
		int TargetScreenHeight { get; }

		float TrackingStageX { get; }
		float TrackingStageY { get; }

		Vector2 GetScreenPositionFromRelativePosition (float x, float y);
	}
}