using Assets.Pharus_Tracking_Framework.Player;
using UnityEngine;

namespace Assets.Tracking_Example.Scripts
{
    public class TestPlayer : ATrackingEntity
    {
        public override void SetPosition(Vector2 theNewPosition)
        {
            base.SetPosition(theNewPosition);
        }
    }
}
