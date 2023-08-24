//using System;
//using System.Collections.Concurrent;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Text;
//using System.Text.RegularExpressions;
//using System.Threading.Tasks;
//using System.Xml;
//using System.Xml.Linq;
////using NLog;
////using NLog.Config;
////using NLog.Targets;


//namespace DepIdentifier
//{
//    internal static class Utilities
//    {

//        //static string ChangeToClonedPathFromVirtual(string path, string parentFilePath = "")
//        //{

//        //    if (path.Contains("$(ClonedRepo)"))
//        //    {
//        //        path = path.Replace("$(ClonedRepo)", "g:\\");
//        //        if (path.Contains(".."))
//        //        {
//        //            string resolvedPath = Path.Combine(Path.GetDirectoryName(parentFilePath), path);
//        //            if (Directory.Exists(resolvedPath))
//        //            {
//        //                path = Path.GetFullPath(resolvedPath);
//        //            }
//        //        }
//        //        if (path.Contains("root\\"))
//        //            return path;
//        //    }

//        //    if (path.StartsWith("g:", StringComparison.OrdinalIgnoreCase))
//        //        return path;

//        //    //path = path.Replace("$(ClonedRepo)", "").ToLower();

//        //    int colonIndex = path.IndexOf(":");
//        //    //int lastBackslashIndex = path.LastIndexOf('\\');
//        //    if (colonIndex >= 0)
//        //    {
//        //        return @"g:\" + path.Substring(0, colonIndex) + "root" + path.Substring(colonIndex + 1);
//        //    }
//        //    else
//        //        return string.Empty;
//        //}

//        //public static List<string> ResolveFromLocalDirectoryOrPatcher(string projectFilePath, List<string> dependenciesList, bool fromPatcher = true, string additionalIncludeDirectories = "")
//        //{
//        //    List<string> resolvedList = new List<string>();
//        //    List<string> unResolvedList = new List<string>();
//        //    string localDirectory = Path.GetDirectoryName(projectFilePath);
//        //    foreach (var dependentFile in dependenciesList)
//        //    {
//        //        if (File.Exists(dependentFile))
//        //        {
//        //            if (dependentFile.StartsWith("g:", StringComparison.OrdinalIgnoreCase))
//        //            {
//        //                resolvedList.Add(dependentFile);
//        //                continue;
//        //            }
//        //            else
//        //                resolvedList.Add(ChangeToClonedPathFromVirtual(dependentFile, projectFilePath));
//        //        }
//        //        string combinedPath = Path.Combine(localDirectory, dependentFile.Replace("$(ClonedRepo)", "g:\\"));
//        //        if (File.Exists(combinedPath))
//        //        {
//        //            resolvedList.Add(combinedPath);
//        //            continue;
//        //        }

//        //        if (combinedPath.Contains(".."))
//        //        {
//        //            combinedPath = Path.GetFullPath(combinedPath);
//        //        }
//        //        if (File.Exists(combinedPath))
//        //        {
//        //            string clonedRepoPath = ChangeToClonedPathFromVirtual(combinedPath);
//        //            if (!String.IsNullOrEmpty(clonedRepoPath))
//        //                resolvedList.Add(clonedRepoPath);
//        //            else
//        //                DepIdentifierUtils.WriteTextInLog($"Unable to get cloned repo resolved path for {clonedRepoPath}");
//        //        }
//        //        else
//        //        {
//        //            if (fromPatcher)
//        //                unResolvedList.Add(dependentFile);
//        //            else
//        //                unResolvedList.Add(combinedPath);
//        //        }
//        //    }

//        //    if (fromPatcher)
//        //    {
//        //        List<string> resolvedListFromPatcher = GetAllMatchingFilesFromS3DFilesList(unResolvedList, additionalIncludeDirectories: additionalIncludeDirectories, projectFilePath: projectFilePath);
//        //        resolvedList.AddRange(resolvedListFromPatcher.ToList());
//        //    }

