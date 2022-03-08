using Assets.Tracking_Framework.Enums;
using Assets.Tracking_Framework.Interfaces;
using Assets.Tracking_Framework.TransmissionFramework;
using Assets.Tracking_Framework.TransmissionFramework.UnityPharusFramework;
using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Tracking_Framework.Services
{
    /// <summary>
    /// The TracklinkTrackingService keeps control over the UnityPharusListener and the UnityPharusEventProcessor.
    /// </summary>
    public class TracklinkTrackingService : ITrackingService
    {
        public static event EventHandler<EventArgs> OnTrackingInitialized;
        private TrackingSettings settings;
        private UnityPharusListener listener;
        private UnityPharusEventProcessor eventProcessor;

        public UnityPharusEventProcessor EventProcessor
        {
            get { return eventProcessor; }
        }
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

        /// <summary>
        /// Used to calculate player position based on the tracking resolution.
        /// </summary>
        /// <param name="x">The x coordinate</param>
        /// <param name="y">The y coordinate</param>
        /// <returns></returns>
        public Vector2 GetScreenPositionFromRelativePosition(float x, float y)
        {
            return new Vector2((int)Mathf.Round(x * settings.TrackingResolutionX), settings.TrackingResolutionY - (int)Mathf.Round(y * settings.TrackingResolutionY));
        }

        /// <summary>
        /// Initializes the Tracklink tracking service.
        /// </summary>
        /// <param name="settings">The settings xml file, which can be edited externally in the Streaming Assets.</param>
        public void Initialize(TrackingSettings settings)
        {
            Debug.Log("Initialize Pharus");
            this.settings = settings;

            if (this.settings.CheckServerReconnectIntervall > 0)
            {
                Task.Run(() => CheckServerAlive(this.settings.CheckServerReconnectIntervall));
            }

            if (this.settings.TracklinkProtocol == EProtocolType.TCP)
            {
                listener = UnityPharusListener.NewUnityPharusListenerTCP(this.settings.TracklinkTcpIp, this.settings.TracklinkTcpPort);
            }
            else if (this.settings.TracklinkProtocol == EProtocolType.UDP)
            {
                listener = UnityPharusListener.NewUnityPharusListenerUDP(this.settings.TracklinkMulticastIp, this.settings.TracklinkUdpPort);
            }
            else
            {
                Debug.LogError("Invalid pharus settings!");
            }
            eventProcessor = new UnityPharusEventProcessor(listener);

            if (OnTrackingInitialized != null)
            {
                OnTrackingInitialized(this, new EventArgs());
            }

            TrackingAdapter.InjectTrackingManager(this);
        }

        public void Update()
        {
            if (eventProcessor != null)
            {
                eventProcessor.Process();
            }
        }

        /// <summary>
        /// Shuts down the Tracklink tracking service
        /// </summary>
        public void Shutdown()
        {
            if (listener != null)
            {
                listener.Shutdown();
            }
        }

        /// Reconnects the tracking service with an optional delay.
        /// </summary>
        /// <param name="theDelay">The delay in milliseconds.</param>
        public void Reconnect(int theDelay = -1)
        {
            if (theDelay <= 0)
            {
                listener.Reconnect();
            }
            else
            {
                this.ReconnectTuioListenerDelayed(theDelay);
            }
        }

        private async void ReconnectTuioListenerDelayed(int theDelay)
        {
            listener.Shutdown();
            await Task.Delay(theDelay);
            listener.Reconnect();
        }

        /// <summary>
        /// Checks if the Tracklink tracking service is running correctly.
        /// </summary>
        /// <param name="theWaitBetweenCheck"></param>
        private async void CheckServerAlive(int theWaitBetweenCheck)
        {
            while (true)
            {
                Debug.Log("CheckServerAlive");
                await Task.Delay(theWaitBetweenCheck);
                if (listener != null && !listener.IsCurrentlyConnecting && !listener.HasDataReceivedSinceLastCheck())
                {
                    Debug.LogWarning(string.Format("--- There might be a connection problem. (No data received in the past {0} seconds)---", theWaitBetweenCheck));
                    this.ReconnectTuioListenerDelayed(1000);
                }
            }
        }
    }
}
