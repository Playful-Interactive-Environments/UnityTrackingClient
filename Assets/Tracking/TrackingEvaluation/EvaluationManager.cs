using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace TrackingEvaluation
{
	public class EvaluationManager : AManager<EvaluationManager>
	{
		private enum ETrackingType
		{
			Pharus,
			TUIO
		}

		[SerializeField] private ETrackingType _trackingType = ETrackingType.Pharus;
		[SerializeField] private bool _active = false;
		[SerializeField] private List<string> _levelsToTrack;

		private const string PATH_DIR = @"\recording\";

		private bool _trackingCurrentLevel = false;
		private IRecorder _recorder;
		private LinkedList<EvaluationEvent> _eventList = new LinkedList<EvaluationEvent>();
		private string _timeStampStart;

		private GUIStyle _guiStyle;
		private float _showInfoTimeStamp;


		public Action OnGameOver;

		#region unity messages
		protected override void Awake ()
		{
			base.Awake ();
			OnGameOver = StopRecording;
			_guiStyle = new GUIStyle();
			_guiStyle.fontSize = 40;
			_guiStyle.normal.textColor = Color.black;
		}

		void OnLevelWasLoaded(int theLevel)
		{
			_trackingCurrentLevel = false;

			if(_active && 
			   _levelsToTrack.Contains(Application.loadedLevelName))
			{
				StartRecording();
			}
		}

		void Update()
		{
			if(Input.GetKeyDown(KeyCode.T))
			{
				ToggleRecording();
			}
		}
		#endregion

		#region public methods
		public void TriggerEvent(string theEventName, params KeyValuePair<string, string>[] theEventData)
		{
			if(!_trackingCurrentLevel)
			{
				return;
			}

			KeyValuePair<string, string> aTimeStampData = new KeyValuePair<string, string>("GameTime", Time.time.ToString());
			KeyValuePair<string, string>[] anEventData = new KeyValuePair<string, string>[theEventData.Length+1];
			anEventData[0] = aTimeStampData;
			for(int i = 1; i < anEventData.Length; i++)
			{
				anEventData[i] = theEventData[i-1];
			}

			EvaluationEvent anEvent = new EvaluationEvent(theEventName, anEventData);
			_eventList.AddLast(anEvent);

			Debug.Log (string.Format("event: {0}", theEventName));
		}
		#endregion

		#region private methods
		private void ToggleRecording ()
		{
			_active = !_active;
			_showInfoTimeStamp = Time.time + 3f;

			if(_active && _levelsToTrack.Contains(Application.loadedLevelName))
			{
				StartRecording();
			}
			else
			{
				TriggerEvent("GameEnd", new KeyValuePair<string, string>("Result","aborted"));
				StopRecording();
			}
		}

		private void StartRecording ()
		{

			if(_trackingType == ETrackingType.TUIO)
			{
				switch(Application.loadedLevelName)
				{
					default:
						_recorder = new TuioRecorder(5);
						break;
//					case "FishFeast":
//						_recorder = new TuioRecorderFishFeast(5);
//						break;
//					case "TowerOfPower":
//						_recorder = new TuioRecorderTowerOfPower(5);
//						Debug.Log ("recording ToP");
//						break;
				}
			}
			else if(_trackingType == ETrackingType.Pharus)
			{
				switch(Application.loadedLevelName)
				{
					default:
						_recorder = new PharusRecorder(5);
						break;
				}
			}
			else
			{
				Debug.LogWarning("no valid tracking type used");
				return;
			}
			
			_trackingCurrentLevel = true;
			_recorder.StartRecording();
			_timeStampStart = System.DateTime.Now.ToString("HH-mm-ss");
			
			Debug.Log (string.Format("tracking events for: {0}", Application.loadedLevelName));
		}

		private void StopRecording()
		{
			if(!_trackingCurrentLevel)
			{
				return;
			}
			
			_recorder.StopRecording();
			string aDateStamp = System.DateTime.Now.ToString("dd-MM-yyyy");
			string aTimeStampEnd = System.DateTime.Now.ToString("HH-mm-ss");
			System.IO.Directory.CreateDirectory(Application.persistentDataPath + PATH_DIR);
			FileWriter.SetPath (string.Format ("{0}{1}{2}_{3}_{4}.txt", Application.persistentDataPath, PATH_DIR, Application.loadedLevelName, aDateStamp, aTimeStampEnd));

			// session data
			FileWriter.WriteLine(string.Format("{{ \"SessionData\": {{ \"Game\": \"{0}\", \"DateStamp\": \"{1}\", \"TimeStampStart\": \"{2}\", \"TimeStampEnd\": \"{3}\" }}, \"Events\": {{ ", Application.loadedLevelName, aDateStamp, _timeStampStart, aTimeStampEnd));

			// events
			string[] lines = new string[_eventList.Count];
			int i = 0;
			foreach(EvaluationEvent anEvent in _eventList)
			{
				lines[i] = string.Format("\"{0}\": {{ ", anEvent.Name);

				int ii = 0;
				foreach (KeyValuePair<string, string> aDataSet in anEvent.Data) 
				{
					lines[i] += string.Format("\"{0}\": \"{1}\"", aDataSet.Key, aDataSet.Value);
					if((ii+1) < anEvent.Data.Count)
					{
						lines[i] += ",";
					}
					ii++;
				}

				lines[i] += "}";
				if((i+1) < _eventList.Count)
				{
					lines[i] += ",";
				}

				i++;
			}
			_eventList.Clear();
			FileWriter.WriteLines(lines);
			FileWriter.WriteLine(" }, \"PharusData\": { ");

			// coordinates
			System.Data.DataRow[] dataRows = _recorder.SelectFromTable();
			lines = new string[dataRows.Length];

			switch(Application.loadedLevelName)
			{
				default:
					for(int j = 0; j < lines.Length; j++) 
					{
						lines[j] = string.Format("\"{0}\": {{ ", dataRows[j]["id"]);
						Vector2 pos = (Vector2)dataRows[j]["position"];
						lines[j] += string.Format("\"GameTime\": \"{0}\", \"TrackRecordId\":\"{1}\", \"X\": \"{2}\", \"Y\": \"{3}\" }}", dataRows[j]["applicationTime"], dataRows[j]["trackRecordId"], pos.x, pos.y);
						if((j+1) < lines.Length)
						{
							lines[j] += ",";
						}
					}
					break;
//				case "FishFeast":
//					for(int j = 0; j < lines.Length; j++) 
//					{
//						lines[j] = string.Format("\"{0}\": {{ ", dataRows[j]["id"]);
//						Vector2 pos = (Vector2)dataRows[j]["position"];
//						lines[j] += string.Format("\"GameTime\": \"{0}\", \"TrackRecordId\":\"{1}\", \"X\": \"{2}\", \"Y\": \"{3}\", \"Status\": \"{4}\", \"Size\": \"{5}\" }}", dataRows[j]["applicationTime"], dataRows[j]["trackRecordId"], pos.x, pos.y, dataRows[j]["status"], dataRows[j]["size"]);
//						if((j+1) < lines.Length)
//						{
//							lines[j] += ",";
//						}
//					}
//					break;
//				case "TowerOfPower":
//					for(int j = 0; j < lines.Length; j++) 
//					{
//						lines[j] = string.Format("\"{0}\": {{ ", dataRows[j]["id"]);
//						Vector2 pos = (Vector2)dataRows[j]["position"];
//						lines[j] += string.Format("\"GameTime\": \"{0}\", \"TrackRecordId\":\"{1}\", \"X\": \"{2}\", \"Y\": \"{3}\", \"Team\": \"{4}\"}}", dataRows[j]["applicationTime"], dataRows[j]["trackRecordId"], pos.x, pos.y, dataRows[j]["team"]);
//						if((j+1) < lines.Length)
//						{
//							lines[j] += ",";
//						}
//					}
//					break;
			}

			_recorder.ClearTable();
			FileWriter.WriteLines(lines);
			FileWriter.WriteLine("} }");

			_trackingCurrentLevel = false;
		}
		#endregion

		#region OnGUI
		private void OnGUI()
		{
			if(Time.time < _showInfoTimeStamp)
			{
				string label = string.Format ("recording evaluation data: {0}", _active.ToString().ToUpper());
				GUI.Label(new Rect(50, 50, 960, 200), label, _guiStyle);
			}
		}
		#endregion
		
	}
}