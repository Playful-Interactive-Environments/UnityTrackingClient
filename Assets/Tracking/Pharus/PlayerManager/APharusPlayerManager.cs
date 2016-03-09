using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityPharus;
using UnityTracking;

abstract public class APharusPlayerManager : MonoBehaviour
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
		if(UnityPharusManager.Instance != null)
		{
			if(UnityPharusManager.Instance.EventProcessor == null)
			{
				UnityPharusManager.Instance.OnTrackingInitialized += SubscribeTrackingEvents;
			}
			else
			{
				SubscribeTrackingEvents(this, null);
			}
		}
	}
	
	void OnDisable()
	{
		if(UnityPharusManager.Instance != null)
		{
			UnityPharusManager.Instance.EventProcessor.TrackAdded -= OnTrackAdded;
			UnityPharusManager.Instance.EventProcessor.TrackUpdated -= OnTrackUpdated;
			UnityPharusManager.Instance.EventProcessor.TrackRemoved -= OnTrackRemoved;
			UnityPharusManager.Instance.OnTrackingInitialized -= SubscribeTrackingEvents;
		}
	}

	#region private methods
	private void SubscribeTrackingEvents (object theSender, System.EventArgs e)
	{
		UnityPharusManager.Instance.EventProcessor.TrackAdded += OnTrackAdded;
		UnityPharusManager.Instance.EventProcessor.TrackUpdated += OnTrackUpdated;
		UnityPharusManager.Instance.EventProcessor.TrackRemoved += OnTrackRemoved;
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
	public virtual void AddPlayer (PharusTransmission.TrackRecord trackRecord)
	{
//		Vector2 position = UnityPharusManager.GetScreenPositionFromRelativePosition(trackRecord.relPos);
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
	
	public virtual void UpdatePlayerPosition (PharusTransmission.TrackRecord trackRecord)
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
				aPlayer.SetPosition(UnityPharusManager.GetScreenPositionFromRelativePosition(trackRecord.relPos));
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
