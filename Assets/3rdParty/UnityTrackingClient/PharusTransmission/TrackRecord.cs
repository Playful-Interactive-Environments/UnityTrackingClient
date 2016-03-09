using System.Collections.Generic;

namespace PharusTransmission
{
	/// <summary>
	/// Record of a tracking point
	/// </summary>
	public struct TrackRecord
	{
		/// <summary>
		/// The track's unique ID
		/// </summary>
		public int trackID;
		/// <summary>
		/// The track's current position in meters
		/// </summary>
		public Vector2f currentPos;
		/// <summary>
		/// The position the track will be expected in the next frame
		/// </summary>
		public Vector2f expectPos;
		/// <summary>
		/// The track's current position in relative coordinates (TUIO style)
		/// </summary>
		public Vector2f relPos;
		/// <summary>
		/// The track's current heading (normalized). Valid if speed is above 0.25 m/s.
		/// </summary>
		public Vector2f orientation;
		/// <summary>
		/// The track's current speed in meters per second
		/// </summary>
		public float speed;
		/// <summary>
		/// Yields in what state the track currently is
		/// </summary>
		public ETrackState state;
		/// <summary>
		/// List of CONFIRMED echoes that 'belong' to this track in relative coordinates (TUIO style)
		/// </summary>
		public List<Vector2f> echoes;
	}

	public struct Vector2f
	{
		public float x, y;
	}
}

