using UnityPharusAPI.TransmissionFrameworks.Tracklink;

namespace UnityPharusAPI.Interfaces
{
    public interface ITrackingService
    {
        int TrackingInterpolationX { get; }
        int TrackingInterpolationY { get; }
        float TrackingStageX { get; }
        float TrackingStageY { get; }

        bool IsActivelyReceiving { get; }
        Vector2f GetScreenPositionFromRelativePosition(float x, float y);
        void Initialize(TrackingSettings settings);
        void Update();
        void Shutdown();
        void Reconnect(int delay);
    }
}
