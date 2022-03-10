﻿using Assets.Pharus_Tracking_Framework.TransmissionFrameworks.Tuio;
using Assets.Pharus_Tracking_Framework.TransmissionFrameworks.Tuio.TUIO;
using System.Collections.Generic;
using System.Data;

namespace Assets.Pharus_Tracking_Framework.TrackingEvaluation.TUIO
{
	/// <summary>
	/// This class stores received TUIO data in a DataTable. It makes use of a buffer to improve performance.
	/// </summary>
	public class TuioRecorder : IRecorder
	{
		protected const int BUFFER_LENGTH = 4;
		
		private int _recordsPerPlayerPerSecond;
		protected int _bufferIndex = 0;
		protected int _bufferIterations = 0;
		private DataRow[] _dataRowBuffer = new DataRow[BUFFER_LENGTH];
		protected DataTable _recordTable;
		private List<long> _recordedPlayersInCurrentSecond;
		private int _currentSecond;
		
		private bool _recordTuioCursors = true;
		private bool _recordTuioObjects = false;
		private bool _recordTuioBlobs = false;

		#region constructor
		public TuioRecorder(int theRecordsPerPlayerPerSecond = 5, bool theRecordCursors = true, bool theRecordObjects = false, bool theRecordBlobs = false)
		{
			_recordsPerPlayerPerSecond = theRecordsPerPlayerPerSecond;
			_recordedPlayersInCurrentSecond = new List<long>();
			_currentSecond = 0;

			_recordTuioCursors = theRecordCursors;
			_recordTuioObjects = theRecordObjects;
			_recordTuioBlobs = theRecordBlobs;
		}
		#endregion

		#region buffer
		private void StoreContainer (TuioContainer tuioContainer)
		{
			if(_currentSecond != (int)(UnityEngine.Time.time * _recordsPerPlayerPerSecond))
			{
				_currentSecond = (int)(UnityEngine.Time.time * _recordsPerPlayerPerSecond);
				_recordedPlayersInCurrentSecond.Clear();
			}
			else
			{
				if(_recordedPlayersInCurrentSecond.Contains(tuioContainer.SessionID))
				{
					return;
				}

			}
			_recordedPlayersInCurrentSecond.Add(tuioContainer.SessionID);
			
			DataRow newRow = GetNewDataRow (tuioContainer);
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

		#region tuio event handlers
		private void OnObjectAdded (object sender, TuioEventProcessor.TuioEventObjectArgs e)
		{
			StoreContainer(e.tuioObject);
		}
		private void OnObjectUpdated (object sender, TuioEventProcessor.TuioEventObjectArgs e)
		{
			StoreContainer(e.tuioObject);
		}
		private void OnObjectRemoved (object sender, TuioEventProcessor.TuioEventObjectArgs e)
		{
			StoreContainer(e.tuioObject);
		}
		
		private void OnCursorAdded (object sender, TuioEventProcessor.TuioEventCursorArgs e)
		{
			StoreContainer(e.tuioCursor);
		}
		private void OnCursorUpdated (object sender, TuioEventProcessor.TuioEventCursorArgs e)
		{
			StoreContainer(e.tuioCursor);
		}
		private void OnCursorRemoved (object sender, TuioEventProcessor.TuioEventCursorArgs e)
		{
			StoreContainer(e.tuioCursor);
		}
		
		private void OnBlobAdded (object sender, TuioEventProcessor.TuioEventBlobArgs e)
		{
			StoreContainer(e.tuioBlob);
		}
		private void OnBlobUpdated (object sender, TuioEventProcessor.TuioEventBlobArgs e)
		{
			StoreContainer(e.tuioBlob);
		}
		private void OnBlobRemoved (object sender, TuioEventProcessor.TuioEventBlobArgs e)
		{
			StoreContainer(e.tuioBlob);
		}
		#endregion
		
		#region private methods
		protected virtual DataRow GetNewDataRow (TuioContainer tuioContainer)
		{
			DataRow newRow = _recordTable.NewRow ();
			newRow ["id"] = (BUFFER_LENGTH * _bufferIterations) + _bufferIndex;
			newRow ["applicationTime"] = UnityEngine.Time.time;
			newRow ["trackRecordId"] = tuioContainer.SessionID;
			newRow ["position"] = new UnityEngine.Vector2 (tuioContainer.Position.X, tuioContainer.Position.Y);
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

            if (_recordTuioCursors)
            {
                TuioEventProcessor.CursorAdded += OnCursorAdded;
                TuioEventProcessor.CursorUpdated += OnCursorUpdated;
                TuioEventProcessor.CursorRemoved += OnCursorRemoved;
            }
            if (_recordTuioObjects)
            {
                TuioEventProcessor.ObjectAdded += OnObjectAdded;
                TuioEventProcessor.ObjectUpdated += OnObjectUpdated;
                TuioEventProcessor.ObjectRemoved += OnObjectRemoved;
            }
            if (_recordTuioBlobs)
            {
                TuioEventProcessor.BlobAdded += OnBlobAdded;
                TuioEventProcessor.BlobUpdated += OnBlobUpdated;
                TuioEventProcessor.BlobRemoved += OnBlobRemoved;
            }
        }

		public void StopRecording()
		{
            if (_recordTuioCursors)
            {
                TuioEventProcessor.CursorAdded -= OnCursorAdded;
                TuioEventProcessor.CursorUpdated -= OnCursorUpdated;
                TuioEventProcessor.CursorRemoved -= OnCursorRemoved;
            }
            if (_recordTuioObjects)
            {
                TuioEventProcessor.ObjectAdded -= OnObjectAdded;
                TuioEventProcessor.ObjectUpdated -= OnObjectUpdated;
                TuioEventProcessor.ObjectRemoved -= OnObjectRemoved;
            }
            if (_recordTuioBlobs)
            {
                TuioEventProcessor.BlobAdded -= OnBlobAdded;
                TuioEventProcessor.BlobUpdated -= OnBlobUpdated;
                TuioEventProcessor.BlobRemoved -= OnBlobRemoved;
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