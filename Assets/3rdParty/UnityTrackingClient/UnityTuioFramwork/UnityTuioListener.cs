using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TUIO;

namespace UnityTuio
{
	/// <summary>
	/// The UnityTuioListener has a TuioClient which listens for TUIO tracking data.
	/// The received data are stored in a Queue as TuioEvents.
	/// </summary>
	public class UnityTuioListener : TuioListener 
	{
		/// <summary>
		/// Specifies the type of the TuioEvent
		/// </summary>
		public enum ETuioEventType 
		{ 
			ADD_OBJECT, 
			UPDATE_OBJECT, 
			REMOVE_OBJECT,
			ADD_CURSOR, 
			UPDATE_CURSOR, 
			REMOVE_CURSOR,
			ADD_BLOB, 
			UPDATE_BLOB, 
			REMOVE_BLOB
		}
		
		/// <summary>
		/// A helper struct that encapsulates a new TUIO object together with the desired operation
		/// </summary>
		public struct TuioEvent
		{
			private ETuioEventType m_tuioEventType;
			private TuioContainer m_tuioEntity;
			public TuioEvent(ETuioEventType eventType, TuioContainer entity)
			{
				m_tuioEventType = eventType;
				m_tuioEntity = entity;
			}
			
			public ETuioEventType TuioEventType
			{
				get { return m_tuioEventType; }
			}
			public TuioContainer TuioEntity
			{
				get { return m_tuioEntity; }
			}
		}

		/// <summary>
		/// The UDP Port on which the TUIO Client should listen for TUIO data.
		/// </summary>
		private int m_udpPort = 3333;

		/// <summary>
		/// The TUIO client object responsible for receiving tracking data.
		/// </summary>
		private TuioClient m_client;

		private readonly object m_lockObj;
		/// <summary>
		/// Empty lock object for thread safety.
		/// </summary>
		public object LockObj
		{
			get { return m_lockObj; }
		}

		private Queue<TuioEvent> m_eventQueue;
		/// <summary>
		/// Contains all registered TuioEvents
		/// </summary>
		public Queue<TuioEvent> EventQueue
		{
			get { return m_eventQueue; }
		}
		
		#region constructors
		public UnityTuioListener()
		{
			m_lockObj = new object();
			InitTracking();
		}
		public UnityTuioListener(int udpPort)
		{
			m_udpPort = udpPort;
			m_lockObj = new object();
			InitTracking();
		}
		#endregion

		#region private methods
		private void InitTracking ()
		{
			while (m_client != null && m_client.isConnected())
			{
				Debug.LogWarning("Client still connecting. Waiting...");
			}
			
			m_eventQueue = new Queue<TuioEvent>();
			// Create a new TUIO client and listen for data on the specified port
			m_client = new TuioClient (m_udpPort);
			m_client.addTuioListener (this);
			m_client.connect ();
			if (!m_client.isConnected ()) 
			{
				Debug.LogError ("Couldn't listen at port " + m_udpPort + " for TUIO data. Check if port isn't already in use. (netstat -ano | find \"" + m_udpPort + "\")\n(also be sure to kill adb.exe if its still running)");
				m_client.removeTuioListener (this);
				m_client = null;
			}
			else 
			{
				Debug.Log ("--- Connection establised: listening at port " + m_udpPort + " for TUIO data. ---");
			}
		}
		#endregion

		#region public methods
		/// <summary>
		/// Returns if the Tuio Client is connected and listens for data.
		/// </summary>
		/// <returns>The listening status.</returns>
		public bool IsConnected()
		{
			if(m_client != null)
			{
				return m_client.isConnected();
			}
			Debug.LogWarning("--- No TuioClient object available ---");
			return false;
		}

		/// <summary>
		/// Shuts down the reception of tracking data by disconnecting the TUIO client.
		/// </summary>
		public void Shutdown()
		{
			if (m_client != null)
			{
				m_client.removeTuioListener(this);
				if(m_client.isConnected()) m_client.disconnect();
				m_client = null;
				Debug.Log("--- Disconnected TUIO client: port is now free ---");
			}
			else
			{
//				Debug.LogWarning("--- Disconnect failed: No TuioClient object available ---");
			}
		}

		/// <summary>
		/// Shuts down the TUIO client and immediately initializes the tracking again.
		/// </summary>
		public void Reconnect ()
		{
			Shutdown();
			Debug.Log ("--- Trying to reconnect tracking service... ---");
			InitTracking();
		}

		public bool HasTuioContainers()
		{
			return (HasTuioObjects() || HasTuioCursors() || HasTuioBlobs());
		}

		public bool HasTuioObjects()
		{
			return (m_client.getTuioObjects().Count > 0);
		}
		
		public bool HasTuioCursors()
		{
			return (m_client.getTuioCursors().Count > 0);
		}
		
		public bool HasTuioBlobs()
		{
			return (m_client.getTuioBlobs().Count > 0);
		}
		#endregion

		#region TuioListener implementation
		public void addTuioObject (TuioObject tobj)
		{
			lock(m_lockObj)
			{
				m_eventQueue.Enqueue(new TuioEvent(ETuioEventType.ADD_OBJECT, tobj));
			}
		}
		
		public void updateTuioObject (TuioObject tobj)
		{
			lock(m_lockObj)
			{
				m_eventQueue.Enqueue(new TuioEvent(ETuioEventType.UPDATE_OBJECT, tobj));
			}
		}
		
		public void removeTuioObject (TuioObject tobj)
		{
			lock(m_lockObj)
			{
				m_eventQueue.Enqueue(new TuioEvent(ETuioEventType.REMOVE_OBJECT, tobj));
			}
		}
		
		public void addTuioCursor (TuioCursor tcur)
		{
			lock(m_lockObj)
			{
				m_eventQueue.Enqueue(new TuioEvent(ETuioEventType.ADD_CURSOR, tcur));
			}
		}
		
		public void updateTuioCursor (TuioCursor tcur)
		{
			lock(m_lockObj)
			{
				m_eventQueue.Enqueue(new TuioEvent(ETuioEventType.UPDATE_CURSOR, tcur));
			}
		}
		
		public void removeTuioCursor (TuioCursor tcur)
		{
			lock(m_lockObj)
			{
				m_eventQueue.Enqueue(new TuioEvent(ETuioEventType.REMOVE_CURSOR, tcur));
			}
		}
		
		public void addTuioBlob (TuioBlob tblb)
		{
			lock(m_lockObj)
			{
				m_eventQueue.Enqueue(new TuioEvent(ETuioEventType.ADD_BLOB, tblb));
			}
		}
		
		public void updateTuioBlob (TuioBlob tblb)
		{
			lock(m_lockObj)
			{
				m_eventQueue.Enqueue(new TuioEvent(ETuioEventType.UPDATE_BLOB, tblb));
			}
		}
		
		public void removeTuioBlob (TuioBlob tblb)
		{
			lock(m_lockObj)
			{
				m_eventQueue.Enqueue(new TuioEvent(ETuioEventType.REMOVE_BLOB, tblb));
			}
		}
		
		public void refresh (TuioTime ftime)
		{
			// Intentionally left empty. We don't need the extra update loop but TUIO forces us to implent it.
		}
		#endregion
	}
}

