using Assets.UnityPharusAPI.Managers;
using System;
using System.Collections;
using UnityEngine;

namespace Assets.Tracking_Example.Scripts
{
    public class ExampleTrackingManager : ATrackingManager
    {
        public static new ATrackingManager Instance()
        {
            return ATrackingManager.Instance;
        }

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
            Debug.Log("Reconnecting!");
            base.Reconnect();
        }

        public override void ShutDown()
        {
            base.ShutDown();
        }
    }
}