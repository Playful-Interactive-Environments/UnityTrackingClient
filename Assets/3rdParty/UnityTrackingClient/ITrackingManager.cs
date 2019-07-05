using UnityEngine;

namespace UnityTracking
{
    public interface ITrackingManager
    {
        int TrackingInterpolationX { get; }
        int TrackingInterpolationY { get; }

        float TrackingStageX { get; }
        float TrackingStageY { get; }

        Vector2 GetScreenPositionFromRelativePosition(float x, float y);

        void ReconnectListener(float delay);
        void SetTrackingInterpolation(int width, int height);
        void SetTrackingStage(float x, float y);
    }
}
