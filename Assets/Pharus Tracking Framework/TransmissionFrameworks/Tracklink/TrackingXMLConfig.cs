using System.IO;
using System.Xml.Serialization;

namespace Assets.Pharus_Tracking_Framework.TransmissionFrameworks.Tracklink
{
    [XmlRoot("TrackingXMLConfig")]
    public class TrackingXMLConfig
    {
        [XmlArray("ConfigNodes"), XmlArrayItem("ConfigNode")]
        public ConfigNode[] ConfigNodes;

        public void Save(string path)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(TrackingXMLConfig));
            using (FileStream stream = new FileStream(path, FileMode.Create))
            {
                serializer.Serialize(stream, this);
            }
        }

        public static TrackingXMLConfig Load(string path)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(TrackingXMLConfig));
            using (FileStream stream = new FileStream(path, FileMode.Open))
            {
                return serializer.Deserialize(stream) as TrackingXMLConfig;
            }
        }

        //Loads the xml directly from the given string. Useful in combination with www.text.
        public static TrackingXMLConfig LoadFromText(string text)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(TrackingXMLConfig));
            return serializer.Deserialize(new StringReader(text)) as TrackingXMLConfig;
        }

        public class ConfigNode
        {
            [XmlAttribute("name")]
            public string Name;

            public string Value;
        }
    }
}