//        //    if (fromPatcher == false && unResolvedList.Count > 0)
//        //    {
//        //        foreach (var unResolvedFilePath in unResolvedList)
//        //        {
//        //            DepIdentifierUtils.WriteTextInLog($"The {unResolvedFilePath} is not found which is used in the file: {projectFilePath}");
//        //        }
//        //    }

//        //    //ConcurrentBag<string> resolvedListFromPatcher = ResolveFileNamesFromPatcher(filePath, unResolvedList, fromClonedRepo);

//        //    return resolvedList;
//        //}

//        //static List<string> GetAllMatchingFilesFromS3DFilesList(List<string> searchStrings, string additionalIncludeDirectories = "", string projectFilePath = "")
//        //{
//        //    List<string> allMatchingFiles = new List<string>();

//        //    //foreach (string searchString in searchStrings)
//        //    //{
//        //    //    // In a synchronous method
//        //    //    Task<List<string>> task = GetFilePathFromS3DFilesList(searchString);
//        //    //    List<string> matchingFiles = Task.Run(() => task).GetAwaiter().GetResult();
//        //    //    //List<string> matchingFiles = await GetFilePathFromS3DFilesList(searchString).GetAwaiter().GetResult();
//        //    //    if (additionalIncludeDirectories != "")
//        //    //    {
//        //    //        if (matchingFiles.Count > 1)
//        //    //        {
//        //    //            matchingFiles = ComapareAndRemoveFromAdditionalIncludeDirectories(matchingFiles, additionalIncludeDirectories);
//        //    //        }
//        //    //    }
//        //    //    allMatchingFiles.AddRange(matchingFiles);
//        //    //}

//        //    XmlDocument xmlDocument = new XmlDocument();
//        //    xmlDocument.Load(DepIdentifierUtils.m_FilesListXMLPath);

//        //    XmlNodeList xmlNodeList;


//        //    foreach (string searchString in searchStrings)
//        //    {
//        //        xmlNodeList = xmlDocument.SelectNodes("//filepath[@ShortName='" + searchString.ToLower() + "']");

//        //        if (xmlNodeList != null)
//        //        {
//        //            long countOfMatchingFiles = xmlNodeList.Count;
//        //            if (countOfMatchingFiles == 1)
//        //            {
//        //                XmlElement xmlElement = xmlNodeList[0] as XmlElement;
//        //                allMatchingFiles.Add(xmlElement.GetAttribute("Name"));
//        //            }
//        //            else if (!string.IsNullOrEmpty(additionalIncludeDirectories))
//        //            {
//        //                List<string> additionalIncludeDirectoriesList = GeAdditionalDirectoriesInList(additionalIncludeDirectories, projectFilePath);

//        //                bool matchFound = false;
//        //                List<string> unresolvedMultipleFiles = new List<string>();
//        //                foreach (XmlElement xmlElement in xmlNodeList)
//        //                {
//        //                    string filePath = xmlElement.GetAttribute("Name");
//        //                    string resolvedPath = GetFileFromAdditionalIncludeDirectories(additionalIncludeDirectoriesList, filePath);
//        //                    if (!String.IsNullOrEmpty(resolvedPath))
//        //                    {
//        //                        allMatchingFiles.Add(filePath);
//        //                        matchFound = true;
//        //                        break;
//        //                    }
//        //                    else
//        //                        unresolvedMultipleFiles.Add(filePath);
//        //                }
//        //                if (matchFound == false)
//        //                {
//        //                    //MessageBox.Show("Issue");
//        //                    DepIdentifierUtils.WriteTextInLog("match not found and multiple files are identified as dependencies. ");
//        //                    if (unresolvedMultipleFiles.Count > 0)
//        //                    {
//        //                        foreach (var duplicateDep in unresolvedMultipleFiles)
//        //                        {
//        //                            DepIdentifierUtils.WriteTextInLog(duplicateDep);
//        //                        }
//        //                        allMatchingFiles.AddRange(unresolvedMultipleFiles);
//        //                    }
//        //                    else
//        //                    {
//        //                        DepIdentifierUtils.WriteTextInLog($"File Not in Patcher: The {searchString} is not found which is used in the file: {projectFilePath}");
//        //                    }
//        //                }
//        //            }
//        //            else
//        //            {
//        //                DepIdentifierUtils.WriteTextInLog($"Reference Issue: The {searchString} is not found which is used in the file: {projectFilePath}");
//        //                //The file which is not available is added as a dependecy.
//        //            }
//        //        }
//        //    }
//        //    return allMatchingFiles;
//        //}

