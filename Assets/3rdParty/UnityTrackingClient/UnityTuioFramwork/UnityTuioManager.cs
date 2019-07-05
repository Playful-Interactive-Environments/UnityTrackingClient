using System;
using System.Collections;
using System.IO;
using TUIO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityTracking;

namespace UnityTuio
{
    /// <summary>
    /// The UnityTuioManager keeps control over the UnityTuioListener and the UnityTuioEventProcessor.
    /// </summary>
    [AddComponentMenu("UnityTuio/UnityTuioManager")]
    public class UnityTuioManager : MonoBehaviour, ITrackingManager
    {
        /// <summary>
        /// General TUIO Settings
        /// </summary>
        [System.Serializable]
        public class TuioSettings
        {
            [SerializeField] private bool _trackingEnabled = true;
            [SerializeField] private int _udpPort = 3333;

            [Tooltip("in pixel")]
            [SerializeField]
            private int _trackingInterpolationX = 1920;

            [Tooltip("in pixel")]
            [SerializeField]
            private int _trackingInterpolationY = 1080;

            [Tooltip("in centimeter")]
            [SerializeField]
            private float _trackingStageX = 1600f;

            [Tooltip("in centimeter")]
            [SerializeField]
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

        #region event handlers

        public event EventHandler<EventArgs> OnTrackingInitialized;

        #endregion event handlers

        #region exposed inspector fields
        [SerializeField] private bool m_persistent = true;
        [SerializeField] private TuioSettings m_tuioSettings = new TuioSettings();

        public GameObject canvasControl;
        public Text trackingType;
        public Text protocolStatus;
        public Text interpolationStatus;
        public Text stageStatus;
        #endregion exposed inspector fields

        private UnityTuioXMLConfig m_unityTuioXMLConfig;
        private bool m_initialized = false;
        private UnityTuioListener m_listener;
        private UnityTuioEventProcessor m_eventProcessor;

        public UnityTuioEventProcessor EventProcessor
        {
            get { return m_eventProcessor; }
        }

