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
        /// <summary>
        /// General TUIO Settings
        /// </summary>
        public class TuioSettings
        {
            private bool _trackingEnabled = true;
            private int _udpPort = 3333;
            private int _trackingInterpolationX = 1920;
            private int _trackingInterpolationY = 1080;
            private float _trackingStageX = 1600f;
            private float _trackingStageY = 900f;

            public bool TrackingEnabled
            {
                get { return _trackingEnabled; }
                set { _trackingEnabled = value; }
            }
            public int UDP_Port
            {
                get { return _udpPort; }
                set { this._udpPort = value; }
            }
            public int TrackingInterpolationX
            {
                get { return _trackingInterpolationX; }
                set { this._trackingInterpolationX = value; }
            }
            public int TrackingInterpolationY
            {
                get { return _trackingInterpolationY; }
                set { this._trackingInterpolationY = value; }
            }
            public float TrackingStageX
            {
                get { return this._trackingStageX; }
                set { this._trackingStageX = value; }
            }
            public float TrackingStageY
            {
                get { return this._trackingStageY; }
                set { this._trackingStageY = value; }
            }
        }

        public static event EventHandler<EventArgs> OnTrackingInitialized;

        private TuioSettings m_tuioSettings = new TuioSettings();
        private TrackingXMLConfig xmlConfig;
        private UnityTuioListener m_listener;
        private UnityTuioEventProcessor m_eventProcessor;

        public void Update()
        {
            //Lister for Tuio Data if enabled
            if (m_eventProcessor != null)
            {
                m_eventProcessor.Process();
            }
        }

        public void Shutdown()
        {
            if (m_listener != null)
            {
                m_listener.Shutdown();
            }
        }

        public void Initialize(TrackingXMLConfig config)
        {
            this.xmlConfig = config;
            if (xmlConfig != null)
            {
                string configTrackingEnabled = null;
                string configUDPPort = null;
                string interpolationX = null;
                string interpolationY = null;
                string stageX = null;
                string stageY = null;
                for (int i = 0; i < xmlConfig.ConfigNodes.Length; i++)
                {
                    switch (xmlConfig.ConfigNodes[i].Name)
                    {
                        case "tuio-enabled":
                            configTrackingEnabled = xmlConfig.ConfigNodes[i].Value;
                            break;

                        case "tuio-udp-port":
                            configUDPPort = xmlConfig.ConfigNodes[i].Value;
                            break;

                        case "trackingInterpolationX":
                            interpolationX = xmlConfig.ConfigNodes[i].Value;
                            break;

                        case "trackingInterpolationY":
                            interpolationY = xmlConfig.ConfigNodes[i].Value;
                            break;

                        case "trackingStageX":
                            stageX = xmlConfig.ConfigNodes[i].Value;
                            break;

                        case "trackingStageY":
                            stageY = xmlConfig.ConfigNodes[i].Value;
                            break;

                        default:
                            break;
                    }
                }

                bool configTrackingEnabledBool;
                if (configTrackingEnabled != null && Boolean.TryParse(configTrackingEnabled, out configTrackingEnabledBool))
                {
                    m_tuioSettings.TrackingEnabled = configTrackingEnabledBool;
                    Debug.Log(string.Format("TUIO XML config: TUIO tracking enabled: {0}", m_tuioSettings.TrackingEnabled));
                }
                //else
                //{
                //	Debug.Log(string.Format("TUIO XML config: couldn't load enabled config. Tracking enabled: {0}", m_tuioSettings.TrackingEnabled));
                //}

                int configUDPPortInt;
                if (configUDPPort != null && int.TryParse(configUDPPort, out configUDPPortInt))
                {
                    m_tuioSettings.UDP_Port = configUDPPortInt;
                    Debug.Log(string.Format("TUIO XML config: TUIO using UDP Port: {0}", configUDPPort));
                }
                else
                {
                    Debug.LogWarning("TUIO XML config: invalid TUIO Port config");
                }

                int configInterpolationIntX;
                int configInterpolationIntY;
                if (interpolationX != null && int.TryParse(interpolationX, out configInterpolationIntX) &&
                    interpolationY != null && int.TryParse(interpolationY, out configInterpolationIntY))
                {
                    m_tuioSettings.TrackingInterpolationX = configInterpolationIntX;
                    m_tuioSettings.TrackingInterpolationY = configInterpolationIntY;
                    Debug.Log(string.Format("TUIO XML config: tracking interpolation: {0}x{1}", m_tuioSettings.TrackingInterpolationX, m_tuioSettings.TrackingInterpolationY));
                }
                //else
                //{
                //	Debug.Log(string.Format("TUIO XML config: invalid interpolation config. Using: {0}x{1}", m_tuioSettings.TrackingInterpolationX, m_tuioSettings.TrackingInterpolationY));
                //}

                float configStageFloatX;
                float configStageFloatY;
                if (stageX != null && float.TryParse(stageX, out configStageFloatX) &&
                    stageY != null && float.TryParse(stageY, out configStageFloatY))
                {
                    m_tuioSettings.TrackingStageX = configStageFloatX;
                    m_tuioSettings.TrackingStageY = configStageFloatY;
                    Debug.Log(string.Format("TUIO XML config: stage size: {0}x{1}", m_tuioSettings.TrackingStageX, m_tuioSettings.TrackingStageY));
                }
                //else
                //{
                //	Debug.Log(string.Format("TUIO XML config: invalid stage config. Using: {0}x{1}", m_tuioSettings.TrackingStageX, m_tuioSettings.TrackingStageY));
                //}
            }
            else
            {
                Debug.Log("no TUIO config xml file found in resources: Disable and Destroy TuioTrackingService");
                //TODO: Deal with missing config
                //this.enabled = false;
                //Destroy(this);
                //yield break;
            }
            // end: load config file

            if (!m_tuioSettings.TrackingEnabled)
            {
                Debug.Log("TUIO tracking disabled in config: Disable and Destroy TuioTrackingService");
                //TODO: Deal with missing config
                //this.enabled = false;
                //Destroy(this);
                //yield break;
            }

            m_listener = new UnityTuioListener(m_tuioSettings.UDP_Port);
            m_eventProcessor = new UnityTuioEventProcessor(m_listener);

            if (OnTrackingInitialized != null)
            {
                OnTrackingInitialized(this, new EventArgs());
            }

            TrackingAdapter.InjectTrackingManager(this);

            //UpdateDebugGUI();
        }

        public void Reconnect(int theDelay = -1)
        {
            if (m_listener == null || m_listener.HasTuioContainers())
                return;

            if (theDelay <= 0)
            {
                m_listener.Reconnect();
            }
            else
            {
                this.ReconnectTuioListenerDelayed(theDelay);
            }
        }

        public void SetTrackingInterpolation(int width, int height)
        {
            this.m_tuioSettings.TrackingInterpolationX = width;
            this.m_tuioSettings.TrackingInterpolationY = height;
        }

        public void SetTrackingStage(float x, float y)
        {
            this.m_tuioSettings.TrackingStageX = x;
            this.m_tuioSettings.TrackingStageY = y;
        }

        private async void ReconnectTuioListenerDelayed(int theDelay)
        {
            m_listener.Shutdown();
            await Task.Delay(theDelay);
            m_listener.Reconnect();
        }

        #region Interface properties
        public int TrackingInterpolationX
        {
            get { return m_tuioSettings.TrackingInterpolationX; }
        }
        public int TrackingInterpolationY
        {
            get { return m_tuioSettings.TrackingInterpolationY; }
        }

        public float TrackingStageX
        {
            get { return m_tuioSettings.TrackingStageX; }
        }
        public float TrackingStageY
        {
            get { return m_tuioSettings.TrackingStageY; }
        }
        #endregion Interface properties

        #region Interface methods
        public Vector2 GetScreenPositionFromRelativePosition(float x, float y)
        {
            return new Vector2((int)Mathf.Round(x * m_tuioSettings.TrackingInterpolationX), m_tuioSettings.TrackingInterpolationY - (int)Mathf.Round(y * m_tuioSettings.TrackingInterpolationY));
        }
        #endregion Interface methods
    }
}
