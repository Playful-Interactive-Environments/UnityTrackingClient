using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[RequireComponent(typeof(Collider2D))]
public class TriggerScript : MonoBehaviour 
{
	public event Action<GameObject> OnTriggerEnterEvent;
	public event Action<GameObject> OnTriggerExitEvent;

	public bool itsTrackOtherTriggers = false;
	public string[] itsTagsToTrack;
	public LayerMask itsLayersToTrack;
	public Collider2D[] itsIgnoreColliders;

	private List<GameObject> itsOverlappingGameObjects;

	void Awake()
	{
		itsOverlappingGameObjects = new List<GameObject>();
		if(!this.GetComponent<Collider2D>().isTrigger)
		{
			this.GetComponent<Collider2D>().isTrigger = true;
		}
	}

	#region OnTriggerEnter
	private void OnTriggerEnter2D(Collider2D otherCol)
	{
		// is this collider a trigger?
		if(!itsTrackOtherTriggers && otherCol.isTrigger) return;

		// is this collider in our ignore list?
		foreach(Collider2D aIgnoreCollider in itsIgnoreColliders)
		{
			if(aIgnoreCollider == otherCol)	return;
		}

		bool aTrackIt = false;
		GameObject aGameObject = otherCol.gameObject;

		// check tag
		foreach (string aTag in itsTagsToTrack) 
		{
			aTrackIt = (otherCol.CompareTag(aTag));
			if(aTrackIt) break;
		}

		// check layer if not already interested
		if(!aTrackIt)
		{
			aTrackIt = (itsLayersToTrack == (itsLayersToTrack | (1 << aGameObject.layer)));
		}

		// not interested or already tracked
		if(!aTrackIt || itsOverlappingGameObjects.Contains(aGameObject)) return;

		// track it!
		itsOverlappingGameObjects.Add(aGameObject);
		if(OnTriggerEnterEvent != null)
		{
			OnTriggerEnterEvent.Invoke(aGameObject);
		}
	}
	#endregion

	#region OnTriggerExit
	private void OnTriggerExit2D(Collider2D otherCol)
	{
		GameObject aGameObject = otherCol.gameObject;

		// if not registered 
		if(!itsOverlappingGameObjects.Contains(aGameObject)) return;
		
		// track it!
		itsOverlappingGameObjects.Remove(aGameObject);
		if(OnTriggerExitEvent != null)
		{
			OnTriggerExitEvent.Invoke(aGameObject);
		}
	}
	#endregion

	#region public methods
	public List<GameObject> GetOverlappingGameObjects()
	{
		return itsOverlappingGameObjects;
	}
	#endregion
}
