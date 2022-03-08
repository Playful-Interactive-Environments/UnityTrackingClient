using Assets.Pharus_Tracking_Framework.Managers;
using Assets.Pharus_Tracking_Framework.TransmissionFrameworks.Tracklink;

namespace Assets.Tracking_Example.Scripts
{
    public class CustomTracklinkPlayerManager : TracklinkPlayerManager
    {
        public override void AddPlayer(TrackRecord trackRecord)
        {
            base.AddPlayer(trackRecord);
        }

        public override void RemovePlayer(int trackID)
        {
            base.RemovePlayer(trackID);
        }

        public override void UpdatePlayerPosition(TrackRecord trackRecord)
        {
            base.UpdatePlayerPosition(trackRecord);
        }
    }
}
