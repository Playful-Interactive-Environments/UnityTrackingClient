using UnityEngine;
using System.Collections;
using TUIO;
using System.IO;
using System;
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
		/// Overall TUIO Settings
		/// </summary>
		[System.Serializable]
		public class TuioSettings
		{
			[SerializeField] private bool _trackingEnabled = true;
			[SerializeField] private int _udpPort = 3333;
			[Tooltip("in pixel")]
			[SerializeField] private int _targetScreenWidth = 1920;
			[Tooltip("in pixel")]
			[SerializeField] private int _targetScreenHeight = 1080;
			[Tooltip("in centimeter")]
			[SerializeField] private float _stageX = 1600f;
			[Tooltip("in centimeter")]
			[SerializeField] private float _stageY = 900f;
			
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
			public int TargetScreenHeight 
			{
				get { return _targetScreenHeight; }
				set { this._targetScreenHeight = value; }
			}
			public int TargetScreenWidth 
			{
				get { return _targetScreenWidth; }
				set { this._targetScreenWidth = value; }
			}
			public float StageX
			{
				get { return this._stageX; }
				set { this._stageX = value; }
			}
			public float StageY
			{
				get { return this._stageY; }
				set { this._stageY = value; }
			}
		}
		
		#region event handlers
		public event EventHandler<EventArgs> OnTrackingInitialized;
		#endregion
		
		#region exposed inspector fields
		[SerializeField] private bool m_persistent = true;
		[SerializeField] private TuioSettings m_tuioSettings = new TuioSettings();
		
		public GameObject canvasControl;
		public Text trackingType;
		public Text protocolStatus;
		public Text resolutionStatus;
		#endregion
		
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
					m_instance = (UnityTuioManager) FindObjectOfType(typeof(UnityTuioManager));
					if (m_instance == null) 
					{
//						Debug.LogWarning (string.Format ("No instance of {0} available.", typeof(UnityTuioManager)));
					}
					else
					{
						m_instance.Awake();
					}
				}
				return m_instance;
			}
		}
		#endregion

		#region unity messages
		void Awake ()
		{
			if (m_instance == null) 
			{
				m_instance = this;
			}
			else
			{
				if(m_instance != this)
				{
					Debug.LogWarning (string.Format ("Other instance of {0} detected (will be destroyed)", typeof(UnityTuioManager)));
					GameObject.Destroy(this.gameObject);
					return;
				}
			}

			if (!m_initialized)
			{
				StartCoroutine(InitInstance());
			}
		}

		void Update()
		{
			HandleKeyboardInputs();
			
			//Lister for Tuio Data if enabled
			if (m_eventProcessor != null)
			{
				m_eventProcessor.Process();
			}
		}

		void OnDestroy()
		{
			if(m_listener != null)
			{
				m_listener.Shutdown();
			}
		}
		#endregion

		#region public methods
		public void ReconnectTuioListener(float theDelay = -1f)
		{
			if(m_listener == null || m_listener.HasTuioContainers()) return;

			if(theDelay <= 0)
			{
				m_listener.Reconnect();
			}
			else
			{
				StartCoroutine(ReconnectTuioListenerDelayed(theDelay));
			}
		}
		#endregion

		#region private methods
		private IEnumerator InitInstance()
		{
			m_initialized = true;

			Application.runInBackground = true;
			
			if(m_persistent)
			{
				GameObject.DontDestroyOnLoad(this.gameObject);
			}

			// start: load config file
			yield return StartCoroutine(LoadConfigXML());
//			Debug.Log ("UnityTuioManager config loaded, continue InitInstance");
			if (m_unityTuioXMLConfig != null)
			{
				string configTrackingEnabled = null;
				string configUDPPort = null;
				string targetResolutionX = null;
				string targetResolutionY = null;
				string stageX = null;
				string stageY = null;
				for (int i = 0; i < m_unityTuioXMLConfig.ConfigNodes.Length; i++)
				{
					switch(m_unityTuioXMLConfig.ConfigNodes[i].Name)
					{
						case "enabled":
							configTrackingEnabled = m_unityTuioXMLConfig.ConfigNodes[i].Value;
							break;
						case "udp-port":
							configUDPPort = m_unityTuioXMLConfig.ConfigNodes[i].Value;
							break;
						case "targetResolutionX":
							targetResolutionX = m_unityTuioXMLConfig.ConfigNodes[i].Value;
							break;
						case "targetResolutionY":
							targetResolutionY = m_unityTuioXMLConfig.ConfigNodes[i].Value;
							break;
						case "stageX":
							stageX = m_unityTuioXMLConfig.ConfigNodes[i].Value;
							break;
						case "stageY":
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
					Debug.Log(string.Format("XML config: TUIO tracking enabled: {0}", m_tuioSettings.TrackingEnabled));
				} else {
					Debug.Log(string.Format("XML config: invalid TUIO enabled config. Using settings from prefab instead: TUIO tracking enabled: {0}", m_tuioSettings.TrackingEnabled));
				}
				
				int configUDPPortInt;
				if (configUDPPort != null && int.TryParse(configUDPPort, out configUDPPortInt))
				{
					m_tuioSettings.UDP_Port = configUDPPortInt;
					Debug.Log(string.Format("XML config: TUIO using UDP Port: {0}", configUDPPort));
				} else { 	
					Debug.LogWarning("XML config: invalid TUIO Port config");
				}
				
				int configResolutionIntX;
				int configResolutionIntY;
				if (targetResolutionX != null && int.TryParse(targetResolutionX, out configResolutionIntX) &&
				    targetResolutionY != null && int.TryParse(targetResolutionY, out configResolutionIntY))
				{
					m_tuioSettings.TargetScreenWidth = configResolutionIntX;
					m_tuioSettings.TargetScreenHeight = configResolutionIntY;
					Debug.Log(string.Format("XML config: new target resolution: {0}x{1}", m_tuioSettings.TargetScreenWidth, m_tuioSettings.TargetScreenHeight));
				}
				else
				{
					Debug.LogWarning(string.Format("XML config: invalid resolution config, using resolution specified in TuioManager prefab instead: {0}x{1}", m_tuioSettings.TargetScreenWidth, m_tuioSettings.TargetScreenHeight));
				}

				float configStageFloatX;
				float configStageFloatY;
				if (stageX != null && float.TryParse(stageX, out configStageFloatX) &&
					stageY != null && float.TryParse(stageY, out configStageFloatY))
				{
					m_tuioSettings.StageX = configStageFloatX;
					m_tuioSettings.StageY = configStageFloatY;
					Debug.Log(string.Format("XML config: new stage size: {0}x{1}", m_tuioSettings.StageX, m_tuioSettings.StageY));
				}
				else
				{
					Debug.LogWarning(string.Format("XML config: invalid stage size config, using stage size specified in TuioManager prefab instead: {0}x{1}", m_tuioSettings.StageX, m_tuioSettings.StageY));
				}
			}
			else
			{
				Debug.Log("no config xml file found in resources: using default TUIO settings");
			}
			// end: load config file
			
			if (!m_tuioSettings.TrackingEnabled) 
			{
				Debug.Log("Disable and Destroy UnityTuioManager");
				this.enabled = false;
				Destroy(this);
				yield break;
			}

			Screen.SetResolution(m_tuioSettings.TargetScreenWidth, m_tuioSettings.TargetScreenHeight, true);

			m_listener = new UnityTuioListener(m_tuioSettings.UDP_Port);
			m_eventProcessor = new UnityTuioEventProcessor(m_listener);
			
			
			if (OnTrackingInitialized != null)
			{
				OnTrackingInitialized(this, new EventArgs());
			}

			TrackingAdapter.InjectTrackingManager (m_instance);
			
			UpdateDebugGUI();
		}
		
		private void UpdateDebugGUI()
		{
			if (trackingType != null) {
				trackingType.text = "Tracking System: TUIO";
			}
			if (protocolStatus != null) {
				protocolStatus.text = string.Format ("Protocol: UDP Port:{0}", m_tuioSettings.UDP_Port);
			}
			if (resolutionStatus != null) {
				resolutionStatus.text = string.Format ("Resolution: {0} x {1}", m_tuioSettings.TargetScreenWidth, m_tuioSettings.TargetScreenHeight);
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
//			Debug.Log ("Trying to load config file");
			string aPathToConfigXML = Path.Combine(Application.dataPath, "tuioConfig.xml");
			aPathToConfigXML = "file:///" + aPathToConfigXML;
			WWW aWww = new WWW(aPathToConfigXML);
//			Debug.Log ("start loading file...");
			yield return aWww;
//			Debug.Log ("file loading complete");
			
			if (aWww.isDone && string.IsNullOrEmpty(aWww.error))
			{
//				Debug.Log ("no errors occured during config file load");
				m_unityTuioXMLConfig = UnityTuioXMLConfig.LoadFromText(aWww.text);
			}
		}

		private void HandleKeyboardInputs()
		{
			if(Input.GetKeyDown(KeyCode.R))
			{
				ReconnectTuioListener();
			}

			if (Input.GetKeyDown(KeyCode.Tab))
			{
				if(canvasControl != null)
				{
					canvasControl.SetActive(true);
					UpdateDebugGUI();
				}
			} 
			else if (Input.GetKeyUp(KeyCode.Tab)) 
			{
				if(canvasControl != null)
				{
					canvasControl.SetActive(false);
					UpdateDebugGUI();
				}
			}
		}
		#endregion

		#region Interface properties
		public int TargetScreenWidth
		{
			get{ return Instance.m_tuioSettings.TargetScreenWidth; }
		}
		public int TargetScreenHeight
		{
			get{ return Instance.m_tuioSettings.TargetScreenHeight; }
		}

		public float TrackingStageX
		{
			get { return Instance.m_tuioSettings.StageX; }
		}
		public float TrackingStageY
		{
			get { return Instance.m_tuioSettings.StageY; }
		}
		#endregion

		#region Interface methods
		public Vector2 GetScreenPositionFromRelativePosition(float x, float y)
		{
			return new Vector2((int)Mathf.Round(x * m_tuioSettings.TargetScreenWidth), m_tuioSettings.TargetScreenHeight - (int)Mathf.Round(y * m_tuioSettings.TargetScreenHeight));
		}
		#endregion

		#region static methods
		public static Vector2 GetScreenPositionFromRelativePosition (TuioPoint tuioPoint)
		{
			return new Vector2(tuioPoint.getScreenX(Instance.m_tuioSettings.TargetScreenWidth), Instance.m_tuioSettings.TargetScreenHeight - tuioPoint.getScreenY(Instance.m_tuioSettings.TargetScreenHeight));
		}
		#endregion
	}
}