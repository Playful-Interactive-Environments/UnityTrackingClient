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
        /// <summary>
        /// General PharusTransmission Settings
        /// </summary>
        public class PharusSettings
        {
            public enum EProtocolType
            {
                TCP,
                UDP
            }

            private bool _tracklinkEnabled = true;
            private EProtocolType _protocol = EProtocolType.UDP;
            private string _tcpRemoteIpAddress = "127.0.0.1";
            private int _tcpLocalPort = 44345;
            private string _udpMulticastIpAddress = "239.1.1.1";
            private int _udpLocalPort = 44345;
            // the resolution, in unity units
            private int _trackingInterpolationX = 1920;
            private int _trackingInterpolationY = 1080;
            // the real size of play space in cm
            private float _trackingStageX = 1600f;
            private float _trackingStageY = 900f;
            // in milliseconds
            private int _checkServerReconnectIntervall = 5000;

            public bool TracklinkEnabled
            {
                get { return this._tracklinkEnabled; }
                set { this._tracklinkEnabled = value; }
            }
            public EProtocolType Protocol
            {
                get { return this._protocol; }
                set { this._protocol = value; }
            }
            public string TCP_IP_Address
            {
                get { return this._tcpRemoteIpAddress; }
                set { this._tcpRemoteIpAddress = value; }
            }
            public int TCP_Port
            {
                get { return this._tcpLocalPort; }
                set { this._tcpLocalPort = value; }
            }
            public string UDP_Multicast_IP_Address
            {
                get { return this._udpMulticastIpAddress; }
                set { this._udpMulticastIpAddress = value; }
            }
            public int UDP_Port
            {
                get { return this._udpLocalPort; }
                set { this._udpLocalPort = value; }
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
            public int CheckServerReconnectIntervall
            {
                get { return this._checkServerReconnectIntervall; }
            }
        }

        #region event handlers

        public static event EventHandler<EventArgs> OnTrackingInitialized;

        #endregion event handlers

        #region exposed inspector fields
        [SerializeField] private bool m_persistent = true;
        [SerializeField] private PharusSettings m_pharusSettings = new PharusSettings();
        #endregion exposed inspector fields

        private TrackingXMLConfig _mTrackingXmlConfig;
        private bool m_initialized = false;
        private UnityPharusListener m_listener;
        private UnityPharusEventProcessor m_eventProcessor;

        public UnityPharusEventProcessor EventProcessor
        {
            get { return m_eventProcessor; }
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
        //    if (m_eventProcessor != null)
        //    {
        //        m_eventProcessor.Process();
        //    }
        //}

        //private void OnDestroy()
        //{
        //    if (m_listener != null)
        //    {
        //        m_listener.Shutdown();
        //    }
        //}
        #endregion unity messages

        #region public methods
        public void Reconnect(int theDelay = -1)
        {
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
            this.m_pharusSettings.TrackingInterpolationX = width;
            this.m_pharusSettings.TrackingInterpolationY = height;
        }

        public void SetTrackingStage(float x, float y)
        {
            this.m_pharusSettings.TrackingStageX = x;
            this.m_pharusSettings.TrackingStageY = y;
        }

        public void Update()
        {
            //Lister for Pharus Data if Tracklink is enabled

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
            Debug.Log("Initialize Pharus");

            if (m_pharusSettings.CheckServerReconnectIntervall > 0)
            {
                Task.Run(() => CheckServerAlive(m_pharusSettings.CheckServerReconnectIntervall));
            }

            //if (m_persistent)
            //{
            //    GameObject.DontDestroyOnLoad(this.gameObject);
            //}

            // start: load config file
            if (_mTrackingXmlConfig != null)
            {
                string configTrackLinkEnabled = null;
                string configProtocol = null;
                string configTCPIP = null;
                string configTCPPort = null;
                string configUDPMulticastIP = null;
                string configUDPPort = null;
                string interpolationX = null;
                string interpolationY = null;
                string stageX = null;
                string stageY = null;
                for (int i = 0; i < _mTrackingXmlConfig.ConfigNodes.Length; i++)
                {
                    switch (_mTrackingXmlConfig.ConfigNodes[i].Name)
                    {
                        case "tracklink-enabled":
                            configTrackLinkEnabled = _mTrackingXmlConfig.ConfigNodes[i].Value;
                            break;

                        case "tracklink-protocol":
                            configProtocol = _mTrackingXmlConfig.ConfigNodes[i].Value;
                            break;

                        case "tracklink-tcp-ip":
                            configTCPIP = _mTrackingXmlConfig.ConfigNodes[i].Value;
                            break;

                        case "tracklink-tcp-port":
                            configTCPPort = _mTrackingXmlConfig.ConfigNodes[i].Value;
                            break;

                        case "tracklink-multicast-ip":
                            configUDPMulticastIP = _mTrackingXmlConfig.ConfigNodes[i].Value;
                            break;

                        case "tracklink-udp-port":
                            configUDPPort = _mTrackingXmlConfig.ConfigNodes[i].Value;
                            break;

                        case "trackingInterpolationX":
                            interpolationX = _mTrackingXmlConfig.ConfigNodes[i].Value;
                            break;

                        case "trackingInterpolationY":
                            interpolationY = _mTrackingXmlConfig.ConfigNodes[i].Value;
                            break;

                        case "trackingStageX":
                            stageX = _mTrackingXmlConfig.ConfigNodes[i].Value;
                            break;

                        case "trackingStageY":
                            stageY = _mTrackingXmlConfig.ConfigNodes[i].Value;
                            break;

                        default:
                            break;
                    }
                }

                bool configTracklinkBool;
                if (configTrackLinkEnabled != null && Boolean.TryParse(configTrackLinkEnabled, out configTracklinkBool))
                {
                    m_pharusSettings.TracklinkEnabled = configTracklinkBool;
                    Debug.Log(string.Format("TrackLink XML config: TrackLink enabled: {0}", m_pharusSettings.TracklinkEnabled));
                }
                //else
                //{
                //	Debug.Log(string.Format("TrackLink XML config: couldn't load enabled config. TrackLink enabled: {0}", m_pharusSettings.TracklinkEnabled));
                //}

                if (configProtocol != null)
                {
                    configProtocol = configProtocol.ToUpper();
                    switch (configProtocol)
                    {
                        case "UDP":
                            int configUDPPortInt;
                            if (configUDPMulticastIP != null &&
                                configUDPPort != null && int.TryParse(configUDPPort, out configUDPPortInt))
                            {
                                m_pharusSettings.Protocol = PharusSettings.EProtocolType.UDP;
                                m_pharusSettings.UDP_Multicast_IP_Address = configUDPMulticastIP;
                                m_pharusSettings.UDP_Port = configUDPPortInt;
                                Debug.Log(string.Format("TrackLink XML config: using UDP: {0}:{1}", configUDPMulticastIP, configUDPPort));
                            }
                            else
                            {
                                Debug.LogWarning("TrackLink XML config: invalid UDP config data");
                            }
                            break;

                        case "TCP":
                            int configTCPPortInt;
                            if (configTCPIP != null &&
                                configTCPPort != null && int.TryParse(configTCPPort, out configTCPPortInt))
                            {
                                m_pharusSettings.Protocol = PharusSettings.EProtocolType.TCP;
                                m_pharusSettings.TCP_IP_Address = configTCPIP;
                                m_pharusSettings.TCP_Port = configTCPPortInt;
                                Debug.Log(string.Format("TrackLink XML config: using TCP: {0}:{1}", configTCPIP, configTCPPort));
                            }
                            else
                            {
                                Debug.LogWarning("TrackLink XML config: invalid TCP config data");
                            }
                            break;

                        default:
                            Debug.LogWarning("TrackLink XML config: invalid protocol specification");
                            break;
                    }
                }
                else
                {
                    Debug.LogWarning("TrackLink XML config: invalid protocol specification");
                }

                int configInterpolationIntX;
                int configInterpolationIntY;
                if (interpolationX != null && int.TryParse(interpolationX, out configInterpolationIntX) &&
                    interpolationY != null && int.TryParse(interpolationY, out configInterpolationIntY))
                {
                    m_pharusSettings.TrackingInterpolationX = configInterpolationIntX;
                    m_pharusSettings.TrackingInterpolationY = configInterpolationIntY;
                    Debug.Log(string.Format("TrackLink XML config: tracking interpolation: {0}x{1}", m_pharusSettings.TrackingInterpolationX, m_pharusSettings.TrackingInterpolationY));
                }
                //else
                //{
                //	Debug.Log(string.Format("TrackLink XML config: invalid interpolation config. Using: {0}x{1}", m_pharusSettings.TrackingInterpolationX, m_pharusSettings.TrackingInterpolationY));
                //}

                float configStageFloatX;
                float configStageFloatY;
                if (stageX != null && float.TryParse(stageX, out configStageFloatX) &&
                    stageY != null && float.TryParse(stageY, out configStageFloatY))
                {
                    m_pharusSettings.TrackingStageX = configStageFloatX;
                    m_pharusSettings.TrackingStageY = configStageFloatY;
                    Debug.Log(string.Format("TrackLink XML config: stage size: {0}x{1}", m_pharusSettings.TrackingStageX, m_pharusSettings.TrackingStageY));
                }
                //else
                //{
                //	Debug.Log(string.Format("TrackLink XML config: invalid stage size config. Using: {0}x{1}", m_pharusSettings.TrackingStageX, m_pharusSettings.TrackingStageY));
                //}
            }
            else
            {
                Debug.Log("no TrackLink config xml file found in resources: Disable and Destroy PharusTrackingService");
                //this.enabled = false;
                //Destroy(this);
            }
            // end: load config file

            if (!m_pharusSettings.TracklinkEnabled)
            {
                Debug.Log("Pharus tracking disabled in config: Disable and Destroy PharusTrackingService");
                //this.enabled = false;
                //Destroy(this);
            }

            if (m_pharusSettings.Protocol == PharusSettings.EProtocolType.TCP)
            {
                m_listener = UnityPharusListener.NewUnityPharusListenerTCP(m_pharusSettings.TCP_IP_Address, m_pharusSettings.TCP_Port);
            }
            else if (m_pharusSettings.Protocol == PharusSettings.EProtocolType.UDP)
            {
                m_listener = UnityPharusListener.NewUnityPharusListenerUDP(m_pharusSettings.UDP_Multicast_IP_Address, m_pharusSettings.UDP_Port);
            }
            else
            {
                Debug.LogError("Invalid pharus settings!");
            }
            m_eventProcessor = new UnityPharusEventProcessor(m_listener);

            if (OnTrackingInitialized != null)
            {
                OnTrackingInitialized(this, new EventArgs());
            }

            TrackingAdapter.InjectTrackingManager(this);

            //UpdateDebugGUI();
        }

        #endregion public methods

        #region private methods

        private async void ReconnectTuioListenerDelayed(int theDelay)
        {
            m_listener.Shutdown();
            await Task.Delay(theDelay);
            m_listener.Reconnect();
        }
        
        private async void CheckServerAlive(int theWaitBetweenCheck)
        {
            while (true)
            {
                Debug.Log("CheckServerAlive");
                await Task.Delay(theWaitBetweenCheck);
                if (m_listener != null && !m_listener.IsCurrentlyConnecting && !m_listener.HasDataReceivedSinceLastCheck())
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
            get { return m_pharusSettings.TrackingInterpolationX; }
        }
        public int TrackingInterpolationY
        {
            get { return m_pharusSettings.TrackingInterpolationY; }
        }

        public float TrackingStageX
        {
            get { return m_pharusSettings.TrackingStageX; }
        }
        public float TrackingStageY
        {
            get { return m_pharusSettings.TrackingStageY; }
        }
        #endregion interface properties

        #region interface methods
        public Vector2 GetScreenPositionFromRelativePosition(float x, float y)
        {
            return new Vector2((int)Mathf.Round(x * m_pharusSettings.TrackingInterpolationX), m_pharusSettings.TrackingInterpolationY - (int)Mathf.Round(y * m_pharusSettings.TrackingInterpolationY));
        }

        #endregion interface methods

        //#region static methods
        //public Vector2 GetScreenPositionFromRelativePosition(Vector2f pharusTrackPosition)
        //{
        //    return GetScreenPositionFromRelativePosition(pharusTrackPosition.x, pharusTrackPosition.y);
        //}
        //#endregion static methods
    }
}
