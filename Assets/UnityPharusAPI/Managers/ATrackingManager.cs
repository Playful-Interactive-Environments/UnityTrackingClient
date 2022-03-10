using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityPharusAPI;
using UnityPharusAPI.Interfaces;
using UnityPharusAPI.Services;
using UnityPharusAPI.TransmissionFrameworks.Tracklink;

namespace Assets.UnityPharusAPI.Managers
{
    /// <summary>
    /// The tracking manager, used for initializing and managing different tracking services.
    /// </summary>
    public abstract class ATrackingManager : MonoBehaviour
    {
        /// <summary>
        /// Currently available tracking services.
        /// </summary>
        private ITrackingService tuioService;
        private ITrackingService tracklinkService;

        /// <summary>
        /// The external configuration xml, located in the Streaming Assets folder
        /// </summary>
        private TrackingXMLConfig config;

        /// <summary>
        /// The tracking settings, loaded from the xml config file.
        /// </summary>
        private TrackingSettings settings = new TrackingSettings();

        private static ATrackingManager instance;
        public static ATrackingManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = (ATrackingManager)FindObjectOfType(typeof(ATrackingManager));
                    if (instance == null)
                    {
                        Debug.Log($"No instance of {typeof(TuioTrackingService)} available.");
                    }
                    else
                    {
                        instance.Awake();
                    }
                }
                return instance;
            }
        }

        public TrackingSettings Settings => settings;

        protected virtual void Awake()
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

        protected virtual void Update()
        {
            this.HandleKeyboardInputs();

            if (this.tuioService != null)
            {
                this.tuioService.Update();
            }

            if (this.tracklinkService != null)
            {
                this.tracklinkService.Update();
            }
        }

        /// <summary>
        /// Reconnects all tracking services.
        /// </summary>
        public virtual void Reconnect()
        {
            tuioService.Reconnect(1000);
            tracklinkService.Reconnect(1000);
        }

        /// <summary>
        /// Loads xml configuration and initializes tracking services
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerator InitializeServices()
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
                Debug.LogError($"Tracking config not loaded correctly. Using default settings");
            }

            // Create services
            this.tuioService = new TuioTrackingService();
            this.tracklinkService = new TracklinkTrackingService();


            // Initialize both services
            if (this.settings.TuioEnabled)
            {
                tuioService.Initialize(this.settings);
            }

            if (this.settings.TracklinkEnabled)
            {
                tracklinkService.Initialize(this.settings);
            }

            // If both tracking services are active, automatically prefer Tracklink data to avoid duplicate players
            while (this.settings.TracklinkEnabled && this.settings.TuioEnabled)
            {
                yield return new WaitForSeconds(1f);
                if (tracklinkService.IsActivelyReceiving && tuioService.IsActivelyReceiving)
                {
                    Debug.LogWarning($"There's more than one tracking service active. Automatically defaulting to Tracklink, no TUIO data will be received! If you want to use a specific tracking service, set it in the trackingConfig.xml in the Streaming Assets.");
                    tuioService.Shutdown();
                }
            }
        }

        /// <summary>
        /// Loads the configuration xml from the Streaming Assets folder.
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerator LoadConfig()
        {
            string aPathToConfigXML = Path.Combine(Application.streamingAssetsPath, "trackingConfig.xml");
            if (File.Exists(aPathToConfigXML))
            {
                UnityWebRequest request = UnityWebRequest.Get("file:///" + aPathToConfigXML);
                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.ConnectionError && request.result != UnityWebRequest.Result.ProtocolError)
                {
                    Debug.Log("no errors occured during config file load");
                    config = TrackingXMLConfig.Load(aPathToConfigXML);
                }
            }
            else
            {
                Debug.LogWarning($"No config file found at {aPathToConfigXML}. Using default settings: ");
            }
        }

        protected virtual void HandleKeyboardInputs()
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                Reconnect();
            }
        }
    }
}