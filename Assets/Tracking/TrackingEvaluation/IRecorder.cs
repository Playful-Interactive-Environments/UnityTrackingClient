using System.Data;

namespace TrackingEvaluation
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

