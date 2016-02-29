using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

namespace UnityPharus
{
	[XmlRoot("UnityPharusConfig")]
	public class UnityPharusXMLConfig
	{
		[XmlArray("ConfigNodes"),XmlArrayItem("ConfigNode")]
		public ConfigNode[] ConfigNodes;

		public void Save(string path)
		{
			XmlSerializer serializer = new XmlSerializer(typeof(UnityPharusXMLConfig));
			using(FileStream stream = new FileStream(path, FileMode.Create))
			{
				serializer.Serialize(stream, this);
			}
		}
		
		public static UnityPharusXMLConfig Load(string path)
		{
			XmlSerializer serializer = new XmlSerializer(typeof(UnityPharusXMLConfig));
			using(FileStream stream = new FileStream(path, FileMode.Open))
			{
				return serializer.Deserialize(stream) as UnityPharusXMLConfig;
			}
		}
		
		//Loads the xml directly from the given string. Useful in combination with www.text.
		public static UnityPharusXMLConfig LoadFromText(string text) 
		{
			XmlSerializer serializer = new XmlSerializer(typeof(UnityPharusXMLConfig));
			return serializer.Deserialize(new StringReader(text)) as UnityPharusXMLConfig;
		}

		public class ConfigNode
		{ 
			[XmlAttribute("name")]
			public string Name;
			
			public string Value;
		}
	}
}