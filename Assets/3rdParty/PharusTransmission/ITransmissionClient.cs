using System;

namespace PharusTransmission
{
	public interface ITransmissionClient
	{
		bool Connected{ get; }

		void Connect();
		void Disconnect();

		void RegisterTransmissionReceiver(ITransmissionReceiver newReceiver);
		void UnregisterTransmissionReceiver(ITransmissionReceiver oldReceiver);
	}
}