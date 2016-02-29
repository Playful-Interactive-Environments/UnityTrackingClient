using UnityEngine;

public static class Utilities
{
	#region Math helper
	public static Vector3 RandomCircle(Vector3 theCenter, float theRadius)
	{ 
		float anAngle = Random.value * 360; 
		Vector3 aPosOnCircle = new Vector3(
			theCenter.x + theRadius * Mathf.Sin(anAngle * Mathf.Deg2Rad),
			theCenter.y + theRadius * Mathf.Cos(anAngle * Mathf.Deg2Rad),
			theCenter.z
			);
		return aPosOnCircle; 
	}
	#endregion

	#region extension methods for: Color
	public static Color SetAlpha(this Color theColor, float theAlpha)
	{
		theColor.a = theAlpha;
		return theColor;
	}
	#endregion
}