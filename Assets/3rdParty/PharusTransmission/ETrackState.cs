namespace PharusTransmission
{
	/// <summary>
	/// Denotes the track's state
	/// </summary>
	public enum ETrackState
	{
		/// <summary>
		/// The track has been made public for the first iteration
		/// </summary>
		TS_NEW = 0,
		/// <summary>
		/// The track is already known - this is a position update
		/// </summary>
		TS_CONT = 1,
		/// <summary>
		/// The track has disappeared - this is the last notification of it
		/// </summary>
		TS_OFF = 2
	}
}

