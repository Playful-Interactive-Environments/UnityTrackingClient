using UnityEngine;
using System.Collections;

namespace UnityTracking
{
	public static class TrackingAdapter
	{
		private const string NOT_READY = "TrackingManager not ready yet!";

		private static ITrackingManager _trackingManager;

		public static int TrackingInterpolationX
		{
			get
			{
				if (_trackingManager == null)
				{
					Debug.LogWarning(NOT_READY);
					return -1;
				}
				return _trackingManager.TrackingInterpolationX;
			}
		}
		public static int TrackingInterpolationY
		{
			get
			{
				if (_trackingManager == null)
				{
					Debug.LogWarning(NOT_READY);
					return -1;
				}
				return _trackingManager.TrackingInterpolationY;
			}
		}

		public static float TrackingStageX
		{
			get
			{
				if (_trackingManager == null)
				{
					Debug.LogWarning(NOT_READY);
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
					Debug.LogWarning(NOT_READY);
					return -1f;
				}
				return _trackingManager.TrackingStageY;
			}
		}

		#region public methods
		public static void InjectTrackingManager (ITrackingManager manager)
		{
			if (manager.Equals(_trackingManager))
			{
				Debug.LogWarning(string.Format("Tracking Manager of Type {0} has already been registered. Are you sure you know What you are doing?", _trackingManager.GetType()));
			}

			if (_trackingManager != null)
			{
				Debug.LogWarning(string.Format("Overriding registered {0} with {1}. This can result in weird behavior", _trackingManager.GetType(), manager.GetType()));
			}

			_trackingManager = manager;
		}

		public static Vector2 GetScreenPositionFromRelativePosition (float x, float y)
		{
			if (_trackingManager == null)
			{
				Debug.LogWarning(NOT_READY);
				return Vector2.zero;
			}
			return _trackingManager.GetScreenPositionFromRelativePosition(x, y);
		}
		#endregion
	}
}