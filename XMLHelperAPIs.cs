using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace DepIdentifier
{
    internal class XMLHelperAPIs
    {

        #region Identification and Adding to XML

        public static List<string> FindIDLDependenciesAndAddToXml(string filePath, string folder)
        {
            List<string> parsedIdlFilePaths = FileDepIdentifier.FindIDLDependencies(filePath, folder);
            string dependencyFiles = string.Empty;
            if (parsedIdlFilePaths != null && parsedIdlFilePaths.Count > 0)
            {
                UpdateTheXmlAttributeDependenciesPathAsync(filePath, parsedIdlFilePaths, folder);
                UpdateTheXmlAttributeReferencesPathAsync(filePath, parsedIdlFilePaths);
            }
            else
            {
                UpdateTheXmlAttributeDependenciesPathAsync(filePath, new List<string> { "No Dependencies" }, folder);
            }
            return parsedIdlFilePaths; 
        }

        public static List<string> FindDependenciesInADOtHCppAndRCFileAndAddtoXml(string filePath, string folder, string filesListXMLPath)
        {
            List<string> resolvedList = new List<string>();
            try
            {
                List<string> dependenciesList = FileDepIdentifier.FindDependenciesInADOtHCppAndRCFile(filePath);
                if (dependenciesList != null && dependenciesList.Count > 0)
                {
                    XmlDocument xmlDocument = new XmlDocument();
                    xmlDocument.Load(DepIdentifierUtils.m_FilesListXMLPath);
                    string projectName = XMLHelperAPIs.GetAttributeOfFilePathFromXML(xmlDocument, "Project", filePath);
                    string additionalIncludeDirectories = XMLHelperAPIs.GetAttributeOfFilePathFromXML(xmlDocument, "AdditionalIncludeDirectories", projectName);

                    resolvedList = DepIdentifierUtils.ResolveFromLocalDirectoryOrPatcher(filePath, dependenciesList, fromPatcher: true, additionalIncludeDirectories: additionalIncludeDirectories);

                    resolvedList = DepIdentifierUtils.RemoveTheMIDLGeneratedFilesFromTheList(resolvedList);

                    UpdateTheXmlAttributeReferencesPathAsync(filePath, resolvedList);

                    UpdateTheXmlAttributeDependenciesPathAsync(filePath, resolvedList, folder, filesListXMLPath);
                }
                else
                {
                    UpdateTheXmlAttributeDependenciesPathAsync(filePath, new List<string> { "no dependencies"}, folder, filesListXMLPath);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to FindCppDependencies with exception: " + ex.Message);
            }
            return resolvedList;
        }

        public static List<string> FindVcxprojDependenciesAndAddToXml(string filePath, string folder, string filesListXMLPath)
        {
            List<string> resolvedList = new List<string>();
            try
            {
                List<string> dependenciesList = FileDepIdentifier.FindDependenciesInVcxprojFiles(filePath);
                
                if (dependenciesList != null && dependenciesList.Count > 0)
                {
                    List<string> propsDependencies = FileDepIdentifier.FindPropsFileDependenciesInVcxprojFiles(filePath);

                    string additionalIncludeDirs = string.Empty;
                    List<string> additionalIncludeDirsList = new List<string>();
                    propsDependencies = DepIdentifierUtils.ResolveFromLocalDirectoryOrPatcher(projectFilePath: filePath, propsDependencies, false);
                    foreach (string propFileDependency in propsDependencies)
                    {
                        additionalIncludeDirsList.AddRange(FileDepIdentifier.FindAdditionalIncludeDirectorisInAPropFile(propFileDependency, folder, filesListXMLPath));
                    }
                    //props file

                    additionalIncludeDirsList.AddRange(FileDepIdentifier.FindAdditionalIncludeDirectoriesOfVCXproj(filePath));

                    additionalIncludeDirs = string.Join(";", DepIdentifierUtils.ResolveAdditionalDirectoriesInList(additionalIncludeDirsList, filePath));

                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.Load(DepIdentifierUtils.m_FilesListXMLPath);
                    UpdateTheXmlAttribute(xmlDoc, folder.Replace("//", "_") + "/filepath", "Name", filePath, "AdditionalIncludeDirectories", additionalIncludeDirs);
                    XMLHelperAPIs.SaveXmlToFile(xmlDoc, DepIdentifierUtils.m_FilesListXMLPath);

                    resolvedList = DepIdentifierUtils.ResolveFromLocalDirectoryOrPatcher(filePath, dependenciesList);

                    UpdateTheXmlAttributeDependenciesPathAsync(filePath, resolvedList, folder, filesListXMLPath);
                    UpdateProjectNameForTheFilesUnderVCXProjAsync(resolvedList, "Project", filePath);
                    UpdateTheXmlAttributeReferencesPathAsync(filePath, resolvedList);
                }
                else
                {
                    UpdateTheXmlAttributeDependenciesPathAsync(filePath, new List<string> { "no dependencies" }, folder, filesListXMLPath);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to FindVcxprojDependenciesAndAddToXml for the '{filePath}' with exception: '{ex.Message}'");
            }
            return resolvedList;
        }

        public static List<string> FindVBPDependenciesAndAddToXml(string filePath, string folder, string filesListXMLPath)
        {
            List<string> dependenciesList = new List<string>();
            try
            {
                dependenciesList = FileDepIdentifier.FindVBPFileDependencies(filePath);
                if (dependenciesList != null && dependenciesList.Count > 0)
                {
                    //resolvedList = ResolveFromLocalDirectoryOrPatcher(filePath, dependenciesList, fromPatcher: true);

                    UpdateTheXmlAttributeDependenciesPathAsync(filePath, dependenciesList, folder, filesListXMLPath);
                    UpdateTheXmlAttributeReferencesPathAsync(filePath, dependenciesList);
                }
                else
                {
                    UpdateTheXmlAttributeDependenciesPathAsync(filePath, new List<string> { "no dependencies" }, folder, filesListXMLPath);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to FindVBPDependenciesAndAddToXml for the '{filePath}' with exception: {ex.Message}");
            }
            return dependenciesList;
        }

        public static List<string> FindLstFileDependenciesAndAddToXml(string filePath, string folder, string filesListXMLPath)
        {
            List<string> resolvedList = new List<string>();
            try
            {
                List<string> dependencies = FileDepIdentifier.FindLstDependencies(filePath);

                if (dependencies != null && dependencies.Count > 0)
                {
                    resolvedList = DepIdentifierUtils.ResolveFromLocalDirectoryOrPatcher(filePath, dependencies, fromPatcher: true);

                    UpdateTheXmlAttributeDependenciesPathAsync(filePath, resolvedList, folder, filesListXMLPath);
                    UpdateTheXmlAttributeReferencesPathAsync(filePath, resolvedList);
                }
                else
                {
                    UpdateTheXmlAttributeDependenciesPathAsync(filePath, new List<string> { "no dependencies" }, folder, filesListXMLPath);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to FindDotHDependencies with exception: " + ex.Message);
            }
            return resolvedList;
        }

        public static List<string> FindWixProjDependenicesAndAddToXML(string filePath, string folder, string filesListXMLPath)
        {
            List<string> dependencies = new List<string>();
            try
            {
                dependencies = FileDepIdentifier.FindWixProjDependenices(filePath, folder, filesListXMLPath);

                if (dependencies != null && dependencies.Count > 0)
                {
                    dependencies = DepIdentifierUtils.ResolveFromLocalDirectoryOrPatcher(filePath, dependencies, fromPatcher: true);

                    UpdateTheXmlAttributeDependenciesPathAsync(filePath, dependencies, folder, filesListXMLPath);
                    UpdateTheXmlAttributeReferencesPathAsync(filePath, dependencies);
                }
                else
                {
                    UpdateTheXmlAttributeDependenciesPathAsync(filePath, new List<string> { "no dependencies" }, folder, filesListXMLPath);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to FindWixProjDependenicesAndAddToXML with exception: " + ex.Message);
            }
            return dependencies;
        }

        public static List<string> Find409VCXProjDependendenciesAndAddToXML(string filePath, string folder, string filesListXMLPath)
        {
            List<string> dependencies = new List<string>();
            try
            {
                dependencies = FileDepIdentifier.Find409VCXProjDependendencies(filePath, folder, filesListXMLPath);

                if (dependencies != null && dependencies.Count > 0)
                {
                    dependencies = DepIdentifierUtils.ResolveFromLocalDirectoryOrPatcher(filePath, dependencies, fromPatcher: true);

                    UpdateTheXmlAttributeDependenciesPathAsync(filePath, dependencies, folder, filesListXMLPath);
                    UpdateTheXmlAttributeReferencesPathAsync(filePath, dependencies);
                }
                else
                {
                    UpdateTheXmlAttributeDependenciesPathAsync(filePath, new List<string> { "no dependencies" }, folder, filesListXMLPath);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to Find409VCXProjDependendenciesAndAddToXML with exception: " + ex.Message);
            }
            return dependencies;
        }

        public static List<string> FindCSProjDependenciesAndAddToXml(string filePath, string folder, string filesListXMLPath)
        {
            List<string> resolvedList = new List<string>();
            try
            {
                List<string> dependenciesList = FileDepIdentifier.FindDependenciesInCsprojFiles(filePath);
                if (dependenciesList != null && dependenciesList.Count > 0)
                {
                    resolvedList = DepIdentifierUtils.ResolveFromLocalDirectoryOrPatcher(filePath, dependenciesList, fromPatcher: true);

                    UpdateTheXmlAttributeDependenciesPathAsync(filePath, resolvedList, folder, filesListXMLPath);
                    UpdateTheXmlAttributeReferencesPathAsync(filePath, resolvedList);
                }
                else
                {
                    UpdateTheXmlAttributeDependenciesPathAsync(filePath, new List<string> { "no dependencies" }, folder, filesListXMLPath);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to FindCSProjDependenciesAndAddToXml for the '{filePath}' with exception: {ex.Message}");
            }
            return resolvedList;
        }

        #endregion

        public static string GetAttributeOfFilePathFromXML(XmlDocument xmlDocument, string attributeName, string fileName)
        {
            try
            {
                string currentFilter = DepIdentifierUtils.GetCurrentFilterFromFilePath(fileName).ToLower();
                XmlNode xmlNode = xmlDocument.SelectSingleNode($"//{currentFilter}/filepath[@Name='{fileName.ToLower()}']");

                if (xmlNode != null)
                {
                    XmlElement xmlElement = xmlNode as XmlElement;
                    if (xmlElement != null)
                        return xmlElement.GetAttribute(attributeName);
                }
            }
            catch (Exception ex)
            {
                //
            }
            return string.Empty;
        }

        public static void UpdateTheXmlAttributeDependenciesPathAsync(string filePath, List<string> updatedParsedIdlFilePaths, string folder, string filesListXMLPath = "")
        {
            //Update the XML attribute with IDL path information asynchronously
            if (filesListXMLPath == "")
                filesListXMLPath = DepIdentifierUtils.m_FilesListXMLPath;

            if (System.IO.File.Exists(filesListXMLPath))
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.Load(filesListXMLPath);

                string dependencyFiles = string.Empty;
                if (updatedParsedIdlFilePaths.Count == 0)
                {
                    dependencyFiles = "no dependencies";
                }
                else
                {
                    foreach (var file in updatedParsedIdlFilePaths)
                    {
                        dependencyFiles = dependencyFiles + file + ";";
                    }
                }
                //Utilities.AppendNewAttribute(xmlDoc, m_selectedFilterPath.Replace("\\", "_") + "/FilePath", "IDL", string.Join(";", m_DependencyList));
                UpdateTheXmlAttribute(xmlDoc, DepIdentifierUtils.GetCurrentFilterFromFilePath(filePath) + "/filepath", "Name", filePath, "Dependency", string.Join(";", dependencyFiles));
                XMLHelperAPIs.SaveXmlToFile(xmlDoc, DepIdentifierUtils.m_FilesListXMLPath);
            }
        }
        
        public static void UpdateTheXmlAttributeReferencesPathAsync(string filePath, List<string> updatedParsedIdlFilePaths, string filesListXMLPath = "")
        {
            try
            {
                //Update the XML attribute with IDL path information asynchronously
                if (filesListXMLPath == "")
                    filesListXMLPath = DepIdentifierUtils.m_FilesListXMLPath;

                if (System.IO.File.Exists(filesListXMLPath))
                {
                    var xmlDoc = new XmlDocument();
                    xmlDoc.Load(filesListXMLPath);

                    string dependencyFiles = string.Empty;
                    //for each file Update the filePath as referenced file here..
                    foreach (var file in updatedParsedIdlFilePaths)
                    {
                        string folder = DepIdentifierUtils.GetCurrentFilterFromFilePath(file).ToLower(); ;
                        UpdateTheXmlAttribute(xmlDoc, folder + "/filepath", "Name", file, "Reference", filePath, true);
                    }
                    XMLHelperAPIs.SaveXmlToFile(xmlDoc, DepIdentifierUtils.m_FilesListXMLPath);
                }
            }
            catch (Exception ex)
            {
                DepIdentifierUtils.WriteTextInLog($"Failed to UpdateTheXmlAttributeReferencesPath for the filepath: {filePath} with exception: {ex.Message}");
            }
        }

        public static async Task SaveXmlToFileAsync(XmlDocument xmlDoc, string filePath)
        {
            try
            {
                await AsyncFileLock.LockAsync(filePath);
                xmlDoc.Save(filePath);
                DepIdentifierUtils.WriteTextInLog("XML file updated successfully.");
            }
            catch (Exception ex)
            {
                DepIdentifierUtils.WriteTextInLog("Error saving XML: " + ex.Message);
            }
            finally
            {
                AsyncFileLock.Unlock(filePath);
            }
        }

        public static void SaveXmlToFile(XmlDocument xmlDoc, string filePath)
        {
            try
            {
                xmlDoc.Save(filePath);
                //DepIdentifierUtils.WriteTextInLog("XML file updated successfully.");
            }
            catch (Exception ex)
            {
                DepIdentifierUtils.WriteTextInLog("Error saving XML: " + ex.Message);
            }
        }

        public static void UpdateTheXmlAttribute(XmlDocument xmlDoc, string elementName, string attributeNameToSearch, string attributeValueToSearch, string attributeNameToUpdate, string attributeValueToUpdate, bool apppend = false)
        {
            try
            {
                string searchPath = attributeValueToSearch.Replace(@"\\", @"\");
                // Get the elements with the specified name and attribute value
                //XmlNodeList filterNodes = xmlDoc.DocumentElement.SelectNodes($"//{elementName}[@{attributeNameToSearch}='{searchPath}']");

                XmlNode xmlNode = xmlDoc.SelectSingleNode($"//{elementName}[@{attributeNameToSearch}= '" + attributeValueToSearch.ToLower() + "']");

                if (xmlNode != null)
                {
                    var xmlElement = xmlNode as XmlElement;
                    if (xmlElement != null)
                    {
                        XmlAttribute xmlAttribute = xmlElement.GetAttributeNode(attributeNameToUpdate);
                        if (xmlAttribute == null)
                        {
                            xmlElement.SetAttribute(attributeNameToUpdate, attributeValueToUpdate.ToLower());
                        }
                        else
                        {
                            if(apppend) 
                            { 
                                string existingValue = xmlAttribute.Value;
                                if(!existingValue.Contains(attributeValueToUpdate.ToLower(), StringComparison.OrdinalIgnoreCase))
                                    xmlAttribute.Value = xmlAttribute.Value + ";" + attributeValueToUpdate.ToLower();
                            }
                            else
                                xmlAttribute.Value = attributeValueToUpdate.ToLower();
                        }
                    }
                }
                //foreach (XmlElement element in filterNodes)
                //{
                //    string currentValue = element.GetAttribute(attributeNameToUpdate);
                //    string updatedValue;
                //    if (currentValue != attributeValueToUpdate && currentValue != string.Empty)
                //    {
                //        updatedValue = currentValue + attributeValueToUpdate;
                //    }
                //    else
                //        updatedValue = attributeValueToUpdate;

                //    // Update the attribute value
                //    element.SetAttribute(attributeNameToUpdate, updatedValue);
                //}
            }
            catch (Exception ex)
            {
                //MessageBox.Show("Issue1");
                DepIdentifierUtils.WriteTextInLog("Error updating attribute in XML: " + ex.Message);
            }
        }

        public static void CreateOrUpdateListXml(List<string> stringsList, string filePath, string parentNode, string rootElementName, string currentElementName)
        {
            try
            {
                // Load existing XML document if it exists, otherwise create a new one
                XmlDocument xmlDoc = new XmlDocument();
                if (File.Exists(filePath))
                {
                    xmlDoc.Load(filePath);
                }
                else
                {
                    XmlDeclaration xmlDeclaration = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", null);
                    XmlElement root = xmlDoc.CreateElement(parentNode);
                    xmlDoc.AppendChild(root);
                    xmlDoc.InsertBefore(xmlDeclaration, root);
                }

                // Get or create the parent node
                XmlElement parentNodeElement = xmlDoc.SelectSingleNode($"//{parentNode}") as XmlElement;
                if (parentNodeElement == null)
                {
                    parentNodeElement = xmlDoc.CreateElement(parentNode);
                    xmlDoc.AppendChild(parentNodeElement);
                }

                // Get the root element
                XmlElement rootElement = parentNodeElement.SelectSingleNode($"{rootElementName}") as XmlElement;
                if (rootElement == null)
                {
                    rootElement = xmlDoc.CreateElement(rootElementName);
                    parentNodeElement.AppendChild(rootElement);
                }

                if (currentElementName != "")
                {
                    // Process each string
                    foreach (string str in stringsList)
                    {
                        if(string.Compare("g:\\kroot\\commonroute\\testing\\doc\\new_atp's_testplan.xls", str, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            //
                        }
                        // Check if the element already exists
                        XmlElement existingElement = null;
                        try
                        {
                            existingElement = rootElement.SelectSingleNode($"{currentElementName}[@Name='{str}']") as XmlElement;

                        }
                        catch(Exception)
                        {
                            existingElement = null;
                        }
                        if (existingElement == null)
                        {
                            // Create a new element and add it to the root
                            XmlElement newElement = xmlDoc.CreateElement(currentElementName);
                            newElement.SetAttribute("Name", str);
                            if (File.Exists(str))
                                newElement.SetAttribute("ShortName", Path.GetFileName(str));
                            rootElement.AppendChild(newElement);
                        }
                        else
                        {
                            if (File.Exists(str))
                            {
                                XmlAttribute xmlNode = existingElement.GetAttributeNode("ShortName");
                                if (xmlNode == null)
                                {
                                    existingElement.SetAttribute("ShortName", Path.GetFileName(str));
                                }
                            }
                        }
                        // If the element exists, you can choose to do something here
                    }
                }

                // Save the updated XML
                xmlDoc.Save(filePath);

                DepIdentifierUtils.WriteTextInLog("XML file updated/created successfully.");
            }
            catch (Exception ex)
            {
                DepIdentifierUtils.WriteTextInLog("Error updating/creating XML file: " + ex.Message);
                throw new Exception("Failed to UpdateListInFilesListXml with exception: " + ex.Message);
            }
        }

        public static List<string> GetXmlData(string xml, string node, string attribute)
        {
            List<string> filters = new List<string>();

            XmlDocument xmlDoc = new XmlDocument();
            if (File.Exists(xml))
            {
                xmlDoc.Load(xml);

                XmlNode xmlNode = xmlDoc.SelectSingleNode(node);
                XmlNodeList xmlNodeList = xmlNode.ChildNodes;

                foreach (XmlNode filterNode in xmlNodeList)
                {
                    string filterName = filterNode.Attributes[attribute].InnerXml;
                    filters.Add(filterName);
                }
            }
            else
            {
                throw new Exception("Xml file not found..!");
            }
            return filters;
        }

        public static void UpdateProjectNameForTheFilesUnderVCXProjAsync(List<string> dependenciesList, string attributeValueToSearch, string projectName)
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(DepIdentifierUtils.m_FilesListXMLPath);
            foreach (var filepath in dependenciesList)
            {
                XMLHelperAPIs.UpdateTheXmlAttribute(xmlDoc, "filepath", "Name", filepath, attributeValueToSearch, projectName);
            }
            XMLHelperAPIs.SaveXmlToFile(xmlDoc, DepIdentifierUtils.m_FilesListXMLPath);
        }

        public static string GetDependecyStringFromXML(XmlDocument xmlDoc, string parentElementName, string elementName, string attributeNameToSearch, string attributeValueToSearch)
        {
            try
            {
                // Get the elements with the specified name and attribute valu
                string xPathXpression = $"//{parentElementName}/{elementName}[@{attributeNameToSearch}='{attributeValueToSearch}']";

                XmlNodeList filterNodes = xmlDoc.DocumentElement.SelectNodes(xPathXpression);

                // Check if any matching element is found
                if (filterNodes.Count > 0)
                {
                    // Get the "name" attribute value of the first matching element
                    XmlElement element = (XmlElement)filterNodes[0];
                    string nameValue = element.GetAttribute("Dependency");
                    return nameValue;
                }
                else
                {
                    DepIdentifierUtils.WriteTextInLog("No matching element found.");
                    return null;
                }
            }
            catch (Exception ex)
            {
                DepIdentifierUtils.WriteTextInLog("Error extracting GetDependecyStringFromXML from XML: " + ex.Message);
                return null;
            }
        }


    }
}
