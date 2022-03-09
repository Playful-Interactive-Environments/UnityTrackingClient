using Assets.UnityPharusAPI.Managers;
using System.Collections;

namespace Assets.Tracking_Example.Scripts
{
    public class ExampleTrackingManager : ATrackingManager
    {
        protected override void Awake()
        {
            base.Awake();
        }

        protected override IEnumerator InitializeServices()
        {
            return base.InitializeServices();
        }

        protected override void Update()
        {
            base.Update();
        }

        protected override IEnumerator LoadConfig()
        {
            return base.LoadConfig();
        }

        protected override void HandleKeyboardInputs()
        {
            base.HandleKeyboardInputs();
        }

        public override void Reconnect()
        {
            base.Reconnect();
        }
    }
}