//        //static string GetFileFromAdditionalIncludeDirectories(List<string> directoryPath, string filePath)
//        //{
//        //    foreach (var additionalIncludeDirectory in directoryPath)
//        //    {
//        //        string folderPathFromAdditionIncludeDir = Path.GetFullPath(additionalIncludeDirectory);
//        //        string filefolder = Path.GetDirectoryName(filePath);
//        //        if (string.Compare(folderPathFromAdditionIncludeDir, filefolder, StringComparison.OrdinalIgnoreCase) == 0)
//        //        {
//        //            return filePath;
//        //        }
//        //    }
//        //    return string.Empty;
//        //}


//        //private static List<string> GeAdditionalDirectoriesInList(string additionalIncludeDirectories, string projectFilePath = "")
//        //{
//        //    List<string> list = new List<string>();
//        //    if (!string.IsNullOrEmpty(additionalIncludeDirectories))
//        //    {
//        //        list = additionalIncludeDirectories.Split(";").ToList();

//        //        for (int i = 0; i < list.Count; i++)
//        //        {
//        //            list[i] = list[i].Replace("$(ClonedRepo)", "g:\\");
//        //            if (list[i].Contains("..") || list[i].Contains(".\\"))
//        //            {
//        //                string resolvedPath = Path.Combine(Path.GetDirectoryName(projectFilePath), list[i]);
//        //                if (Directory.Exists(resolvedPath))
//        //                {
//        //                    list[i] = Path.GetFullPath(resolvedPath);
//        //                }
//        //            }
//        //            if (list[i].Contains("root\\"))
//        //                continue;

//        //            list[i] = ChangeToClonedPathFromVirtual(list[i]);
//        //        }

//        //        //       .Select(list => list.Replace("$(ClonedRepo)", ""))
//        //        //       .Select(list => ChangeToClonedPathFromVirtual(list)).ToList();
//        //        list = list.Distinct().ToList();
//        //        list.RemoveAll(list => string.IsNullOrEmpty(list));
//        //    }
//        //    return list;
//        //}

//        //static Logger _logger = LogManager.GetCurrentClassLogger();
//        #region CODE1
//        /// <summary>
//        /// Takes the list of file names and it gets the filepath of those from s3dpatcher.
//        /// </summary>
//        /// <param name="fileNames"> The list of file names for which the filepath is required. </param>
//        /// <returns>List of the filepaths of the filenames provided</returns>
//        public static ConcurrentBag<string> ResolveFileNamesFromPatcher(List<string> fileNames)
//        {
//            // Use ConcurrentBag to collect matching lines
//            var filePaths = new ConcurrentBag<string>();
//            var failedFilePaths = new ConcurrentBag<string>();
//            try
//            { 
//                string prefixToRemove = "Filename:";
//                // Read the file asynchronously in parallel
//                Parallel.ForEach(fileNames, eachfileNameFromFileNames =>
//                {
//                    bool found = false;
//                    foreach (string line in File.ReadLines("X:\\Bldtools\\s3dpatcher.pat"))
//                    {
//                        if (line.IndexOf("\\" + eachfileNameFromFileNames, StringComparison.OrdinalIgnoreCase) >= 0)
//                        {
//                            filePaths.Add(line.Replace(prefixToRemove, ""));
//                            found = true;
//                            break;
//                        }
//                    }

