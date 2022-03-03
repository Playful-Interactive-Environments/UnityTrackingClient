using Assets.Tracking_Framework.TransmissionFramework.UnityPharusFramework;
using UnityEngine;

namespace Assets.Tracking_Framework.Interfaces
{
    public interface ITrackingService
    {
        int TrackingInterpolationX { get; }
        int TrackingInterpolationY { get; }
        float TrackingStageX { get; }
        float TrackingStageY { get; }
        Vector2 GetScreenPositionFromRelativePosition(float x, float y);
        void Initialize(TrackingXMLConfig config);
        void Update();
        void Shutdown();
        void Reconnect(int delay);
        void SetTrackingInterpolation(int width, int height);
        void SetTrackingStage(float x, float y);
    }
}
