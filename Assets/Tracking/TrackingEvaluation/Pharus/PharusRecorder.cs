using UnityPharus;
using System.Data;
using System.Collections;
using System.Collections.Generic;

namespace TrackingEvaluation
{
	/// <summary>
	/// This class stores received Pharus data in a DataTable. It makes use of a buffer to improve performance.
	/// </summary>
	public class PharusRecorder : IRecorder
	{
		protected const int BUFFER_LENGTH = 4;
		
		private int _recordsPerPlayerPerSecond;
		protected int _bufferIndex = 0;
		protected int _bufferIterations = 0;
		private DataRow[] _dataRowBuffer = new DataRow[BUFFER_LENGTH];
		protected DataTable _recordTable;
		private List<long> _recordedPlayersInCurrentSecond;
		private int _currentSecond;

		#region constructor
		public PharusRecorder(int theRecordsPerPlayerPerSecond = 5)
		{
			_recordsPerPlayerPerSecond = theRecordsPerPlayerPerSecond;
			_recordedPlayersInCurrentSecond = new List<long>();
			_currentSecond = 0;
		}
		#endregion

		#region buffer
		private void StoreContainer (PharusTransmission.TrackRecord trackRecord)
		{
			if(_currentSecond != (int)(UnityEngine.Time.time * _recordsPerPlayerPerSecond))
			{
				_currentSecond = (int)(UnityEngine.Time.time * _recordsPerPlayerPerSecond);
				_recordedPlayersInCurrentSecond.Clear();
			}
			else
			{
				if(_recordedPlayersInCurrentSecond.Contains(trackRecord.trackID))
				{
					return;
				}

			}
			_recordedPlayersInCurrentSecond.Add(trackRecord.trackID);
			
			DataRow newRow = GetNewDataRow (trackRecord);
			_dataRowBuffer[_bufferIndex] = newRow;
			
			_bufferIndex++;
			CheckBufferSize ();
		}

		private void CheckBufferSize ()
		{
			// if buffer is full
			if (_bufferIndex >= BUFFER_LENGTH) 
			{
				// store buffer to database
				for (int i = 0; i < BUFFER_LENGTH; i++) 
				{
					_recordTable.Rows.Add (_dataRowBuffer [i]);
				}

				_bufferIndex = 0;
				_bufferIterations++;
			}
		}
		#endregion

		#region pharus event handlers
		private void OnTrackAdded (object sender, UnityPharusEventProcessor.PharusEventTrackArgs e)
		{
			StoreContainer(e.trackRecord);
		}
		private void OnTrackUpdated (object sender, UnityPharusEventProcessor.PharusEventTrackArgs e)
		{
			StoreContainer(e.trackRecord);
		}
		private void OnTrackRemoved (object sender, UnityPharusEventProcessor.PharusEventTrackArgs e)
		{
			StoreContainer(e.trackRecord);
		}
		#endregion
		
		#region private methods
		protected virtual DataRow GetNewDataRow (PharusTransmission.TrackRecord trackRecord)
		{
			DataRow newRow = _recordTable.NewRow ();
			newRow ["id"] = (BUFFER_LENGTH * _bufferIterations) + _bufferIndex;
			newRow ["applicationTime"] = UnityEngine.Time.time;
			newRow ["trackRecordId"] = trackRecord.trackID;
			newRow ["position"] = new UnityEngine.Vector2 (trackRecord.relPos.x, trackRecord.relPos.y);
			return newRow;
		}

		protected virtual DataTable GetNewTable ()
		{
			// The maximum number of rows a DataTable can store is 16,777,216
			DataTable aTable = new DataTable();
			
			aTable.Columns.Add("id", typeof(int));
			aTable.Columns.Add("applicationTime", typeof(float));
			aTable.Columns.Add("trackRecordId", typeof(int));
			aTable.Columns.Add("position", typeof(UnityEngine.Vector2));
			
			// define primary key column
			DataColumn[] primaryKeyColumns = new DataColumn[1];
			primaryKeyColumns[0] = aTable.Columns["id"];
			aTable.PrimaryKey = primaryKeyColumns;
			
			return aTable;
		}
		#endregion

		#region public methods
		public void StartRecording()
		{
			_recordTable = GetNewTable();

			if(UnityPharusManager.Instance != null)
			{
				UnityPharusManager.Instance.EventProcessor.TrackAdded += OnTrackAdded;
				UnityPharusManager.Instance.EventProcessor.TrackUpdated += OnTrackUpdated;
				UnityPharusManager.Instance.EventProcessor.TrackRemoved += OnTrackRemoved;
			}
			else
			{
				UnityEngine.Debug.LogWarning("Couldn't start recording, UnityPharusManager was not found");
			}
		}

		public void StopRecording()
		{
			if(!ReferenceEquals(UnityPharusManager.Instance,null))
			{
				UnityPharusManager.Instance.EventProcessor.TrackAdded -= OnTrackAdded;
				UnityPharusManager.Instance.EventProcessor.TrackUpdated -= OnTrackUpdated;
				UnityPharusManager.Instance.EventProcessor.TrackRemoved -= OnTrackRemoved;
			}
			else
			{
				UnityEngine.Debug.LogWarning("Couldn't stop recording, UnityPharusManager was not found");
			}
		}
		#endregion

		#region public query methods
		public DataRow[] SelectFromTable(string theSelectSQLStatement)
		{
			return _recordTable.Select(theSelectSQLStatement);
		}
		public DataRow[] SelectFromTable()
		{
			return _recordTable.Select();
		}
		
		public void ClearTable()
		{
			_recordTable.Clear();
		}
		#endregion
	}
}
