namespace UnityTuio
{
	public interface IScreenManager
	{
		void SetScreenResolution(int theWidth, int theHeight);

		void InjectReferenceInTuioManager();
	}
}