//                    if (!found)
//                    {
//                        failedFilePaths.Add($"The filepath for the file '{eachfileNameFromFileNames}' not found. Please review it.");
//                        //_logger.Error($"The filepath for the file '{eachfileNameFromFileNames}' not found. Please review it");
//                        //DepIdentifierUtils.WriteTextInLog($"The filepath for the file '{fileName}' not found. Please review it");
//                    }
//                });

//            }
//            catch (Exception ex)
//            {
//                DepIdentifierUtils.WriteTextInLog("Error occurred while ResolveFileNamesFromPatcher: " + ex.Message);
//            }

//            DepIdentifierUtils.WriteTextInLog(failedFilePaths.Distinct().ToList());
//            return filePaths;
//        }
//        #endregion

//        #region code2
//        ///// <summary>
//        ///// Takes the list of file names and it gets the filepath of those from s3dpatcher.
//        ///// </summary>
//        ///// <param name="fileNames"> The list of file names for which the filepath is required. </param>
//        ///// <returns>List of the filepaths of the filenames provided</returns>
//        //public static ConcurrentBag<string> ResolveFileNamesFromPatcher(List<string> fileNames, ref List<string> failedFilesList)
//        //{
//        //    // Use ConcurrentBag to collect matching lines
//        //    var filePaths = new ConcurrentBag<string>();
//        //    var failedFilePaths = new ConcurrentBag<string>();
//        //    try
//        //    {
//        //        string[] lines = File.ReadAllLines("X:\\Bldtools\\s3dpatcher.pat");
//        //        // Read the file asynchronously in parallel
//        //        Parallel.ForEach(fileNames, fileName =>
//        //        {
//        //            bool found = false;
//        //            foreach (string line in lines)
//        //            {
//        //                if (line.IndexOf("\\" + fileName, StringComparison.OrdinalIgnoreCase) >= 0)
//        //                {
//        //                    filePaths.Add(line.Replace("Filename:", ""));
//        //                    found = true;
//        //                    break;
//        //                }
//        //            }

//        //            if (!found)
//        //            {
//        //                failedFilePaths.Add($"The filepath for the file '{fileName}' not found. Please review it.");
//        //                _logger.Error($"The filepath for the file '{fileName}' not found. Please review it");
//        //            }
//        //        });

//        //    }
//        //    catch (Exception ex)
//        //    {
//        //        DepIdentifierUtils.WriteTextInLog("Error occurred while ResolveFileNamesFromPatcher: " + ex.Message);
//        //    }

//        //    failedFilesList.AddRange(failedFilePaths.Distinct().ToList());
//        //    return filePaths;
//        //}
//        #endregion

//        #region code3 9 sec.
//        //public static ConcurrentBag<string> ResolveFileNamesFromPatcher(List<string> fileNames, ref List<string> failedFilesList)
//        //{
//        //    var filePaths = new ConcurrentBag<string>();
//        //    var failedFilePaths = new ConcurrentBag<string>();

//        //    try
//        //    {
//        //        var fileNamePathsDict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

//        //        foreach (string line in File.ReadLines("X:\\Bldtools\\s3dpatcher.pat"))
//        //        {
//        //            string trimmedLine = line.Trim();
//        //            if (trimmedLine.StartsWith("Filename:", StringComparison.OrdinalIgnoreCase))
//        //            {
//        //                string filePath = trimmedLine.Substring(9).Trim();
//        //                string fileName = Path.GetFileName(filePath);
//        //                fileNamePathsDict[fileName] = filePath;
//        //            }
//        //        }

//        //        foreach (string fileName in fileNames)
//        //        {
//        //            if (fileNamePathsDict.TryGetValue(fileName, out string filePath))
//        //            {
//        //                filePaths.Add(filePath);
//        //            }
//        //            else
//        //            {
//        //                failedFilePaths.Add($"The filepath for the file '{fileName}' not found. Please review it.");
//        //                _logger.Error($"The filepath for the file '{fileName}' not found. Please review it");
//        //            }
//        //        }
//        //    }
//        //    catch (Exception ex)
//        //    {
//        //        DepIdentifierUtils.WriteTextInLog("Error occurred while ResolveFileNamesFromPatcher: " + ex.Message);
//        //    }

