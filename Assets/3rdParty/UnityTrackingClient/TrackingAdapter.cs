using UnityEngine;
using System.Collections;

namespace UnityTracking {
	public static class TrackingAdapter
	{
		private static ITrackingManager _trackingManager;

		public static int TargetScreenWidth
		{
			get { return _trackingManager.TargetScreenWidth; }
		}
		public static int TargetScreenHeight
		{
			get { return _trackingManager.TargetScreenHeight; }
		}

		public static float TrackingStageX
		{
			get { return _trackingManager.TrackingStageX; }
		}
		public static float TrackingStageY
		{
			get { return _trackingManager.TrackingStageY; }
		}

		#region public methods
		public static void InjectTrackingManager (ITrackingManager manager)
		{
			_trackingManager = manager;
		}

		public static Vector2 GetScreenPositionFromRelativePosition(float x, float y)
		{
			return _trackingManager.GetScreenPositionFromRelativePosition (x, y);
		}
		#endregion
	}
}