using UnityEngine;

namespace Assets.Pharus_Tracking_Framework.Interfaces
{
    public interface ITrackingService
    {
        int TrackingInterpolationX { get; }
        int TrackingInterpolationY { get; }
        float TrackingStageX { get; }
        float TrackingStageY { get; }
        Vector2 GetScreenPositionFromRelativePosition(float x, float y);
        void Initialize(TrackingSettings settings);
        void Update();
        void Shutdown();
        void Reconnect(int delay);
    }
}
