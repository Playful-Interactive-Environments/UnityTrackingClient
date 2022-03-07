using Assets.Tracking_Framework.Interfaces;
using Assets.Tracking_Framework.TransmissionFramework;
using Assets.Tracking_Framework.TransmissionFramework.UnityPharusFramework;
using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Tracking_Framework.Services
{
    /// <summary>
    /// The PharusTrackingService keeps control over the UnityPharusListener and the UnityPharusEventProcessor.
    /// </summary>
    public class PharusTrackingService : ITrackingService
    {
        public static event EventHandler<EventArgs> OnTrackingInitialized;
        private TrackingSettings settings;
        private UnityPharusListener listener;
        private UnityPharusEventProcessor eventProcessor;

        public UnityPharusEventProcessor EventProcessor
        {
            get { return eventProcessor; }
        }


        #region unity messages
        //private void Awake()
        //{
        //    if (m_instance == null)
        //    {
        //        m_instance = this;
        //    }
        //    else
        //    {
        //        if (m_instance != this)
        //        {
        //            Debug.Log(string.Format("Other instance of {0} detected (will be destroyed)", typeof(PharusTrackingService)));
        //            GameObject.Destroy(this.gameObject);
        //            return;
        //        }
        //    }
        //}

        //private void OnEnable()
        //{
        //    if (!m_initialized)
        //    {
        //        StartCoroutine(InitInstance());
        //    }
        //}

        //private void OnDisable()
        //{
        //    Component.Destroy(this);
        //}

        //private void Update()
        //{
        //    //HandleKeyboardInputs();

        //    //Lister for Pharus Data if Tracklink is enabled
        //    if (eventProcessor != null)
        //    {
        //        eventProcessor.Process();
        //    }
        //}

        //private void OnDestroy()
        //{
        //    if (listener != null)
        //    {
        //        listener.Shutdown();
        //    }
        //}
        #endregion unity messages

        #region public methods
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

        public void Update()
        {
            //Lister for Pharus Data if Tracklink is enabled

            if (eventProcessor != null)
            {
                eventProcessor.Process();
            }
        }

        public void Shutdown()
        {
            if (listener != null)
            {
                listener.Shutdown();
            }
        }

        public void Initialize(TrackingSettings config)
        {
            Debug.Log("Initialize Pharus");
            this.settings = config;

            if (settings.CheckServerReconnectIntervall > 0)
            {
                Task.Run(() => CheckServerAlive(settings.CheckServerReconnectIntervall));
            }

            if (settings.TracklinkProtocol == TrackingSettings.EProtocolType.TCP)
            {
                listener = UnityPharusListener.NewUnityPharusListenerTCP(settings.TracklinkTcpIp, settings.TracklinkTcpPort);
            }
            else if (settings.TracklinkProtocol == TrackingSettings.EProtocolType.UDP)
            {
                listener = UnityPharusListener.NewUnityPharusListenerUDP(settings.TracklinkMulticastIp, settings.TracklinkUdpPort);
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

        #endregion public methods

        #region private methods

        private async void ReconnectTuioListenerDelayed(int theDelay)
        {
            listener.Shutdown();
            await Task.Delay(theDelay);
            listener.Reconnect();
        }
        
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
        #endregion private methods

        #region interface properties
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
        #endregion interface properties

        #region interface methods
        public Vector2 GetScreenPositionFromRelativePosition(float x, float y)
        {
            return new Vector2((int)Mathf.Round(x * settings.TrackingResolutionX), settings.TrackingResolutionY - (int)Mathf.Round(y * settings.TrackingResolutionY));
        }

        #endregion interface methods
    }
}
