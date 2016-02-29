namespace UnityPharus
{
	public interface IScreenManager
	{
		void SetScreenResolution(int theWidth, int theHeight);

		void InjectReferenceInPharusManager();
	}
}