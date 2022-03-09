using Assets.Pharus_Tracking_Framework.Interfaces;
using System;
using UnityEngine;

namespace Assets.Pharus_Tracking_Framework
{
    public static class TrackingAdapter
    {
        private const string NOT_READY = "Tracking Service not ready yet!";

        private static ITrackingService _trackingService;

        public static int TrackingInterpolationX
        {
            get
            {
                if (_trackingService == null)
                {
                    Console.WriteLine(NOT_READY);
                    return -1;
                }
                return _trackingService.TrackingInterpolationX;
            }
        }
        public static int TrackingInterpolationY
        {
            get
            {
                if (_trackingService == null)
                {
                    Console.WriteLine(NOT_READY);
                    return -1;
                }
                return _trackingService.TrackingInterpolationY;
            }
        }

        public static float TrackingStageX
        {
            get
            {
                if (_trackingService == null)
                {
                    Console.WriteLine(NOT_READY);
                    return -1f;
                }
                return _trackingService.TrackingStageX;
            }
        }
        public static float TrackingStageY
        {
            get
            {
                if (_trackingService == null)
                {
                    Console.WriteLine(NOT_READY);
                    return -1f;
                }
                return _trackingService.TrackingStageY;
            }
        }

        #region public methods
        public static void InjectTrackingManager(ITrackingService service)
        {
            if (service.Equals(_trackingService))
            {
                Console.WriteLine(string.Format("Tracking Manager of Type {0} has already been registered. Are you sure you know What you are doing?", _trackingService.GetType()));
            }

            if (_trackingService != null)
            {
                Console.WriteLine(string.Format("Overriding registered {0} with {1}. This can result in weird behavior", _trackingService.GetType(), service.GetType()));
            }

            _trackingService = service;
        }

        public static Vector2 GetScreenPositionFromRelativePosition(float x, float y)
        {
            if (_trackingService == null)
            {
                Console.WriteLine(NOT_READY);
                return Vector2.zero;
            }
            return _trackingService.GetScreenPositionFromRelativePosition(x, y);
        }
        #endregion
    }
}