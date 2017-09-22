using EA;
using System;
using System.Collections.Generic;
using System.Linq;
using Nav.Tools.Common.Client.EAClient.EAQueryResults.EAGUID;

namespace Nav.Tools.Common.Client.EAClient
{
	/// <summary>
	/// Description of EAClient.
	/// </summary>
	public class EAClient
	{
		EA.Repository eaRepository;
		
		public EA.Repository EnterpriseArchitectRaw { get { return eaRepository; } }

		public void OpenRepository(String repositoryName)
		{
			eaRepository = new Repository();
			eaRepository.OpenFile(repositoryName);
		}
		
		public void OpenRepositoryWithPassword(String repositoryName)
		{
			eaRepository = new Repository();
			eaRepository.OpenFile2(repositoryName, "name", "password");
		}
		
		public void CloseRepository()
		{
			eaRepository.CloseFile();
		}

		public EA.Package GetPackageByGUID(String packageGUID)
		{
			EA.Package package;

			package = eaRepository.GetPackageByGuid(packageGUID);
            
			return package;
		}

		public EA.Element GetElementByGUID(String elementGUID)
		{
			EA.Element eaElement;

			eaElement = eaRepository.GetElementByGuid(elementGUID);

			return eaElement;
		}
        
		/*
         * The parent package has a collection of packages. This rutine either creates a new package under the parent
         * or returns the package that already exists under the parent package. The incoming collection will also
         * be updated.
         */
		public Package FindOrCreatePackage(Package parent, String packageToFind, String packageType)
		{
			Package returnPackage = null;

			foreach (Package pack in parent.Packages) {
				if (0 == pack.Name.ToLower().CompareTo(packageToFind.ToLower())) {
					returnPackage = pack;
					break;
				}
			}
			if (null == returnPackage) {
				returnPackage = (Package)parent.Packages.AddNew(packageToFind, packageType);
				returnPackage.Update();
			}

			return returnPackage;
		}
		
		
		public List<EAQueryResults.EAConnector.Row> GetConnectorsByGuid(String fromGuid) {
			String strResult =
				eaRepository.SQLQuery(
					String.Format(
						@"SELECT oStart.Object_ID AS Start_Object_ID, oStart.ea_guid AS Start_ea_guid, oStart.Name AS Start_Name, oEnd.ea_guid AS End_ea_guid, oEnd.Name AS End_Name, 
							conn.Direction AS Direction, conn.Connector_Type AS Connector_Type, conn.StereoType AS Connector_StereoType
  							FROM t_object oStart, t_object oEnd, t_connector conn 
    					   WHERE oStart.ea_guid = '{0}' AND conn.Start_Object_ID = oStart.Object_ID AND conn.End_Object_ID = oEnd.Object_ID", fromGuid));
			List<EAQueryResults.EAConnector.Row> rows = EAQueryResults.EAConnector.EAConnector.Deserialise(strResult);
			return rows;
		}
        
		public List<EA.Package> GetPackagessInPackage(int packageID)
		{
			List<EA.Package> elements = new List<EA.Package>();
			String strResult =
				eaRepository.SQLQuery(
					String.Format(
						"SELECT ea_guid FROM t_object t_package  WHERE Parent_ID = {0}", packageID));
			List<Row> rows = EAGuid.Deserialise(strResult);
			EA.Package rootPackage = eaRepository.GetPackageByID(packageID);

			foreach (Row row in rows) {
				EA.Package eaElement = eaRepository.GetPackageByGuid(row.Ea_guid);
				elements.Add(eaElement);
			}

			return elements;
		}
		
     	public Dictionary<String,EA.Package> GetSubPackages(int packageID)
		{
			Dictionary<String,EA.Package> existingPackages = new Dictionary<String,EA.Package>();
			List<EA.Package> temp;
		
			temp = GetPackagessInPackage(packageID);
			foreach (EA.Package pack in temp) {
				existingPackages.Add(pack.Name, pack);
			}
			
			return existingPackages;
		}
		public List<EA.Element> GetElementsInPackage(int packageID, string stereoType)
		{
			List<EA.Element> elements = new List<EA.Element>();
			String strResult =
				eaRepository.SQLQuery(
					String.Format(
						"SELECT ea_guid FROM t_object WHERE Package_ID = {0} AND StereoType = UPPER('{1}')", packageID, SafeStereoType(stereoType)));
			List<Row> rows = EAGuid.Deserialise(strResult);
			EA.Package rootPackage = eaRepository.GetPackageByID(packageID);

			foreach (Row row in rows) {
				EA.Element eaElement = eaRepository.GetElementByGuid(row.Ea_guid);
				elements.Add(eaElement);
			}

			return elements;
		}

		public List<Row> GetGuidsByAlias(String aliasSearch)
		{
			List<EA.Element> elements = new List<EA.Element>();
			String strResult =
				eaRepository.SQLQuery(
					String.Format(
						"SELECT ea_guid, alias FROM t_object WHERE Alias LIKE '{0}'", aliasSearch));

			List<Row> rows = EAGuid.Deserialise(strResult);

			return rows;
		}
		
                
		public EA.Element GetElementInPackageByName(int packageID, string elementName, string stereoType)
		{
			EA.Element element = null;
			String strResult =
				eaRepository.SQLQuery(
					String.Format(
						"SELECT ea_guid FROM t_object WHERE Package_ID = {0} AND UPPER(Name) = UPPER('{1}') AND StereoType = UPPER('{2}')", packageID, elementName, SafeStereoType(stereoType)));
			List<Row> rows = EAGuid.Deserialise(strResult);

			if (rows.Count > 0) {
				element = eaRepository.GetElementByGuid(rows[0].Ea_guid);
			}

			return element;
		}
		
		   
		public EA.Element GetElementByAlias(string alias)
		{
			EA.Element element = null;
			String strResult =
				eaRepository.SQLQuery(
					String.Format(
						"SELECT ea_guid FROM t_object WHERE UPPER(Alias) = UPPER('{0}')", alias));
			List<Row> rows = EAGuid.Deserialise(strResult);

			if (rows.Count > 0) {
				element = eaRepository.GetElementByGuid(rows[0].Ea_guid);
			}

			return element;
		}


		public EA.Element FindOrCreateElement(Package rootPackage, string elementName, string stereoType)
		{

			EA.Element eaElement = GetElementByName(rootPackage.PackageID, elementName, stereoType);

			if (null == eaElement) {
				eaElement = (EA.Element)rootPackage.Elements.AddNew(elementName, stereoType);
				eaElement.Update();
			}
			return eaElement;
		}
		
		public String SafeStereoType(String stereoType)
		{
			String safeStereoType = stereoType;
			if (safeStereoType.Contains(":")) {
				String[] bits = safeStereoType.Split(':');

				safeStereoType = bits[bits.Count() - 1];
			}

			return safeStereoType;
		}
                
		public EA.Connector FindorCreateConnector(string fromElementGUID, string toElementGUID, string connectorDirection, string connectorStereoType)
		{
			EA.Element fromElement = eaRepository.GetElementByGuid(fromElementGUID);
			EA.Element toElement = eaRepository.GetElementByGuid(toElementGUID);
			EA.Connector connectorFound = null;

			foreach (EA.Connector connector in fromElement.Connectors) {
				if (connector.SupplierID == toElement.ElementID) {
					connectorFound = connector;
				}
			}
			if (null == connectorFound) {
				connectorFound = (EA.Connector)fromElement.Connectors.AddNew("", connectorStereoType);
				connectorFound.SupplierID = toElement.ElementID;
				connectorFound.Direction = connectorDirection;
				connectorFound.Update();
			}
			return connectorFound;
		}
                
		public EA.Element GetElementByName(int packageID, string elementName, string stereoType)
		{
			EA.Element element = null;
			String strResult =
				eaRepository.SQLQuery(
					String.Format(
						"SELECT ea_guid FROM t_object WHERE Package_ID = {0} AND UPPER(Name) = UPPER('{1}') AND UPPER(StereoType) = UPPER('{2}')", packageID, elementName, SafeStereoType(stereoType)));
			List<Row> rows = EAGuid.Deserialise(strResult);

			if (rows.Count > 0) {
				element = eaRepository.GetElementByGuid(rows[0].Ea_guid);
			}

			return element;
		}
	}
}
