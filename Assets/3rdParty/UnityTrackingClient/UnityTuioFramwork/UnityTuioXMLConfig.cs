using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

namespace UnityTuio 
{
	[XmlRoot("UnityTUIOConfig")]
	public class UnityTuioXMLConfig
	{
		[XmlArray("ConfigNodes"),XmlArrayItem("ConfigNode")]
		public ConfigNode[] ConfigNodes;

		public void Save(string path)
		{
			XmlSerializer serializer = new XmlSerializer(typeof(UnityTuioXMLConfig));
			using(FileStream stream = new FileStream(path, FileMode.Create))
			{
				serializer.Serialize(stream, this);
			}
		}
		
		public static UnityTuioXMLConfig Load(string path)
		{
			XmlSerializer serializer = new XmlSerializer(typeof(UnityTuioXMLConfig));
			using(FileStream stream = new FileStream(path, FileMode.Open))
			{
				return serializer.Deserialize(stream) as UnityTuioXMLConfig;
			}
		}
		
		//Loads the xml directly from the given string. Useful in combination with www.text.
		public static UnityTuioXMLConfig LoadFromText(string text) 
		{
			XmlSerializer serializer = new XmlSerializer(typeof(UnityTuioXMLConfig));
			return serializer.Deserialize(new StringReader(text)) as UnityTuioXMLConfig;
		}

		public class ConfigNode
		{ 
			[XmlAttribute("name")]
			public string Name;
			
			public string Value;
		}
	}
}