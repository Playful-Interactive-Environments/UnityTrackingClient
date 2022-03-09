using Unity_Pharus_Tracking_Client.TransmissionFrameworks.Tracklink;

namespace Unity_Pharus_Tracking_Client.Interfaces
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
