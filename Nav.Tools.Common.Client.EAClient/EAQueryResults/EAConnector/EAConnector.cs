using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;

namespace Nav.Tools.Common.Client.EAClient.EAQueryResults.EAConnector
{
	static public class EAConnector
	{
		static public List<Row> Deserialise(String eaSQLResult)
		{
			EADATA eaData;
			List<Row> rows = new List<Row>();
			XmlSerializer serializer = new XmlSerializer(typeof(EADATA));

			try {
				using (TextReader reader = new StringReader(eaSQLResult)) {
					eaData = (EADATA)serializer.Deserialize(reader);
					if (eaData.Dataset_0.Data.Row.Count > 0) {
						rows = eaData.Dataset_0.Data.Row;
					}
				}
			} catch (Exception) {
			}

			return rows;
		}
	}

	[XmlRoot(ElementName = "Row")]
	public class Row
	{
		[XmlElement(ElementName = "Start_Object_ID")]
		public string Start_Object_ID { get; set; }
		[XmlElement(ElementName = "Start_ea_guid")]
		public string Start_ea_guid { get; set; }
		[XmlElement(ElementName = "Start_Name")]
		public string Start_Name { get; set; }
		[XmlElement(ElementName = "End_ea_guid")]
		public string End_ea_guid { get; set; }
		[XmlElement(ElementName = "End_Name")]
		public string End_Name { get; set; }
		[XmlElement(ElementName = "Direction")]
		public string Direction { get; set; }
		[XmlElement(ElementName = "Connector_Type")]
		public string Connector_Type { get; set; }
		[XmlElement(ElementName = "Connector_StereoType")]
		public string Connector_StereoType { get; set; }
		
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
