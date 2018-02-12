using System;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.IO;

namespace GenerateLifeCycleDiagrams.Utility.EA.Diagram
{
	
	public static class DeSerialiseEASQLQueryResult
	{
		public static List<DiagramRow> DeSerialiseAsDiagramRow(String DeSerialiseEASQLQueryResult) 
		{
			XmlSerializer serializer = new XmlSerializer(typeof(EADATA));
			TextReader reader = new StringReader(DeSerialiseEASQLQueryResult);
			EADATA result = (EADATA)serializer.Deserialize(reader);
			
			return result.Dataset_0.Data.Row;
		}
	}

	[XmlRoot(ElementName="Row")]
	public class DiagramRow {
		[XmlElement(ElementName="Diagram_ID")]
		public string Diagram_ID { get; set; }
		[XmlElement(ElementName="ParentID")]
		public string ParentID { get; set; }
		[XmlElement(ElementName="Name")]
		public string Name { get; set; }
		[XmlElement(ElementName="Stereotype")]
		public string Stereotype { get; set; }
	}

	[XmlRoot(ElementName="Data")]
	public class Data {
		[XmlElement(ElementName="Row")]
		public List<DiagramRow> Row { get; set; }
	}

	[XmlRoot(ElementName="Dataset_0")]
	public class Dataset_0 {
		[XmlElement(ElementName="Data")]
		public Data Data { get; set; }
	}

	[XmlRoot(ElementName="EADATA")]
	public class EADATA {
		[XmlElement(ElementName="Dataset_0")]
		public Dataset_0 Dataset_0 { get; set; }
		[XmlAttribute(AttributeName="version")]
		public string Version { get; set; }
		[XmlAttribute(AttributeName="exporter")]
		public string Exporter { get; set; }
	}
}
