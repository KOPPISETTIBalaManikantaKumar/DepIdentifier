using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace DepIdentifier
{
    internal static class DepIdentifierUtils
    {
        #region required memberVaraibles
        public static List<string> patcherDataLines = new List<string>();
        public static string m_PatcherFilePath = @"g:\xroot\bldtools\s3dpatcher.pat";
        public static string m_AllS3DDirectoriesFilePath = @"g:\xroot\bldtools\depidentifier\resources\";
        public static string m_FiltersXMLPath = @"g:\xroot\bldtools\depidentifier\resources\filtersdata.xml";
        public static string m_FilesListXMLPath = @"g:\xroot\bldtools\depidentifier\resources\filesList.xml";
        public static List<string> m_CachedFiltersData = new List<string>();

        public static List<string> Commonfiles = new List<string>{ "ocidl.idl", "atlbase.h", "atlcom.h", "statreg.h" };
        #endregion
        const string m_XMLPath = "D:\\temp\\ProjectFilesInfo.xml";
        private const string m_logFilePath = "D:\\temp\\ProjectFilesData.txt";
        private static string m_XMLSDirectoryPath = @"D:\Tools\DepIdentifier\resource\";
        //private static string m_ResXMLPath = @"D:\Tools\DepIdentifier\resource\Res.xml";
        //private static string m_FiltersXMLPath = @"D:\Tools\DepIdentifier\resource\FilesList.xml";
        private static string m_ClonedRepo = Utilities.GetClonedRepo();

        public static List<string> IncludedExtensions = new List<string>
        {
            ".rc",
            ".cpp",
            ".vcxproj",
            ".vbproj",
            ".props",
            ".vbp",
            ".wixproj",
            ".wxs",
            ".lst",
            ".h",
            ".idl"
        };


        private static Dictionary<string, List<string>> fileContents = new Dictionary<string, List<string>>();

        public static async Task<bool> ComputeDependenciesForAllFilesAsync(List<string> m_FiltersList, string m_XMLSDirectoryPath)
        {
            bool anyIssues = false;
            Tuple<List<string>, List<string>> tuple = new Tuple<List<string>, List<string>>(new List<string>(), new List<string>());

            foreach (var filterPath in m_FiltersList)
            {
                List<string> filesUnderSelectedRoot = Utilities.FindPatternFilesInDirectory(Utilities.GetClonedRepo() + filterPath, "*.*");
                filesUnderSelectedRoot.Sort();

                bool ifXMLFileExist = File.Exists(m_XMLSDirectoryPath + @"\FilesList.xml");

                if (!ifXMLFileExist)
                    await DepIdentifierUtils.WriteListToXmlAsync(filesUnderSelectedRoot, m_XMLSDirectoryPath + @"\FilesList.xml", filterPath.Replace("\\", "_"), "FilePath", true);
                else
                {
                    await UpdateXmlWithDataAsync(filesUnderSelectedRoot, m_XMLSDirectoryPath + @"\FilesList.xml", filterPath.Replace("\\", "_"), "FilePath", true);
                }


                foreach (var eachFilePath in filesUnderSelectedRoot)
                {
                    GetTheFileDependencies(eachFilePath, filterPath);
                }
            }

            MessageBox.Show("File generated..!");


            return anyIssues;
        }

        public static async Task UpdateXmlWithDataAsync(List<string> stringsList, string filePath, string rootElementName, string stringElementName, bool update = true)
        {
            try
            {
                // Remove the already existing nodes
                XDocument xDoc = await LoadXDocumentAsync(filePath);

                XElement nodeToRemove = xDoc.Descendants(rootElementName).FirstOrDefault();
                if (nodeToRemove != null)
                {
                    nodeToRemove.Remove();
                }

                XElement rootElement = xDoc.Root;

                XElement newElement = new XElement(rootElementName);

                // Add the new data to the XML
                foreach (var entry in stringsList)
                {
                    XElement element = new XElement(stringElementName);
                    element.SetAttributeValue("Name", entry);
                    newElement.Add(element);
                }
                rootElement.Add(newElement);

                await SaveXDocumentAsync(xDoc, filePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error updating XML: " + ex.Message);
            }
        }

        private static async Task<XDocument> LoadXDocumentAsync(string filePath)
        {
            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, useAsync: true))
            {
                return await XDocument.LoadAsync(stream, LoadOptions.None, cancellationToken: default);
            }
        }

        private static async Task SaveXDocumentAsync(XDocument xDoc, string filePath)
        {
            using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true))
            {
                await xDoc.SaveAsync(stream, SaveOptions.None, cancellationToken: default);
            }
        }

        public static async Task WriteListToXmlAsync(List<string> stringsList, string filePath, string rootElementName, string stringElementName, bool update = true)
        {
            try
            {
                // Acquire the file lock
                await AsyncFileLock.LockAsync(filePath);

                // Create a new XML writer and set its settings
                XmlWriterSettings settings = new XmlWriterSettings
                {
                    Indent = true, // To format the XML with indentation
                    IndentChars = "    ", // Specify the indentation characters (four spaces in this case)
                    Async = true // Enable asynchronous writing
                };

                using (FileStream fileStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
                {
                    using (XmlWriter writer = XmlWriter.Create(fileStream, settings))
                    {
                        // Write the XML declaration asynchronously
                        await writer.WriteStartDocumentAsync();

                        // Write the root element with the specified name asynchronously
                        await writer.WriteStartElementAsync(null, "FiltersData", null);
                        await writer.WriteStartElementAsync(null, rootElementName, null);

                        // Write each string as a separate element with the specified name asynchronously
                        foreach (string str in stringsList)
                        {
                            await writer.WriteStartElementAsync(null, stringElementName, null);
                            await writer.WriteAttributeStringAsync(null, "Name", null, str);
                            await writer.WriteEndElementAsync();
                        }

                        // Close the root element asynchronously
                        await writer.WriteEndElementAsync();

                        // End the XML document asynchronously
                        await writer.WriteEndDocumentAsync();
                    }
                }

                Console.WriteLine("XML file created successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error writing XML file: " + ex.Message);
            }
            finally
            {
                // Release the file lock
                AsyncFileLock.Unlock(filePath);
            }
        }

        //public static void UpdateXmlWithData(List<string> stringsList, string filePath, string rootElementName, string stringElementName, bool update = true)
        //{
        //    try
        //    {
        //        //Remove the already exiting nodes


        //        XmlDocument xmlDoc = new XmlDocument();
        //        xmlDoc.Load(filePath);

        //        XmlNode nodeToRemove = xmlDoc.SelectSingleNode("//" + rootElementName);
        //        if (nodeToRemove != null)
        //        {
        //            xmlDoc.DocumentElement.RemoveChild(nodeToRemove);
        //        }

        //        XmlElement rootElement = xmlDoc.DocumentElement;

        //        XmlElement newElement = xmlDoc.CreateElement(rootElementName);

        //        // Add the new data to the XML
        //        foreach (var entry in stringsList)
        //        {
        //            XmlElement element = xmlDoc.CreateElement("FilePath");
        //            element.SetAttribute("Name", entry);
        //            newElement.AppendChild(element);
        //        }
        //        rootElement.AppendChild(newElement);
        //        xmlDoc.Save(filePath);
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine("Error updating XML: " + ex.Message);
        //    }
        //}

        private static bool FindVBPDependencies(string filePath, string filterPath)
        {
            return true;
        }

        private static bool FindVcxprojDependencies(string filePath, string filterPath)
        {
            return true;
        }

        private static bool FindCppDependencies(string filePath, string filterPath)
        {
            return true;
        }

        private static bool FindDotHDependencies(string filePath, string filterPath)
        {
            return true;
        }

        private static async void FindIDLDependenciesAsync(string filePath, string filterPath)
        {
            List<string> parsedIdlFilePaths = await GetParsedIdlFilePathsAsync(filePath, filterPath);
            List<string> updatedParsedIdlFilePaths = new List<string>(parsedIdlFilePaths);

            await UpdateTheXmlAttributeIDLPathAsync(filePath, updatedParsedIdlFilePaths, filterPath);
        }

        public static async Task<List<string>> GetParsedIdlFilePathsAsync(string idlFileName, string filterPath)
        {
            List<string> parsedIdlFilePaths = await Task.Run(() => Utilities.ExtractImportedFilesAndResolvePathsFromFile(idlFileName));
            parsedIdlFilePaths = parsedIdlFilePaths.Distinct().ToList();

            await UpdateTheXmlAttributeIDLPathAsync(idlFileName, parsedIdlFilePaths, filterPath);

            return parsedIdlFilePaths;
        }

        public static async Task UpdateTheXmlAttributeIDLPathAsync(string idlFileName, List<string> updatedParsedIdlFilePaths, string filterPath)
        {
            // Update the XML attribute with IDL path information asynchronously
            if (System.IO.File.Exists(m_FilesListXMLPath))
            {
                await AsyncFileLock.LockAsync(m_FilesListXMLPath);

                var xmlDoc = new XmlDocument();
                xmlDoc.Load(m_FilesListXMLPath);

                if (filterPath != null)
                {
                    await UpdateTheXmlAttributeAsync(xmlDoc, filterPath.Replace("\\", "_") + "/FilePath", "Name", idlFileName, "IDL", string.Join(";", updatedParsedIdlFilePaths));
                }

                await UpdateTheXmlAttributeAsync(xmlDoc, filterPath.Replace("\\", "_") + "/FilePath", "Name", idlFileName, "IDL", string.Join(";", updatedParsedIdlFilePaths));

                Utilities.SaveXmlToFile(xmlDoc, m_FilesListXMLPath);

                AsyncFileLock.Unlock(m_FilesListXMLPath);
            }
        }


        public static async Task UpdateTheXmlAttributeAsync(XmlDocument xmlDoc, string elementName, string attributeNameToSearch, string attributeValueToSearch, string attributeNameToUpdate, string attributeValueToUpdate)
        {
            try
            {
                await AsyncFileLock.LockAsync(m_FilesListXMLPath);

                // Get the elements with the specified name and attribute value
                XmlNodeList filterNodes = xmlDoc.DocumentElement.SelectNodes($"//{elementName}[@{attributeNameToSearch}='{attributeValueToSearch}']");

                foreach (XmlElement element in filterNodes)
                {
                    // Update the attribute value asynchronously
                    await Task.Run(() => element.SetAttribute(attributeNameToUpdate, attributeValueToUpdate));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error updating attribute in XML: " + ex.Message);
            }   
            finally
            {
                AsyncFileLock.Unlock(m_FilesListXMLPath);
            }
        }

        #region Public APIs to Identify dependencies, Resolving paths

        /// <summary>
        /// As of now not returning anything and the GetTheFileDependencies is writing the data to xml
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="filesUnderSelectedRoot"></param>
        public static void GetDependenciesOfFilesList(string folder, List<string> filesUnderSelectedRoot)
        {
            foreach (var file in filesUnderSelectedRoot)
            {
                GetTheFileDependencies(file, folder);
            }
        }

        public static bool IsHeaderFilePath(string inputText)
        {
            string pattern = @"(?i)\b(?:[a-z]:\\|\\\\\?\\)[^\s/""*:<>?\|]+\.(?:h)\b";
            Regex regex = new Regex(pattern);
            Match match = regex.Match(inputText);

            if (match.Success)
            {
                return true;
            }
            else
                return false;
        }

        private static bool IsCommonFile(string filename)
        {
            if (Commonfiles.Contains(Path.GetFileName(filename)))
                return true;
            else
                return false;
        }

        private static bool IsValidFilenameWithExtension(string filename)
        {
            try
            {
                string name = Path.GetFileNameWithoutExtension(filename);
                string extension = Path.GetExtension(filename);

                // Check if both name and extension are not empty
                return !string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(extension);
            }
            catch (ArgumentException)
            {
                // ArgumentException will be thrown if the filename is invalid
                return false;
            }
        }

        public static List<string> FindDependenciesInACppFile(string cppFilePath)
        {
            List<string> dependentFiles = new List<string>();

            try
            {
                // Read the content of the .cpp file
                string fileContent = File.ReadAllText(cppFilePath);

                // Define the regular expression pattern to match #include statements
                //string pattern = @"#include\s*[""']([^""']+)[^""']*[""']";
                string pattern = @"#include\s*[""<]([^"">\\/\n]+)[>""]";

                // Create a regular expression object
                Regex regex = new Regex(pattern);

                // Search for matches in the file content
                MatchCollection matches = regex.Matches(fileContent);

                // Extract the file names from the matches and add them to the dependentFiles list
                foreach (Match match in matches)
                {
                    string fileName = match.Groups[1].Value;
                    if (IsValidFilenameWithExtension(fileName) && !IsCommonFile(fileName))
                        dependentFiles.Add(fileName);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error occurred: " + ex.Message);
            }

            return dependentFiles;
        }

        private static List<string> FindCppDependenciesAndAddtoXml(string filePath, string folder, string filtersXMLPath)
        {
            List<string> resolvedList = new List<string>();
            try
            {
                List<string> dependenciesList = FindDependenciesInACppFile(filePath);
                if (dependenciesList != null && dependenciesList.Count > 0)
                {
                    resolvedList = ResolveFromLocalDirectoryOrPatcher(filePath, dependenciesList, fromPatcher: true);

                    resolvedList = RemoveTheMIDLGeneratedFilesFromTheList(resolvedList);


                    UpdateTheXmlAttributeDependenciesPath(filePath, resolvedList, folder, filtersXMLPath);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to FindCppDependencies with exception: " + ex.Message);
            }
            return resolvedList;
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

        private static List<string> FindVcxprojDependenciesAndAddToXml(string filePath, string folder, string filtersXMLPath)
        {
            List<string> resolvedList = new List<string>();
            try
            {
                List<string> dependenciesList = FindDependenciesInVcxprojFiles(filePath);
                if (dependenciesList != null && dependenciesList.Count > 0)
                {
                    resolvedList = ResolveFromLocalDirectoryOrPatcher(filePath, dependenciesList);

                    UpdateTheXmlAttributeDependenciesPath(filePath, resolvedList, folder, filtersXMLPath);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to FindVcxprojDependenciesAndAddToXml for the '{filePath}' with exception: '{ex.Message}'");
            }
            return resolvedList;
        }

        static bool IsFileUnderDirectory(string directoryPath, string filePath)
        {
            string fullPath = Path.GetFullPath(filePath);
            string fullDirectoryPath = Path.GetFullPath(directoryPath);

            return fullPath.StartsWith(fullDirectoryPath, StringComparison.OrdinalIgnoreCase);
        }

        private static List<string> FindVBPFileDependencies(string vbpFilePath)
        {
            List<string> vbpDependencies = new List<string>();
            List<string> classDependencies = new List<string>();
            List<string> moduleDependencies = new List<string>();
            string m_DirectoryRoot = ReversePatcher.GetCurrentFilterFromFilePath(vbpFilePath);
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
                                if (!IsFileUnderDirectory(m_DirectoryRoot, fullPath))
                                {
                                    classDependencies.Add(ChangeToClonedPathFromVirtual(fullPath));
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
                                if (!IsFileUnderDirectory(m_DirectoryRoot, fullPath))
                                    moduleDependencies.Add(ChangeToClonedPathFromVirtual(fullPath));
                            }
                            else
                            {
                                Console.WriteLine($"Unable to resolve path for {resolvedPath}");
                            }
                        }
                        else if (!componentPath.StartsWith("..") && componentPath.Contains("\\")) //Like M:\Equipment\Middle\Include\Constants.bas or
                                                                                                  //L10N\Include\L10NInsertComponentCmds.bas
                        {
                            if (componentPath.Contains(":"))
                            {
                                if (!IsFileUnderDirectory(m_DirectoryRoot, componentPath))
                                    moduleDependencies.Add(ChangeToClonedPathFromVirtual(componentPath));
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

        private static List<string> FindVBPDependenciesAndAddToXml(string filePath, string folder, string filtersXMLPath)
        {
            List<string> dependenciesList = new List<string> ();
            try
            {
                dependenciesList = FindVBPFileDependencies(filePath);
                if (dependenciesList != null && dependenciesList.Count > 0)
                {
                    //resolvedList = ResolveFromLocalDirectoryOrPatcher(filePath, dependenciesList, fromPatcher: true);

                    UpdateTheXmlAttributeDependenciesPath(filePath, dependenciesList, folder, filtersXMLPath);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to FindVBPDependenciesAndAddToXml for the '{filePath}' with exception: {ex.Message}");
            }
            return dependenciesList;
        }

        private static List<string> FindDependenciesInCsprojFiles(string csprojFilePath)
        {
            List<string> dependencies = new List<string>();

            try
            {
                // Load the .csproj file
                XDocument doc = XDocument.Load(csprojFilePath);

                dependencies.AddRange(doc.Descendants("Compile").Select(e => e.Attribute("Include").Value));

                dependencies.AddRange(doc.Descendants("Resource").Select(e => e.Attribute("Include").Value));

                dependencies.AddRange(doc.Descendants("None").Select(e => e.Attribute("Include").Value));
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to FindDependenciesInCsprojFiles for the '{csprojFilePath}' with exception: {ex.Message}");
            }

            return dependencies;
        }

        private static List<string> FindCSProjDependenciesAndAddToXml(string filePath, string folder, string filtersXMLPath)
        {
            List<string> resolvedList = new List<string>();
            try
            {
                List<string> dependenciesList = FindDependenciesInCsprojFiles(filePath);
                if (dependenciesList != null && dependenciesList.Count > 0)
                {
                    resolvedList = ResolveFromLocalDirectoryOrPatcher(filePath, dependenciesList, fromPatcher: true);

                    UpdateTheXmlAttributeDependenciesPath(filePath, resolvedList, folder, filtersXMLPath);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to FindCSProjDependenciesAndAddToXml for the '{filePath}' with exception: {ex.Message}");
            }
            return resolvedList;
        }

        public static List<string> GetTheFileDependencies(string filePath, string folder, string filtersXMLPath = "")
        {
            Console.WriteLine("Identifying " + filePath + " Dependencies.");
            List<string> dependentFiles = new List<string>();
            try
            {
                if (filePath.Contains(".idl"))
                {
                    dependentFiles = FindIDLDependenciesAndAddToXml(filePath, folder);
                }
                else if (IsHeaderFilePath(filePath))
                {
                    //dependentFiles = FindDotHDependenciesAndAddToXml(filePath, folder, filtersXMLPath);
                    dependentFiles = FindCppDependenciesAndAddtoXml(filePath, folder, filtersXMLPath);
                }
                else if (filePath.Contains(".cpp"))
                {
                    dependentFiles = FindCppDependenciesAndAddtoXml(filePath, folder, filtersXMLPath);
                }
                else if (filePath.Contains(".vcxproj") && !(filePath.Contains("409")))
                {
                    dependentFiles = FindVcxprojDependenciesAndAddToXml(filePath, folder, filtersXMLPath);
                }
                else if (filePath.Contains(".vbproj"))
                {
                    dependentFiles = new List<string>();
                }
                else if (filePath.Contains(".vbp"))
                {
                    dependentFiles = FindVBPDependenciesAndAddToXml(filePath, folder, filtersXMLPath);
                }
                else if (filePath.Contains(".csproj"))
                {
                    dependentFiles = FindCSProjDependenciesAndAddToXml(filePath, folder, filtersXMLPath);
                }
                else if (filePath.Contains(".rc"))
                {
                    //dependentFiles = FindRCDependenciesAndAddToXml(filePath, folder, filtersXMLPath);
                    dependentFiles = FindCppDependenciesAndAddtoXml(filePath, folder, filtersXMLPath);
                }
                else
                {
                    //No dependent files
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unable to get Dependencies for " + filePath + " with exception " + ex.Message);
            }
            return dependentFiles;
        }
        private static List<string> FindIDLDependenciesAndAddToXml(string filePath, string folder)
        {
            List<string> parsedIdlFilePaths = FindIDLDependencies(filePath, folder);
            string dependencyFiles = string.Empty;
            if (parsedIdlFilePaths != null && parsedIdlFilePaths.Count > 0)
            {
                UpdateTheXmlAttributeDependenciesPath(filePath, parsedIdlFilePaths, folder);
            }
            return parsedIdlFilePaths;
        }

        private static List<string> FindIDLDependencies(string idlFileName, string folder)
        {
            List<string> parsedIdlFilePaths = ExtractImportedFilesAndResolvePathsFromFile(idlFileName);

            // Update the XML attribute with IDL path information for the current idlFileName
            //UpdateTheXmlAttributeIDLPath(idlFileName, parsedIdlFilePaths);

            return parsedIdlFilePaths;
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
                    if(!IsCommonFile(importedFile))
                        importedFiles.Add(importedFile);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error occurred while getting Imported files: " + ex.Message);
            }
            return importedFiles.Distinct().ToList();
        }

        public static List<string> ResolveFromLocalDirectoryOrPatcher(string projectFilePath, List<string> dependenciesList, bool fromPatcher = true)
        {
            List<string> resolvedList = new List<string>();
            List<string> unResolvedList = new List<string>();
            string localDirectory = Path.GetDirectoryName(projectFilePath);
            foreach (var dependentFile in dependenciesList)
            {
                string combinedPath = Path.Combine(localDirectory, dependentFile);
                if (combinedPath.Contains(".."))
                {
                    combinedPath = Path.GetFullPath(combinedPath);
                }
                if (File.Exists(combinedPath))
                {
                    resolvedList.Add(combinedPath);
                }
                else
                {
                    if (fromPatcher)
                        unResolvedList.Add(dependentFile);
                    else
                        unResolvedList.Add(combinedPath);
                }
            }

            if (fromPatcher)
            {
                List<string> resolvedListFromPatcher = GetAllMatchingFilesFromS3DFilesList(unResolvedList);
                resolvedList.AddRange(resolvedListFromPatcher.ToList());
            }

            if (unResolvedList.Count > 0)
            {
                foreach (var unResolvedFilePath in unResolvedList)
                {
                    Console.WriteLine($"The {unResolvedFilePath} is not found which is used in the file: {projectFilePath}");
                }
            }

            //ConcurrentBag<string> resolvedListFromPatcher = ResolveFileNamesFromPatcher(filePath, unResolvedList, fromClonedRepo);

            return resolvedList;
        }

        private static List<string> ExtractImportedFilesAndResolvePathsFromFile(string fileName)
        {
            List<string> importedFiles = GetImportedFiles(fileName);
            //DisplayList(importedFiles, "Imported files:");

            //Resolved Imported files
            List<string> idlFilePaths = ResolveFromLocalDirectoryOrPatcher(fileName, importedFiles, fromPatcher: true);
            return idlFilePaths;
        }

        public static bool IsMIDLGenerated(string fileContent)
        {
            // You can add specific conditions or pattern matching logic here to check if the file is MIDL generated.
            // For simplicity, we'll assume that the presence of "#pragma once" indicates MIDL generated file.
            // You may need to adapt this logic based on your actual MIDL generated file characteristics.
            return (fileContent.Contains("File created by MIDL compiler") || fileContent.Contains("ALWAYS GENERATED file"));
        }
        private static List<string> FindDependenciesInAHeaderFile(string headerFilePath)
        {
            List<string> dependentHeaderFiles = new List<string>();

            try
            {
                // Read the content of the .h file
                string fileContent = File.ReadAllText(headerFilePath);

                // Check if the file is MIDL generated
                if (IsMIDLGenerated(fileContent))
                {
                    return dependentHeaderFiles; // Return an empty list for MIDL generated files
                }

                // Define the regular expression pattern to match #include statements for .h files
                string pattern = @"#include\s*[""']([^""']+\.(h|hpp))[^""']*[""']";

                // Create a regular expression object
                Regex regex = new Regex(pattern);

                // Search for matches in the file content
                MatchCollection matches = regex.Matches(fileContent);

                // Extract the file names from the matches and add them to the dependentHeaderFiles list
                foreach (Match match in matches)
                {
                    string headerFileName = match.Groups[1].Value;
                    dependentHeaderFiles.Add(headerFileName);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to FindDependentHeaderFiles with exception " + ex.Message);
            }

            return dependentHeaderFiles;
        }

        public static List<string> RemoveTheMIDLGeneratedFilesFromTheList(List<string> resolvedList)
        {
            List<string> nonMidlFIles = new List<string>();
            foreach (var file in resolvedList)
            {
                string fileContent = File.ReadAllText(file);
                // Check if the file is MIDL generated
                if (!IsMIDLGenerated(fileContent))
                {
                    nonMidlFIles.Add(file); // Return an empty list for MIDL generated files
                }
            }
            return nonMidlFIles;
        }

        private static List<string> FindDotHDependenciesAndAddToXml(string filePath, string folder, string filtersXMLPath)
        {
            List<string> resolvedList = new List<string>();
            try
            {
                List<string> dependenciesList = FindDependenciesInAHeaderFile(filePath);
                if (dependenciesList != null && dependenciesList.Count > 0)
                {
                    resolvedList = ResolveFromLocalDirectoryOrPatcher(filePath, dependenciesList, fromPatcher: true);
                    resolvedList = RemoveTheMIDLGeneratedFilesFromTheList(resolvedList);

                    UpdateTheXmlAttributeDependenciesPath(filePath, resolvedList, folder, filtersXMLPath);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to FindDotHDependencies with exception: " + ex.Message);
            }
            return resolvedList;
        }


        private static List<string> FindRCDependenciesAndAddToXml(string filePath, string folder, string filtersXMLPath)
        {
            List<string> resolvedList = new List<string>();
            try
            {
                resolvedList = FindDotHDependenciesAndAddToXml(filePath, folder, filtersXMLPath);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to FindRCDependenciesAndAddToXml for the '{filePath}' with exception: {ex.Message}");
            }
            return resolvedList;
        }


        public static List<string> ResolveFromLocalDirectoryOrPatcherUsingAdditionalIncludeDirectories(string projectFilePath, List<string> dependenciesList, string additionalIncludeDirectories)
        {
            List<string> resolvedList = new List<string>();
            List<string> unResolvedList = new List<string>();
            string localDirectory = Path.GetDirectoryName(projectFilePath);
            foreach (var dependentFile in dependenciesList)
            {
                string combinedPath = Path.Combine(localDirectory, dependentFile);
                if (combinedPath.Contains(".."))
                {
                    combinedPath = Path.GetFullPath(combinedPath);
                }
                if (File.Exists(combinedPath))
                {
                    resolvedList.Add(combinedPath);
                }
                else
                {
                    unResolvedList.Add(dependentFile);
                }
            }

            List<string> resolvedListFromPatcher = GetAllMatchingFilesFromS3DFilesList(unResolvedList);
            resolvedList.AddRange(resolvedListFromPatcher.ToList());

            if (unResolvedList.Count > 0 && resolvedListFromPatcher.Count < unResolvedList.Count)
            {
                foreach (var unResolvedFilePath in unResolvedList)
                {
                    Console.WriteLine($"The {unResolvedFilePath} is not found which is used in the file: {projectFilePath}");
                }
            }


            //ConcurrentBag<string> resolvedListFromPatcher = ResolveFileNamesFromPatcher(filePath, unResolvedList, fromClonedRepo);

            return resolvedList;
        }

        static List<string> GetAllMatchingFilesFromS3DFilesList(List<string> searchStrings, string additionalIncludeDirectories = "")
        {
            List<string> allMatchingFiles = new List<string>();

            foreach (string searchString in searchStrings)
            {
                // In a synchronous method
                Task<List<string>> task = GetFilePathFromS3DFilesList(searchString);
                List<string> matchingFiles = Task.Run(() => task).GetAwaiter().GetResult();
                //List<string> matchingFiles = await GetFilePathFromS3DFilesList(searchString).GetAwaiter().GetResult();
                if (additionalIncludeDirectories != "")
                {
                    if (matchingFiles.Count > 1)
                    {
                        matchingFiles = ComapareAndRemoveFromAdditionalIncludeDirectories(matchingFiles, additionalIncludeDirectories);
                    }
                }
                allMatchingFiles.AddRange(matchingFiles);
            }

            return allMatchingFiles;
        }

        private static List<string> ComapareAndRemoveFromAdditionalIncludeDirectories(List<string> matchingFiles, string additionalIncludeDirectories)
        {
            List<string> includeDirectories = additionalIncludeDirectories.Split(';').ToList();
            List<string> matchingFilesInIncludeDirectories = new List<string>();

            foreach (string filePath in matchingFiles)
            {
                string directoryPath = Path.GetDirectoryName(filePath);
                string fileName = Path.GetFileName(filePath);

                foreach (string includeDir in includeDirectories)
                {
                    if (directoryPath.Equals(includeDir, StringComparison.OrdinalIgnoreCase))
                    {
                        matchingFilesInIncludeDirectories.Add(filePath);
                        break;
                    }
                }
            }

            return matchingFilesInIncludeDirectories;
        }

        static async Task ProcessFilesAsync(string inputFilePath, string outputDirectory)
        {
            var virtualDriveFiles = new Dictionary<string, StringBuilder>();
            string[] lines = await File.ReadAllLinesAsync(inputFilePath);

            foreach (string line in lines)
            {
                if (line.StartsWith("Filename:", StringComparison.OrdinalIgnoreCase))
                {
                    string path = line.Substring("Filename:".Length).Trim();
                    string virtualDrive = GetReplacedVirtualDriveLetter(path);

                    if (virtualDrive == string.Empty)
                        continue;
                    if (!virtualDriveFiles.TryGetValue(virtualDrive, out var stringBuilder))
                    {
                        stringBuilder = new StringBuilder();
                        virtualDriveFiles[virtualDrive] = stringBuilder;
                    }

                    stringBuilder.AppendLine(path);
                }
            }

            foreach (var (virtualDrive, stringBuilder) in virtualDriveFiles)
            {
                string virtualDriveFilePath = Path.Combine(outputDirectory, $"AllFilesInS3D{virtualDrive}.txt");
                await File.WriteAllTextAsync(virtualDriveFilePath, stringBuilder.ToString());
            }
        }





        static async Task ReadFilesAsync(string directoryPath)
        {
            string[] files = Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories);

            foreach (string file in files)
            {
                string contents = await File.ReadAllTextAsync(file);
                fileContents[file] = contents.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).ToList();
            }
        }


        static async Task<List<string>> GetFilePathFromS3DFilesList(string searchString)
        {
            try
            {
                //await ReadFilesAsync(m_AllS3DDirectoriesFilePath);

                List<string> matchingFiles = await SearchMatchingFilesAsync(searchString);

                if (matchingFiles.Count > 0)
                {
                    Console.WriteLine("Multiple files found with the given search string:");
                    foreach (string file in matchingFiles)
                    {
                        Console.WriteLine(file);
                    }
                    return matchingFiles;
                }
                else
                {
                    Console.WriteLine($"No matching file found for the given search string ->{searchString}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }
            return new List<string>();
        }

        static async Task<List<string>> SearchMatchingFilesAsync(string searchString)
        {
            var tasks = new List<Task<List<string>>>();

            foreach (var kvp in fileContents)
            {
                tasks.Add(Task.Run(() => SearchInFile(kvp.Key, kvp.Value, searchString)));
            }

            List<string> matchingFiles = new List<string>();

            while (tasks.Count > 0)
            {
                Task<List<string>> completedTask = await Task.WhenAny(tasks);
                tasks.Remove(completedTask);
                List<string> files = await completedTask;

                if (files.Count > 0)
                {
                    matchingFiles.AddRange(files);
                }
            }

            return matchingFiles;
        }

        /// <summary>
        /// This searches and stops after first file found
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="updatedParsedIdlFilePaths"></param>
        /// <param name="folder"></param>
        /// <param name="filtersXMLPath"></param>
        //static string SearchInFile(string filePath, List<string> lines, string searchString)
        //{
        //    foreach (string line in lines)
        //    {
        //        if (line.Contains(searchString, StringComparison.OrdinalIgnoreCase))
        //        {
        //            return filePath;
        //        }
        //    }

        //    return null;
        //}

        static List<string> SearchInFile(string filePath, List<string> lines, string searchString)
        {
            List<string> matchingFiles = new List<string>();

            foreach (string line in lines)
            {
                string fileName = Path.GetFileName(line);
                if (fileName != null)
                {
                    if (searchString.Contains("\\"))
                    {
                        searchString = Path.GetFileName(searchString);
                    }
                    if (fileName.Equals(searchString, StringComparison.OrdinalIgnoreCase))
                    {
                        matchingFiles.Add(line);
                        // If you don't want to continue searching in the same file after a match, you can break here.
                        // break;
                    }
                }
            }

            return matchingFiles;
        }

        #endregion

        #region XML related APIs

        public static void UpdateTheXmlAttributeDependenciesPath(string filePath, List<string> updatedParsedIdlFilePaths, string folder, string filtersXMLPath = "")
        {
            //Update the XML attribute with IDL path information asynchronously
            if (filtersXMLPath == "")
                filtersXMLPath = m_FilesListXMLPath;

            if (System.IO.File.Exists(filtersXMLPath))
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.Load(filtersXMLPath);

                string dependencyFiles = string.Empty;
                foreach (var file in updatedParsedIdlFilePaths)
                {
                    dependencyFiles = dependencyFiles + file + ";";
                }
                //Utilities.AppendNewAttribute(xmlDoc, m_selectedFilterPath.Replace("\\", "_") + "/FilePath", "IDL", string.Join(";", m_DependencyList));
                UpdateTheXmlAttribute(xmlDoc, folder.Replace("\\", "_") + "/filePath", "Name", filePath, "Dependency", string.Join(";", dependencyFiles));
                Utilities.SaveXmlToFile(xmlDoc, filtersXMLPath);
            }
        }

        public static void SaveXmlToFile(XmlDocument xmlDoc, string filePath)
        {
            try
            {
                xmlDoc.Save(filePath);
                //Console.WriteLine("XML file updated successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error saving XML: " + ex.Message);
            }
        }

        public static void UpdateTheXmlAttribute(XmlDocument xmlDoc, string elementName, string attributeNameToSearch, string attributeValueToSearch, string attributeNameToUpdate, string attributeValueToUpdate)
        {
            try
            {
                string searchPath = attributeValueToSearch.Replace(@"\\", @"\");
                // Get the elements with the specified name and attribute value
                //XmlNodeList filterNodes = xmlDoc.DocumentElement.SelectNodes($"//{elementName}[@{attributeNameToSearch}='{searchPath}']");

                XmlNode xmlNode = xmlDoc.SelectSingleNode("//filepath[@Name='" + attributeNameToUpdate + "']");

                if(xmlNode != null )
                {
                    var xmlElement = xmlNode as XmlElement;
                    if(xmlElement != null )
                    {
                        XmlAttribute xmlAttribute = xmlElement.GetAttributeNode("Dependency");
                        if(xmlAttribute == null)
                        {
                            xmlElement.SetAttribute("Dependency", attributeValueToUpdate);
                        }
                        else
                            xmlAttribute.Value = attributeValueToUpdate;
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
                Console.WriteLine("Error updating attribute in XML: " + ex.Message);
            }
        }

        //public static void UpdateTheXmlAttribute(XmlDocument xmlDoc, string elementName, string attributeNameToSearch, string attributeValueToSearch, string attributeNameToUpdate, string attributeValueToUpdate)
        //{
        //    try
        //    {
        //        // Create a namespace manager to handle namespaces
        //        XmlNamespaceManager nsmgr = new XmlNamespaceManager(xmlDoc.NameTable);
        //        nsmgr.AddNamespace("ns", "http://schemas.microsoft.com/developer/msbuild/2003");

        //        // Get the elements with the specified name and attribute value
        //        XmlNodeList filterNodes = xmlDoc.DocumentElement.SelectNodes($"//ns:{elementName}[@{attributeNameToSearch}='{attributeValueToSearch}']", nsmgr);

        //        foreach (XmlElement element in filterNodes)
        //        {
        //            string currentValue = element.GetAttribute(attributeNameToUpdate);
        //            string updatedValue;
        //            if (currentValue != attributeValueToUpdate && currentValue != string.Empty)
        //            {
        //                updatedValue = currentValue + attributeValueToUpdate;
        //            }
        //            else
        //                updatedValue = attributeValueToUpdate;

        //            // Update the attribute value
        //            element.SetAttribute(attributeNameToUpdate, updatedValue);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine("Error updating attribute in XML: " + ex.Message);
        //    }
        //}
        //Need a way to update the folders data to xml either to get it from .pat file or manually check and update
        public static List<string> GetS3DFoldersDataFromXML(string XMLSDirectoryPath)
        {

            List<string> s3dFoldersDataList = new List<string>();
            if (File.Exists(m_FiltersXMLPath))
            {
                try
                {
                    s3dFoldersDataList = GetXmlData(m_FiltersXMLPath, "DATA/FILTERS", "Name");
                }
                catch (Exception ex)
                {
                    throw new Exception("Failed to GetS3DFoldersDataFromXML with exception: " + ex.Message);
                }
            }
            return s3dFoldersDataList;
        }

        public static List<string> GetXmlData(string xml, string node, string attribute)
        {
            List<string> xmlData = new List<string>();
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                if (File.Exists(xml))
                {
                    xmlDoc.Load(xml);

                    XmlNode xmlNode = xmlDoc.SelectSingleNode(node);
                    XmlNodeList xmlNodeList = xmlNode.ChildNodes;

                    foreach (XmlNode filterNode in xmlNodeList)
                    {
                        string filterName = filterNode.Attributes[attribute].InnerXml;
                        xmlData.Add(filterName);
                    }
                }
                else
                {
                    throw new Exception("Xml file not found..!");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to GetXmlData with exception: " + ex.Message);
            }
            return xmlData;
        }

        private static void GetFilesDataAndDependenciesWriteToXml(List<string> s3dFoldersDataList, string clonedRepo)
        {
            foreach (var folder in s3dFoldersDataList)
            {
                Stopwatch stopwatch = Stopwatch.StartNew();
                List<string> filesUnderSelectedRoot = GetFilesDataAndWriteToXml(folder, clonedRepo);
                GetDependenciesOfFilesList(folder, filesUnderSelectedRoot);
                stopwatch.Stop();
                TimeSpan elapsed = stopwatch.Elapsed;
                Console.WriteLine("Elapsed Time: " + elapsed + "\n");
            }
        }

        public static List<string> FindPatternFilesInDirectory(string directoryPath, string searchPattern)
        {
            List<string> foundFilesList = new List<string>();
            try
            {
                List<string> subdirectories = Directory.GetDirectories(directoryPath, "*", SearchOption.AllDirectories).ToList();
                subdirectories.Add(directoryPath); // Include the main directory

                Parallel.ForEach(subdirectories, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, subdirectory =>
                {
                    string[] foundFiles = Directory.GetFiles(subdirectory, searchPattern);
                    foundFilesList.AddRange(foundFiles);
                });
            }
            catch (Exception ex)
            {
                throw new Exception("Unable to fetch the files from the location: " + directoryPath + " with exception: " + ex.ToString());
            }
            return foundFilesList;
        }

        public static List<string> GetFilesDataAndWriteToXml(string folder, string clonedRepo, string xmlDirectoryPath = "")
        {
            List<string> filesUnderSelectedRoot = new List<string>();
            try
            {
                if (xmlDirectoryPath == "")
                {
                    xmlDirectoryPath = m_XMLSDirectoryPath;
                }

                Console.WriteLine("Folder: " + folder);
                filesUnderSelectedRoot = FindPatternFilesInDirectory(clonedRepo + folder, "*.*");

                bool ifXMLFileExist = File.Exists(xmlDirectoryPath + @"\FilesList.xml");

                if (!ifXMLFileExist)
                    WriteListToFilesListXml(filesUnderSelectedRoot, xmlDirectoryPath + @"\FilesList.xml", "filtersdata", folder.Replace("\\", "_"), "filepath", true);
                else
                {
                    UpdateXmlWithData(filesUnderSelectedRoot, xmlDirectoryPath + @"\FilesList.xml", folder.Replace("\\", "_"), "FilePath", true);
                }
                Console.WriteLine("Identifying the " + folder + " dependencies.");
                Console.WriteLine("-------------------------------------------------------------");
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to GetFilesDataAndWriteToXml with exception " + ex.Message);
            }
            return filesUnderSelectedRoot;
        }

        public static void UpdateXmlWithData(List<string> stringsList, string filePath, string rootElementName, string stringElementName, bool update = true)
        {
            try
            {
                //Remove the already exiting nodes
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(filePath);

                XmlNode nodeToRemove = xmlDoc.SelectSingleNode("//" + rootElementName);
                if (nodeToRemove != null)
                {
                    xmlDoc.DocumentElement.RemoveChild(nodeToRemove);
                }

                XmlElement rootElement = xmlDoc.DocumentElement;

                XmlElement newElement = xmlDoc.CreateElement(rootElementName);

                // Add the new data to the XML
                foreach (var entry in stringsList)
                {
                    XmlElement element = xmlDoc.CreateElement("FilePath");
                    element.SetAttribute("Name", entry);
                    newElement.AppendChild(element);
                }
                rootElement.AppendChild(newElement);
                xmlDoc.Save(filePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error updating XML: " + ex.Message);
                throw new Exception("Failed to UpdateXmlWithData with exception: " + ex.Message);
            }
        }

        //public static void CreateOrUpdateListXml(List<string> stringsList, string filePath, string parentNode, string rootElementName, string currentElementName)
        //{
        //    try
        //    {
        //        // Load existing XML document if it exists, otherwise create a new one
        //        XmlDocument xmlDoc = new XmlDocument();
        //        if (File.Exists(filePath))
        //        {
        //            xmlDoc.Load(filePath);
        //        }
        //        else
        //        {
        //            XmlDeclaration xmlDeclaration = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", null);
        //            XmlElement root = xmlDoc.DocumentElement;
        //            xmlDoc.InsertBefore(xmlDeclaration, root);
        //        }

        //        // Get the root element
        //        XmlElement rootElement = xmlDoc.SelectSingleNode($"//{parentNode}/{rootElementName}") as XmlElement;
        //        if (rootElement == null)
        //        {
        //            rootElement = xmlDoc.CreateElement(rootElementName);
        //            xmlDoc.SelectSingleNode($"//{parentNode}").AppendChild(rootElement);
        //        }

        //        if (currentElementName != "")
        //        {
        //            // Process each string
        //            foreach (string str in stringsList)
        //            {
        //                // Check if the element already exists
        //                XmlElement existingElement = rootElement.SelectSingleNode($"{currentElementName}[@Name='{str}']") as XmlElement;

        //                if (existingElement == null)
        //                {
        //                    // Create a new element and add it to the root
        //                    XmlElement newElement = xmlDoc.CreateElement(currentElementName);
        //                    newElement.SetAttribute("Name", str);
        //                    rootElement.AppendChild(newElement);
        //                }
        //                // If the element exists, you can choose to do something here

        //            }
        //        }

        //        // Save the updated XML
        //        xmlDoc.Save(filePath);

        //        Console.WriteLine("XML file updated/created successfully.");
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine("Error updating/creating XML file: " + ex.Message);
        //        throw new Exception("Failed to UpdateListInFilesListXml with exception: " + ex.Message);
        //    }
        //}

        public static void CreateOrUpdateListXml(List<string> stringsList, string filePath, string parentNode, string rootElementName, string currentElementName)
        {
            if(rootElementName == "sroot_civil")
            {
                //
            }
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
                        // Check if the element already exists
                        XmlElement existingElement = rootElement.SelectSingleNode($"{currentElementName}[@Name='{str}']") as XmlElement;

                        if (existingElement == null)
                        {
                            // Create a new element and add it to the root
                            XmlElement newElement = xmlDoc.CreateElement(currentElementName);
                            newElement.SetAttribute("Name", str);
                            if(File.Exists(str))
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

                Console.WriteLine("XML file updated/created successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error updating/creating XML file: " + ex.Message);
                throw new Exception("Failed to UpdateListInFilesListXml with exception: " + ex.Message);
            }
        }



        public static void WriteListToFilesListXml(List<string> stringsList, string filePath, string parentNode, string rootElementName, string currentElementName, bool update = true)
        {
            try
            {
                // Create a new XML writer and set its settings
                XmlWriterSettings settings = new XmlWriterSettings
                {
                    Indent = true, // To format the XML with indentation
                    IndentChars = "    " // Specify the indentation characters (four spaces in this case)
                };

                using (XmlWriter writer = XmlWriter.Create(filePath, settings))
                {
                    // Write the XML declaration
                    writer.WriteStartDocument();

                    // Write the root element with the specified name
                    writer.WriteStartElement(parentNode);
                    writer.WriteStartElement(rootElementName);

                    // Write each string as a separate element with the specified name
                    foreach (string str in stringsList)
                    {
                        writer.WriteStartElement(currentElementName);
                        writer.WriteAttributeString("Name", str);
                        writer.WriteEndElement();
                    }

                    // Close the root element
                    writer.WriteEndElement();

                    // End the XML document
                    writer.WriteEndDocument();
                }

                Console.WriteLine("XML file created successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error writing XML file: " + ex.Message);
                throw new Exception("Failed to WriteListToXml with exception: " + ex.Message);
            }
        }

        #endregion

        #region Generate Data from the .pat file

        //Get Filters Data into Res.xml


        /// <summary>
        /// Assumptions: The patcher file will be available at X:\BldTools\
        /// The Output location is "D:\\Tools\\GIT\\S3DDependencyIdentifierTool\\ResourceFiles\\"; for now..
        /// It will create list of txt files with the files available in S3D getting from the .pat file data
        /// </summary>
        /// <returns></returns>
        public static async Task GenerateAllS3DFilesListAndFiltersListFromPatFile()
        {
            try
            {
                var virtualDriveFiles = new Dictionary<string, StringBuilder>();
                List<string> patcherDataLines = File.ReadAllLines(m_PatcherFilePath).ToList();

                List<string> filtersDataList = new List<string>();

                foreach (string line in patcherDataLines)
                {
                    if (line.StartsWith("Filename:", StringComparison.OrdinalIgnoreCase))
                    {
                        string path = line.Substring("Filename:".Length).Trim();
                        string virtualDrive = GetReplacedVirtualDriveLetter(path);
                        if (virtualDrive == string.Empty)
                            continue;
                        path = ChangeToClonedPathFromVirtual(path);
                        if (path == string.Empty)
                            continue;
                        
                        if (!virtualDriveFiles.TryGetValue(virtualDrive, out var stringBuilder))
                        {
                            stringBuilder = new StringBuilder();
                            virtualDriveFiles[virtualDrive] = stringBuilder;
                        }

                        stringBuilder.AppendLine(path);
                    }

                    //For creation of the filters xml
                    if (line.StartsWith("Filter:", StringComparison.OrdinalIgnoreCase))
                    {
                        string path = line.Substring("Filter:".Length).Trim();
                        string virtualDriveLetter = GetReplacedVirtualDriveLetter(path, forFilter:true);
                        if (virtualDriveLetter == string.Empty)
                            continue;
                        filtersDataList.Add(virtualDriveLetter + "_" + path.Substring(path.LastIndexOf("\\") + 1).Trim());
                    }
                }
                m_CachedFiltersData = filtersDataList;
                CreateOrUpdateListXml(filtersDataList, m_FiltersXMLPath, "data", "filters", "filter");

                foreach (var (virtualDrive, stringBuilder) in virtualDriveFiles)
                {
                    string virtualDriveFilePath = Path.Combine(m_AllS3DDirectoriesFilePath, $"AllFilesInS3D{GetRootLetter(virtualDrive)}root.txt");
                    List<string> cachedRootList = DepIdentifierUtils.GetSpecificCachedRootList(virtualDrive + "root");
                    cachedRootList = ConvertToStringList(stringBuilder.ToString());

                    if(File.Exists(virtualDriveFilePath))
                    {
                        File.Delete(virtualDriveFilePath);
                    }
                    await File.WriteAllTextAsync(virtualDriveFilePath, stringBuilder.ToString());
                }

                Console.WriteLine("Paths copied to respective files successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }
        }

        static List<string> ConvertToStringList(string data)
        {
            return data.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                       .ToList();
        }

        // Function to get the virtual drive from a given path
        static string GetReplacedVirtualDriveLetter(string path, bool forFilter = false)
        {
            int colonIndex = path.IndexOf(":");
            if (colonIndex >= 0)
            {
                if (path.Substring(0, colonIndex) == "c")
                    return string.Empty;

                if (!forFilter)
                    return @"g:\" + path.Substring(0, colonIndex) + "root\\";
                else
                    return path.Substring(0, colonIndex) + "root";
            }
            else
            {
                //throw new Exception($"ReplaceVirtualDriveLetter failed for {path}");
                return string.Empty;
            }
        }
        static string GetRootLetter(string path)
        {
            string root = Path.GetPathRoot(path);
            return (path.Substring(root.Length, 1));
        }

        // Function to get the virtual drive from a given path
        static string ChangeToClonedPathFromVirtual(string path)
        {

            if (path.StartsWith("g:", StringComparison.OrdinalIgnoreCase))
                return path;

            path = path.ToLower();
            int colonIndex = path.IndexOf(":");
            //int lastBackslashIndex = path.LastIndexOf('\\');
            if (colonIndex >= 0)
            {
                return @"g:\" + path.Substring(0, colonIndex) + "root" + path.Substring(colonIndex+1);
            }
            else
                return string.Empty;
        }


        //To create FilesList.xml
        public static void CreateFilesListTemplateXML()
        {
            if (ReversePatcher.cachedKrootFiles.Count < 0)
            {
                ReversePatcher.CacheAllRootFiles();
            }
            try
            {
                if (m_CachedFiltersData.Count == 0)
                {
                    MessageBox.Show("Filters Data is empty.");
                    throw new Exception("Filters Data is empty.");
                }
                else
                {
                    if (!File.Exists(m_FilesListXMLPath))
                    {
                        CreateFiltersInXML();

                    }

                    
                    foreach (var filterPath in m_CachedFiltersData)
                    {
                        if(filterPath == "sroot_civil")
                        {

                        }
                        //Get Filters Data from the Res file and later use it to add the xmlDirectoryPath
                        List<string> filesToAddInXML = ReversePatcher.GetAllFilesFromSelectedRoot(GetSpecificCachedRootList(filterPath), filterPath);
                        filesToAddInXML = FilterFilePathsByExtensions(filesToAddInXML, IncludedExtensions);

                        CreateOrUpdateListXml(filesToAddInXML, m_FilesListXMLPath, "filtersdata", filterPath.Replace("\\", "_"), "filepath");
                        
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Exception occurred while creating files list template xml");
            }
        }

        public static List<string> FilterFilePathsByExtensions(List<string> filePaths, List<string> extensions)
        {
            return filePaths.Where(filePath => extensions.Any(ext => filePath.EndsWith(ext, StringComparison.OrdinalIgnoreCase)))
                            .ToList();
        }

        public static List<string> GetSpecificCachedRootList(string filterPath)
        {
            ReversePatcher.CacheAllRootFiles();

            if (filterPath.Contains("mroot"))
            {
                return ReversePatcher.GetCachedMrootFiles();
            }
            else if (filterPath.Contains("kroot"))
            {
                return ReversePatcher.GetCachedKrootFiles();
            }
            else if (filterPath.Contains("rroot"))
            {
                return ReversePatcher.GetCachedRrootFiles();
            }
            else if (filterPath.Contains("sroot"))
            {
                return ReversePatcher.GetCachedSrootFiles();
            }
            else if (filterPath.Contains("troot"))
            {
                return ReversePatcher.GetCachedTrootFiles();
            }
            else if (filterPath.Contains("xroot"))
            {
                return ReversePatcher.GetCachedXrootFiles();
            }
            else if (filterPath.Contains("yroot"))
            {
                return ReversePatcher.GetCachedYrootFiles();
            }
            else
            {
                // Return a default list or handle the case where no specific root matches
                return new List<string>();
            }
        }

        private static void CreateFiltersInXML()
        {
            if(m_CachedFiltersData.Count == 0)
            {
                MessageBox.Show("Filters Data is empty.");
                throw new Exception("Filters Data is empty.");
            }
            foreach (var filterPath in m_CachedFiltersData)
            {
                CreateOrUpdateListXml(new List<string>(), m_FilesListXMLPath, "filtersdata", filterPath.Replace("//", "_"), "");
            }
        }

        #endregion


    }
}