//        //    failedFilesList.AddRange(failedFilePaths.Distinct().ToList());
//        //    return filePaths;
//        //}
//        #endregion

//        #region code4 12 sec
//        //public static ConcurrentBag<string> ResolveFileNamesFromPatcher(List<string> fileNames, ref List<string> failedFilesList)
//        //{
//        //    var filePaths = new ConcurrentBag<string>();
//        //    var failedFilePaths = new ConcurrentBag<string>();

//        //    try
//        //    {
//        //        var fileNamePathsDict = new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);

//        //        Parallel.ForEach(File.ReadLines("X:\\Bldtools\\s3dpatcher.pat"), line =>
//        //        {
//        //            string trimmedLine = line.Trim();
//        //            if (trimmedLine.StartsWith("Filename:", StringComparison.OrdinalIgnoreCase))
//        //            {
//        //                string filePath = trimmedLine.Substring(9).Trim();
//        //                string fileName = Path.GetFileName(filePath);
//        //                fileNamePathsDict.TryAdd(fileName, filePath);
//        //            }
//        //        });

//        //        Parallel.ForEach(fileNames, fileName =>
//        //        {
//        //            if (fileNamePathsDict.TryGetValue(fileName, out string filePath))
//        //            {
//        //                filePaths.Add(filePath);
//        //            }
//        //            else
//        //            {
//        //                failedFilePaths.Add($"The filepath for the file '{fileName}' not found. Please review it.");
//        //                _logger.Error($"The filepath for the file '{fileName}' not found. Please review it");
//        //            }
//        //        });
//        //    }
//        //    catch (Exception ex)
//        //    {
//        //        DepIdentifierUtils.WriteTextInLog("Error occurred while ResolveFileNamesFromPatcher: " + ex.Message);
//        //    }

//        //    failedFilesList.AddRange(failedFilePaths.Distinct().ToList());
//        //    return filePaths;
//        //}

//        #endregion


//        //public static void WriteListToXml(List<string> stringsList, string filePath, string rootElementName, string stringElementName, bool update = true)
//        //{
//        //    try
//        //    {
//        //        // Create a new XML writer and set its settings
//        //        XmlWriterSettings settings = new XmlWriterSettings
//        //        {
//        //            Indent = true, // To format the XML with indentation
//        //            IndentChars = "    " // Specify the indentation characters (four spaces in this case)
//        //        };

//        //        using (XmlWriter writer = XmlWriter.Create(filePath, settings))
//        //        {
//        //            // Write the XML declaration
//        //            writer.WriteStartDocument();

//        //            // Write the root element with the specified name
//        //            writer.WriteStartElement("FiltersData");
//        //            writer.WriteStartElement(rootElementName);

//        //            // Write each string as a separate element with the specified name
//        //            foreach (string str in stringsList)
//        //            {
//        //                writer.WriteStartElement("FilePath");
//        //                writer.WriteAttributeString("Name", str);
//        //                writer.WriteEndElement();
//        //            }

//        //            // Close the root element
//        //            writer.WriteEndElement();

//        //            // End the XML document
//        //            writer.WriteEndDocument();
//        //        }

//        //        DepIdentifierUtils.WriteTextInLog("XML file created successfully.");
//        //    }
//        //    catch (Exception ex)
//        //    {
//        //        DepIdentifierUtils.WriteTextInLog("Error writing XML file: " + ex.Message);
//        //    }
//        //}

//        ///// <summary>
//        ///// This API is useful for getting the AdditionalIncludeDirectories from the vcxproj
//        ///// </summary>
//        ///// <param name="filePath"></param>
//        ///// <returns>List of additional directories included</returns>
//        //public static List<string> ExtractAdditionalDirectoriesFromVcxproj(string filePath)
//        //{
//        //    List<string> additionalDirectories = new List<string>();
//        //    try
//        //    {
//        //        XmlDocument xmlDoc = new XmlDocument();
//        //        xmlDoc.Load(filePath);

