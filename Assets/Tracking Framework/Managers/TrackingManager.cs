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
    /// <summary>
    /// The tracking manager, used for initializing and managing different tracking services.
    /// </summary>
    public class TrackingManager : MonoBehaviour
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

        private static TrackingManager instance;
        public static TrackingManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = (TrackingManager)FindObjectOfType(typeof(TrackingManager));
                    if (instance == null)
                    {
                        Debug.Log ($"No instance of {typeof(TuioTrackingService)} available.");
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

        private void Update()
        {
            this.HandleKeyboardInputs();
            this.tuioService.Update();
            this.tracklinkService.Update();
        }

        /// <summary>
        /// Reconnects all tracking services.
        /// </summary>
        public void Reconnect()
        {
            tuioService.Reconnect(1000);
            tracklinkService.Reconnect(1000);
        }

        /// <summary>
        /// Loads xml configuration and initializes tracking services
        /// </summary>
        /// <returns></returns>
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
                Debug.LogError($"Tracking config not loaded correctly. Using default settings");
            }

            // Create services
            this.tuioService = new TuioTrackingService();
            this.tracklinkService = new TracklinkTrackingService();

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

        /// <summary>
        /// Loads the configuration xml from the Streaming Assets folder.
        /// </summary>
        /// <returns></returns>
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

        private void HandleKeyboardInputs()
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                Reconnect();
            }
        }
    }
}