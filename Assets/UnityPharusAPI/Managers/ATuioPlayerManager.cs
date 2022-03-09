using Assets.UnityPharusAPI.Helper;
using Assets.UnityPharusAPI.Player;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityPharusAPI;
using UnityPharusAPI.TransmissionFrameworks.Tracklink;
using UnityPharusAPI.TransmissionFrameworks.Tuio;
using UnityPharusAPI.TransmissionFrameworks.Tuio.TUIO;

namespace Assets.UnityPharusAPI.Managers
{
    abstract public class ATuioPlayerManager : MonoBehaviour
    {
        protected List<ATrackingEntity> _playerList;
        [SerializeField] private GameObject _playerPrefab;
        [SerializeField] private bool _addUnknownPlayerOnUpdate = true;
        [SerializeField] private bool _subscribeTuioCursors = true;
        [SerializeField] private bool _subscribeTuioObjects = false;
        [SerializeField] private bool _subscribeTuioBlobs = false;

        /// <summary>
        /// A list of current active players
        /// </summary>
        public List<ATrackingEntity> PlayerList
        {
            get { return _playerList; }
        }

        protected virtual void Awake()
        {
            _playerList = new List<ATrackingEntity>();
        }

        /// <summary>
        /// Destroy all spawned objects when service shuts down.
        /// </summary>
        protected virtual void UnityTuioListenerOnServiceShutdown(object sender, EventArgs e)
        {
            foreach (ATrackingEntity player in _playerList.ToArray())
            {
                GameObject.Destroy(player.gameObject);
                _playerList.Remove(player);
            }
        }

        protected virtual void OnEnable()
        {
            SubscribeTrackingEvents(this, null);
        }

        protected virtual void OnDisable()
        {
            if (_subscribeTuioCursors)
            {
                TuioEventProcessor.CursorAdded -= OnCursorAdded;
                TuioEventProcessor.CursorUpdated -= OnCursorUpdated;
                TuioEventProcessor.CursorRemoved -= OnCursorRemoved;
            }
            if (_subscribeTuioObjects)
            {
                TuioEventProcessor.ObjectAdded -= OnObjectAdded;
                TuioEventProcessor.ObjectUpdated -= OnObjectUpdated;
                TuioEventProcessor.ObjectRemoved -= OnObjectRemoved;
            }
            if (_subscribeTuioBlobs)
            {
                TuioEventProcessor.BlobAdded -= OnBlobAdded;
                TuioEventProcessor.BlobUpdated -= OnBlobUpdated;
                TuioEventProcessor.BlobRemoved -= OnBlobRemoved;
            }
            UnityTuioListener.ServiceShutdown -= UnityTuioListenerOnServiceShutdown;
        }

        #region private methods
        protected virtual void SubscribeTrackingEvents(object theSender, System.EventArgs e)
        {
            if (_subscribeTuioCursors)
            {
                TuioEventProcessor.CursorAdded += OnCursorAdded;
                TuioEventProcessor.CursorUpdated += OnCursorUpdated;
                TuioEventProcessor.CursorRemoved += OnCursorRemoved;
            }
            if (_subscribeTuioObjects)
            {
                TuioEventProcessor.ObjectAdded += OnObjectAdded;
                TuioEventProcessor.ObjectUpdated += OnObjectUpdated;
                TuioEventProcessor.ObjectRemoved += OnObjectRemoved;
            }
            if (_subscribeTuioBlobs)
            {
                TuioEventProcessor.BlobAdded += OnBlobAdded;
                TuioEventProcessor.BlobUpdated += OnBlobUpdated;
                TuioEventProcessor.BlobRemoved += OnBlobRemoved;
            }
            UnityTuioListener.ServiceShutdown += UnityTuioListenerOnServiceShutdown;
        }
        #endregion

