using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace TrackingEvaluation
{
	public class EvaluationEvent
	{
		private string _name;
		private Dictionary<string, string> _data;
		
		public string Name
		{
			get { return _name; }
		}
		public Dictionary<string, string> Data
		{
			get { return _data; }
		}
		
		public EvaluationEvent(string theEventName, params KeyValuePair<string, string>[] theEventData)
		{
			_name = theEventName;
			_data = new Dictionary<string, string>();
			foreach (KeyValuePair<string, string> aDataSet in theEventData) 
			{
				_data.Add(aDataSet.Key, aDataSet.Value);	
			}
		}
	}
}