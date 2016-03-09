using UnityEngine;
using System.Collections;

namespace UnityTracking {
	public static class TrackingAdapter
	{
		private const string NOT_READY = "TrackingManager not ready yet!";

		private static ITrackingManager _trackingManager;

		public static int TargetScreenWidth
		{
			get 
			{ 
				if (_trackingManager == null) 
				{
					Debug.LogWarning (NOT_READY);
					return -1;
				}
				return _trackingManager.TargetScreenWidth; 
			}
		}
		public static int TargetScreenHeight
		{
			get 
			{
				if (_trackingManager == null) 
				{
					Debug.LogWarning (NOT_READY);
					return -1;
				}
				return _trackingManager.TargetScreenHeight; 
			}
		}

		public static float TrackingStageX
		{
			get 
			{
				if (_trackingManager == null) 
				{
					Debug.LogWarning (NOT_READY);
					return -1f;
				}
				return _trackingManager.TrackingStageX; 
			}
		}
		public static float TrackingStageY
		{
			get 
			{
				if (_trackingManager == null) 
				{
					Debug.LogWarning (NOT_READY);
					return -1f;
				}
				return _trackingManager.TrackingStageY; 
			}
		}

		#region public methods
		public static void InjectTrackingManager (ITrackingManager manager)
		{
			_trackingManager = manager;
		}

		public static Vector2 GetScreenPositionFromRelativePosition(float x, float y)
		{
			if (_trackingManager == null) 
			{
				Debug.LogWarning (NOT_READY);
				return Vector2.zero;
			}
			return _trackingManager.GetScreenPositionFromRelativePosition (x, y);
		}
		#endregion
	}
}