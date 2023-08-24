using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace DepIdentifier
{
    public static class DepIdentifierUtils
    {
        #region required memberVaraibles
        public static List<string> patcherDataLines = new List<string>();
        public static string m_PatcherFilePath = @"g:\xroot\bldtools\s3dpatcher.pat";
        public static string m_AllS3DDirectoriesFilePath = ReversePatcher.resourcePath;
        public static string m_FiltersXMLPath = ReversePatcher.resourcePath + "\\filtersdata.xml";
        public static string m_FilesListXMLPath = ReversePatcher.resourcePath + "\\filesList.xml";
        public static List<string> m_CachedFiltersData = new List<string>();

        public static List<string> Commonfiles = new List<string> { "oaidl.idl", "ocidl.idl", "atlbase.h", "atlcom.h", "statreg.h", "wtypes.idl", "comdef.h",
                                                                    "math.h", "initguid.h", "objbase.h", "share.h", "olectl.h", "oledb.h", "OLEDBERR.h",
                                                                    "activscp.h", "adoint.h", "afxdisp.h", "afxres.h", "afxwin.h", "assert.h", "ATLBASE.h",
                                                                    "atlcomcli.h", "atlconv.h", "atlctl.h", "atldbcli.h", "atldbsch.h", "atlpath.h", "atlsafe.h",
                                                                    "atlstr.h", "COMDEF.H", "comip.h", "comsvcs.h", "Comutil.h", "crtdbg.h", "ctype.h", "float.h",
                                                                    "guiddef.h", "INITGUID.H", "inttypes.h", "limits.h", "locale.h", "malloc.h", "Math.h", "msxml6.h",
                                                                    "oaidl.h", "Objbase.h", "ocidl.h", "ole2.h"};
        private static Dictionary<string, List<string>> fileContents = new Dictionary<string, List<string>>();
        #endregion
        private static string m_ClonedRepo = DepIdentifierUtils.GetClonedRepo();

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

        public static void WriteTextInLog(string textData)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(ReversePatcher.m_logFilePath, true))
                {
                    writer.WriteLine(textData);
                }
            }
            catch { }
        }

        public static void WriteTextInLog(List<string> textData)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(ReversePatcher.m_logFilePath, true))
                {
                    writer.WriteLine(string.Join(Environment.NewLine, textData));
                }
            }
            catch { }
        }

        public static string GetClonedRepo()
        {
            return "g:\\";
            //// Load the .props file and get the value of the variable
            //XDocument propsFile = XDocument.Load(@"X:\Bldtools\PropertySheets\SP3D.Release.Win32.props");
            //XNamespace ns = "http://schemas.microsoft.com/developer/msbuild/2003";
            //string myPathVariable = propsFile.Descendants(ns + "ClonedRepo").FirstOrDefault()?.Value;

            //return myPathVariable;
        }

        public static bool IsFileExtensionAllowed(string filePath)
        {
            string fileExtension = System.IO.Path.GetExtension(filePath);
            return ReversePatcher.m_AllowedExtensions.Contains(fileExtension, StringComparer.OrdinalIgnoreCase);
        }

        public static string GetCurrentFilterFromFilePath(string file)
        {
            try
            {
                string[] filter = file.Split("\\");
                string currentFileFilter = filter[1] + "_" + filter[2];
                return currentFileFilter;
            }
            catch
            {
                return string.Empty;
            }
        }


        #region Public APIs to Identify dependencies, Resolving paths

        ///// <summary>
        ///// As of now not returning anything and the GetTheFileDependencies is writing the data to xml
        ///// </summary>
        ///// <param name="folder"></param>
        ///// <param name="filesUnderSelectedRoot"></param>
        //public static void GetDependenciesOfFilesList(string folder, List<string> filesUnderSelectedRoot)
        //{
        //    foreach (var file in filesUnderSelectedRoot)
        //    {
        //        FileDepIdentifier.GetTheFileDependencies(file, folder);
        //    }
        //}


        public static bool IsCommonFile(string filename)
        {
            if (Commonfiles.Contains(Path.GetFileName(filename)))
                return true;
            else
                return false;
        }

        public static bool IsValidFilenameWithExtension(string filename)
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
        

        public static bool IsFileUnderDirectory(string directoryPath, string filePath)
        {
            string fullPath = Path.GetFullPath(filePath);
            string fullDirectoryPath = Path.GetFullPath(directoryPath);

            return fullPath.StartsWith(fullDirectoryPath, StringComparison.OrdinalIgnoreCase);
        }


        public static bool IsMIDLGenerated(string fileContent)
        {
            // You can add specific conditions or pattern matching logic here to check if the file is MIDL generated.
            // For simplicity, we'll assume that the presence of "#pragma once" indicates MIDL generated file.
            // You may need to adapt this logic based on your actual MIDL generated file characteristics.
            return (fileContent.Contains("File created by MIDL compiler") || fileContent.Contains("ALWAYS GENERATED file"));
        }

        //public static Dictionary<string, string> ExtractVariableDefinitionsFromWixProj(string wixprojFilePath)
        //{
        //    Dictionary<string, string> variableDefinitions = new Dictionary<string, string>();

        //    try
        //    {
        //        XDocument wixprojDocument = XDocument.Load(wixprojFilePath);

        //        var defineConstantsElement = wixprojDocument.Descendants()
        //            .FirstOrDefault(e => e.Name.LocalName == "DefineConstants");

        //        if (defineConstantsElement != null)
        //        {
        //            string[] constantPairs = defineConstantsElement.Value.Split(';');
        //            foreach (string constantPair in constantPairs)
        //            {
        //                string[] parts = constantPair.Split('=');
        //                if (parts.Length == 2)
        //                {
        //                    string variableName = parts[0];
        //                    string variableValue = parts[1];
        //                    variableDefinitions[variableName] = variableValue;
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        DepIdentifierUtils.WriteTextInLog($"Error extracting variable definitions: {ex.Message}");
        //    }

        //    return variableDefinitions;
        //}

        //public static List<string> FindWxsDependencies(string wxsFilePath, Dictionary<string, string> variablesDictionary)
        //{
        //    List<string> dependencies = new List<string>();

        //    try
        //    {
        //        XmlDocument xmlDoc = new XmlDocument();
        //        xmlDoc.Load(wxsFilePath);

        //        XmlNamespaceManager namespaceManager = new XmlNamespaceManager(xmlDoc.NameTable);
        //        namespaceManager.AddNamespace("wix", "http://schemas.microsoft.com/wix/2006/wi");

        //        XmlNodeList sourceAttributes = xmlDoc.SelectNodes("//wix:File/@Source", namespaceManager);
        //        foreach (XmlNode fileNode in sourceAttributes)
        //        {
        //            string filePath = fileNode.Value;
        //            if (File.Exists(filePath))
        //                dependencies.Add(filePath);
        //            else
        //                dependencies.Add(ResolveWixFileConstantInPath(filePath, variablesDictionary));

        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        DepIdentifierUtils.WriteTextInLog($"Error processing .wxs file: {ex.Message}");
        //    }

        //    return dependencies;
        //}

        //private static string ResolveWixFileConstantInPath(string filePath, Dictionary<string, string> variablesDictionary)
        //{
        //    string resolvedPath = filePath;
        //    try
        //    {
        //        if (filePath.Contains("$(var."))
        //        {
        //            foreach (var kvp in variablesDictionary)
        //            {
        //                string variablePlaceholder = $"$(var.{kvp.Key})";
        //                resolvedPath = resolvedPath.Replace(variablePlaceholder, kvp.Value);
        //            }
        //        }
        //    }
        //    catch(Exception ex)
        //    {
        //        DepIdentifierUtils.WriteTextInLog($"ResolveWixFileConstantInPath failed for the {filePath} with excpetion {ex.Message}");
        //    }
        //    return resolvedPath;
        //}





        public static List<string> FindAdditionalIncludeDirectorisInAPropFile(string filePath, string folder, string filtersXMLPath)
        {
            List<string> additionalIncludeDirectories = new List<string>();
            if(File.Exists(filePath))
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

        

        //private static List<string> FindIDLDependencies(string idlFileName, string folder)
        //{
        //    List<string> parsedIdlFilePaths = ExtractImportedFilesAndResolvePathsFromFile(idlFileName);

        //    // Update the XML attribute with IDL path information for the current idlFileName
        //    //UpdateTheXmlAttributeIDLPath(idlFileName, parsedIdlFilePaths);

        //    return parsedIdlFilePaths;
        //}

        public static List<string> ResolveFromLocalDirectoryOrPatcher(string projectFilePath, List<string> dependenciesList, bool fromPatcher = true, string additionalIncludeDirectories = "")
        {
            List<string> resolvedList = new List<string>();
            List<string> unResolvedList = new List<string>();
            string localDirectory = Path.GetDirectoryName(projectFilePath);
            foreach (var dependentFile in dependenciesList)
            {
                if(File.Exists(dependentFile))
                {
                    if (dependentFile.StartsWith("g:", StringComparison.OrdinalIgnoreCase))
                    {
                        resolvedList.Add(dependentFile);
                        continue;
                    }
                    else
                        resolvedList.Add(ChangeToClonedPathFromVirtual(dependentFile, projectFilePath));
                }
                string combinedPath = Path.Combine(localDirectory, dependentFile.Replace("$(ClonedRepo)", "g:\\"));
                if (File.Exists(combinedPath))
                {
                    resolvedList.Add(combinedPath);
                    continue;
                }
               
                if (combinedPath.Contains(".."))
                {
                    combinedPath = Path.GetFullPath(combinedPath);
                }
                if (File.Exists(combinedPath))
                {
                    string clonedRepoPath = ChangeToClonedPathFromVirtual(combinedPath);
                    if (!String.IsNullOrEmpty(clonedRepoPath))
                        resolvedList.Add(clonedRepoPath);
                    else
                        DepIdentifierUtils.WriteTextInLog($"Unable to get cloned repo resolved path for {clonedRepoPath}");
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
                List<string> resolvedListFromPatcher = GetAllMatchingFilesFromS3DFilesList(unResolvedList, additionalIncludeDirectories: additionalIncludeDirectories, projectFilePath: projectFilePath);
                resolvedList.AddRange(resolvedListFromPatcher.ToList());
            }

            if (fromPatcher == false && unResolvedList.Count > 0)
            {
                foreach (var unResolvedFilePath in unResolvedList)
                {
                    DepIdentifierUtils.WriteTextInLog($"The {unResolvedFilePath} is not found which is used in the file: {projectFilePath}");
                }
            }

            //ConcurrentBag<string> resolvedListFromPatcher = ResolveFileNamesFromPatcher(filePath, unResolvedList, fromClonedRepo);

            return resolvedList;
        }

        //private static string GetAttributeOfFilePathFromXML(XmlDocument xmlDocument, string attributeName, string fileName)
        //{
        //    try
        //    {
        //        string currentFilter = GetCurrentFilterFromFilePath(fileName).ToLower();
        //        XmlNode xmlNode = xmlDocument.SelectSingleNode($"//{currentFilter}/filepath[@Name='{fileName.ToLower()}']");

        //        if (xmlNode != null)
        //        {
        //            XmlElement xmlElement = xmlNode as XmlElement;
        //            if (xmlElement != null)
        //                return xmlElement.GetAttribute(attributeName);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        //
        //    }
        //    return string.Empty;
        //}


        //private static List<string> FindDependenciesInAHeaderFile(string headerFilePath)
        //{
        //    List<string> dependentHeaderFiles = new List<string>();

        //    try
        //    {
        //        // Read the content of the .h file
        //        string fileContent = File.ReadAllText(headerFilePath);

        //        // Check if the file is MIDL generated
        //        if (IsMIDLGenerated(fileContent))
        //        {
        //            return dependentHeaderFiles; // Return an empty list for MIDL generated files
        //        }

        //        // Define the regular expression pattern to match #include statements for .h files
        //        string pattern = @"#include\s*[""']([^""']+\.(h|hpp))[^""']*[""']";

        //        // Create a regular expression object
        //        Regex regex = new Regex(pattern);

        //        // Search for matches in the file content
        //        MatchCollection matches = regex.Matches(fileContent);

        //        // Extract the file names from the matches and add them to the dependentHeaderFiles list
        //        foreach (Match match in matches)
        //        {
        //            string headerFileName = match.Groups[1].Value;
        //            dependentHeaderFiles.Add(headerFileName);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception("Failed to FindDependentHeaderFiles with exception " + ex.Message);
        //    }

        //    return dependentHeaderFiles;
        //}


        //private static List<string> FindDotHDependenciesAndAddToXml(string filePath, string folder, string filtersXMLPath)
        //{
        //    List<string> resolvedList = new List<string>();
        //    try
        //    {
        //        List<string> dependenciesList = FindDependenciesInAHeaderFile(filePath);
        //        if (dependenciesList != null && dependenciesList.Count > 0)
        //        {
        //            resolvedList = ResolveFromLocalDirectoryOrPatcher(filePath, dependenciesList, fromPatcher: true);
        //            resolvedList = RemoveTheMIDLGeneratedFilesFromTheList(resolvedList);

        //            UpdateTheXmlAttributeDependenciesPath(filePath, resolvedList, folder, filtersXMLPath);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception("Failed to FindDotHDependencies with exception: " + ex.Message);
        //    }
        //    return resolvedList;
        //}


        //private static List<string> FindRCDependenciesAndAddToXml(string filePath, string folder, string filtersXMLPath)
        //{
        //    List<string> resolvedList = new List<string>();
        //    try
        //    {
        //        resolvedList = FindDotHDependenciesAndAddToXml(filePath, folder, filtersXMLPath);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception($"Failed to FindRCDependenciesAndAddToXml for the '{filePath}' with exception: {ex.Message}");
        //    }
        //    return resolvedList;
        //}


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

        //public static List<string> ResolveFromLocalDirectoryOrPatcherUsingAdditionalIncludeDirectories(string projectFilePath, List<string> dependenciesList, string additionalIncludeDirectories)
        //{
        //    List<string> resolvedList = new List<string>();
        //    List<string> unResolvedList = new List<string>();
        //    string localDirectory = Path.GetDirectoryName(projectFilePath);
        //    foreach (var dependentFile in dependenciesList)
        //    {
        //        string combinedPath = Path.Combine(localDirectory, dependentFile);
        //        if (combinedPath.Contains(".."))
        //        {
        //            combinedPath = Path.GetFullPath(combinedPath);
        //        }
        //        if (File.Exists(combinedPath))
        //        {
        //            resolvedList.Add(combinedPath);
        //        }
        //        else
        //        {
        //            unResolvedList.Add(dependentFile);
        //        }
        //    }

        //    List<string> resolvedListFromPatcher = GetAllMatchingFilesFromS3DFilesList(unResolvedList);
        //    resolvedList.AddRange(resolvedListFromPatcher.ToList());

        //    if (unResolvedList.Count > 0 && resolvedListFromPatcher.Count < unResolvedList.Count)
        //    {
        //        foreach (var unResolvedFilePath in unResolvedList)
        //        {
        //            DepIdentifierUtils.WriteTextInLog($"The {unResolvedFilePath} is not found which is used in the file: {projectFilePath}");
        //        }
        //    }
        //    //ConcurrentBag<string> resolvedListFromPatcher = ResolveFileNamesFromPatcher(filePath, unResolvedList, fromClonedRepo);

        //    return resolvedList;
        //}

        static string GetFileFromAdditionalIncludeDirectories(List<string> directoryPath, string filePath)
        {
            foreach (var additionalIncludeDirectory in directoryPath)
            {
                string folderPathFromAdditionIncludeDir = Path.GetFullPath(additionalIncludeDirectory);
                string filefolder = Path.GetDirectoryName(filePath);
                if (string.Compare(folderPathFromAdditionIncludeDir, filefolder, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return filePath;
                }
            }
            return string.Empty;
        }

        static List<string> GetAllMatchingFilesFromS3DFilesList(List<string> searchStrings, string additionalIncludeDirectories = "", string projectFilePath = "")
        {
            List<string> allMatchingFiles = new List<string>();

            //foreach (string searchString in searchStrings)
            //{
            //    // In a synchronous method
            //    Task<List<string>> task = GetFilePathFromS3DFilesList(searchString);
            //    List<string> matchingFiles = Task.Run(() => task).GetAwaiter().GetResult();
            //    //List<string> matchingFiles = await GetFilePathFromS3DFilesList(searchString).GetAwaiter().GetResult();
            //    if (additionalIncludeDirectories != "")
            //    {
            //        if (matchingFiles.Count > 1)
            //        {
            //            matchingFiles = ComapareAndRemoveFromAdditionalIncludeDirectories(matchingFiles, additionalIncludeDirectories);
            //        }
            //    }
            //    allMatchingFiles.AddRange(matchingFiles);
            //}

            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(m_FilesListXMLPath);

            XmlNodeList xmlNodeList;


            foreach (string searchString in searchStrings)
            {
                xmlNodeList = xmlDocument.SelectNodes("//filepath[@ShortName='" + searchString.ToLower() + "']");

                if(xmlNodeList != null)
                {
                    long countOfMatchingFiles = xmlNodeList.Count;
                    if (countOfMatchingFiles == 1)
                    {
                        XmlElement xmlElement = xmlNodeList[0] as XmlElement;
                        allMatchingFiles.Add(xmlElement.GetAttribute("Name"));
                    }
                    else if (!string.IsNullOrEmpty(additionalIncludeDirectories))
                    {
                        List<string> additionalIncludeDirectoriesList = ResolveAdditionalDirectoriesInList(additionalIncludeDirectories, projectFilePath);

                        bool matchFound = false;
                        List<string> unresolvedMultipleFiles = new List<string>();
                        foreach (XmlElement xmlElement in xmlNodeList)
                        {
                            string filePath = xmlElement.GetAttribute("Name");
                            string resolvedPath = GetFileFromAdditionalIncludeDirectories(additionalIncludeDirectoriesList, filePath);
                            if (!String.IsNullOrEmpty(resolvedPath))
                            {
                                allMatchingFiles.Add(filePath);
                                matchFound = true;
                                break;
                            }
                            else
                                unresolvedMultipleFiles.Add(filePath);
                        }
                        if(matchFound == false)
                        {
                            //MessageBox.Show("Issue");
                            DepIdentifierUtils.WriteTextInLog("match not found and multiple files are identified as dependencies. ");
                            if (unresolvedMultipleFiles.Count > 0)
                            {
                                foreach (var duplicateDep in unresolvedMultipleFiles)
                                {
                                    DepIdentifierUtils.WriteTextInLog(duplicateDep);
                                }
                                allMatchingFiles.AddRange(unresolvedMultipleFiles);
                            }
                            else
                            {
                                DepIdentifierUtils.WriteTextInLog($"File Not in Patcher: The {searchString} is not found which is used in the file: {projectFilePath}");
                            }
                        }
                    }
                    else
                    {
                        DepIdentifierUtils.WriteTextInLog($"Reference Issue: The {searchString} is not found which is used in the file: {projectFilePath}");
                        //The file which is not available is added as a dependecy.
                    }
                }
            }
            return allMatchingFiles;
        }

        public static List<string> ResolveAdditionalDirectoriesInList(string additionalIncludeDirectories, string projectFilePath = "")
        {
            List<string> list = new List<string>();
            if (!string.IsNullOrEmpty(additionalIncludeDirectories))
            {
                list = additionalIncludeDirectories.Split(";").ToList();
                
                for(int i = 0; i< list.Count;i++)
                {
                    list[i] = list[i].Replace("$(ClonedRepo)", "g:\\");
                    if (list[i].Contains("..") || list[i].Contains(".\\"))
                    {
                        string resolvedPath = Path.Combine(Path.GetDirectoryName(projectFilePath), list[i]);
                        if (Directory.Exists(resolvedPath))
                        {
                            list[i] = Path.GetFullPath(resolvedPath);
                        }
                    }
                    if (list[i].Contains("root\\"))
                        continue;
                    
                    list[i] = ChangeToClonedPathFromVirtual(list[i]);
                }

                //       .Select(list => list.Replace("$(ClonedRepo)", ""))
                //       .Select(list => ChangeToClonedPathFromVirtual(list)).ToList();
                list = list.Distinct().ToList();
                list.RemoveAll(list => string.IsNullOrEmpty(list));
            }
            return list;
        }

        //static bool IsPathInsideFolder(string filePathToCheck, string parentFolderPath)
        //{
        //    string normalizedParentPath = System.IO.Path.GetFullPath(parentFolderPath);
        //    string normalizedFilePath = System.IO.Path.GetFullPath(filePathToCheck);

        //    return normalizedFilePath.StartsWith(normalizedParentPath, StringComparison.OrdinalIgnoreCase);
        //}


        //private static List<string> ComapareAndRemoveFromAdditionalIncludeDirectories(List<string> matchingFiles, string additionalIncludeDirectories)
        //{
        //    List<string> includeDirectories = additionalIncludeDirectories.Split(';').ToList();
        //    List<string> matchingFilesInIncludeDirectories = new List<string>();

        //    foreach (string filePath in matchingFiles)
        //    {
        //        string directoryPath = Path.GetDirectoryName(filePath);
        //        string fileName = Path.GetFileName(filePath);

        //        foreach (string includeDir in includeDirectories)
        //        {
        //            if (directoryPath.Equals(includeDir, StringComparison.OrdinalIgnoreCase))
        //            {
        //                matchingFilesInIncludeDirectories.Add(filePath);
        //                break;
        //            }
        //        }
        //    }

        //    return matchingFilesInIncludeDirectories;
        //}

        //static async Task ProcessFilesAsync(string inputFilePath, string outputDirectory)
        //{
        //    var virtualDriveFiles = new Dictionary<string, StringBuilder>();
        //    string[] lines = await File.ReadAllLinesAsync(inputFilePath);

        //    foreach (string line in lines)
        //    {
        //        if (line.StartsWith("Filename:", StringComparison.OrdinalIgnoreCase))
        //        {
        //            string path = line.Substring("Filename:".Length).Trim();
        //            string virtualDrive = GetReplacedVirtualDriveLetter(path);

        //            if (virtualDrive == string.Empty)
        //                continue;
        //            if (!virtualDriveFiles.TryGetValue(virtualDrive, out var stringBuilder))
        //            {
        //                stringBuilder = new StringBuilder();
        //                virtualDriveFiles[virtualDrive] = stringBuilder;
        //            }

        //            stringBuilder.AppendLine(path);
        //        }
        //    }

        //    foreach (var (virtualDrive, stringBuilder) in virtualDriveFiles)
        //    {
        //        string virtualDriveFilePath = Path.Combine(outputDirectory, $"AllFilesInS3D{virtualDrive}.txt");
        //        await File.WriteAllTextAsync(virtualDriveFilePath, stringBuilder.ToString());
        //    }
        //}
        //static async Task ReadFilesAsync(string directoryPath)
        //{
        //    string[] files = Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories);

        //    foreach (string file in files)
        //    {
        //        string contents = await File.ReadAllTextAsync(file);
        //        fileContents[file] = contents.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).ToList();
        //    }
        //}


        //static async Task<List<string>> GetFilePathFromS3DFilesList(string searchString)
        //{
        //    try
        //    {
        //        //await ReadFilesAsync(m_AllS3DDirectoriesFilePath);

        //        List<string> matchingFiles = await SearchMatchingFilesAsync(searchString);

        //        if (matchingFiles.Count > 0)
        //        {
        //            DepIdentifierUtils.WriteTextInLog("Multiple files found with the given search string:");
        //            foreach (string file in matchingFiles)
        //            {
        //                DepIdentifierUtils.WriteTextInLog(file);
        //            }
        //            return matchingFiles;
        //        }
        //        else
        //        {
        //            DepIdentifierUtils.WriteTextInLog($"No matching file found for the given search string ->{searchString}");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        DepIdentifierUtils.WriteTextInLog("An error occurred: " + ex.Message);
        //    }
        //    return new List<string>();
        //}

        //static async Task<List<string>> SearchMatchingFilesAsync(string searchString)
        //{
        //    var tasks = new List<Task<List<string>>>();

        //    foreach (var kvp in fileContents)
        //    {
        //        tasks.Add(Task.Run(() => SearchInFile(kvp.Key, kvp.Value, searchString)));
        //    }

        //    List<string> matchingFiles = new List<string>();

        //    while (tasks.Count > 0)
        //    {
        //        Task<List<string>> completedTask = await Task.WhenAny(tasks);
        //        tasks.Remove(completedTask);
        //        List<string> files = await completedTask;

        //        if (files.Count > 0)
        //        {
        //            matchingFiles.AddRange(files);
        //        }
        //    }

        //    return matchingFiles;
        //}

        ///// <summary>
        ///// This searches and stops after first file found
        ///// </summary>
        ///// <param name="filePath"></param>
        ///// <param name="updatedParsedIdlFilePaths"></param>
        ///// <param name="folder"></param>
        ///// <param name="filtersXMLPath"></param>
        ////static string SearchInFile(string filePath, List<string> lines, string searchString)
        ////{
        ////    foreach (string line in lines)
        ////    {
        ////        if (line.Contains(searchString, StringComparison.OrdinalIgnoreCase))
        ////        {
        ////            return filePath;
        ////        }
        ////    }

        ////    return null;
        ////}

        //static List<string> SearchInFile(string filePath, List<string> lines, string searchString)
        //{
        //    List<string> matchingFiles = new List<string>();

        //    foreach (string line in lines)
        //    {
        //        string fileName = Path.GetFileName(line);
        //        if (fileName != null)
        //        {
        //            if (searchString.Contains("\\"))
        //            {
        //                searchString = Path.GetFileName(searchString);
        //            }
        //            if (fileName.Equals(searchString, StringComparison.OrdinalIgnoreCase))
        //            {
        //                matchingFiles.Add(line);
        //                // If you don't want to continue searching in the same file after a match, you can break here.
        //                // break;
        //            }
        //        }
        //    }

        //    return matchingFiles;
        //}

        #endregion

        

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
        //        DepIdentifierUtils.WriteTextInLog("Error updating attribute in XML: " + ex.Message);
        //    }
        //}
        //Need a way to update the folders data to xml either to get it from .pat file or manually check and update
        //public static List<string> GetS3DFoldersDataFromXML(string XMLSDirectoryPath)
        //{

        //    List<string> s3dFoldersDataList = new List<string>();
        //    if (File.Exists(m_FiltersXMLPath))
        //    {
        //        try
        //        {
        //            s3dFoldersDataList = GetXmlData(m_FiltersXMLPath, "DATA/FILTERS", "Name");
        //        }
        //        catch (Exception ex)
        //        {
        //            throw new Exception("Failed to GetS3DFoldersDataFromXML with exception: " + ex.Message);
        //        }
        //    }
        //    return s3dFoldersDataList;
        //}

        //public static List<string> GetXmlData(string xml, string node, string attribute)
        //{
        //    List<string> xmlData = new List<string>();
        //    try
        //    {
        //        XmlDocument xmlDoc = new XmlDocument();
        //        if (File.Exists(xml))
        //        {
        //            xmlDoc.Load(xml);

        //            XmlNode xmlNode = xmlDoc.SelectSingleNode(node);
        //            XmlNodeList xmlNodeList = xmlNode.ChildNodes;

        //            foreach (XmlNode filterNode in xmlNodeList)
        //            {
        //                string filterName = filterNode.Attributes[attribute].InnerXml;
        //                xmlData.Add(filterName);
        //            }
        //        }
        //        else
        //        {
        //            throw new Exception("Xml file not found..!");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception("Failed to GetXmlData with exception: " + ex.Message);
        //    }
        //    return xmlData;
        //}

        //private static void GetFilesDataAndDependenciesWriteToXml(List<string> s3dFoldersDataList, string clonedRepo)
        //{
        //    foreach (var folder in s3dFoldersDataList)
        //    {
        //        Stopwatch stopwatch = Stopwatch.StartNew();
        //        List<string> filesUnderSelectedRoot = GetFilesDataAndWriteToXml(folder, clonedRepo);
        //        GetDependenciesOfFilesList(folder, filesUnderSelectedRoot);
        //        stopwatch.Stop();
        //        TimeSpan elapsed = stopwatch.Elapsed;
        //        DepIdentifierUtils.WriteTextInLog("Elapsed Time: " + elapsed + "\n");
        //    }
        //}

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

        //public static List<string> GetFilesDataAndWriteToXml(string folder, string clonedRepo, string xmlDirectoryPath = "")
        //{
        //    List<string> filesUnderSelectedRoot = new List<string>();
        //    try
        //    {
        //        if (xmlDirectoryPath == "")
        //        {
        //            xmlDirectoryPath = ReversePatcher.resourcePath;
        //        }

        //        DepIdentifierUtils.WriteTextInLog("Folder: " + folder);
        //        filesUnderSelectedRoot = FindPatternFilesInDirectory(clonedRepo + folder, "*.*");

        //        bool ifXMLFileExist = File.Exists(xmlDirectoryPath + @"\FilesList.xml");

        //        if (!ifXMLFileExist)
        //            WriteListToFilesListXml(filesUnderSelectedRoot, xmlDirectoryPath + @"\FilesList.xml", "filtersdata", folder.Replace("\\", "_"), "filepath", true);
        //        else
        //        {
        //            UpdateXmlWithData(filesUnderSelectedRoot, xmlDirectoryPath + @"\FilesList.xml", folder.Replace("\\", "_"), "FilePath", true);
        //        }
        //        DepIdentifierUtils.WriteTextInLog("Identifying the " + folder + " dependencies.");
        //        DepIdentifierUtils.WriteTextInLog("-------------------------------------------------------------");
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception("Failed to GetFilesDataAndWriteToXml with exception " + ex.Message);
        //    }
        //    return filesUnderSelectedRoot;
        //}

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
        //        DepIdentifierUtils.WriteTextInLog("Error updating XML: " + ex.Message);
        //        throw new Exception("Failed to UpdateXmlWithData with exception: " + ex.Message);
        //    }
        //}

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

        //        DepIdentifierUtils.WriteTextInLog("XML file updated/created successfully.");
        //    }
        //    catch (Exception ex)
        //    {
        //        DepIdentifierUtils.WriteTextInLog("Error updating/creating XML file: " + ex.Message);
        //        throw new Exception("Failed to UpdateListInFilesListXml with exception: " + ex.Message);
        //    }
        //}


        //public static void WriteListToFilesListXml(List<string> stringsList, string filePath, string parentNode, string rootElementName, string currentElementName, bool update = true)
        //{
        //    try
        //    {
        //        // Create a new XML writer and set its settings
        //        XmlWriterSettings settings = new XmlWriterSettings
        //        {
        //            Indent = true, // To format the XML with indentation
        //            IndentChars = "    " // Specify the indentation characters (four spaces in this case)
        //        };

        //        using (XmlWriter writer = XmlWriter.Create(filePath, settings))
        //        {
        //            // Write the XML declaration
        //            writer.WriteStartDocument();

        //            // Write the root element with the specified name
        //            writer.WriteStartElement(parentNode);
        //            writer.WriteStartElement(rootElementName);

        //            // Write each string as a separate element with the specified name
        //            foreach (string str in stringsList)
        //            {
        //                writer.WriteStartElement(currentElementName);
        //                writer.WriteAttributeString("Name", str);
        //                writer.WriteEndElement();
        //            }

        //            // Close the root element
        //            writer.WriteEndElement();

        //            // End the XML document
        //            writer.WriteEndDocument();
        //        }

        //        DepIdentifierUtils.WriteTextInLog("XML file created successfully.");
        //    }
        //    catch (Exception ex)
        //    {
        //        DepIdentifierUtils.WriteTextInLog("Error writing XML file: " + ex.Message);
        //        throw new Exception("Failed to WriteListToXml with exception: " + ex.Message);
        //    }
        //}

        #region Generate Data from the .pat file

        //Get Filters Data into Res.xml

        internal static List<string> ConvertToStringList(string data)
        {
            return data.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                       .ToList();
        }

        // Function to get the virtual drive from a given path
        internal static string GetReplacedVirtualDriveLetter(string path, bool forFilter = false)
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
        internal static string GetRootLetter(string path)
        {
            string root = Path.GetPathRoot(path);
            return (path.Substring(root.Length, 1));
        }

        // Function to get the virtual drive from a given path
        public static string ChangeToClonedPathFromVirtual(string path, string parentFilePath = "")
        {

            if (path.Contains("$(ClonedRepo)"))
            {
                path = path.Replace("$(ClonedRepo)", "g:\\");
                if (path.Contains(".."))
                {
                    string resolvedPath = Path.Combine(Path.GetDirectoryName(parentFilePath), path);
                    if (Directory.Exists(resolvedPath))
                    {
                        path = Path.GetFullPath(resolvedPath);
                    }
                }
                if (path.Contains("root\\"))
                    return path;
            }

            if (path.StartsWith("g:", StringComparison.OrdinalIgnoreCase))
                return path;

            //path = path.Replace("$(ClonedRepo)", "").ToLower();

            int colonIndex = path.IndexOf(":");
            //int lastBackslashIndex = path.LastIndexOf('\\');
            if (colonIndex >= 0)
            {
                return @"g:\" + path.Substring(0, colonIndex) + "root" + path.Substring(colonIndex+1);
            }
            else
                return string.Empty;
        }

        public static List<string> GetAllFilesFromSelectedRoot(List<string> textFilesPath, string rootFolder)
        {

            List<string> filteredFiles = new List<string>();

            rootFolder = DepIdentifierUtils.GetClonedRepo() + rootFolder.Replace("_", "\\");
            foreach (string filePath in textFilesPath)
            {
                if (filePath.StartsWith(rootFolder, StringComparison.OrdinalIgnoreCase))
                {
                    filteredFiles.Add(filePath);
                }
            }

            return filteredFiles;
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

        
        #endregion


    }
}
