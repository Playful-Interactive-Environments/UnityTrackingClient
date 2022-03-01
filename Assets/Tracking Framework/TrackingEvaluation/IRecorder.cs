using System.Data;

namespace Assets.Tracking_Framework.TrackingEvaluation
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

