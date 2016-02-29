using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Toggle))]
public class ToggleUIColor : MonoBehaviour 
{
	public MaskableGraphic[] itsUIElements;
	public float itsAnimationSpeed = 12.0f;
	public Color itsActiveColor;
	public Color itsInactiveColor;
	
	private Toggle itsToggle;
	private bool itsIsActive;
	private float itsLerpProgress = 0;

	void Start()
	{
		if(itsToggle == null)
		{
			itsToggle = this.GetComponent<Toggle>();
		}

		itsToggle.onValueChanged.AddListener(OnValueChanged);
		itsIsActive = itsToggle.isOn;
	}

	void Update()
	{
		Color aToColor = (itsIsActive) ? itsActiveColor : itsInactiveColor ;

		if(itsLerpProgress < 1.0f)
		{
			itsLerpProgress += Time.deltaTime * itsAnimationSpeed;
			foreach(MaskableGraphic aMaskableGraphic in itsUIElements)
			{
				aMaskableGraphic.color = Color.Lerp(aMaskableGraphic.color, aToColor, itsLerpProgress);
			}
		}

	}

	#region private methods
	private void OnValueChanged(bool theValue)
	{
		itsIsActive = theValue;
		itsLerpProgress = 0;
	}
	#endregion
}
