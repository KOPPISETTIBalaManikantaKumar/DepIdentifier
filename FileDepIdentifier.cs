using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace DepIdentifier
{
    internal class FileDepIdentifier
    {
        #region public APIS

        public static List<string> GetTheFileDependencies(string filePath, string folder, string filtersXMLPath = "")
        {
            DepIdentifierUtils.WriteTextInLog("Identifying " + filePath + " Dependencies.");
            List<string> dependentFiles = new List<string>();
            try
            {
                if (string.Compare(Path.GetExtension(filePath), ".idl", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    dependentFiles = XMLHelperAPIs.FindIDLDependenciesAndAddToXml(filePath, folder);
                }
                else if (string.Compare(Path.GetExtension(filePath), ".h", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    dependentFiles = XMLHelperAPIs.FindDependenciesInADOtHCppAndRCFileAndAddtoXml(filePath, folder, filtersXMLPath);
                }
                else if (string.Compare(Path.GetExtension(filePath), ".cpp", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    dependentFiles = XMLHelperAPIs.FindDependenciesInADOtHCppAndRCFileAndAddtoXml(filePath, folder, filtersXMLPath);
                }
                else if (string.Compare(Path.GetExtension(filePath), ".vcxproj", StringComparison.OrdinalIgnoreCase) == 0 && !(filePath.Contains("409")))
                {
                    dependentFiles = XMLHelperAPIs.FindVcxprojDependenciesAndAddToXml(filePath, folder, filtersXMLPath);
                }
                else if (string.Compare(Path.GetExtension(filePath), ".vbp", StringComparison.OrdinalIgnoreCase) == 0 || string.Compare(Path.GetExtension(filePath), ".vbproj", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    dependentFiles = XMLHelperAPIs.FindVBPDependenciesAndAddToXml(filePath, folder, filtersXMLPath);
                }
                else if (string.Compare(Path.GetExtension(filePath), ".csproj", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    dependentFiles = XMLHelperAPIs.FindCSProjDependenciesAndAddToXml(filePath, folder, filtersXMLPath);
                }
                else if (string.Compare(Path.GetExtension(filePath), ".rc", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    dependentFiles = XMLHelperAPIs.FindDependenciesInADOtHCppAndRCFileAndAddtoXml(filePath, folder, filtersXMLPath);
                }
                else if (string.Compare(Path.GetExtension(filePath), ".lst", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    dependentFiles = XMLHelperAPIs.FindLstFileDependenciesAndAddToXml(filePath, folder, filtersXMLPath);
                }
                else if (string.Compare(Path.GetExtension(filePath), ".wixproj", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    dependentFiles = XMLHelperAPIs.FindWixProjDependenicesAndAddToXML(filePath, folder, filtersXMLPath);
                }
                else if (string.Compare(Path.GetExtension(filePath), ".vcxproj", StringComparison.OrdinalIgnoreCase) == 0 && filePath.Contains("409"))
                {
                    dependentFiles = XMLHelperAPIs.Find409VCXProjDependendenciesAndAddToXML(filePath, folder, filtersXMLPath);
                }
                else
                {
                    //No dependent files
                }
            }
            catch (Exception ex)
            {
                DepIdentifierUtils.WriteTextInLog("Unable to get Dependencies for " + filePath + " with exception " + ex.Message);
            }
            return dependentFiles;
        }

        public static List<string> FindIDLDependencies(string idlFileName, string folder)
        {
            List<string> parsedIdlFilePaths = ExtractImportedFilesAndResolvePathsFromFile(idlFileName);

            // Update the XML attribute with IDL path information for the current idlFileName
            //UpdateTheXmlAttributeIDLPath(idlFileName, parsedIdlFilePaths);

            return parsedIdlFilePaths;
        }

        public static List<string> FindDependenciesInADOtHCppAndRCFile(string cppFilePath)
        {
            List<string> dependentFiles = new List<string>();

            try
            {
                // Read the content of the .cpp file
                string fileContent = File.ReadAllText(cppFilePath);

                // Define the regular expression pattern to match #include statements
                //string pattern = @"#include\s*[""']([^""']+)[^""']*[""']";
                //string pattern = @"#include\s*[""<]([^"">\\/\n]+)[>""]";

                string pattern = @"^\s*#include\s*[""<]([^"">\\/\\n]+)[\"">](?![^\\n]*\\/\\*.*?\\*\\/)[^\\n]*";

                // Create a regular expression object
                Regex regex = new Regex(pattern);

                // Search for matches in the file content
                MatchCollection matches = regex.Matches(fileContent);

                // Extract the file names from the matches and add them to the dependentFiles list
                foreach (Match match in matches)
                {
                    string fileName = match.Groups[1].Value;
                    if (DepIdentifierUtils.IsValidFilenameWithExtension(fileName) && !DepIdentifierUtils.IsCommonFile(fileName) && !fileName.EndsWith("_i.c", StringComparison.OrdinalIgnoreCase))
                        dependentFiles.Add(fileName);
                }
            }
            catch (Exception ex)
            {
                DepIdentifierUtils.WriteTextInLog("Error occurred: " + ex.Message);
            }

            return dependentFiles;
        }

        public static List<string> FindDependenciesInVcxprojFiles(string vcxprojFilePath)
        {
            List<string> dependencies = new List<string>();
            if (vcxprojFilePath != null)
            {
                try
                {
                    // Load the .vcxproj file
                    XDocument doc = XDocument.Load(vcxprojFilePath);

                    // Find references to other C++ projects within the same solution
                    //dependencies.AddRange(doc.Descendants("ProjectReference").Select(e => e.Attribute("Include")?.Value).Where(value => value != null));

                    //// Find references to library files
                    //dependencies.AddRange(doc.Descendants("Library").Select(e => e.Attribute("Include")?.Value).Where(value => value != null));

                    //// Find references to .NET Framework assemblies
                    //dependencies.AddRange(doc.Descendants("Reference").Select(e => e.Attribute("Include")?.Value).Where(value => value != null));

                    // Find references to idl files
                    dependencies.AddRange(doc.Descendants(XName.Get("Midl", "http://schemas.microsoft.com/developer/msbuild/2003")).Select(e => e.Attribute("Include")?.Value).Where(value => value != null));
                    // Find references to idl files
                    dependencies.AddRange(doc.Descendants(XName.Get("ClInclude", "http://schemas.microsoft.com/developer/msbuild/2003")).Select(e => e.Attribute("Include")?.Value).Where(value => value != null));
                    // Find references to idl files
                    dependencies.AddRange(doc.Descendants(XName.Get("ClCompile", "http://schemas.microsoft.com/developer/msbuild/2003")).Select(e => e.Attribute("Include")?.Value).Where(value => value != null));

                    //// Find references to .h files (ClInclude)
                    //dependencies.AddRange(doc.Descendants("ClInclude").Select(e => e.Attribute("Include")?.Value).Where(value => value != null));

                    //// Find references to .cpp files (ClCompile)
                    //dependencies.AddRange(doc.Descendants("ClCompile").Select(e => e.Attribute("Include")?.Value).Where(value => value != null));

                }
                catch (Exception ex)
                {
                    throw new Exception($"Failed to FindDependenciesInVcxprojFiles for the file '{vcxprojFilePath}' with exception: '{ex.Message}'");
                }

            }
            return dependencies;
        }

        public static List<string> FindPropsFileDependenciesInVcxprojFiles(string vcxprojFilePath)
        {
            List<string> dependencies = new List<string>();
            if (vcxprojFilePath != null)
            {
                try
                {
                    // Load the .vcxproj file
                    XDocument doc = XDocument.Load(vcxprojFilePath);

                    dependencies.AddRange(doc.Descendants(XName.Get("Import", "http://schemas.microsoft.com/developer/msbuild/2003")).Select(e => e.Attribute("Project")?.Value).Where(value => value != null));

                }
                catch (Exception ex)
                {
                    throw new Exception($"Failed to FindDependenciesInVcxprojFiles for the file '{vcxprojFilePath}' with exception: '{ex.Message}'");
                }

            }
            return dependencies;
        }

        public static List<string> FindVBPFileDependencies(string vbpFilePath)
        {
            List<string> vbpDependencies = new List<string>();
            List<string> classDependencies = new List<string>();
            List<string> moduleDependencies = new List<string>();
            string m_DirectoryRoot = DepIdentifierUtils.GetCurrentFilterFromFilePath(vbpFilePath);
            try
            {
                // Read all lines from the .vbp file
                string[] lines = File.ReadAllLines(vbpFilePath);

                foreach (string line in lines)
                {
                    string vbpDirectory = Path.GetDirectoryName(vbpFilePath) + "\\";
                    if (line.StartsWith("Class=") || line.StartsWith("Module="))
                    {
                        // Extract the component path from the line
                        string componentPath = line.Split(';')[1].TrimStart();
                        if (componentPath.Contains("..") || componentPath.Contains("\\"))
                        {

                            string fullPath = Path.Combine(vbpDirectory, componentPath);
                            if (File.Exists(fullPath))
                            {
                                fullPath = Path.GetFullPath(fullPath);
                                if (!DepIdentifierUtils.IsFileUnderDirectory(m_DirectoryRoot, fullPath))
                                {
                                    classDependencies.Add(DepIdentifierUtils.ChangeToClonedPathFromVirtual(fullPath));
                                }
                            }
                        }
                        else
                        {
                            //Do nothing since the file will be in the same directory..
                        }
                    }

                    if (line.StartsWith("Module="))
                    {
                        // Extract the component path from the line
                        string componentPath = line.Split(';')[1].TrimStart();
                        if (componentPath.StartsWith("..")) //Like ..\..\..\..\Xroot\Container\Include\CoreTraderKeys.bas
                        {
                            string resolvedPath = Path.Combine(vbpDirectory, componentPath);
                            if (File.Exists(resolvedPath))
                            {
                                string fullPath = Path.GetFullPath(resolvedPath);
                                if (!DepIdentifierUtils.IsFileUnderDirectory(m_DirectoryRoot, fullPath))
                                    moduleDependencies.Add(DepIdentifierUtils.ChangeToClonedPathFromVirtual(fullPath));
                            }
                            else
                            {
                                DepIdentifierUtils.WriteTextInLog($"Unable to resolve path for {resolvedPath}");
                            }
                        }
                        else if (!componentPath.StartsWith("..") && componentPath.Contains("\\")) //Like M:\Equipment\Middle\Include\Constants.bas or
                                                                                                  //L10N\Include\L10NInsertComponentCmds.bas
                        {
                            if (componentPath.Contains(":"))
                            {
                                if (!DepIdentifierUtils.IsFileUnderDirectory(m_DirectoryRoot, componentPath))
                                    moduleDependencies.Add(DepIdentifierUtils.ChangeToClonedPathFromVirtual(componentPath));
                            }
                            else
                            {
                                //Do Nothing
                            }
                        }
                        else //Like ArgumentKey.bas
                        {
                            //Do nothing since the file will be in the same directory..
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to FindVBPFileDependencies for the '{vbpFilePath}' with exception: " + ex.Message);
            }

            vbpDependencies.AddRange(classDependencies);
            vbpDependencies.AddRange(moduleDependencies);
            return vbpDependencies;
        }

        public static List<string> FindDependenciesInCsprojFiles(string csprojFilePath)
        {
            List<string> dependencies = new List<string>();

            try
            {
                dependencies = ExtractAllDependenciesOfCSProj(csprojFilePath);
                dependencies = dependencies
            .Where(dependency =>
                !dependency.Contains("\\.vs\\") &&
                !dependency.Contains("\\obj\\") &&
                !dependency.Contains("\\bin\\"))
            .ToList();

            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to FindDependenciesInCsprojFiles for the '{csprojFilePath}' with exception: {ex.Message}");
            }

            return dependencies;
        }

        public static List<string> Find409VCXProjDependendencies(string filePath, string folder, string filtersXMLPath)
        {
            List<string> dependencies = new List<string>();

            try
            {
                XDocument csprojDocument = XDocument.Load(filePath);

                var allDependencyItems = csprojDocument.Descendants()
                    .Where(e => e.Name.LocalName == "ResourceCompile" || e.Name.LocalName == "ClInclude" || e.Name.LocalName == "None")
                    .Select(e => e.Attribute("Include")?.Value)
                    .Where(include => !string.IsNullOrEmpty(include));

                dependencies.AddRange(allDependencyItems);
            }
            catch (Exception ex)
            {
                DepIdentifierUtils.WriteTextInLog($"Error extracting dependencies from {filePath}: {ex.Message}");
            }
            return dependencies;
        }

        public static List<string> FindLstDependencies(string filePath)
        {
            List<string> sqlDependencies = new List<string>();

            try
            {
                using (StreamReader reader = new StreamReader(filePath))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (!line.StartsWith("#") && line.IndexOf(".sql", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            sqlDependencies.Add(line);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DepIdentifierUtils.WriteTextInLog($"Error processing file: {ex.Message}");
            }

            return sqlDependencies;
        }

        public static List<string> FindWixProjDependenices(string wixprojFilePath, string folder, string filtersXMLPath)
        {
            List<string> dependencies = new List<string>();

            try
            {
                XDocument wixprojDocument = XDocument.Load(wixprojFilePath);

                List<string> compileItems = wixprojDocument.Descendants()
                    .Where(e => e.Name.LocalName == "Compile")
                    .Select(e => e.Attribute("Include")?.Value)
                    .Where(include => include.EndsWith(".wxs", StringComparison.OrdinalIgnoreCase)).ToList();

                string wixprojFolder = Path.GetDirectoryName(wixprojFilePath);

                dependencies = compileItems.Select(compileItem => DepIdentifierUtils.ChangeToClonedPathFromVirtual(Path.Combine(wixprojFolder, compileItem))).ToList();

                //Dictionary<string, string> variablesDictionary = DepIdentifierUtils.ExtractVariableDefinitionsFromWixProj(wixprojFilePath);
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(DepIdentifierUtils.m_FilesListXMLPath);

                XMLHelperAPIs.UpdateProjectNameForTheFilesUnderVCXProj(xmlDoc, compileItems, "Project", wixprojFilePath);

                //foreach (string compileItem in compileItems)
                //{
                //    string wxsFilePath = Path.Combine(wixprojFolder, compileItem);
                //    if (File.Exists(wxsFilePath))
                //    {
                //        List<string> wixFileDependencies = FindWxsDependencies(wxsFilePath, variablesDictionary);

                //        wixFileDependencies = ResolveFromLocalDirectoryOrPatcher(wxsFilePath, wixFileDependencies, fromPatcher: true);

                //        UpdateTheXmlAttributeDependenciesPath(wxsFilePath, new List<string>(), folder, filtersXMLPath);

                //        //dependencies.AddRange(wixFileDependencies);
                //    }
                //    else
                //    {
                //        DepIdentifierUtils.WriteTextInLog($"The {wxsFilePath} path do not exist");
                //    }
                //}
            }
            catch (Exception ex)
            {
                DepIdentifierUtils.WriteTextInLog($"Error extracting dependencies: {ex.Message}");
            }

            return dependencies;
        }

        public static List<string> GetDependencyDataOfGivenFile(string file, XmlDocument xmlDoc, bool isRecompute = false, string currentFileFilter = "")
        {
            List<string> dependenicesOfCurrentFile = new List<string>();
            try
            {
                string dependentList = GetDependencyDataOfGivenFileFromXML(file, xmlDoc);

                if (String.IsNullOrEmpty(dependentList) || isRecompute)
                {
                    dependenicesOfCurrentFile = GetTheFileDependencies(file, ReversePatcher.m_selectedFilterPath);

                    //Update the xml accordingly
                }
                else
                {
                    string[] splittedStrings = dependentList.Split(new[] { ";" }, StringSplitOptions.None);
                    dependenicesOfCurrentFile.AddRange(splittedStrings);
                }
            }
            catch (Exception ex) { }

            return dependenicesOfCurrentFile;
                
        }

        public static string GetDependencyDataOfGivenFileFromXML(string file, XmlDocument xmlDoc)
        {
            string dependentListSemiColonSeperated = string.Empty;
            try
            {
                if (File.Exists(DepIdentifierUtils.m_FilesListXMLPath))
                {
                    if (xmlDoc == null)
                    {
                        xmlDoc = new XmlDocument();
                        xmlDoc.Load(DepIdentifierUtils.m_FilesListXMLPath);
                    }
                    string elementName = "filepath";
                    string attributeNameToSearch = "Name";
                    string attributeValueToSearch = file.ToLower();

                    string currentFileFilter = DepIdentifierUtils.GetCurrentFilterFromFilePath(file);
                    //dependentList = Utilities.GetNameAttributeValue(xmlDoc, m_selectedFilterPath.Replace("\\", "_") + "/FilePath", "Name", file);
                    dependentListSemiColonSeperated = XMLHelperAPIs.GetDependecyStringFromXML(xmlDoc, currentFileFilter, elementName, attributeNameToSearch, attributeValueToSearch);
                    //dependentList = Utilities.GetNameAttributeValue(xmlDoc, m_selectedFilterPath.Replace("\\", "_") + "/FilePath", "Name", file);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("GetDependencyDataOfFilsFromXML failed with exception " + ex.Message);
            }
            return dependentListSemiColonSeperated;
        }

        public static void GetFileDependenciesRecursively(List<string> m_filesForWhichDependenciesNeedToBeIdentified, XmlDocument xmlDocument)
        {
            List<string> currentListOfFilesDependencies = new List<string>();
            foreach (var file in m_filesForWhichDependenciesNeedToBeIdentified)
            {
                if (ReversePatcher.m_DependencyDictionary.ContainsKey(file))
                    continue;

                string currentFileFilter = DepIdentifierUtils.GetCurrentFilterFromFilePath(file);
                List<string> dependenicesOfCurrentFile = FileDepIdentifier.GetDependencyDataOfGivenFile(file, xmlDocument, currentFileFilter: currentFileFilter);

                currentListOfFilesDependencies.AddRange(dependenicesOfCurrentFile);

                ReversePatcher.m_DependencyDictionary.Add(file, dependenicesOfCurrentFile);

                if (dependenicesOfCurrentFile.Count > 0)
                    GetFileDependenciesRecursively(dependenicesOfCurrentFile, xmlDocument);
            }
        }

        public static List<string> FindAdditionalIncludeDirectoriesOfVCXproj(string vcxprojFilePath)
        {
            List<string> additionalIncludeDirs = new List<string>();
            try
            {
                string includeDirs = string.Empty;
                if (vcxprojFilePath != null && vcxprojFilePath != String.Empty)
                {
                    // Replace this with your actual implementation to fetch additional include directories
                    // This method should return a list of strings containing additional include directories
                    // defined in the given .vcxproj file.
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.Load(vcxprojFilePath);

                    XmlNamespaceManager nsmgr = new XmlNamespaceManager(xmlDoc.NameTable);
                    nsmgr.AddNamespace("ns", "http://schemas.microsoft.com/developer/msbuild/2003");

                    XmlNodeList includeDirsNodes = xmlDoc.SelectNodes("//ns:ClCompile/ns:AdditionalIncludeDirectories", nsmgr);

                    foreach (XmlNode node in includeDirsNodes)
                    {
                        includeDirs = includeDirs + node.InnerText;

                    }

                    List<string> paths = includeDirs.Split(';')
                                              .Select(path => path.Trim())  // Remove leading/trailing spaces
                                              .Select(path => path.Replace("%(AdditionalIncludeDirectories)", ""))
                                              .ToList();


                    HashSet<string> uniquePathsSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    List<string> uniquePathsList = new List<string>();

                    foreach (string path in paths)
                    {
                        if (uniquePathsSet.Add(path)) // Add returns true if the path is unique
                        {
                            uniquePathsList.Add(path);
                        }
                    }

                    additionalIncludeDirs = uniquePathsList;
                }
            }
            catch (Exception ex)
            {
                DepIdentifierUtils.WriteTextInLog($"Error: Faile in AdditionalIncludeDirectoriesOfVCXproj for the file: {vcxprojFilePath} with exception: {ex.Message}");
            }
            return additionalIncludeDirs;
        }

        public static List<string> FindAdditionalIncludeDirectorisInAPropFile(string filePath, string folder, string filtersXMLPath)
        {
            List<string> additionalIncludeDirectories = new List<string>();
            if (File.Exists(filePath))
            {
                // Load the .vcxproj file
                XDocument xDoc = XDocument.Load(filePath);

                // Find references to other C++ projects within the same solution
                //dependencies.AddRange(doc.Descendants("ProjectReference").Select(e => e.Attribute("Include")?.Value).Where(value => value != null));

                //// Find references to library files
                //dependencies.AddRange(doc.Descendants("Library").Select(e => e.Attribute("Include")?.Value).Where(value => value != null));

                //// Find references to .NET Framework assemblies
                //dependencies.AddRange(doc.Descendants("Reference").Select(e => e.Attribute("Include")?.Value).Where(value => value != null));

                // Find references to idl files
                XNamespace ns = "http://schemas.microsoft.com/developer/msbuild/2003";

                additionalIncludeDirectories.Add(xDoc.Descendants(ns + "ClCompile")
                                                        .Elements(ns + "AdditionalIncludeDirectories")
                                                        .Select(element => element.Value)
                                                        .FirstOrDefault());

                additionalIncludeDirectories.Add(xDoc.Descendants(ns + "ResourceCompile")
                                                            .Elements(ns + "AdditionalIncludeDirectories")
                                                            .Select(element => element.Value)
                                                            .FirstOrDefault());

            }
            return additionalIncludeDirectories;
        }
        #endregion

        #region private APIs

        private static List<string> ExtractImportedFilesAndResolvePathsFromFile(string fileName)
        {
            List<string> importedFiles = GetImportedFiles(fileName);

            if (importedFiles.Count == 0)
                return importedFiles;

            //DisplayList(importedFiles, "Imported files:");

            //Resolved Imported files
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(DepIdentifierUtils.m_FilesListXMLPath);

            string projectName = XMLHelperAPIs.GetAttributeOfFilePathFromXML(xmlDocument, "Project", fileName);
            string additionalIncludeDirectories = "";
            if (projectName != null || projectName != "")
                additionalIncludeDirectories = XMLHelperAPIs.GetAttributeOfFilePathFromXML(xmlDocument, "AdditionalIncludeDirectories", projectName);


            List<string> idlFilePaths = DepIdentifierUtils.ResolveFromLocalDirectoryOrPatcher(fileName, importedFiles, fromPatcher: true, additionalIncludeDirectories: additionalIncludeDirectories);
            return idlFilePaths;
        }
        private static List<string> GetImportedFiles(string filePath)
        {
            List<string> importedFiles = new List<string>();
            try
            {
                string fileContent = File.ReadAllText(filePath);

                // Regular expression pattern to match import/include statements
                string pattern = @"(?:#include\s+|import\s+)[\""\<]([^\""\<]+)[\""\<]";

                // Match the pattern in the file content
                MatchCollection matches = Regex.Matches(fileContent, pattern, RegexOptions.IgnoreCase);

                // Iterate through the matches and extract the imported files
                foreach (Match match in matches)
                {
                    string importedFile = match.Groups[1].Value;
                    if (!DepIdentifierUtils.IsCommonFile(importedFile))
                        importedFiles.Add(importedFile);
                }
            }
            catch (Exception ex)
            {
                DepIdentifierUtils.WriteTextInLog("Error occurred while getting Imported files: " + ex.Message);
            }
            return importedFiles.Distinct().ToList();
        }

        private static List<string> ExtractAllDependenciesOfCSProj(string csprojFilePath)
        {
            List<string> dependencies = new List<string>();

            try
            {
                string projectDirectory = Path.GetDirectoryName(csprojFilePath);

                // Extract dependencies from the .csproj file
                dependencies.AddRange(ExtractDependenciesFromCsprojFile(csprojFilePath));

                // Add all files from the project directory
                string[] allFiles = Directory.GetFiles(projectDirectory, "*.*", SearchOption.AllDirectories);
                dependencies.AddRange(allFiles);
            }
            catch (Exception ex)
            {
                DepIdentifierUtils.WriteTextInLog($"Error extracting dependencies: {ex.Message}");
            }

            return dependencies;
        }

        private static List<string> ExtractDependenciesFromCsprojFile(string csprojFilePath)
        {
            List<string> dependencies = new List<string>();

            try
            {
                XDocument csprojDocument = XDocument.Load(csprojFilePath);

                var allDependencyItems = csprojDocument.Descendants()
                    .Where(e => e.Name.LocalName == "Compile" || e.Name.LocalName == "Content" || e.Name.LocalName == "None")
                    .Select(e => e.Attribute("Include")?.Value)
                    .Where(include => !string.IsNullOrEmpty(include));

                dependencies.AddRange(allDependencyItems);
            }
            catch (Exception ex)
            {
                DepIdentifierUtils.WriteTextInLog($"Error extracting dependencies from {csprojFilePath}: {ex.Message}");
            }

            return dependencies;
        }

        #endregion


    }
}
