using EAAPI = EA;
using System.Collections.Generic;
using System.Linq;
using System;

namespace GenerateLifeCycleDiagrams.Utility
{
	/// <summary>
	/// Description of EAWrapper.
	/// </summary>
	public class EAWrapper
	{
		private EAAPI.Repository eaRepo;
		private Utility.Diagram diagramConfig;
		
		const int EA_ELEMENT_NOT_SEQUENCED = 999999;
		
		public EAWrapper(Utility.Diagram diagramConfig)
		{
			this.diagramConfig = diagramConfig;
		}
		
		
		List<EA.Diagram.DiagramRow> lifeCycleDiagrams;
		public int GetLifeCycleDiagramID(int parentDiagramID)
		{
			// Load the list if not loaded already
			if (null == lifeCycleDiagrams) {
				String returnXML = eaRepo.SQLQuery(
					                   String.Format("SELECT Diagram_ID, ParentID, Name, Stereotype FROM t_diagram WHERE Stereotype = '{0}'",
						                   diagramConfig.Configuration.LifeCycleAlias));
				lifeCycleDiagrams = EA.Diagram.DeSerialiseEASQLQueryResult.DeSerialiseAsDiagramRow(returnXML);
			}
			
			
			String lifeCycleDiagramID;			
			lifeCycleDiagramID = (from f in lifeCycleDiagrams
			                      where f.ParentID == parentDiagramID.ToString()
			                      select f.Diagram_ID).First();
			
			return SafeToInt(lifeCycleDiagramID, 0);
		}
		
		public int CreateDiagram(int parentDiagramID)
		{
			EAAPI.Diagram lifeCycleDiagram;
			EAAPI.Element parentElement;
			String lifeCycleAlias;
			
			parentElement = eaRepo.GetElementByID(parentDiagramID);
			lifeCycleAlias = FormatLifeCycleDiagramName(parentElement.Name, diagramConfig.Configuration.LifeCycleAlias);
			
			
			// Create the child diagram that will contain the Life Cycle information.
			lifeCycleDiagram = (EAAPI.Diagram)parentElement.Diagrams.AddNew(
				lifeCycleAlias,
				diagramConfig.Configuration.EAStereotype);
			lifeCycleDiagram.Stereotype = diagramConfig.Configuration.LifeCycleAlias;
			
			// Attach it to the parent
			lifeCycleDiagram.ParentID = parentDiagramID;
						
			parentElement.Update();
			parentElement.Refresh();
						
			lifeCycleDiagram.Update();
			
			return lifeCycleDiagram.DiagramID;
		}
		
		public void DrawDiagram(int parentDiagramID, int lifeCycleDiagramID)
		{
			EAAPI.Element parentElement = eaRepo.GetElementByID(parentDiagramID);
			EAAPI.Diagram lifeCycleDiagram = eaRepo.GetDiagramByID(lifeCycleDiagramID);
			EAAPI.Package parentPackage = eaRepo.GetPackageByID(lifeCycleDiagram.PackageID);
			
			// convert to Dictionary so it is easier to lookup.
			Dictionary<String, EAAPI.Element> elements = ConvertEAElementsToDictionary(parentPackage.Elements);
			
			AddOrUpdateElements(parentPackage, parentElement, lifeCycleDiagram, ref elements);
			
			AddOrUpdateElementsOnDiagram(parentElement, lifeCycleDiagram, ref elements);
			
			FixDiagramObjectsSequence(lifeCycleDiagram, elements);
			
			lifeCycleDiagram.Update();
		}
		

