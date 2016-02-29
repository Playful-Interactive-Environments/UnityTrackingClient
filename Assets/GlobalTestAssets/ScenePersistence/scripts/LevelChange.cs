using UnityEngine;
using System.Collections;

public class LevelChange : MonoBehaviour
{
	public void ChangeLevel(string theLevelName)
	{
		Application.LoadLevel(theLevelName);
	}
}
