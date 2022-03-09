using Assets.UnityPharusAPI.Managers;
using UnityPharusAPI.TransmissionFrameworks.Tuio.TUIO;

namespace Assets.Tracking_Example.Scripts
{
    public class ExampleTuioPlayerManager : ATuioPlayerManager
    {
        public override void AddPlayer(TuioContainer theTuioContainer)
        {
            base.AddPlayer(theTuioContainer);
        }

        public override void RemovePlayer(long sessionID)
        {
            base.RemovePlayer(sessionID);
        }

        public override void UpdatePlayerPosition(TuioContainer theTuioContainer)
        {
            base.UpdatePlayerPosition(theTuioContainer);
        }
    }
}