//        //        XmlNamespaceManager namespaceManager = new XmlNamespaceManager(xmlDoc.NameTable);
//        //        namespaceManager.AddNamespace("ns", "http://schemas.microsoft.com/developer/msbuild/2003");

//        //        XmlNodeList itemNodes = xmlDoc.SelectNodes("//ns:ClCompile/ns:AdditionalIncludeDirectories", namespaceManager);

//        //        foreach (XmlNode itemNode in itemNodes)
//        //        {
//        //            string includeAttribute = itemNode.InnerText;
//        //            additionalDirectories.AddRange(includeAttribute.Split(';'));
//        //        }
//        //    }
//        //    catch (Exception ex)
//        //    {
//        //        DepIdentifierUtils.WriteTextInLog("Error occurred while extracting additional directories: " + ex.Message);
//        //    }

//        //    return additionalDirectories.Distinct().ToList();
//        //}

//        ///// <summary>
//        ///// This API is useful for getting the Item Group values from the vcxproj
//        ///// </summary>
//        ///// <param name="filePath">path of the Vcxproj file</param>
//        ///// <param name="elementName">The Tag name of the item group which we need to get</param>
//        ///// <returns>List of included values in the specified  Item group Tags</returns>
//        //public static List<string> ExtractItemGroupValuesFromVcxProj(string filePath, string elementName, bool excludeLocalizationProjects = false)
//        //{
//        //    List<string> includedFiles = new List<string>();

//        //    try
//        //    {
//        //        XmlDocument xmlDoc = new XmlDocument();
//        //        if (excludeLocalizationProjects && filePath.Contains("409"))
//        //            return includedFiles;
//        //        xmlDoc.Load(filePath);

//        //        XmlNamespaceManager namespaceManager = new XmlNamespaceManager(xmlDoc.NameTable);
//        //        namespaceManager.AddNamespace("ns", "http://schemas.microsoft.com/developer/msbuild/2003");

//        //        XmlNodeList itemNodes = xmlDoc.SelectNodes("//ns:ItemGroup/ns:" + elementName, namespaceManager);

//        //        foreach (XmlNode itemNode in itemNodes)
//        //        {
//        //            string includeAttribute = itemNode.Attributes["Include"].Value;
//        //            if(includeAttribute.Contains("ClonedRepo"))
//        //            {
//        //                string clonedRepo = GetClonedRepo();
//        //                includeAttribute = includeAttribute.Replace("$(ClonedRepo)", clonedRepo);
//        //            }
//        //            includedFiles.Add(includeAttribute);
//        //        }
//        //    }
//        //    catch (Exception ex)
//        //    {
//        //        DepIdentifierUtils.WriteTextInLog("Error occurred while extracting ItemGroups: " + ex.Message);
//        //    }

//        //    return includedFiles.Distinct().ToList();
//        //}

//        //public static string GetClonedRepo()
//        //{
//        //    return "g:\\";
//        //    //// Load the .props file and get the value of the variable
//        //    //XDocument propsFile = XDocument.Load(@"X:\Bldtools\PropertySheets\SP3D.Release.Win32.props");
//        //    //XNamespace ns = "http://schemas.microsoft.com/developer/msbuild/2003";
//        //    //string myPathVariable = propsFile.Descendants(ns + "ClonedRepo").FirstOrDefault()?.Value;

//        //    //return myPathVariable;
//        //}

//        ///// <summary>
//        ///// This API identifies all the included and the imported statements in a particular file and returns them
//        ///// </summary>
//        ///// <param name="filePath">path of the Vcxproj file</param>
//        ///// <returns>Returns all the included or imported files</returns>
//        //public static List<string> GetImportedFiles(string filePath)
//        //{
//        //    List<string> importedFiles = new List<string>();
//        //    try
//        //    {
//        //        string fileContent = File.ReadAllText(filePath);