        #region tuio event handlers
        void OnCursorAdded(object sender, TuioEventProcessor.TuioEventCursorArgs e)
        {
            AddPlayer(e.tuioCursor);
        }
        void OnObjectAdded(object sender, TuioEventProcessor.TuioEventObjectArgs e)
        {
            AddPlayer(e.tuioObject);
        }
        void OnBlobAdded(object sender, TuioEventProcessor.TuioEventBlobArgs e)
        {
            AddPlayer(e.tuioBlob);
        }

        void OnCursorUpdated(object sender, TuioEventProcessor.TuioEventCursorArgs e)
        {
            UpdatePlayerPosition(e.tuioCursor);
        }
        void OnObjectUpdated(object sender, TuioEventProcessor.TuioEventObjectArgs e)
        {
            UpdatePlayerPosition(e.tuioObject);
        }
        void OnBlobUpdated(object sender, TuioEventProcessor.TuioEventBlobArgs e)
        {
            UpdatePlayerPosition(e.tuioBlob);
        }

        void OnCursorRemoved(object sender, TuioEventProcessor.TuioEventCursorArgs e)
        {
            RemovePlayer(e.tuioCursor.SessionID);
        }
        void OnObjectRemoved(object sender, TuioEventProcessor.TuioEventObjectArgs e)
        {
            RemovePlayer(e.tuioObject.SessionID);
        }
        void OnBlobRemoved(object sender, TuioEventProcessor.TuioEventBlobArgs e)
        {
            RemovePlayer(e.tuioBlob.SessionID);
        }
        #endregion

        #region player management
        /// <summary>
        /// Spans a Unity player object from TUIO data.
        /// </summary>
        /// <param name="theTuioContainer"></param>
        public virtual void AddPlayer(TuioContainer theTuioContainer)
        {
            Vector2 position = VectorAdapter.ToUnityVector2(TrackingAdapter.GetScreenPositionFromRelativePosition(theTuioContainer.Position.X, theTuioContainer.Position.Y));

            ATrackingEntity aPlayer = (GameObject.Instantiate(_playerPrefab, new Vector3(position.x, position.y, 0), Quaternion.identity) as GameObject).GetComponent<ATrackingEntity>();
            aPlayer.TrackID = theTuioContainer.SessionID;
            aPlayer.RelativePosition = new Vector2(theTuioContainer.Position.X, theTuioContainer.Position.Y);

            aPlayer.gameObject.name = string.Format("TuioPlayer_{0}", aPlayer.TrackID);

            _playerList.Add(aPlayer);
        }

        /// <summary>
        /// Updates player position.
        /// </summary>
        /// <param name="theTuioContainer"></param>
        public virtual void UpdatePlayerPosition(TuioContainer theTuioContainer)
        {
            foreach (ATrackingEntity player in _playerList)
            {
                if (player.TrackID.Equals(theTuioContainer.SessionID))
                {
                    //				Vector2 position = TuioTrackingService.GetScreenPositionFromRelativePosition (theTuioContainer.Position);
                    Vector2 position = VectorAdapter.ToUnityVector2(TrackingAdapter.GetScreenPositionFromRelativePosition(theTuioContainer.Position.X, theTuioContainer.Position.Y));
                    player.SetPosition(position);
                    player.RelativePosition = new Vector2(theTuioContainer.Position.X, theTuioContainer.Position.Y);
                    return;
                }
            }

            if (_addUnknownPlayerOnUpdate)
            {
                AddPlayer(theTuioContainer);
            }
        }

        /// <summary>
        /// Removes a player.
        /// </summary>
        /// <param name="sessionID"></param>
        public virtual void RemovePlayer(long sessionID)
        {
            foreach (ATrackingEntity player in _playerList.ToArray())
            {
                if (player.TrackID.Equals(sessionID))
                {
                    GameObject.Destroy(player.gameObject);
                    _playerList.Remove(player);
                    // return here in case you are really really sure the trackID is in our list only once!
                    //				return;
                }
            }
        }
        #endregion
    }
}
