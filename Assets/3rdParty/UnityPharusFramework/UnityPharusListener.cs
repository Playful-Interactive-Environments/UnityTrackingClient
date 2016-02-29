using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PharusTransmission;

namespace UnityPharus
{
	/// <summary>
	/// The UnityPharusListener has a TransmissionClient which connects via TCP to a PharusServer.
	/// The received data are stored in a Queue as PharusEvents.
	/// </summary>
	public class UnityPharusListener : ITransmissionReceiver
	{
		/// <summary>
		/// A helper struct that encapsulates a new Pharus track together with the desired operation
		/// </summary>
		public struct PharusEvent
		{
			private ETrackState m_pharusEventType;
			private TrackRecord m_trackRecord;
			public PharusEvent(ETrackState eventType, TrackRecord track)
			{
				m_pharusEventType = eventType;
				m_trackRecord = track;
			}
			
			public ETrackState PharusEventType
			{
				get { return m_pharusEventType; }
			}
			public TrackRecord TrackRecord
			{
				get { return m_trackRecord; }
			}
		}
		
		/// <summary>
		/// The Ip Address which the TransmissionClient should use to connect via TCP to the remote PharusServer.
		/// </summary>
		private string m_ipAddress = "127.0.0.1";

		/// <summary>
		/// The local Port on which the TransmissionClient should listen for incoming TCP packages from the remote PharusServer.
		/// </summary>
		private int m_port = 44345;

		/// <summary>
		/// The TransmissionClient object responsible for receiving tracking data.
		/// </summary>
		private ITransmissionClient m_client;

		private int eventCounter = 0;
		private readonly object m_lockObj;

		/// <summary>
		/// Empty lock object for thread safety.
		/// </summary>
		public object LockObj
		{
			get { return m_lockObj; }
		}
		
		private Queue<PharusEvent> m_eventQueue;
		/// <summary>
		/// Contains all registered PharusEvents
		/// </summary>
		public Queue<PharusEvent> EventQueue
		{
			get { return m_eventQueue; }
		}
	
		private bool m_connecting = false;
		public bool IsCurrentlyConnecting
		{
			get { return m_connecting; }
		}

		private bool m_useUDP = true;

		#region constructors
		private UnityPharusListener()
		{
			m_lockObj = new object();
		}

		public static UnityPharusListener NewUnityPharusListenerUDP(string remoteMulticastIpAdress, int localPort)
		{
			UnityPharusListener listener = new UnityPharusListener();
			listener.m_useUDP = true;
			listener.m_ipAddress = remoteMulticastIpAdress;
			listener.m_port = localPort;
			listener.InitTracking();
			return listener;
		}
		public static UnityPharusListener NewUnityPharusListenerTCP(string remoteServerIpAddress, int localPort)
		{
			UnityPharusListener listener = new UnityPharusListener();
			listener.m_useUDP = false;
			listener.m_ipAddress = remoteServerIpAddress;
			listener.m_port = localPort;
			listener.InitTracking();
			return listener;
		}
		#endregion
		
		#region private methods
		private void InitTracking ()
		{
			while (m_client != null && m_client.Connected)
			{
				Debug.LogWarning("Client still connecting. Waiting...");
			}
			
			m_eventQueue = new Queue<PharusEvent>();
			// Create a new TransmissionClient and listen for data on the specified port
			m_connecting = true;
			m_client = null;
			if(m_useUDP)
			{
				m_client = new UDPTransmissionClient(m_ipAddress, m_port, OnAbruptDisconnect);
			}
			else
			{
				m_client = new TCPTransmissionClient(m_ipAddress, m_port, OnAbruptDisconnect);
			}
			m_client.RegisterTransmissionReceiver (this);
			m_client.Connect();
			m_connecting = false;
			if (!m_client.Connected) 
			{
				Debug.LogWarning (string.Format ("Couldn't connect to remote PharusServer at {0}:{1}", m_ipAddress, m_port));
				m_client.UnregisterTransmissionReceiver (this);
				m_client.Disconnect();
				m_client = null;
			}
			else 
			{
				Debug.Log (string.Format ("--- Connection establised: receiving data from PharusServer at {0}:{1} ---", m_ipAddress, m_port));
			}
		}

		private void OnAbruptDisconnect(string message)
		{
			Debug.LogWarning(string.Format ("--- Abrupt disconnect from Server: {0} ---", message));
		}
		#endregion
		
		#region public methods
		/// <summary>
		/// Returns if the TransmissionClient is connected properly to a remote PharusServer.
		/// </summary>
		/// <returns>The connection status.</returns>
		public bool IsConnected()
		{
			if(m_client != null)
			{
				return m_client.Connected;
			}
			Debug.LogWarning("--- No TransmissionClient available ---");
			return false;
		}
		
		/// <summary>
		/// Shuts down the connection to the remote PharusServer
		/// </summary>
		public void Shutdown()
		{
			if (m_client != null)
			{
				if(m_client.Connected)
				{
					m_client.Disconnect();
					m_client.UnregisterTransmissionReceiver(this);
				} 
				m_client = null;
				Debug.Log("--- Disconnected from PharusServer: port is now free ---");
			}
			else
			{
//				Debug.LogWarning("--- Disconnect failed: No TransmissionClient available ---");
			}
		}
		
		/// <summary>
		/// Shuts down the TransmissionClient and immediately tries to reconnect.
		/// </summary>
		public void Reconnect ()
		{
			Shutdown();
			Debug.Log ("--- Trying to reconnect tracking service... ---");
			InitTracking();
		}

		/// <summary>
		/// Determines whether this instance has any PharusData received since last check.
		/// </summary>
		/// <returns><c>true</c> if this instance has data received since last check; otherwise, <c>false</c>.</returns>
		public bool HasDataReceivedSinceLastCheck ()
		{
			bool aReturn = (eventCounter > 0);
			eventCounter = 0;
			return aReturn;
		}
		#endregion
		
		#region ITransmissionReceiver implementation
		public void OnTrackNew (TrackRecord track)
		{
			eventCounter++;
			lock(m_lockObj)
			{
				m_eventQueue.Enqueue(new PharusEvent(track.state, track));
			}
		}

		public void OnTrackUpdate (TrackRecord track)
		{
			eventCounter++;
			lock(m_lockObj)
			{
				m_eventQueue.Enqueue(new PharusEvent(track.state, track));
			}
		}

		public void OnTrackLost (TrackRecord track)
		{
			eventCounter++;
			lock(m_lockObj)
			{
				m_eventQueue.Enqueue(new PharusEvent(track.state, track));
			}
		}
		#endregion
	}
}

