using System;
using System.Threading.Tasks;
using UnityPharusAPI.Interfaces;
using UnityPharusAPI.TransmissionFrameworks.Tracklink;
using UnityPharusAPI.TransmissionFrameworks.Tuio;

namespace UnityPharusAPI.Services
{
    /// <summary>
    /// The TUIO Tracking Service keeps control over the UnityTuioListener and the TuioEventProcessor.
    /// </summary>
    public class TuioTrackingService : ITrackingService
    {
        public static event EventHandler<EventArgs> OnTrackingInitialized;
        private TrackingSettings settings;
        private UnityTuioListener listener;
        private TuioEventProcessor eventProcessor;

        public bool IsActivelyReceiving
        {
            get
            {
                if (listener != null)
                {
                    if (listener.HasDataReceivedSinceLastCheck())
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
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
        public Vector2f GetScreenPositionFromRelativePosition(float x, float y)
        {
            return new Vector2f((int)Math.Round(x * settings.TrackingResolutionX), settings.TrackingResolutionY - (int)Math.Round(y * settings.TrackingResolutionY));
        }

        /// <summary>
        /// Initializes the tracking service by initializing the TUIO listener and event processor.
        /// </summary>
        /// <param name="settings">The settings xml file, which can be edited externally in the Streaming Assets.</param>
        public void Initialize(TrackingSettings settings)
        {
            this.settings = settings;

            listener = new UnityTuioListener(this.settings.TuioUdpPort);
            eventProcessor = new TuioEventProcessor(listener);

            if (OnTrackingInitialized != null)
            {
                OnTrackingInitialized(this, new EventArgs());
            }

            TrackingAdapter.InjectTrackingManager(this);
        }

        /// <summary>
        /// Updates the event processor.
        /// </summary>
        public void Update()
        {
            //Listen for tuio data if enabled
            if (eventProcessor != null)
            {
                eventProcessor.Process();
            }
        }

        /// <summary>
        /// Reconnects the tracking service with an optional delay.
        /// </summary>
        /// <param name="theDelay">The delay in milliseconds.</param>
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

        /// <summary>
        /// Shuts down the TUIO tracking service
        /// </summary>
        public void Shutdown()
        {
            if (listener != null)
            {
                listener.Shutdown();
            }
        }

        private async void ReconnectTuioListenerDelayed(int theDelay)
        {
            listener.Shutdown();
            await Task.Delay(theDelay);
            listener.Reconnect();
        }
    }
}
