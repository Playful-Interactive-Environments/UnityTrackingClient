using Assets.Tracking_Framework.Interfaces;
using Assets.Tracking_Framework.TransmissionFramework;
using Assets.Tracking_Framework.TransmissionFramework.TuioTransmission.TUIO;
using Assets.Tracking_Framework.TransmissionFramework.UnityPharusFramework;
using Assets.Tracking_Framework.TransmissionFramework.UnityTuioFramwork;
using System;
using System.Collections;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Assets.Tracking_Framework.Services
{
    /// <summary>
    /// The TuioTrackingService keeps control over the UnityTuioListener and the UnityTuioEventProcessor.
    /// </summary>
    public class TuioTrackingService : ITrackingService
    {
        public static event EventHandler<EventArgs> OnTrackingInitialized;
        private TrackingSettings settings;
        private UnityTuioListener listener;
        private UnityTuioEventProcessor m_eventProcessor;

        public void Update()
        {
            //Listen for tuio data if enabled
            if (m_eventProcessor != null)
            {
                m_eventProcessor.Process();
            }
        }

        public void Shutdown()
        {
            if (listener != null)
            {
                listener.Shutdown();
            }
        }

        public void Initialize(TrackingSettings settings)
        {
            this.settings = settings;

            listener = new UnityTuioListener(this.settings.TuioUdpPort);
            m_eventProcessor = new UnityTuioEventProcessor(listener);

            if (OnTrackingInitialized != null)
            {
                OnTrackingInitialized(this, new EventArgs());
            }

            TrackingAdapter.InjectTrackingManager(this);
        }

        public void Reconnect(int theDelay = -1)
        {
            if (listener == null || listener.HasTuioContainers())
                return;

            if (theDelay <= 0)
            {
                listener.Reconnect();
            }
            else
            {
                this.ReconnectTuioListenerDelayed(theDelay);
            }
        }

        public void SetTrackingInterpolation(int width, int height)
        {
            this.settings.TrackingResolutionX = width;
            this.settings.TrackingResolutionY = height;
        }

        public void SetTrackingStage(float x, float y)
        {
            this.settings.StageSizeX = x;
            this.settings.StageSizeY = y;
        }

        private async void ReconnectTuioListenerDelayed(int theDelay)
        {
            listener.Shutdown();
            await Task.Delay(theDelay);
            listener.Reconnect();
        }

        #region Interface properties
        public int TrackingInterpolationX
        {
            get { return settings.TrackingResolutionX; }
        }
        public int TrackingInterpolationY
        {
            get { return settings.TrackingResolutionY; }
        }

        public float TrackingStageX
        {
            get { return settings.StageSizeX; }
        }
        public float TrackingStageY
        {
            get { return settings.StageSizeY; }
        }
        #endregion Interface properties

        #region Interface methods
        public Vector2 GetScreenPositionFromRelativePosition(float x, float y)
        {
            return new Vector2((int)Mathf.Round(x * settings.TrackingResolutionX), settings.TrackingResolutionY - (int)Mathf.Round(y * settings.TrackingResolutionY));
        }
        #endregion Interface methods
    }
}