        #region Singleton pattern
        private static UnityTuioManager m_instance;
        public static UnityTuioManager Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = (UnityTuioManager)FindObjectOfType(typeof(UnityTuioManager));
                    if (m_instance == null)
                    {
                        // Debug.Log (string.Format ("No instance of {0} available.", typeof(UnityTuioManager)));
                    }
                    else
                    {
                        m_instance.Awake();
                    }
                }
                return m_instance;
            }
        }
        #endregion Singleton pattern

        #region unity messages
        private void Awake()
        {
            if (m_instance == null)
            {
                m_instance = this;
            }
            else
            {
                if (m_instance != this)
                {
                    Debug.Log(string.Format("Other instance of {0} detected (will be destroyed)", typeof(UnityTuioManager)));
                    GameObject.Destroy(this.gameObject);
                    return;
                }
            }
        }

        private void OnEnable()
        {
            if (!m_initialized)
            {
                StartCoroutine(InitInstance());
            }
        }

        private void OnDisable()
        {
            Component.Destroy(this);
        }

        private void Update()
        {
            HandleKeyboardInputs();

            //Lister for Tuio Data if enabled
            if (m_eventProcessor != null)
            {
                m_eventProcessor.Process();
            }
        }

        private void OnDestroy()
        {
            if (m_listener != null)
            {
                m_listener.Shutdown();
            }
        }
        #endregion unity messages

        #region public methods
        public void ReconnectListener(float theDelay = -1f)
        {
            if (m_listener == null || m_listener.HasTuioContainers())
                return;

            if (theDelay <= 0)
            {
                m_listener.Reconnect();
            }
            else
            {
                StartCoroutine(ReconnectTuioListenerDelayed(theDelay));
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
        #endregion public methods

        #region private methods
        private IEnumerator InitInstance()
        {
            m_initialized = true;

            if (m_persistent)
            {
                GameObject.DontDestroyOnLoad(this.gameObject);
            }

            // start: load config file
            yield return StartCoroutine(LoadConfigXML());
            if (m_unityTuioXMLConfig != null)
            {
                string configTrackingEnabled = null;
                string configUDPPort = null;
                string interpolationX = null;
                string interpolationY = null;
                string stageX = null;
                string stageY = null;
                for (int i = 0; i < m_unityTuioXMLConfig.ConfigNodes.Length; i++)
                {
                    switch (m_unityTuioXMLConfig.ConfigNodes[i].Name)
                    {
                        case "enabled":
                            configTrackingEnabled = m_unityTuioXMLConfig.ConfigNodes[i].Value;
                            break;

                        case "udp-port":
                            configUDPPort = m_unityTuioXMLConfig.ConfigNodes[i].Value;
                            break;

                        case "trackingInterpolationX":
                            interpolationX = m_unityTuioXMLConfig.ConfigNodes[i].Value;
                            break;

                        case "trackingInterpolationY":
                            interpolationY = m_unityTuioXMLConfig.ConfigNodes[i].Value;
                            break;

                        case "trackingStageX":
                            stageX = m_unityTuioXMLConfig.ConfigNodes[i].Value;
                            break;

                        case "trackingStageY":
                            stageY = m_unityTuioXMLConfig.ConfigNodes[i].Value;
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
                Debug.Log("no TUIO config xml file found in resources: Disable and Destroy UnityTuioManager");
                this.enabled = false;
                Destroy(this);
                yield break;
            }
            // end: load config file

            if (!m_tuioSettings.TrackingEnabled)
            {
                Debug.Log("TUIO tracking disabled in config: Disable and Destroy UnityTuioManager");
                this.enabled = false;
                Destroy(this);
                yield break;
            }

            m_listener = new UnityTuioListener(m_tuioSettings.UDP_Port);
            m_eventProcessor = new UnityTuioEventProcessor(m_listener);

            if (OnTrackingInitialized != null)
            {
                OnTrackingInitialized(this, new EventArgs());
            }

            TrackingAdapter.InjectTrackingManager(m_instance);

            UpdateDebugGUI();
        }

        private void UpdateDebugGUI()
        {
            if (trackingType != null)
            {
                trackingType.text = "Tracking System: TUIO";
            }
            if (protocolStatus != null)
            {
                protocolStatus.text = string.Format("Protocol: UDP port:{0}", m_tuioSettings.UDP_Port);
            }
            if (interpolationStatus != null)
            {
                interpolationStatus.text = string.Format("Interpolation: {0}px x {1}px", m_tuioSettings.TrackingInterpolationX, m_tuioSettings.TrackingInterpolationY);
            }
            if (stageStatus != null)
            {
                stageStatus.text = string.Format("Stage Dimensions: {0}cm x {1}cm", m_tuioSettings.TrackingStageX, m_tuioSettings.TrackingStageY);
            }
        }

        private IEnumerator ReconnectTuioListenerDelayed(float theDelay)
        {
            m_listener.Shutdown();
            yield return new WaitForSeconds(theDelay);
            m_listener.Reconnect();
        }

        private IEnumerator LoadConfigXML()
        {
            // Debug.Log("Trying to load config file");
            string aPathToConfigXML = Path.Combine(Application.dataPath, "tuioConfig.xml");
            aPathToConfigXML = "file:///" + aPathToConfigXML;
            UnityWebRequest request = UnityWebRequest.Get(aPathToConfigXML);
            // Debug.Log ("start loading file...");
            yield return request.SendWebRequest();
            // Debug.Log ("file loading complete");

            if (!request.isNetworkError && !request.isHttpError)
            {
                // Debug.Log ("no errors occured during config file load");
                m_unityTuioXMLConfig = UnityTuioXMLConfig.LoadFromText(request.downloadHandler.text);
            }
        }

        private void HandleKeyboardInputs()
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                ReconnectListener();
            }

            if (Input.GetKeyDown(KeyCode.Tab))
            {
                if (canvasControl != null)
                {
                    canvasControl.SetActive(true);
                    UpdateDebugGUI();
                }
            }
            else if (Input.GetKeyUp(KeyCode.Tab))
            {
                if (canvasControl != null)
                {
                    canvasControl.SetActive(false);
                    UpdateDebugGUI();
                }
            }
        }
        #endregion private methods

        #region Interface properties
        public int TrackingInterpolationX
        {
            get { return Instance.m_tuioSettings.TrackingInterpolationX; }
        }
        public int TrackingInterpolationY
        {
            get { return Instance.m_tuioSettings.TrackingInterpolationY; }
        }

        public float TrackingStageX
        {
            get { return Instance.m_tuioSettings.TrackingStageX; }
        }
        public float TrackingStageY
        {
            get { return Instance.m_tuioSettings.TrackingStageY; }
        }
        #endregion Interface properties

        #region Interface methods
        public Vector2 GetScreenPositionFromRelativePosition(float x, float y)
        {
            return new Vector2((int)Mathf.Round(x * m_tuioSettings.TrackingInterpolationX), m_tuioSettings.TrackingInterpolationY - (int)Mathf.Round(y * m_tuioSettings.TrackingInterpolationY));
        }
        #endregion Interface methods

        #region static methods
        public static Vector2 GetScreenPositionFromRelativePosition(TuioPoint tuioPoint)
        {
            return new Vector2(tuioPoint.getScreenX(Instance.m_tuioSettings.TrackingInterpolationX), Instance.m_tuioSettings.TrackingInterpolationY - tuioPoint.getScreenY(Instance.m_tuioSettings.TrackingInterpolationY));
        }
        #endregion static methods
    }
}
