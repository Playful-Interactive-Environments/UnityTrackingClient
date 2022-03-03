using Assets.Tracking_Framework.Interfaces;
using Assets.Tracking_Framework.Services;
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
        private bool tuioEnabled;
        private bool tracklinkEnabled;
        private TrackingXMLConfig config;
        private static TrackingManager instance;
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

            // Create services
            this.tuioService = new TuioTrackingService();
            this.tracklinkService = new PharusTrackingService();

            StartCoroutine(TryLoadXml());
        }

        private void InitializeServices()
        {
            tuioService.Initialize(this.config);
            tracklinkService.Initialize(this.config);
        }

        private void Start()
        {


        }

        private void OnDestroy()
        {

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

        private IEnumerator TryLoadXml()
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
                    this.InitializeServices();
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
        //        protocolStatus.text = string.Format("Protocol: UDP port:{0}", m_tuioSettings.UDP_Port);
        //    }
        //    if (interpolationStatus != null)
        //    {
        //        interpolationStatus.text = string.Format("Interpolation: {0}px x {1}px", m_tuioSettings.TrackingInterpolationX, m_tuioSettings.TrackingInterpolationY);
        //    }
        //    if (stageStatus != null)
        //    {
        //        stageStatus.text = string.Format("Stage Dimensions: {0}cm x {1}cm", m_tuioSettings.TrackingStageX, m_tuioSettings.TrackingStageY);
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
        //        string ipAddress = m_pharusSettings.Protocol == PharusSettings.EProtocolType.UDP ? m_pharusSettings.UDP_Multicast_IP_Address : m_pharusSettings.TCP_IP_Address;
        //        string port = m_pharusSettings.Protocol == PharusSettings.EProtocolType.UDP ? m_pharusSettings.UDP_Port.ToString() : m_pharusSettings.TCP_Port.ToString();
        //        protocolStatus.text = string.Format("Protocol: {0} {1} : {2}", m_pharusSettings.Protocol, ipAddress, port);
        //    }
        //    if (interpolationStatus != null)
        //    {
        //        interpolationStatus.text = string.Format("Interpolation: {0}px x {1}px", m_pharusSettings.TrackingInterpolationX, m_pharusSettings.TrackingInterpolationY);
        //    }
        //    if (stageStatus != null)
        //    {
        //        stageStatus.text = string.Format("Stage Dimensions: {0}cm x {1}cm", m_pharusSettings.TrackingStageX, m_pharusSettings.TrackingStageY);
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