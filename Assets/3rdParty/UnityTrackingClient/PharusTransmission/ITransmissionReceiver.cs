namespace PharusTransmission
{
	/// <summary>
	/// Interface for everything that wants to receive track updates from TransmissionClient
	/// </summary>
	public interface ITransmissionReceiver
	{
		void OnTrackNew(TrackRecord track);
		void OnTrackUpdate(TrackRecord track);
		void OnTrackLost(TrackRecord track);
	}
}