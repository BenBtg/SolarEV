using System.Xml.Serialization;

namespace SolarEV.Models
{
    // using System.Xml.Serialization;
    // XmlSerializer serializer = new XmlSerializer(typeof(Solar));
    // using (StringReader reader = new StringReader(xml))
    // {
    //    var test = (Solar)serializer.Deserialize(reader);
    // }

    [XmlRoot(ElementName = "generating")]
    public class Generating
    {

        [XmlAttribute(AttributeName = "units")]
        public string Units { get; set; }

        [XmlText]
        public double Text { get; set; }
    }

    [XmlRoot(ElementName = "exporting")]
    public class Exporting
    {

        [XmlAttribute(AttributeName = "units")]
        public string Units { get; set; }

        [XmlText]
        public double Text { get; set; }
    }

    [XmlRoot(ElementName = "current")]
    public class Current
    {

        [XmlElement(ElementName = "generating")]
        public Generating Generating { get; set; }

        [XmlElement(ElementName = "exporting")]
        public Exporting Exporting { get; set; }
    }

    [XmlRoot(ElementName = "generated")]
    public class Generated
    {

        [XmlAttribute(AttributeName = "units")]
        public string Units { get; set; }

        [XmlText]
        public double Text { get; set; }
    }

    [XmlRoot(ElementName = "exported")]
    public class Exported
    {

        [XmlAttribute(AttributeName = "units")]
        public string Units { get; set; }

        [XmlText]
        public double Text { get; set; }
    }

    [XmlRoot(ElementName = "day")]
    public class Day
    {

        [XmlElement(ElementName = "generated")]
        public Generated Generated { get; set; }

        [XmlElement(ElementName = "exported")]
        public Exported Exported { get; set; }
    }

    [XmlRoot(ElementName = "solar")]
    public class Solar
    {

        [XmlElement(ElementName = "timestamp")]
        public int Timestamp { get; set; }

        [XmlElement(ElementName = "current")]
        public Current Current { get; set; }

        [XmlElement(ElementName = "day")]
        public Day Day { get; set; }

        [XmlAttribute(AttributeName = "id")]
        public string Id { get; set; }

        [XmlText]
        public string Text { get; set; }
    }


}