//        //        // Regular expression pattern to match import/include statements
//        //        string pattern = @"(?:#include\s+|import\s+)[\""\<]([^\""\<]+)[\""\<]";

//        //        // Match the pattern in the file content
//        //        MatchCollection matches = Regex.Matches(fileContent, pattern, RegexOptions.IgnoreCase);

//        //        // Iterate through the matches and extract the imported files
//        //        foreach (Match match in matches)
//        //        {
//        //            string importedFile = match.Groups[1].Value;
//        //            importedFiles.Add(importedFile);
//        //        }

//        //    }
//        //    catch(Exception ex)
//        //    {
//        //        DepIdentifierUtils.WriteTextInLog("Error occurred while getting Imported files: " + ex.Message);
//        //    }
//        //    return importedFiles.Distinct().ToList();
//        //}

//        //public static List<string> ExtractImportedFilesAndResolvePathsFromFile(string fileName)
//        //{
//        //    List<string> importedFiles = GetImportedFiles(fileName);
//        //    //DisplayList(importedFiles, "Imported files:");

//        //    //Resolved Imported files
//        //    ConcurrentBag<string> idlFilePaths = ResolveFileNamesFromPatcher(importedFiles);
//        //    return idlFilePaths.ToList();
//        //}

//        ///// <summary>
//        ///// This API is used to fetch all the related files from the directory with the given search pattern
//        ///// </summary>
//        ///// <param name="directoryPath"></param>
//        ///// <returns></returns>
//        ///// <exception cref="Exception"></exception>
//        //public static List<string> FindPatternFilesInDirectory(string directoryPath, string searchPattern)
//        //{
//        //    List<string> foundFilesList = new List<string>();
//        //    try
//        //    {
//        //        ConcurrentBag<string> foundFilePaths = new ConcurrentBag<string>();
//        //        // Get all subdirectories recursively
//        //        string[] subdirectories = Directory.GetDirectories(directoryPath, "*", SearchOption.AllDirectories);

//        //        Parallel.ForEach(subdirectories, new ParallelOptions { MaxDegreeOfParallelism = 24 }, subdirectory =>
//        //        {
//        //            // Get all .vcxproj files in the subdirectory
//        //            string[] foundFiles = Directory.GetFiles(subdirectory, searchPattern);

//        //            // Add the paths of .vcxproj files to the list
//        //            foreach (string file in foundFiles)
//        //            {
//        //                foundFilePaths.Add(file);
//        //            }
//        //        });

//        //        foundFilesList = foundFilePaths.ToList();

//        //        string[] foundFilesFromDirectory = Directory.GetFiles(directoryPath, searchPattern);
//        //        foundFilesList.AddRange(foundFilesFromDirectory);
//        //    }
//        //    catch (Exception ex)
//        //    {
//        //        throw new Exception("Unable to fetch the files from the location: " + directoryPath + " with exception: " + ex.ToString());
//        //    }
//        //    return foundFilesList;
//        //}


//        //public static void UpdateTheXmlAttribute(XmlDocument xmlDoc, string elementName, string attributeNameToSearch, string attributeValueToSearch, string attributeNameToUpdate, string attributeValueToUpdate)
//        //{
//        //    try
//        //    {
//        //        // Get the elements with the specified name and attribute value
//        //        XmlNodeList filterNodes = xmlDoc.DocumentElement.SelectNodes($"//{elementName}[@{attributeNameToSearch}='{attributeValueToSearch}']");

//        //        foreach (XmlElement element in filterNodes)
//        //        {
//        //            // Update the attribute value
//        //            element.SetAttribute(attributeNameToUpdate, attributeValueToUpdate);
//        //        }
//        //    }
//        //    catch (Exception ex)
//        //    {
//        //        DepIdentifierUtils.WriteTextInLog("Error updating attribute in XML: " + ex.Message);
//        //    }
//        //}

