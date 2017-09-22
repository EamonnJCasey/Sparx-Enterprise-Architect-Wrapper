using System;
using System.IO;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Nav.Tools.Common.Client.EAClient.EAQueryResults.EAGUID
{
	/// <summary>
	/// Description of EAGuid.
	/// </summary>
	public class EAGuid
	{
        static public List<Row> Deserialise(String eaSQLResult)
        {
            EADATA eaData;
            List<Row> rows = new List<Row>();
            XmlSerializer serializer = new XmlSerializer(typeof(EADATA));

            try
            {
                using (TextReader reader = new StringReader(eaSQLResult))
                {
                    eaData = (EADATA)serializer.Deserialize(reader);
                    if (eaData.Dataset_0.Data.Row.Count > 0)
                    {
                        rows = eaData.Dataset_0.Data.Row;
                    }
                }
            }
            catch (Exception)
            {
            }

            return rows;
        }
    }

    [XmlRoot(ElementName = "Row")]
    public class Row
    {
        [XmlElement(ElementName = "EA_GUID")]
        public string Ea_guid { get; set; }
        [XmlElement(ElementName = "ALIAS")]
        public string Alias { get; set; }
    }

    [XmlRoot(ElementName = "Data")]
    public class Data
    {
        [XmlElement(ElementName = "Row")]
        public List<Row> Row { get; set; }
    }

    [XmlRoot(ElementName = "Dataset_0")]
    public class Dataset_0
    {
        [XmlElement(ElementName = "Data")]
        public Data Data { get; set; }
    }

    [XmlRoot(ElementName = "EADATA")]
    public class EADATA
    {
        [XmlElement(ElementName = "Dataset_0")]
        public Dataset_0 Dataset_0 { get; set; }
        [XmlAttribute(AttributeName = "version")]
        public string Version { get; set; }
        [XmlAttribute(AttributeName = "exporter")]
        public string Exporter { get; set; }
    }
}
