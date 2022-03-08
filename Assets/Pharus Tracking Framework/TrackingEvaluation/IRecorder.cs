using System.Data;

namespace Assets.Pharus_Tracking_Framework.TrackingEvaluation
{
	public interface IRecorder
	{
		void StartRecording();
		void StopRecording();
		DataRow[] SelectFromTable(string theSelectSQLStatement);
		DataRow[] SelectFromTable();
		void ClearTable();
	}
}