		private void AddOrUpdateElements(EAAPI.Package parentPackage, EAAPI.Element parentElement, EAAPI.Diagram lifeCycleDiagram, ref Dictionary<String, EAAPI.Element> elements)
		{
			// Get the Tagged Values from the parent diagram.
			EAAPI.Collection taggedValues = parentElement.TaggedValues;
			
			foreach (Utility.Element element in diagramConfig.Elements.Element.AsQueryable()) {
				String elementName = element.Name;
				String elementType = element.Type;
				String elementText = element.Text;
				
				if (!String.IsNullOrEmpty(elementText) && true == elementText.Contains("@TAGVALUE")) {
					
					// Get the value from the TaggedValues
					String[] keyValuePair = elementText.Split(':');
					EAAPI.TaggedValue taggedValue = (EAAPI.TaggedValue)taggedValues.GetByName(keyValuePair[1]);
					
					elementText = taggedValue.Value;				
					
					// If there is an associated Map then use that.
					if (!String.IsNullOrEmpty(element.TextMap)) {
						// Get the text map to use from the Configuration
						Utility.Map mapValues = (from t in diagramConfig.Configuration.Map
						                         where t.Name.Contains(element.TextMap)
						                         select t).First();
						
						// Map the key to the value and that is the string to add on the diagram.
						elementText = (from t in mapValues.MapValue
						               where t.Key.Contains(elementText)
						               select t.Value).First(); 
						
					}
				}				
				
				// Add the elements to the Repository.
				// Update only the text if it is already in the Repository.
				AddOrUpdateElement(
					parentPackage,
					ref elements,
					FormatDiagramElementAlias(lifeCycleDiagram.DiagramID, elementName),
					elementType,
					elementText,
					element.BackColor);
			}
			
			parentPackage.Elements.Refresh();
		}
		
			
		private void AddOrUpdateElement(
			EAAPI.Package parentPackage,
			ref Dictionary<String, EAAPI.Element> elements,
			String alias,
			String type,
			String value,
			String backgroundColor)
		{
			// Try to update the existing element
			if (true == elements.ContainsKey(alias)) {
				if (null != value) {
					elements[alias].Notes = value;
				} else {
					elements[alias].Notes = "";
				}
				if (!String.IsNullOrEmpty(backgroundColor)) {
					elements[alias].Properties.Item("BorderStyle").Value = "Solid";
				}
				elements[alias].Update();
			} else {
			
				// If the element is not in the list, then add it.
				EAAPI.Element elementNew = (EAAPI.Element)parentPackage.Elements.AddNew("", type);
				elementNew.Alias = alias;
				if (null != value) {
					elementNew.Notes = value;				
				}
			
				if (!String.IsNullOrEmpty(backgroundColor)) {
					elementNew.Properties.Item("BorderStyle").Value = "Solid";
				}
				elementNew.Update();
			
				elements.Add(alias, elementNew);
			}
		}
		
		private void AddOrUpdateElementsOnDiagram(EAAPI.Element parentElement, EAAPI.Diagram lifeCycleDiagram, ref Dictionary<String, EAAPI.Element> elements)
		{
			String alias;
			int elementID;

			// Get all of the diagram elements on the existing diagram into a list that is easier to search.
			Dictionary<String, int> preExistingObjects = new Dictionary<String, int>();
			foreach (EAAPI.DiagramObject digObject in lifeCycleDiagram.DiagramObjects) {
				EAAPI.Element packageElement = eaRepo.GetElementByID(digObject.ElementID);
								
				alias = packageElement.Alias;
				elementID = packageElement.ElementID;
				
				if (!String.IsNullOrEmpty(alias)) {
					preExistingObjects.Add(packageElement.Alias, packageElement.ElementID);	
				}				
			}			
			
			int topDef;
			int leftDef;
			int heightDef;
			int widthDef;
			String elementName;
			int top;
			int left;
			int right;
			int bottom;
			
			int row;
			int column;
		
			topDef = SafeToInt(diagramConfig.Configuration.Position.DefaultTop, 0);
			leftDef = SafeToInt(diagramConfig.Configuration.Position.DefalutLeft, 0);
			heightDef = SafeToInt(diagramConfig.Configuration.Position.DefaultHeight, 0);
			widthDef = SafeToInt(diagramConfig.Configuration.Position.DefaultWidth, 0);
			
			String backColor;
			
			// Get the parent diagram. This contains the Tagged Values.
			EAAPI.Collection taggedValues = parentElement.TaggedValues;
			
			// Ad or update the diagram elements.
			foreach (Utility.Element element in diagramConfig.Elements.Element.AsQueryable()) {
				elementName = FormatDiagramElementAlias(lifeCycleDiagram.DiagramID, element.Name);
				elementID = elements[elementName].ElementID;
			
				row = SafeToInt(element.Cell.Row, 0);
				column = SafeToInt(element.Cell.Column, 0);
				
				top = topDef + ((row - 1) * heightDef) + SafeToInt(element.PositionOffset.Top, 0);
				left = leftDef + ((column - 1) * widthDef) + SafeToInt(element.PositionOffset.Left, 0);
				right = widthDef + left + SafeToInt(element.PositionOffset.Width, 0);
				bottom = heightDef + top + SafeToInt(element.PositionOffset.Height, 0);
				
				backColor = element.BackColor;
				
				if (!String.IsNullOrEmpty(backColor) && true == backColor.Contains("@TAGVALUE")) {					
					// Get the value from the TaggedValues
					String[] keyValuePair = backColor.Split(':');
					EAAPI.TaggedValue taggedValue = (EAAPI.TaggedValue)taggedValues.GetByName(keyValuePair[1]);
					
					// Get the color map to use
					Utility.TaggedValueColorMap mapValues = (from t in diagramConfig.Configuration.TaggedValueColorMap
					                                         where t.Name.Contains(element.ColorMap)
					                                         select t).First();
					
					backColor = MapColor(mapValues.Color, taggedValue.Value);
				}
				
				if (!preExistingObjects.ContainsKey(elements[elementName].Alias)) {
					AddElementToDiagram(
						lifeCycleDiagram, elementID, element.Type, left, right, top, bottom,
						MapFont(diagramConfig.Configuration.FontMap.Font, element.Font),
						MapColor(diagramConfig.Configuration.ColorMap.Color, backColor));
				} else {
					UpdateElementOnDiagram(
						lifeCycleDiagram, preExistingObjects[elementName], MapColor(diagramConfig.Configuration.ColorMap.Color, backColor));
				}
			}			
		}

