using System;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Linq;
using System.IO;


namespace GenerateLifeCycleDiagrams.Utility
{
	public static class DeSerialiseDiagram
	{
		public static Diagram DeSerialise()
		{
			XmlSerializer serializer = new XmlSerializer(typeof(Diagram));
			TextReader reader = File.OpenText("EADiagramMetaData.xml");
			Diagram result = (Diagram)serializer.Deserialize(reader);
			
			return result;
		}
	}
	
	[XmlRoot(ElementName = "Position")]
	public class Position
	{
		[XmlAttribute(AttributeName = "DefaultTop")]
		public string DefaultTop { get; set; }
		[XmlAttribute(AttributeName = "DefalutLeft")]
		public string DefalutLeft { get; set; }
		[XmlAttribute(AttributeName = "DefaultWidth")]
		public string DefaultWidth { get; set; }
		[XmlAttribute(AttributeName = "DefaultHeight")]
		public string DefaultHeight { get; set; }
	}

	[XmlRoot(ElementName = "Font")]
	public class Font
	{
		[XmlAttribute(AttributeName = "Name")]
		public string Name { get; set; }
		[XmlText]
		public string Text { get; set; }
	}

	[XmlRoot(ElementName = "FontMap")]
	public class FontMap
	{
		[XmlElement(ElementName = "Font")]
		public List<Font> Font { get; set; }
	}

	[XmlRoot(ElementName = "Color")]
	public class Color
	{
		[XmlAttribute(AttributeName = "Name")]
		public string Name { get; set; }
		[XmlAttribute(AttributeName = "Value")]
		public string Value { get; set; }
	}

	[XmlRoot(ElementName = "ColorMap")]
	public class ColorMap
	{
		[XmlElement(ElementName = "Color")]
		public List<Color> Color { get; set; }
	}
	[XmlRoot(ElementName = "TaggedValueColorMap")]
	public class TaggedValueColorMap
	{
		[XmlAttribute(AttributeName = "Name")]
		public string Name { get; set; }
		[XmlElement(ElementName = "Color")]
		public List<Color> Color { get; set; }
	}

	[XmlRoot(ElementName = "MapValue")]
	public class MapValue
	{
		[XmlAttribute(AttributeName = "Key")]
		public string Key { get; set; }
		[XmlAttribute(AttributeName = "Value")]
		public string Value { get; set; }
	}
		
	[XmlRoot(ElementName = "Map")]
	public class Map
	{
		[XmlAttribute(AttributeName = "Name")]
		public string Name { get; set; }
		[XmlElement(ElementName = "MapValue")]
		public List<MapValue> MapValue { get; set; }
	}

	[XmlRoot(ElementName = "Configuration")]
	public class Configuration
	{
		[XmlElement(ElementName = "EARepository")]
		public string EARepository { get; set; }
		[XmlElement(ElementName = "EAStereotype")]
		public string EAStereotype { get; set; }
		[XmlElement(ElementName = "LifeCycleAlias")]
		public string LifeCycleAlias { get; set; }
		[XmlElement(ElementName = "Position")]
		public Position Position { get; set; }
		[XmlElement(ElementName = "FontMap")]
		public FontMap FontMap { get; set; }
		[XmlElement(ElementName = "TaggedValueColorMap")]
		public List<TaggedValueColorMap> TaggedValueColorMap { get; set; }
		[XmlElement(ElementName = "ColorMap")]
		public ColorMap ColorMap { get; set; }
		[XmlElement(ElementName = "Map")]
		public List<Map> Map { get; set; }
	}

	[XmlRoot(ElementName = "Cell")]
	public class Cell
	{
		[XmlAttribute(AttributeName = "Row")]
		public string Row { get; set; }
		[XmlAttribute(AttributeName = "Column")]
		public string Column { get; set; }
	}

	[XmlRoot(ElementName = "PositionOffset")]
	public class PositionOffset
	{
		[XmlAttribute(AttributeName = "Top")]
		public string Top { get; set; }
		[XmlAttribute(AttributeName = "Left")]
		public string Left { get; set; }
		[XmlAttribute(AttributeName = "Width")]
		public string Width { get; set; }
		[XmlAttribute(AttributeName = "Height")]
		public string Height { get; set; }
	}

	[XmlRoot(ElementName = "Element")]
	public class Element
	{
		[XmlElement(ElementName = "Cell")]
		public Cell Cell { get; set; }
		[XmlElement(ElementName = "PositionOffset")]
		public PositionOffset PositionOffset { get; set; }
		[XmlAttribute(AttributeName = "Type")]
		public string Type { get; set; }
		[XmlAttribute(AttributeName = "Name")]
		public string Name { get; set; }
		[XmlAttribute(AttributeName = "Text")]
		public string Text { get; set; }
		[XmlAttribute(AttributeName = "BackColor")]
		public string BackColor { get; set; }
		[XmlAttribute(AttributeName = "ColorMap")]
		public string ColorMap { get; set; }
		[XmlAttribute(AttributeName = "Font")]
		public string Font { get; set; }
		[XmlAttribute(AttributeName = "TextMap")]
		public string TextMap { get; set; }
	}
	
	[XmlRoot(ElementName = "Elements")]
	public class Elements
	{
		[XmlElement(ElementName = "Element")]
		public List<Element> Element { get; set; }
	}

	[XmlRoot(ElementName = "Diagram")]
	public class Diagram
	{
		[XmlElement(ElementName = "Configuration")]
		public Configuration Configuration { get; set; }
		[XmlElement(ElementName = "Elements")]
		public Elements Elements { get; set; }
	}

}