using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace DepIdentifier
{
    internal static class XMLHelperAPIs
    {
        private static XmlDocument? cachedXmlDocument;
        private static DateTime lastModifiedTime;

        static XMLHelperAPIs()
        {
            cachedXmlDocument = null;
            lastModifiedTime = DateTime.MinValue;
        }

        public static XmlDocument GetFilesListXmlDocument()
        {
            try
            {
                DateTime currentModifiedTime = File.GetLastWriteTime(ReversePatcher.m_FilesListXMLPath);

                // Check if the XML file has been modified since the last access
                if (cachedXmlDocument == null || currentModifiedTime > lastModifiedTime || ReversePatcher.isXMLSaved == true)
                {
                    // Load the XML document
                    cachedXmlDocument = new XmlDocument();
                    cachedXmlDocument.Load(ReversePatcher.m_FilesListXMLPath);

                    // Update the last modified time
                    lastModifiedTime = currentModifiedTime;
                }
                ReversePatcher.isXMLSaved = false;
            }
            catch(Exception)
            {
                cachedXmlDocument = new XmlDocument();
                cachedXmlDocument.Load(ReversePatcher.m_FilesListXMLPath);
            }
            return cachedXmlDocument;
        }

        #region Identification and Adding to XML

        public static List<string> FindIDLDependenciesAndAddToXml(string filePath, string folder)
        {
            FileDepIdentifier fileDepIdentifier = new FileDepIdentifier();
            List<string> parsedIdlFilePaths = fileDepIdentifier.FindIDLDependencies(filePath, folder);
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
                FileDepIdentifier fileDepIdentifier = new FileDepIdentifier();
                List<string> dependenciesList = fileDepIdentifier.FindDependenciesInADOtHCppAndRCFile(filePath);
                if (dependenciesList != null && dependenciesList.Count > 0)
                {
                    XmlDocument xmlDocument = XMLHelperAPIs.GetFilesListXmlDocument();
                    string projectName = XMLHelperAPIs.GetAttributeOfFilePathFromXML(xmlDocument, "Project", filePath);
                    string additionalIncludeDirectories = "";
                    if (projectName != null)
                    {
                        additionalIncludeDirectories = XMLHelperAPIs.GetAttributeOfFilePathFromXML(xmlDocument, "AdditionalIncludeDirectories", projectName);

                    }
                    resolvedList = DepIdentifierUtils.ResolveFromLocalDirectoryOrPatcher(filePath, dependenciesList, fromPatcher: true, additionalIncludeDirectories: additionalIncludeDirectories);

                    resolvedList = DepIdentifierUtils.RemoveTheMIDLGeneratedFilesFromTheList(resolvedList);

                    UpdateTheXmlAttributeReferencesPathAsync(filePath, resolvedList);

                    UpdateTheXmlAttributeDependenciesPathAsync(filePath, resolvedList, folder);
                }
                else
                {
                    UpdateTheXmlAttributeDependenciesPathAsync(filePath, new List<string> { "no dependencies"}, folder);
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
                FileDepIdentifier fileDepIdentifier = new FileDepIdentifier();
                List<string> dependenciesList = fileDepIdentifier.FindDependenciesInVcxprojFiles(filePath);
                
                if (dependenciesList != null && dependenciesList.Count > 0)
                {
                    List<string> propsDependencies = fileDepIdentifier.FindPropsFileDependenciesInVcxprojFiles(filePath);

                    string additionalIncludeDirs = string.Empty;
                    List<string> additionalIncludeDirsList = new List<string>();
                    propsDependencies = DepIdentifierUtils.ResolveFromLocalDirectoryOrPatcher(projectFilePath: filePath, propsDependencies, false);
                    foreach (string propFileDependency in propsDependencies)
                    {
                        additionalIncludeDirsList.AddRange(fileDepIdentifier.FindAdditionalIncludeDirectorisInAPropFile(propFileDependency, folder, filesListXMLPath));
                    }
                    //props file

                    additionalIncludeDirsList.AddRange(fileDepIdentifier.FindAdditionalIncludeDirectoriesOfVCXproj(filePath));

                    additionalIncludeDirs = string.Join(";", DepIdentifierUtils.ResolveAdditionalDirectoriesInList(additionalIncludeDirsList, filePath));

                    XmlDocument xmlDocument = XMLHelperAPIs.GetFilesListXmlDocument();
                    UpdateTheXmlAttribute(xmlDocument, folder.Replace("//", "_") + "/filepath", "Name", filePath, "AdditionalIncludeDirectories", additionalIncludeDirs);
                    XMLHelperAPIs.SaveXmlToFile(xmlDocument, ReversePatcher.m_FilesListXMLPath);

                    resolvedList = DepIdentifierUtils.ResolveFromLocalDirectoryOrPatcher(filePath, dependenciesList);

                    UpdateTheXmlAttributeDependenciesPathAsync(filePath, resolvedList, folder);
                    UpdateProjectNameForTheFilesUnderVCXProjAsync(resolvedList, "Project", filePath);
                    UpdateTheXmlAttributeReferencesPathAsync(filePath, resolvedList);
                }
                else
                {
                    UpdateTheXmlAttributeDependenciesPathAsync(filePath, new List<string> { "no dependencies" }, folder);
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
                FileDepIdentifier fileDepIdentifier = new FileDepIdentifier();
                dependenciesList = fileDepIdentifier.FindVBPFileDependencies(filePath);
                if (dependenciesList != null && dependenciesList.Count > 0)
                {
                    //resolvedList = ResolveFromLocalDirectoryOrPatcher(filePath, dependenciesList, fromPatcher: true);

                    UpdateTheXmlAttributeDependenciesPathAsync(filePath, dependenciesList, folder);
                    UpdateTheXmlAttributeReferencesPathAsync(filePath, dependenciesList);
                }
                else
                {
                    UpdateTheXmlAttributeDependenciesPathAsync(filePath, new List<string> { "no dependencies" }, folder);
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
                FileDepIdentifier fileDepIdentifier = new FileDepIdentifier();
                List<string> dependencies = fileDepIdentifier.FindLstDependencies(filePath);

                if (dependencies != null && dependencies.Count > 0)
                {
                    resolvedList = DepIdentifierUtils.ResolveFromLocalDirectoryOrPatcher(filePath, dependencies, fromPatcher: true);

                    UpdateTheXmlAttributeDependenciesPathAsync(filePath, resolvedList, folder);
                    UpdateTheXmlAttributeReferencesPathAsync(filePath, resolvedList);
                }
                else
                {
                    UpdateTheXmlAttributeDependenciesPathAsync(filePath, new List<string> { "no dependencies" }, folder);
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
                FileDepIdentifier fileDepIdentifier = new FileDepIdentifier();
                dependencies = fileDepIdentifier.FindWixProjDependenices(filePath, folder, filesListXMLPath);

                if (dependencies != null && dependencies.Count > 0)
                {
                    dependencies = DepIdentifierUtils.ResolveFromLocalDirectoryOrPatcher(filePath, dependencies, fromPatcher: true);

                    UpdateTheXmlAttributeDependenciesPathAsync(filePath, dependencies, folder);
                    UpdateTheXmlAttributeReferencesPathAsync(filePath, dependencies);
                }
                else
                {
                    UpdateTheXmlAttributeDependenciesPathAsync(filePath, new List<string> { "no dependencies" }, folder);
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
                FileDepIdentifier fileDepIdentifier = new FileDepIdentifier();
                dependencies = fileDepIdentifier.Find409VCXProjDependendencies(filePath, folder, filesListXMLPath);

                if (dependencies != null && dependencies.Count > 0)
                {
                    dependencies = DepIdentifierUtils.ResolveFromLocalDirectoryOrPatcher(filePath, dependencies, fromPatcher: true);

                    UpdateTheXmlAttributeDependenciesPathAsync(filePath, dependencies, folder);
                    UpdateTheXmlAttributeReferencesPathAsync(filePath, dependencies);
                }
                else
                {
                    UpdateTheXmlAttributeDependenciesPathAsync(filePath, new List<string> { "no dependencies" }, folder);
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
                FileDepIdentifier fileDepIdentifier = new FileDepIdentifier();
                List<string> dependenciesList = fileDepIdentifier.FindDependenciesInCsprojFiles(filePath);
                if (dependenciesList != null && dependenciesList.Count > 0)
                {
                    resolvedList = DepIdentifierUtils.ResolveFromLocalDirectoryOrPatcher(filePath, dependenciesList, fromPatcher: true);

                    UpdateTheXmlAttributeDependenciesPathAsync(filePath, resolvedList, folder);
                    UpdateTheXmlAttributeReferencesPathAsync(filePath, resolvedList);
                }
                else
                {
                    UpdateTheXmlAttributeDependenciesPathAsync(filePath, new List<string> { "no dependencies" }, folder);
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

        public static void UpdateTheXmlAttributeDependenciesPathAsync(string filePath, List<string> updatedParsedIdlFilePaths, string folder)
        {
            //Update the XML attribute with IDL path information asynchronously
            XmlDocument xmlDocument = XMLHelperAPIs.GetFilesListXmlDocument();

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
            UpdateTheXmlAttribute(xmlDocument, DepIdentifierUtils.GetCurrentFilterFromFilePath(filePath) + "/filepath", "Name", filePath, "Dependency", string.Join(";", dependencyFiles));
            XMLHelperAPIs.SaveXmlToFile(xmlDocument, ReversePatcher.m_FilesListXMLPath);

        }
        public static void UpdateTheXmlAttributeDependenciesPathAsync(string filePath, string dependenciesListSemicolonSeperated, string folder)
        {
            XmlDocument xmlDocument = XMLHelperAPIs.GetFilesListXmlDocument();

            if (string.IsNullOrEmpty(dependenciesListSemicolonSeperated))
            {
                dependenciesListSemicolonSeperated = "no dependencies";
            }
            //Utilities.AppendNewAttribute(xmlDoc, m_selectedFilterPath.Replace("\\", "_") + "/FilePath", "IDL", string.Join(";", m_DependencyList));
            UpdateTheXmlAttribute(xmlDocument, DepIdentifierUtils.GetCurrentFilterFromFilePath(filePath) + "/filepath", "Name", filePath, "Dependency", string.Join(";", dependenciesListSemicolonSeperated));
            XMLHelperAPIs.SaveXmlToFile(xmlDocument, ReversePatcher.m_FilesListXMLPath);
        }
        
        public static void UpdateTheXmlAttributeReferencesPathAsync(string filePath, List<string> updatedParsedIdlFilePaths)
        {
            try
            {
                XmlDocument xmlDocument = XMLHelperAPIs.GetFilesListXmlDocument();

                string dependencyFiles = string.Empty;
                //for each file Update the filePath as referenced file here..
                foreach (var file in updatedParsedIdlFilePaths)
                {
                    string folder = DepIdentifierUtils.GetCurrentFilterFromFilePath(file).ToLower(); ;
                    UpdateTheXmlAttribute(xmlDocument, folder + "/filepath", "Name", file, "Reference", filePath, true);
                }
                XMLHelperAPIs.SaveXmlToFile(xmlDocument, ReversePatcher.m_FilesListXMLPath);
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
                ReversePatcher.isXMLSaved = true;
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
                ReversePatcher.isXMLSaved = true;
                DepIdentifierUtils.WriteTextInLog("XML file updated successfully.");
            }
            catch (Exception ex)
            {
                DepIdentifierUtils.WriteTextInLog("Error saving XML: " + ex.Message);
                Thread.Sleep(1000);
                SaveXmlToFile(xmlDoc, filePath);
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
                XmlDocument xmlDoc = new XmlDocument();

                if (File.Exists(filePath))
                {
                    xmlDoc.Load(filePath);
                }
                else
                {
                    xmlDoc.AppendChild(xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", null));
                    xmlDoc.AppendChild(xmlDoc.CreateElement(parentNode));
                }

                XmlElement parentNodeElement = xmlDoc.SelectSingleNode($"//{parentNode}") as XmlElement;
                if (parentNodeElement == null)
                {
                    parentNodeElement = xmlDoc.CreateElement(parentNode);
                    xmlDoc.AppendChild(parentNodeElement);
                }

                XmlElement? rootElement = null;
                if (rootElementName != "")
                {
                    rootElement = parentNodeElement.SelectSingleNode($"{rootElementName}") as XmlElement;
                    if (rootElement == null)
                    {
                        rootElement = xmlDoc.CreateElement(rootElementName);
                        parentNodeElement.AppendChild(rootElement);
                    }
                }
                if (!string.IsNullOrEmpty(currentElementName))
                {
                    foreach (string str in stringsList)
                    {
                        XmlElement existingElement = null;
                        try
                        {
                            if (rootElementName == "")
                            {
                                rootElementName = DepIdentifierUtils.GetCurrentFilterFromFilePath(str);

                                rootElement = parentNodeElement.SelectSingleNode($"{rootElementName}") as XmlElement;
                                if (rootElement == null)
                                {
                                    rootElement = xmlDoc.CreateElement(rootElementName);
                                    parentNodeElement.AppendChild(rootElement);
                                }
                            }
                            existingElement = rootElement.SelectSingleNode($"{currentElementName}[@Name='{str}']") as XmlElement;

                        }
                        catch(Exception)
                        {
                            existingElement = null;
                        }
                        if (existingElement == null)
                        {
                            XmlElement newElement = xmlDoc.CreateElement(currentElementName);
                            newElement.SetAttribute("Name", str);

                            if (File.Exists(str))
                            {
                                newElement.SetAttribute("ShortName", Path.GetFileName(str));
                            }

                            rootElement.AppendChild(newElement);
                        }
                        else
                        {
                            if (File.Exists(str))
                            {
                                XmlAttribute shortNameAttribute = existingElement.GetAttributeNode("ShortName");
                                if (shortNameAttribute == null)
                                {
                                    existingElement.SetAttribute("ShortName", Path.GetFileName(str));
                                }
                            }
                        }
                    }
                }

                xmlDoc.Save(filePath);
                if (filePath == ReversePatcher.m_FilesListXMLPath)
                    ReversePatcher.isXMLSaved = true;

                DepIdentifierUtils.WriteTextInLog("XML file updated/created successfully.");
            }
            catch (Exception ex)
            {
                DepIdentifierUtils.WriteTextInLog("Error updating/creating XML file: " + ex.Message);
                // Re-throw the exception to propagate it further if needed
                //throw;
            }
        }


        //public static void CreateOrUpdateListXml(List<string> stringsList, string filePath, string parentNode, string rootElementName, string currentElementName)
        //{
        //    try
        //    {
        //        DepIdentifierUtils.WriteTextInLog(rootElementName);
        //        // Load existing XML document if it exists, otherwise create a new one
        //        XmlDocument xmlDoc = new XmlDocument();
        //        if (File.Exists(filePath))
        //        {
        //            xmlDoc.Load(filePath);
        //        }
        //        else
        //        {
        //            XmlDeclaration xmlDeclaration = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", null);
        //            XmlElement root = xmlDoc.CreateElement(parentNode);
        //            xmlDoc.AppendChild(root);
        //            xmlDoc.InsertBefore(xmlDeclaration, root);
        //        }

        //        // Get or create the parent node
        //        XmlElement parentNodeElement = xmlDoc.SelectSingleNode($"//{parentNode}") as XmlElement;
        //        if (parentNodeElement == null)
        //        {
        //            parentNodeElement = xmlDoc.CreateElement(parentNode);
        //            xmlDoc.AppendChild(parentNodeElement);
        //        }

        //        // Get the root element
        //        XmlElement rootElement = parentNodeElement.SelectSingleNode($"{rootElementName}") as XmlElement;
        //        if (rootElement == null)
        //        {
        //            rootElement = xmlDoc.CreateElement(rootElementName);
        //            parentNodeElement.AppendChild(rootElement);
        //        }

        //        if (currentElementName != "")
        //        {
        //            // Process each string
        //            foreach (string str in stringsList)
        //            {
        //                // Check if the element already exists
        //                XmlElement existingElement = null;
        //                try
        //                {
        //                    existingElement = rootElement.SelectSingleNode($"{currentElementName}[@Name='{str}']") as XmlElement;

        //                }
        //                catch(Exception)
        //                {
        //                    existingElement = null;
        //                }
        //                if (existingElement == null)
        //                {
        //                    // Create a new element and add it to the root
        //                    XmlElement newElement = xmlDoc.CreateElement(currentElementName);
        //                    newElement.SetAttribute("Name", str);
        //                    if (File.Exists(str))
        //                        newElement.SetAttribute("ShortName", Path.GetFileName(str));
        //                    rootElement.AppendChild(newElement);
        //                }
        //                else
        //                {
        //                    if (File.Exists(str))
        //                    {
        //                        XmlAttribute xmlNode = existingElement.GetAttributeNode("ShortName");
        //                        if (xmlNode == null)
        //                        {
        //                            existingElement.SetAttribute("ShortName", Path.GetFileName(str));
        //                        }
        //                    }
        //                }
        //                // If the element exists, you can choose to do something here
        //            }
        //        }

        //        // Save the updated XML
        //        xmlDoc.Save(filePath);

        //        DepIdentifierUtils.WriteTextInLog("XML file updated/created successfully.");
        //    }
        //    catch (Exception ex)
        //    {
        //        DepIdentifierUtils.WriteTextInLog("Error updating/creating XML file: " + ex.Message);
        //        //throw new Exception("Failed to UpdateListInFilesListXml with exception: " + ex.Message);
        //    }
        //}

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
            XmlDocument xmlDocument = XMLHelperAPIs.GetFilesListXmlDocument();
            foreach (var filepath in dependenciesList)
            {
                XMLHelperAPIs.UpdateTheXmlAttribute(xmlDocument, "filepath", "Name", filepath, attributeValueToSearch, projectName);
            }
            XMLHelperAPIs.SaveXmlToFile(xmlDocument, ReversePatcher.m_FilesListXMLPath);
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

        public static bool RemoveNodeFromXML(XmlDocument xmlDocument, string filePath)
        {
            try
            {
                string filter = DepIdentifierUtils.GetCurrentFilterFromFilePath(filePath);

                // Get the elements with the specified name and attribute valu
                string xPathXpression = $"//{filter}/filepath[@Name='{filePath}']";

                XmlNodeList xmlNodeList = xmlDocument.DocumentElement.SelectNodes(xPathXpression);
                if (xmlNodeList.Count > 0)
                {
                    foreach(XmlNode xmlNode in xmlNodeList)
                    {
                        XmlElement element = (XmlElement)xmlNode;
                        string references = element.GetAttribute("References");
                        var referencesList = references.Split(';').ToList();
                        foreach(var referenceFilePath in referencesList)
                        {
                            filter = DepIdentifierUtils.GetCurrentFilterFromFilePath(referenceFilePath);
                            xPathXpression = $"//{filter}/filepath[@Name='{referenceFilePath}']";
                            XmlNodeList xmlReferenceNodeList = xmlDocument.DocumentElement.SelectNodes(xPathXpression);
                            if (xmlReferenceNodeList.Count == 1)
                            {
                                var refElement = (XmlElement)xmlNode;
                                string dependency = element.GetAttribute("Dependencies");
                                dependency.Replace(filePath, "", StringComparison.OrdinalIgnoreCase);
                                UpdateTheXmlAttributeDependenciesPathAsync(referenceFilePath, dependency.Split(";").ToList(), "");
                            }
                            else
                            {
                                DepIdentifierUtils.WriteTextInLog($"RemoveNodeFromXML: Multiple nodes found with same file path for reference: {referenceFilePath}");
                            }
                        }
                        xmlDocument.RemoveChild(xmlNode);
                    }

                    
                }
            }
            catch (Exception ex)
            {
                DepIdentifierUtils.WriteTextInLog($"Unable to RemoveNodeFromXML with exception: {ex.Message}");
                return false;
            }
            XMLHelperAPIs.SaveXmlToFile(xmlDocument, ReversePatcher.m_FilesListXMLPath);
            return true;
        }

        public static bool RemoveReferencesFromDependecniesInXML(XmlDocument xmlDocument, string referenceFilePath)
        {
            try
            {
                string elementName = "filepath";
                string attributeNameToSearch = "Name";
                string attributeValueToSearch = referenceFilePath.ToLower();

                string currentFileFilter = DepIdentifierUtils.GetCurrentFilterFromFilePath(referenceFilePath);
                //dependentList = Utilities.GetNameAttributeValue(xmlDoc, m_selectedFilterPath.Replace("\\", "_") + "/FilePath", "Name", file);
                string dependentListSemiColonSeperated = XMLHelperAPIs.GetDependecyStringFromXML(xmlDocument, currentFileFilter, elementName, attributeNameToSearch, attributeValueToSearch);

                dependentListSemiColonSeperated.Replace(referenceFilePath, "", StringComparison.OrdinalIgnoreCase);
                UpdateTheXmlAttributeDependenciesPathAsync(referenceFilePath, dependentListSemiColonSeperated, "");
            }                   
            
            catch (Exception ex)
            {
                DepIdentifierUtils.WriteTextInLog($"Unable to RemoveNodeFromXML with exception: {ex.Message}");
                return false;
            }
            XMLHelperAPIs.SaveXmlToFile(xmlDocument, ReversePatcher.m_FilesListXMLPath);
            return true;
        }

    }
}