		private void FixDiagramObjectsSequence(EAAPI.Diagram lifeCycleDiagram, Dictionary<String, EAAPI.Element> elements)
		{
			// Get a list of the diagram elements that are actually on the diagram in the repository.
			String resultString = eaRepo.SQLQuery(
				                      "Select DO.Diagram_ID, DO.Object_ID, DO.Sequence, O.Alias from t_diagramobjects DO, t_object O where DO.Object_ID = o.Object_ID and DO.Diagram_ID = "
				                      + lifeCycleDiagram.DiagramID.ToString());
			List<Utility.EA.Sequence.SequenceRow> sequenceRows = Utility.EA.Sequence.DeSerialiseEASQLQueryResult.DeSerialiseAsDiagramRow(resultString);
			EAAPI.DiagramObject diagramObject = null;
			Utility.EA.Sequence.SequenceRow sequenceRow;
			int nAliasIndex;			
			int maxSequence = diagramConfig.Elements.Element.Count();
			int oldSequence;
			
			// Build a list of aliases
			List<String> elementAliases = new List<string>();
			foreach (Element elementName in diagramConfig.Elements.Element) {
				elementAliases.Add(FormatDiagramElementAlias(lifeCycleDiagram.DiagramID, elementName.Name));
			}
			
			for (int nIndex = 0; nIndex < sequenceRows.Count; nIndex++) {
				sequenceRow = sequenceRows[nIndex];
				
				// Is it one of our aliases?
				if (elementAliases.Contains(sequenceRow.Alias)) {
					
					nAliasIndex = elementAliases.IndexOf(sequenceRow.Alias);
					diagramObject = (EAAPI.DiagramObject)lifeCycleDiagram.GetDiagramObjectByID(SafeToInt(sequenceRow.Object_ID, 0), null);
					diagramObject.Sequence = maxSequence - nAliasIndex; // EA goes backwards putting the last element to draw first.
					diagramObject.Update();
				} else {
					oldSequence = diagramObject.Sequence;
					if (EA_ELEMENT_NOT_SEQUENCED != oldSequence && oldSequence <= maxSequence) {						
						diagramObject = (EAAPI.DiagramObject)lifeCycleDiagram.GetDiagramObjectByID(SafeToInt(sequenceRow.Object_ID, 0), null);
						diagramObject.Sequence = oldSequence + (maxSequence - oldSequence) + 1; // EA goes backwards putting the last element to draw first.
						diagramObject.Update();					
					}
				}
			}
		}
		
		private void UpdateElementOnDiagram(
			EAAPI.Diagram lifeCycleDiagram, int elementID, String backgroundColor)
		{
			// Parse out the old BCol and add the new.
			if (!String.IsNullOrEmpty(backgroundColor)) {
				EAAPI.DiagramObject obj = null;
				for (short nIndex = 0; nIndex < lifeCycleDiagram.DiagramObjects.Count; nIndex++) {
					obj = (EAAPI.DiagramObject)lifeCycleDiagram.DiagramObjects.GetAt(nIndex);
					if (obj.ElementID == elementID)
						break;					
				}
				
				if (null != obj) {
			
					String styleString = obj.Style.ToString();
					String[] styleList = styleString.Split(';');
					for (int nIndex = 0; nIndex < styleList.Count(); nIndex++) {
						if (true == styleList[nIndex].Contains("BCol")) {
							styleList[nIndex] = backgroundColor;
						}
					}
					styleString = String.Join(";", styleList);
					obj.Style = styleString;
				
					obj.Update();
				}
			}			
		}
		
