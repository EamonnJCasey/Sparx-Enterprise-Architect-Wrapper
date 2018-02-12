using System;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.IO;

namespace GenerateLifeCycleDiagrams.Utility.EA.Sequence
{
	public static class DeSerialiseEASQLQueryResult
	{
		public static List<SequenceRow> DeSerialiseAsDiagramRow(String DeSerialiseEASQLQueryResult) 
		{
			XmlSerializer serializer = new XmlSerializer(typeof(EADATA));
			TextReader reader = new StringReader(DeSerialiseEASQLQueryResult);
			EADATA result = (EADATA)serializer.Deserialize(reader);
			
			return result.Dataset_0.Data.Row;
		}
	}
	
	[XmlRoot(ElementName="Row")]
	public class SequenceRow {
		[XmlElement(ElementName="Diagram_ID")]
		public string Diagram_ID { get; set; }
		[XmlElement(ElementName="Object_ID")]
		public string Object_ID { get; set; }
		[XmlElement(ElementName="Sequence")]
		public string Sequence { get; set; }
		[XmlElement(ElementName="Alias")]
		public string Alias { get; set; }
	}

	[XmlRoot(ElementName="Data")]
	public class Data {
		[XmlElement(ElementName="Row")]
		public List<SequenceRow> Row { get; set; }
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
