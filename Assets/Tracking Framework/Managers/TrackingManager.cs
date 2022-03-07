using Assets.Tracking_Framework.Interfaces;
using Assets.Tracking_Framework.Services;
using Assets.Tracking_Framework.TransmissionFramework;
using Assets.Tracking_Framework.TransmissionFramework.UnityPharusFramework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

namespace Assets.Tracking_Framework.Managers
{
    public class TrackingManager : MonoBehaviour
    {
        private ITrackingService tuioService;
        private ITrackingService tracklinkService;
        private TrackingXMLConfig config;
        private static TrackingManager instance;
        private TrackingSettings settings = new TrackingSettings();

        //public GameObject canvasControl;
        //public TextMeshProUGUI trackingType;
        //public TextMeshProUGUI protocolStatus;
        //public TextMeshProUGUI interpolationStatus;
        //public TextMeshProUGUI stageStatus;

        public static TrackingManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = (TrackingManager)FindObjectOfType(typeof(TrackingManager));
                    if (instance == null)
                    {
                        // Debug.Log (string.Format ("No instance of {0} available.", typeof(TuioTrackingService)));
                    }
                    else
                    {
                        instance.Awake();
                    }
                }
                return instance;
            }
        }

        private async void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                if (instance != this)
                {
                    Debug.Log(string.Format("Other instance of {0} detected (will be destroyed)", typeof(TuioTrackingService)));
                    Destroy(this.gameObject);
                    return;
                }
            }

            StartCoroutine(InitializeServices());
        }

        private IEnumerator InitializeServices()
        {
            // Wait until config is loaded
            yield return StartCoroutine(nameof(LoadConfig));

            // Check service settings
            if (this.config != null)
            {
                this.settings.Initialize(this.config);
            }
            else
            {
                Debug.LogError($"Tracking config null.");
            }

            // Create services
            this.tuioService = new TuioTrackingService();
            this.tracklinkService = new PharusTrackingService();

            // Initialize services
            if (this.settings.TuioEnabled)
            {
                tuioService.Initialize(this.settings);
            }

            if (this.settings.TracklinkEnabled)
            {
                tracklinkService.Initialize(this.settings);
            }
        }

        public void Reconnect()
        {
            tuioService.Reconnect(1000);
            tracklinkService.Reconnect(1000);
        }

        private void Update()
        {
            tuioService.Update();
            tracklinkService.Update();
        }

        private IEnumerator LoadConfig()
        {
            string aPathToConfigXML = Path.Combine(Application.streamingAssetsPath, "trackingConfig.xml");
            if (File.Exists(aPathToConfigXML))
            {
                UnityWebRequest request = UnityWebRequest.Get("file:///" + aPathToConfigXML);
                yield return request.SendWebRequest();

                if (!request.isNetworkError && !request.isHttpError)
                {
                    Debug.Log("no errors occured during config file load");
                    config = TrackingXMLConfig.Load(aPathToConfigXML);
                }
            }
            else
            {
                Debug.Log($"No config file found at {aPathToConfigXML}. Using default settings: ");
            }
        }

        //private void Initialize()
        //{
        //    for (int i = 0; i < this.config.ConfigNodes.Length; i++)
        //    {
        //        string tracklinkConfig;
        //        string tuioConfig;

        //        switch (this.config.ConfigNodes[i].Name)
        //        {
        //            case "tracklink-enabled":
        //                tracklinkConfig = this.config.ConfigNodes[i].Value;
        //                break;
        //            case "tuio-enabled":
        //                tuioConfig = this.config.ConfigNodes[i].Value;
        //                break;
        //        }

        //        bool configTracklinkBool;
        //        if (tracklinkEnabled != null && Boolean.TryParse(tracklinkEnabled, out configTracklinkBool))
        //        {
        //            this.tracklinkEnabled = configTracklinkBool;
        //            Debug.Log(string.Format("TrackLink XML config: TrackLink enabled: {0}", this.tracklinkEnabled));
        //        }
        //    }
        //}
        //TODO: Adapt input and GUI
        //private void UpdateDebugGUI()
        //{
        //    if (trackingType != null)
        //    {
        //        trackingType.text = "Tracking System: TUIO";
        //    }
        //    if (protocolStatus != null)
        //    {
        //        protocolStatus.text = string.Format("TracklinkProtocol: UDP port:{0}", settings.TracklinkUdpPort);
        //    }
        //    if (interpolationStatus != null)
        //    {
        //        interpolationStatus.text = string.Format("Interpolation: {0}px x {1}px", settings.TrackingResolutionX, settings.TrackingResolutionY);
        //    }
        //    if (stageStatus != null)
        //    {
        //        stageStatus.text = string.Format("Stage Dimensions: {0}cm x {1}cm", settings.StageSizeX, settings.StageSizeY);
        //    }
        //}

        //private void UpdateDebugGUI()
        //{
        //    if (trackingType != null)
        //    {
        //        trackingType.text = "Tracking System: TrackLink";
        //    }
        //    if (protocolStatus != null)
        //    {
        //        string ipAddress = m_pharusSettings.TracklinkProtocol == PharusSettings.EProtocolType.UDP ? m_pharusSettings.TracklinkMulticastIp : m_pharusSettings.TracklinkTcpIp;
        //        string port = m_pharusSettings.TracklinkProtocol == PharusSettings.EProtocolType.UDP ? m_pharusSettings.TracklinkUdpPort.ToString() : m_pharusSettings.TracklinkTcpPort.ToString();
        //        protocolStatus.text = string.Format("TracklinkProtocol: {0} {1} : {2}", m_pharusSettings.TracklinkProtocol, ipAddress, port);
        //    }
        //    if (interpolationStatus != null)
        //    {
        //        interpolationStatus.text = string.Format("Interpolation: {0}px x {1}px", m_pharusSettings.TrackingResolutionX, m_pharusSettings.TrackingResolutionY);
        //    }
        //    if (stageStatus != null)
        //    {
        //        stageStatus.text = string.Format("Stage Dimensions: {0}cm x {1}cm", m_pharusSettings.StageSizeX, m_pharusSettings.StageSizeY);
        //    }
        //}

        //private IEnumerator LoadConfigXML()
        //{
        //    // Debug.Log("Trying to load config file");
        //    string aPathToConfigXML = Path.Combine(Application.dataPath, "tuioConfig.xml");
        //    aPathToConfigXML = "file:///" + aPathToConfigXML;
        //    UnityWebRequest request = UnityWebRequest.Get(aPathToConfigXML);
        //    // Debug.Log ("start loading file...");
        //    yield return request.SendWebRequest();
        //    // Debug.Log ("file loading complete");

        //    if (!request.isNetworkError && !request.isHttpError)
        //    {
        //        // Debug.Log ("no errors occured during config file load");
        //        xmlConfig = UnityTuioXMLConfig.LoadFromText(request.downloadHandler.text);
        //    }
        //}

        //private void HandleKeyboardInputs()
        //{
        //    if (Input.GetKeyDown(KeyCode.R))
        //    {
        //        Reconnect();
        //    }

        //    if (Input.GetKeyDown(KeyCode.Tab))
        //    {
        //        if (canvasControl != null)
        //        {
        //            canvasControl.SetActive(true);
        //            UpdateDebugGUI();
        //        }
        //    }
        //    else if (Input.GetKeyUp(KeyCode.Tab))
        //    {
        //        if (canvasControl != null)
        //        {
        //            canvasControl.SetActive(false);
        //            UpdateDebugGUI();
        //        }
        //    }
        //}
    }
}