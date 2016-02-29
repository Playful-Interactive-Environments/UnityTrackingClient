using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ColorAnimationHelper : MonoBehaviour 
{
	public Color itsTheColor;
	public Text itsText;
	public Image itsImage;

	void Update()
	{
		if(itsText != null) itsText.color = itsTheColor;
		if(itsImage != null) itsImage.color = itsTheColor;
	}
}
