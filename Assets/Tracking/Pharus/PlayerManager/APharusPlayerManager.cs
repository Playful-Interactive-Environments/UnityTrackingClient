using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityPharus;

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
	public virtual void AddPlayer (PharusTransmission.TrackRecord theTrackRecord)
	{
		Vector2 position = UnityPharusManager.PharusRelPosToScreenCoord(theTrackRecord.relPos);
		ATrackingEntity aPlayer = (GameObject.Instantiate(_playerPrefab, new Vector3(position.x,position.y,0), Quaternion.identity) as GameObject).GetComponent<ATrackingEntity>();
		aPlayer.TrackID = theTrackRecord.trackID;
		aPlayer.AbsolutePosition = new Vector2(theTrackRecord.currentPos.x,theTrackRecord.currentPos.y);
		aPlayer.NextExpectedAbsolutePosition = new Vector2(theTrackRecord.expectPos.x,theTrackRecord.expectPos.y);
		aPlayer.RelativePosition = new Vector2(theTrackRecord.relPos.x,theTrackRecord.relPos.y);
		aPlayer.Orientation = new Vector2(theTrackRecord.orientation.x,theTrackRecord.orientation.y);
		aPlayer.Speed = theTrackRecord.speed;

		aPlayer.gameObject.name = string.Format("PharusPlayer_{0}", aPlayer.TrackID);

		_playerList.Add(aPlayer);
	}
	
	public virtual void UpdatePlayerPosition (PharusTransmission.TrackRecord theTrackRecord)
	{
		foreach (ATrackingEntity player in _playerList) 
		{
			if(player.TrackID == theTrackRecord.trackID)
			{
				Vector2 position = UnityPharusManager.PharusRelPosToScreenCoord(theTrackRecord.relPos);
				player.AbsolutePosition = new Vector2(theTrackRecord.currentPos.x,theTrackRecord.currentPos.y);
				player.NextExpectedAbsolutePosition = new Vector2(theTrackRecord.expectPos.x,theTrackRecord.expectPos.y);
				player.RelativePosition = new Vector2(theTrackRecord.relPos.x,theTrackRecord.relPos.y);
				player.Orientation = new Vector2(theTrackRecord.orientation.x,theTrackRecord.orientation.y);
				player.Speed = theTrackRecord.speed;
				player.SetPosition(position);
				return;
			}
		}

		if(_addUnknownPlayerOnUpdate)
		{
			AddPlayer(theTrackRecord);
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
