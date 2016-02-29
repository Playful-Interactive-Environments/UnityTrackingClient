using System.Collections;
using System.IO;

namespace TrackingEvaluation
{
	public static class FileWriter
	{
		private static string _pathToCurrentFile = string.Empty;

		#region public methods
		/// <summary>
		/// Sets the path to the destination file. Must be called before WriteLine.
		/// </summary>
		/// <param name="thePathToTheDestinationFile">The path to the destination file.</param>
		public static void SetPath(string thePathToTheDestinationFile)
		{
			_pathToCurrentFile = thePathToTheDestinationFile;
		}

		/// <summary>
		/// Writes (appends) multiple lines into the specified file.
		/// </summary>
		/// <param name="theLines">The lines as string array.</param>
		public static void WriteLines(string[] theLines)
		{
			if(_pathToCurrentFile == string.Empty)
			{
				UnityEngine.Debug.LogWarning("Couldn't write to file as no path is specified!");
				return;
			}

			using (StreamWriter aFile = new System.IO.StreamWriter(_pathToCurrentFile, true))
			{
				foreach (string aLine in theLines)
				{
					aFile.WriteLine(aLine);
				}
			}
		}

		/// <summary>
		/// Writes (appends) a single line into the specified file.
		/// </summary>
		/// <param name="theLine">The line as string.</param>
		public static void WriteLine(string theLine)
		{
			if(_pathToCurrentFile == string.Empty)
			{
				UnityEngine.Debug.LogWarning("Couldn't write to file as no path is specified!");
				return;
			}

			using (StreamWriter aFile = new System.IO.StreamWriter(_pathToCurrentFile, true))
			{
				aFile.WriteLine(theLine);
			}
		}
		#endregion
	}
}
