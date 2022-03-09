﻿namespace Unity_Pharus_Tracking_Client.TransmissionFrameworks.Tracklink
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
