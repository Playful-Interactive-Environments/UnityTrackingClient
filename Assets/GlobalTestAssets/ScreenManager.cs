using UnityEngine;
using System.Collections;
using UnityPharus;
using UnityTuio;

public class ScreenManager : MonoBehaviour, UnityPharus.IScreenManager, UnityTuio.IScreenManager
{
	private static int _screenWidth = 1920;
	private static int _screenHeight = 1080;

	public static int ScreenWidth
	{
		get { return _screenWidth; }
	}
	public static int ScreenHeight
	{
		get { return _screenHeight; }
	}

	#region unity messages
	void Awake()
	{
		InjectReferenceInPharusManager ();
		InjectReferenceInTuioManager ();
	}
	#endregion

	#region public methods
	public void SetScreenResolution(int theScreenWidth, int theScreenHeight)
	{
		SetScreenWidth (theScreenWidth);
		SetScreenHeight (theScreenHeight);
	}

	public void SetScreenWidth(int theScreenWidth)
	{
		_screenWidth = theScreenWidth;
	}

	public void SetScreenHeight(int theScreenHeight)
	{
		_screenHeight = theScreenHeight;
	}

	public void InjectReferenceInPharusManager ()
	{
		if (UnityPharusManager.Instance != null) {
			UnityPharusManager.Instance.SetScreenManager(this);
		}
	}

	public void InjectReferenceInTuioManager ()
	{
		if (UnityTuioManager.Instance != null) {
			UnityTuioManager.Instance.SetScreenManager(this);
		}
	}
	#endregion
}

