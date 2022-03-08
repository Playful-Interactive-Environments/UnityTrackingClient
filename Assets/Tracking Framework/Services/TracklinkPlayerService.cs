using Assets.Tracking_Framework.Player;
using Assets.Tracking_Framework.TransmissionFramework;
using Assets.Tracking_Framework.TransmissionFramework.TracklinkTransmission;
using Assets.Tracking_Framework.TransmissionFramework.UnityPharusFramework;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Tracking_Framework.Services
{
    abstract public class TracklinkPlayerService : MonoBehaviour
    {
        protected List<ATrackingEntity> _playerList;
        public GameObject _playerPrefab;
        public bool _addUnknownPlayerOnUpdate = true;
	
        public List<ATrackingEntity> PlayerList
        {
            get { return _playerList; }
        }
	
        void Awake()
        {
            _playerList = new List<ATrackingEntity>();
        }
	
        void OnEnable()
        {
            SubscribeTrackingEvents(this, null);
        }
	
        void OnDisable()
        {
            UnityPharusEventProcessor.TrackAdded -= OnTrackAdded;
            UnityPharusEventProcessor.TrackUpdated -= OnTrackUpdated;
            UnityPharusEventProcessor.TrackRemoved -= OnTrackRemoved;
        }

        #region private methods
        private void SubscribeTrackingEvents (object theSender, System.EventArgs e)
        {
            UnityPharusEventProcessor.TrackAdded += OnTrackAdded;
            UnityPharusEventProcessor.TrackUpdated += OnTrackUpdated;
            UnityPharusEventProcessor.TrackRemoved += OnTrackRemoved;
        }
        #endregion
	
        #region tuio event handlers
        void OnTrackAdded (object sender, UnityPharusEventProcessor.PharusEventTrackArgs e)
        {
            AddPlayer(e.trackRecord);
        }
        void OnTrackUpdated (object sender, UnityPharusEventProcessor.PharusEventTrackArgs e)
        {
            UpdatePlayerPosition(e.trackRecord);
        }
        void OnTrackRemoved (object sender, UnityPharusEventProcessor.PharusEventTrackArgs e)
        {
            RemovePlayer(e.trackRecord.trackID);
        }
        #endregion
	
        #region player management
        public virtual void AddPlayer (TrackRecord trackRecord)
        {
//		Vector2 position = TracklinkTrackingService.GetScreenPositionFromRelativePosition(trackRecord.relPos);
            Vector2 position = TrackingAdapter.GetScreenPositionFromRelativePosition(trackRecord.relPos.x, trackRecord.relPos.y);
            ATrackingEntity aPlayer = (GameObject.Instantiate(_playerPrefab, new Vector3(position.x,position.y,0), Quaternion.identity) as GameObject).GetComponent<ATrackingEntity>();
            aPlayer.TrackID = trackRecord.trackID;
            aPlayer.AbsolutePosition = new Vector2(trackRecord.currentPos.x,trackRecord.currentPos.y);
            aPlayer.NextExpectedAbsolutePosition = new Vector2(trackRecord.expectPos.x,trackRecord.expectPos.y);
            aPlayer.RelativePosition = new Vector2(trackRecord.relPos.x,trackRecord.relPos.y);
            aPlayer.Orientation = new Vector2(trackRecord.orientation.x,trackRecord.orientation.y);
            aPlayer.Speed = trackRecord.speed;
            aPlayer.Echoes.Clear ();
            trackRecord.echoes.AddToVector2List (aPlayer.Echoes);

            aPlayer.gameObject.name = string.Format("PharusPlayer_{0}", aPlayer.TrackID);

            _playerList.Add(aPlayer);
        }
	
        public virtual void UpdatePlayerPosition (TrackRecord trackRecord)
        {
            foreach (ATrackingEntity aPlayer in _playerList) 
            {
                if(aPlayer.TrackID == trackRecord.trackID)
                {
                    aPlayer.AbsolutePosition = new Vector2(trackRecord.currentPos.x,trackRecord.currentPos.y);
                    aPlayer.NextExpectedAbsolutePosition = new Vector2(trackRecord.expectPos.x,trackRecord.expectPos.y);
                    aPlayer.RelativePosition = new Vector2(trackRecord.relPos.x,trackRecord.relPos.y);
                    aPlayer.Orientation = new Vector2(trackRecord.orientation.x,trackRecord.orientation.y);
                    aPlayer.Speed = trackRecord.speed;
                    // use AddToVector2List() instead of ToVector2List() as it is more performant
                    aPlayer.Echoes.Clear ();
                    trackRecord.echoes.AddToVector2List (aPlayer.Echoes);
                    //aPlayer.SetPosition(TracklinkTrackingService.GetScreenPositionFromRelativePosition(trackRecord.relPos));
                    aPlayer.SetPosition(TrackingAdapter.GetScreenPositionFromRelativePosition(trackRecord.relPos.x, trackRecord.relPos.y));
                    return;
                }
            }

            if(_addUnknownPlayerOnUpdate)
            {
                AddPlayer(trackRecord);
            }
        }
	
        public virtual void RemovePlayer (int trackID)
        {
            foreach (ATrackingEntity player in _playerList.ToArray()) 
            {
                if(player.TrackID.Equals(trackID))
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
