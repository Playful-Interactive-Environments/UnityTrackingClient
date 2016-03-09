using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityTuio;
using UnityTracking;

abstract public class ATuioPlayerManager : MonoBehaviour
{
	protected List<ATrackingEntity> _playerList;
	[SerializeField] private GameObject _playerPrefab;
	[SerializeField] private bool _addUnknownPlayerOnUpdate = true;
	[SerializeField] private bool _subscribeTuioCursors = true;
	[SerializeField] private bool _subscribeTuioObjects = false;
	[SerializeField] private bool _subscribeTuioBlobs = false;

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
		if(UnityTuioManager.Instance != null)
		{
			if(UnityTuioManager.Instance.EventProcessor == null)
			{
				UnityTuioManager.Instance.OnTrackingInitialized += SubscribeTrackingEvents;
			}
			else
			{
				SubscribeTrackingEvents(this, null);
			}
		}
	}
	
	void OnDisable()
	{
		if(UnityTuioManager.Instance != null)
		{
			if(_subscribeTuioCursors)
			{
				UnityTuioManager.Instance.EventProcessor.CursorAdded -= OnCursorAdded;
				UnityTuioManager.Instance.EventProcessor.CursorUpdated -= OnCursorUpdated;
				UnityTuioManager.Instance.EventProcessor.CursorRemoved -= OnCursorRemoved;
			}
			if(_subscribeTuioObjects)
			{
				UnityTuioManager.Instance.EventProcessor.ObjectAdded -= OnObjectAdded;
				UnityTuioManager.Instance.EventProcessor.ObjectUpdated -= OnObjectUpdated;
				UnityTuioManager.Instance.EventProcessor.ObjectRemoved -= OnObjectRemoved;
			}
			if(_subscribeTuioBlobs)
			{
				UnityTuioManager.Instance.EventProcessor.BlobAdded -= OnBlobAdded;
				UnityTuioManager.Instance.EventProcessor.BlobUpdated -= OnBlobUpdated;
				UnityTuioManager.Instance.EventProcessor.BlobRemoved -= OnBlobRemoved;
			}
		}
	}

	#region private methods
	private void SubscribeTrackingEvents(object theSender, System.EventArgs e)
	{
		if(_subscribeTuioCursors)
		{
			UnityTuioManager.Instance.EventProcessor.CursorAdded += OnCursorAdded;
			UnityTuioManager.Instance.EventProcessor.CursorUpdated += OnCursorUpdated;
			UnityTuioManager.Instance.EventProcessor.CursorRemoved += OnCursorRemoved;
		}
		if(_subscribeTuioObjects)
		{
			UnityTuioManager.Instance.EventProcessor.ObjectAdded += OnObjectAdded;
			UnityTuioManager.Instance.EventProcessor.ObjectUpdated += OnObjectUpdated;
			UnityTuioManager.Instance.EventProcessor.ObjectRemoved += OnObjectRemoved;
		}
		if(_subscribeTuioBlobs)
		{
			UnityTuioManager.Instance.EventProcessor.BlobAdded += OnBlobAdded;
			UnityTuioManager.Instance.EventProcessor.BlobUpdated += OnBlobUpdated;
			UnityTuioManager.Instance.EventProcessor.BlobRemoved += OnBlobRemoved;
		}
	}
	#endregion
	
	#region tuio event handlers
	void OnCursorAdded (object sender, UnityTuioEventProcessor.TuioEventCursorArgs e)
	{
		AddPlayer(e.tuioCursor);
	}
	void OnObjectAdded (object sender, UnityTuioEventProcessor.TuioEventObjectArgs e)
	{
		AddPlayer(e.tuioObject);
	}
	void OnBlobAdded (object sender, UnityTuioEventProcessor.TuioEventBlobArgs e)
	{
		AddPlayer(e.tuioBlob);
	}
	
	void OnCursorUpdated (object sender, UnityTuioEventProcessor.TuioEventCursorArgs e)
	{
		UpdatePlayerPosition(e.tuioCursor);
	}
	void OnObjectUpdated (object sender, UnityTuioEventProcessor.TuioEventObjectArgs e)
	{
		UpdatePlayerPosition(e.tuioObject);
	}
	void OnBlobUpdated (object sender, UnityTuioEventProcessor.TuioEventBlobArgs e)
	{
		UpdatePlayerPosition(e.tuioBlob);
	}
	
	void OnCursorRemoved (object sender, UnityTuioEventProcessor.TuioEventCursorArgs e)
	{
		RemovePlayer(e.tuioCursor.SessionID);
	}
	void OnObjectRemoved (object sender, UnityTuioEventProcessor.TuioEventObjectArgs e)
	{
		RemovePlayer(e.tuioObject.SessionID);
	}
	void OnBlobRemoved (object sender, UnityTuioEventProcessor.TuioEventBlobArgs e)
	{
		RemovePlayer(e.tuioBlob.SessionID);
	}
	#endregion

	#region player management
	public virtual void AddPlayer (TUIO.TuioContainer theTuioContainer)
	{
//		Vector2 position = UnityTuioManager.GetScreenPositionFromRelativePosition (theTuioContainer.Position);
		Vector2 position = TrackingAdapter.GetScreenPositionFromRelativePosition(theTuioContainer.Position.X, theTuioContainer.Position.Y);

		ATrackingEntity aPlayer = (GameObject.Instantiate(_playerPrefab, new Vector3(position.x,position.y,0), Quaternion.identity) as GameObject).GetComponent<ATrackingEntity>();
		aPlayer.TrackID = theTuioContainer.SessionID;
		aPlayer.RelativePosition = new Vector2(theTuioContainer.Position.X, theTuioContainer.Position.Y);

		aPlayer.gameObject.name = string.Format("TuioPlayer_{0}", aPlayer.TrackID);

		_playerList.Add(aPlayer);
	}

	public virtual void UpdatePlayerPosition (TUIO.TuioContainer theTuioContainer)
	{
		foreach (ATrackingEntity player in _playerList) 
		{
			if(player.TrackID.Equals(theTuioContainer.SessionID))
			{
//				Vector2 position = UnityTuioManager.GetScreenPositionFromRelativePosition (theTuioContainer.Position);
				Vector2 position = TrackingAdapter.GetScreenPositionFromRelativePosition(theTuioContainer.Position.X, theTuioContainer.Position.Y);
				player.SetPosition(position);
				player.RelativePosition = new Vector2(theTuioContainer.Position.X, theTuioContainer.Position.Y);
				return;
			}
		}
		
		if(_addUnknownPlayerOnUpdate)
		{
			AddPlayer(theTuioContainer);
		}
	}

	public virtual void RemovePlayer (long sessionID)
	{
		foreach (ATrackingEntity player in _playerList.ToArray()) 
		{
			if(player.TrackID.Equals(sessionID))
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