//        //public static async Task UpdateTheXmlAttribute(XmlDocument xmlDoc, string elementName, string attributeNameToSearch, string attributeValueToSearch, string attributeNameToUpdate, string attributeValueToUpdate)
//        //{
//        //    try
//        //    {
//        //        await AsyncFileLock.LockAsync(ReversePatcher.m_XMLSFilesListResourceFileDirectoryPath);

//        //        // Get the elements with the specified name and attribute value
//        //        XmlNodeList filterNodes = xmlDoc.DocumentElement.SelectNodes($"//{elementName}[@{attributeNameToSearch}='{attributeValueToSearch}']");

//        //        foreach (XmlElement element in filterNodes)
//        //        {
//        //            // Update the attribute value asynchronously
//        //            await Task.Run(() => element.SetAttribute(attributeNameToUpdate, attributeValueToUpdate));
//        //        }
//        //    }
//        //    catch (Exception ex)
//        //    {
//        //        DepIdentifierUtils.WriteTextInLog("Error updating attribute in XML: " + ex.Message);
//        //    }
//        //    finally
//        //    {
//        //        AsyncFileLock.Unlock(ReversePatcher.m_XMLSFilesListResourceFileDirectoryPath);
//        //    }
//        //}




//        //public static void AppendNewAttribute(XmlDocument xmlDoc, string elementName, string attributeName, string attributeValue)
//        //{
//        //    try
//        //    {
//        //        // Get the element with the specified name
//        //        XmlElement element = xmlDoc.DocumentElement.SelectSingleNode("//" + elementName) as XmlElement;

//        //        if (element != null)
//        //        {
//        //            // Check if the attribute already exists
//        //            if (element.HasAttribute(attributeName))
//        //            {
//        //                // Update the existing attribute value
//        //                element.SetAttribute(attributeName, attributeValue);
//        //            }
//        //            else
//        //            {
//        //                // Append a new attribute to the element
//        //                XmlAttribute newAttribute = xmlDoc.CreateAttribute(attributeName);
//        //                newAttribute.Value = attributeValue;
//        //                element.Attributes.Append(newAttribute);
//        //            }
//        //        }
//        //    }
//        //    catch (Exception ex)
//        //    {
//        //        DepIdentifierUtils.WriteTextInLog("Error appending a new attribute to XML: " + ex.Message);
//        //    }
//        //}

//        //public static void SaveXmlToFile(XmlDocument xmlDoc, string filePath)
//        //{
//        //    try
//        //    {
//        //        xmlDoc.Save(filePath);
//        //        DepIdentifierUtils.WriteTextInLog("XML file updated successfully.");
//        //    }
//        //    catch (Exception ex)
//        //    {
//        //        DepIdentifierUtils.WriteTextInLog("Error saving XML: " + ex.Message);
//        //    }
//        //}

//        //public static string GetNameAttributeValue(XmlDocument xmlDoc, string elementName, string attributeNameToSearch, string attributeValueToSearch)
//        //{
//        //    try
//        //    {
//        //        // Get the elements with the specified name and attribute value
//        //        XmlNodeList filterNodes = xmlDoc.DocumentElement.SelectNodes(
//        //            $"//{elementName}[@{attributeNameToSearch}='{attributeValueToSearch}']");

//        //        // Use StringComparison.Ordinal to perform case-sensitive comparison
//        //        if (filterNodes.Count > 0)
//        //        {
//        //            // Get the "Dependency" attribute value of the first matching element
//        //            XmlElement element = (XmlElement)filterNodes[0];
//        //            string dependencyValue = element.GetAttribute("Dependency");
//        //            return dependencyValue;
//        //        }
//        //        else
//        //        {
//        //            return null;
//        //        }
//        //    }
//        //    catch (Exception ex)
//        //    {
//        //        DepIdentifierUtils.WriteTextInLog("Error extracting dependency value from XML: " + ex.Message);
//        //        return null;
//        //    }
//        //}
//    }
//}