		private void AddElementToDiagram(
			EAAPI.Diagram lifeCycleDiagram, int elementID, String type,
			int left, int right, int top, int bottom, String font, String backgroundColor)
		{
			String size = String.Format("l={0};r={1};t={2};b={3};", left, right, top, bottom);
			EAAPI.DiagramObject obj = (EAAPI.DiagramObject)lifeCycleDiagram.DiagramObjects.AddNew(size, type);
			
			obj.ElementID = elementID;
			if (!String.IsNullOrEmpty(font)) {
				obj.Style = obj.Style + ";" + font;
			}
			if (!String.IsNullOrEmpty(backgroundColor)) {
				obj.Style = obj.Style + ";" + backgroundColor;
			}
			
			obj.Update();
		}
		
		private Dictionary<String, EAAPI.Element> ConvertEAElementsToDictionary(EAAPI.Collection eaElements)
		{
			Dictionary<String, EAAPI.Element> dicElements = new Dictionary<string, EAAPI.Element>();
			
			for (short nIndex = 0; nIndex < eaElements.Count; nIndex++) {
				EAAPI.Element element = (EAAPI.Element)eaElements.GetAt(nIndex);
				
				if (false == dicElements.ContainsKey(element.Alias)) {
					dicElements.Add(element.Alias, element);
				}
					
			}
			
			return dicElements;
		}
		
		public List<EA.Stereotype.StereotypeRow> GetDiagramsByStereoType(String stereotype)
		{
			List<EA.Stereotype.StereotypeRow> resultIDs;
			String appComp = eaRepo.SQLQuery(
				                 String.Format("Select Object_ID, Name FROM t_object WHERE t_object.Stereotype = '{0}'",
					                 diagramConfig.Configuration.EAStereotype));
			resultIDs = EA.Stereotype.DeSerialiseEAData.DeSerialiseAsDiagramRow(appComp);
			
			return resultIDs;
		}
		
		public EAAPI.Element GetElementByID(int elementID)
		{
			return eaRepo.GetElementByID(elementID);
		}
		
		public EAAPI.Diagram GetLifeCycleDiagram(EAAPI.Element parent, String diagramStereotype)
		{			
			if (null != parent) {
				if (null != parent.Diagrams) {
					for (short nIndex = 0; nIndex < parent.Diagrams.Count; nIndex++) {
						EAAPI.Diagram diagram = (EAAPI.Diagram)parent.Diagrams.GetAt(nIndex);
					
						if (0 == diagramStereotype.CompareTo(diagram.Stereotype)) {
							return diagram;
						}
					}
				}
			}
			return null;
		}
		
		public void OpenEARepository()
		{
			eaRepo = new EAAPI.RepositoryClass();
			eaRepo.OpenFile(diagramConfig.Configuration.EARepository);
		}

		public void CloseEARepository(bool saveDiagrams)
		{
			if (true == saveDiagrams)
				eaRepo.SaveAllDiagrams();
			eaRepo.CloseFile();

		}
		
		public Utility.Diagram LoadLifeCycleDiagramMetaData()
		{
			diagramConfig = Utility.DeSerialiseDiagram.DeSerialise();			
			return diagramConfig;
		}
		
		
		
		private String MapColor(List<Utility.Color> colors, String eaColor)
		{		
			Utility.Color color;
			String colorText = "";
			
			try {
				color = (from f in colors.AsQueryable()
				         where 0 == f.Name.CompareTo(eaColor)
				         select f).First();
				colorText = color.Value;
			} catch (Exception) {
			}
			
			return colorText;
		}
		private String MapFont(List<Utility.Font> fonts, String eaFont)
		{		
			Utility.Font font;
			String fontText = "";
			
			try {
				font = (from f in diagramConfig.Configuration.FontMap.Font.AsQueryable()
				        where 0 == f.Name.CompareTo(eaFont)
				        select f).First();
				fontText = font.Text;
			} catch (Exception) {
					
			}
			
			return fontText;
		}
				
		private String FormatLifeCycleDiagramName(String parentDiagreamName, String sterotype)
		{
			return String.Format("{0} {1}", parentDiagreamName, sterotype);
		}
		
		private String FormatDiagramElementAlias(int diagramID, String name)
		{
			return String.Format("{0}_{1}", diagramID, name);
		}
		
		private int SafeToInt(String intAsString, int safeDefault)
		{
			int returnValue;
			if (false == int.TryParse(intAsString, out returnValue))
				returnValue = safeDefault;
			return returnValue;			
		}
	}
}
