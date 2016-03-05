using System;
using System.Collections.Generic;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace PharusTransmission
{
	public class UDPTransmissionClient : ITransmissionClient
	{		
		private UdpClient udpClient;
		private Thread udpClientThread;
		private Dictionary<int, TrackRecord> trackDict;
		private List<ITransmissionReceiver> transmissionReceiverList;
		private string multicastIpAddress;
		private int localPort;
		private bool connected = false;
		private Action<string> OnAbruptDisconnect;

		#region properties
		public string MulticastIpAddress
		{
			get { return multicastIpAddress; }
		}
		public int LocalPort
		{
			get { return localPort; }
		}
		public bool Connected
		{ 
			get { return connected; }
		}
		public List<ITransmissionReceiver> TransmissionReceivers
		{
			get 
			{
				lock(transmissionReceiverList)
				{
					return transmissionReceiverList; 
				}
			}
		}
		public Dictionary<int, TrackRecord> TrackRecords
		{
			get 
			{
				lock(trackDict)
				{
					return trackDict; 
				}
			}
		}
		#endregion

		#region constructor / destructor
		public UDPTransmissionClient(string multicastIpAddress, int localPort, Action<string> abruptDisconnectCallback = null)
		{
			this.multicastIpAddress = multicastIpAddress;
			this.localPort = localPort;
			this.OnAbruptDisconnect = abruptDisconnectCallback;

			trackDict = new Dictionary<int, TrackRecord>();
			transmissionReceiverList = new List<ITransmissionReceiver>();

//          Connect();
		}
		~UDPTransmissionClient()
		{
			trackDict.Clear();
			transmissionReceiverList.Clear();
			Disconnect();
		}
		#endregion

		#region public methods
		public void Connect()
		{
			if(udpClient != null)
			{
				Disconnect();
			}


			try
			{
				udpClient = new UdpClient();
				
				IPAddress multicastGroupAddress = null;
				if(!IPAddress.TryParse(multicastIpAddress, out multicastGroupAddress))
				{
					OnAbruptDisconnect("TransmissionClient: Invalid multicast group address!");
					return;
				}
				IPEndPoint localIpEndPoint = new IPEndPoint(IPAddress.Any, localPort);

				udpClient.ExclusiveAddressUse = false;
				udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
				udpClient.Client.Bind(localIpEndPoint);
				udpClient.JoinMulticastGroup(multicastGroupAddress);

				if(udpClient == null || udpClient.Client == null)
				{
					OnAbruptDisconnect("TransmissionClient: client not available!");
					return;
				}
				connected = true;
				udpClientThread = new Thread(new ThreadStart(Listen));
				udpClientThread.Start();
			}
			catch (Exception e) 
			{ 
				OnAbruptDisconnect(e.Message);
				Console.WriteLine (string.Format ("TransmissionClient: Failed to connect via UDP to {0}:{1}", multicastIpAddress, localPort));
				Console.WriteLine(e.Message);
			}
		}
		
		public void Disconnect()
		{
			if(!connected)
			{
				return;
			}

			connected = false;
			RemoveAllTrackRecords();
			if (udpClient != null)
			{
				udpClient.Close();
				udpClient = null;
			}
		}

		public void RegisterTransmissionReceiver(ITransmissionReceiver newReceiver)
		{
			lock(transmissionReceiverList)
			{
				if(transmissionReceiverList.Contains(newReceiver))
				{
					return;
				}
				transmissionReceiverList.Add(newReceiver);
			}
		}
		public void UnregisterTransmissionReceiver(ITransmissionReceiver oldReceiver)
		{
			lock(transmissionReceiverList)
			{
				transmissionReceiverList.Remove(oldReceiver);
			}
		}
		#endregion

		#region private methods
		private void Listen()
		{
			while (connected)
			{
				try
				{
					Receive();
				}
				catch (Exception e) 
				{ 
					Console.WriteLine(e.Message);
					
					if(connected && OnAbruptDisconnect != null)
					{
						OnAbruptDisconnect(e.Message);
					}

                    Disconnect();
				}
			}
		}

		private void Receive ()
		{
			IPEndPoint remoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
			byte[] recvData = udpClient.Receive(ref remoteIpEndPoint);
			int recvSize = recvData.Length;
			if(recvData != null && recvSize > 0)
			{
				int i = 0;
				while(i < recvSize)
				{
					if (Convert.ToChar(recvData[i++]) != 'T')
					{
						Console.WriteLine("TransmissionClient: Unexpected header byte, skipping packet.");
						i = recvSize;
						continue;
					}
					
					// get the tracks's id
					int tid;
					tid = UnpackInt(recvData, ref i);
					
					lock(trackDict)
					{
						// is this track known? if so, update, else add:
						bool unknownTrack = !trackDict.ContainsKey(tid);
						
						TrackRecord track;
						if (unknownTrack)
						{
							track = new TrackRecord();
							track.echoes = new List<Vector2f>();
							track.trackID = tid;
							trackDict.Add(tid, track);
						}
						else
						{
							track = trackDict[tid];
						}
						
						track.state = (ETrackState) UnpackInt(recvData, ref i);
						track.currentPos.x = UnpackFloat(recvData, ref i);
						track.currentPos.y = UnpackFloat(recvData, ref i);
						track.expectPos.x = UnpackFloat(recvData, ref i);
						track.expectPos.y = UnpackFloat(recvData, ref i);
						track.orientation.x = UnpackFloat(recvData, ref i);
						track.orientation.y = UnpackFloat(recvData, ref i);
						track.speed = UnpackFloat(recvData, ref i); 
						track.relPos.x = UnpackFloat(recvData, ref i); 
						track.relPos.y = UnpackFloat(recvData, ref i);
						track.echoes.Clear();
						while (Convert.ToChar(recvData[i]) == 'E') // peek if echo(es) available
						{
							++i; // yep, then skip 'E'
							Vector2f echo;
							echo.x = UnpackFloat(recvData, ref i);
							echo.y = UnpackFloat(recvData, ref i);
							track.echoes.Add(echo);
							++i; // 'e'
						}
						
						if (Convert.ToChar(recvData[i++]) != 't')
						{
							Console.WriteLine("TransmissionClient: Unexpected tailing byte, skipping packet.");
							i = recvSize;
							continue;
						}
						
						lock(transmissionReceiverList)
						{
							//notify callbacks
							foreach(ITransmissionReceiver receiver in transmissionReceiverList)
							{
								// track is unknown yet AND is not about to die
								if (unknownTrack && track.state != ETrackState.TS_OFF)
								{
									receiver.OnTrackNew(track);
								}
								// standard track update
								else if (!unknownTrack && track.state != ETrackState.TS_OFF)
								{
									receiver.OnTrackUpdate(track);
								}
								// track is known and this is his funeral
								else if (!unknownTrack && track.state == ETrackState.TS_OFF)
								{
									receiver.OnTrackLost(track);
								}
							}
						}
						
						// remove track from dictionary
						if(track.state == ETrackState.TS_OFF)
						{
							trackDict.Remove(track.trackID);
						}
					}
					
				}
			}
		}

		private void RemoveAllTrackRecords ()
		{
			foreach(KeyValuePair<int, TrackRecord> entry in TrackRecords)
			{
				TrackRecord track = entry.Value;
				foreach(ITransmissionReceiver receiver in transmissionReceiverList)
				{
					track.state = ETrackState.TS_OFF;
					receiver.OnTrackLost(track);
				}
			}
		}
		#endregion

		#region byte unpacking
        //protected static char UnpackChar(byte[] bytes, ref int start)
        //{
        //    byte[] data = { bytes[start], bytes[start + 1] };
        //    start += 2;
        //    return BitConverter.ToChar(data, 0);
        //}
		protected static string UnpackString(byte[] bytes, ref int start)
		{
			int count = 0;
			for(int index = start ; bytes[index] != 0 ; index++, count++) ;
			string s = Encoding.ASCII.GetString(bytes, start, count);
			start += count+1;
			start = (start + 3) / 4 * 4;
			return s;
		}
		protected static int UnpackInt(byte[] bytes, ref int start)
		{
			byte[] data = new byte[4];
			for(int i = 0 ; i < 4 ; i++, start++) data[i] = bytes[start];
            if (!BitConverter.IsLittleEndian) data = SwapEndian(data);
			return BitConverter.ToInt32(data, 0);
		}
		protected static long UnpackLong(byte[] bytes, ref int start)
		{
			byte[] data = new byte[8];
			for(int i = 0 ; i < 8 ; i++, start++) data[i] = bytes[start];
            if (!BitConverter.IsLittleEndian) data = SwapEndian(data);
			return BitConverter.ToInt64(data, 0);
		}
		protected static float UnpackFloat(byte[] bytes, ref int start)
		{
			byte[] data = new byte[4];
			for(int i = 0 ; i < 4 ; i++, start++) data[i] = bytes[start];
            if (!BitConverter.IsLittleEndian) data = SwapEndian(data);
			return BitConverter.ToSingle(data, 0);
		}
		protected static double UnpackDouble(byte[] bytes, ref int start)
		{
			byte[] data = new byte[8];
			for(int i = 0 ; i < 8 ; i++, start++) data[i] = bytes[start];
			if(!BitConverter.IsLittleEndian) data = SwapEndian(data);
			return BitConverter.ToDouble(data, 0);
		}
		protected static byte[] SwapEndian(byte[] data)
		{
			byte[] swapped = new byte[data.Length];
			for(int i = data.Length - 1, j = 0 ; i >= 0 ; i--, j++)
			{
				swapped[j] = data[i];
			}
			return swapped;
		}
		#endregion
	}


}