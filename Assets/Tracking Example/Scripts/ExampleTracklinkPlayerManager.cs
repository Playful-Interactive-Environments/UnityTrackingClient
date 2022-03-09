using Assets.UnityPharusAPI.Managers;
using UnityPharusAPI.TransmissionFrameworks.Tracklink;

namespace Assets.Tracking_Example.Scripts
{
    public class ExampleTracklinkPlayerManager : ATracklinkPlayerManager
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
