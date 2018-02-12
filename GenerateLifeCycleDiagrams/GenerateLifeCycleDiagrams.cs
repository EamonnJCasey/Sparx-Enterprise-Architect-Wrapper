// 1. Get the Application Components
// SELECT Object_ID, Name FROM t_object WHERE Stereotype = 'NAV_ArchiMate_ApplicationComponent'

// 2. Get the Life Cycle Diagrams
// SELECT Diagram_ID, ParentID, Name, Stereotype FROM t_diagram WHERE Stereotype = "Life Cycle"

// 3. Get the elements on the diagram, include BColor and Font.
// SELECT DO.Diagram_ID, DO.Object_ID, DO.ObjectStyle, DO.Sequence, O.Alias FROM t_diagramobjects DO, t_object O WHERE DO.Object_ID = o.Object_ID and DO.Diagram_ID =
using System;
using System.Collections.Generic;
using EAAPI = EA;

namespace GenerateLifeCycleDiagrams
{
	/// <summary>
	/// Description of GenerateLifeCycleDiagrams.
	/// </summary>
	public class GenerateLifeCycleDiagrams
	{
		Utility.Diagram diagramConfig;
		public GenerateLifeCycleDiagrams()
		{
		}
		
		public void Execute()
		{
			Utility.EAWrapper eaWrapper = new Utility.EAWrapper(diagramConfig);
			List<Utility.EA.Stereotype.StereotypeRow> sterotypeList;
			
			int lifeCycleDiagramID = 0;
			bool createDiagram;
			
			Log("Generating Life Cycle Diagrams for NAV ArchiMate Application Components");
			Log("");
			
			Log("  Reading configuration");
			diagramConfig = eaWrapper.LoadLifeCycleDiagramMetaData();
			
			Log("  Opening Repository: " + diagramConfig.Configuration.EARepository);			
			eaWrapper.OpenEARepository();
			
			
			Log("Locating NAV ArchiMate Application Components.");
			sterotypeList = eaWrapper.GetDiagramsByStereoType(diagramConfig.Configuration.EAStereotype);
			
			// For each ArchiMate in the list, greate or update the Life Cycle diagram
			foreach (Utility.EA.Stereotype.StereotypeRow parentDiagram in sterotypeList) {
				System.Console.WriteLine("\tProcessing: {0} - {1}", parentDiagram.Object_ID, parentDiagram.Name);

				// Flag to mark if the diagram should be created or not
				createDiagram = true;

				try {
					lifeCycleDiagramID = eaWrapper.GetLifeCycleDiagramID(parentDiagram.Object_ID);
					createDiagram = false;
				} catch (Exception) {
				}

				if (true == createDiagram) {
					Log(String.Format("  Creating Life Cycle Diagram for: {0}", parentDiagram.Name));
					lifeCycleDiagramID = eaWrapper.CreateDiagram(parentDiagram.Object_ID);
				} else {
					Log(String.Format("  Updating Life Cycle Diagram for: {0}", parentDiagram.Name));
					
				}
				eaWrapper.DrawDiagram(parentDiagram.Object_ID, lifeCycleDiagramID);
			}

			
			Log("  Saving all diagrams and closing Repository.");
			eaWrapper.CloseEARepository(true);
			
			Log("");
			Log("Done.");
		}
		


		private void Log(String message)
		{
			System.Console.WriteLine(message);
		}
		

	}
}
