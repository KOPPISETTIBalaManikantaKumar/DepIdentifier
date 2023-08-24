using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Configuration;

namespace DepIdentifier
{
    public static class DepIdentifierUtils
    {
        #region required memberVaraibles
        public static List<string> patcherDataLines = new List<string>();
        public static string m_PatcherFilePath = ConfigurationManager.AppSettings["PatcherFilePath"];
        public static string m_AllS3DDirectoriesFilePath = ReversePatcher.resourcePath;
        public static string m_FiltersXMLPath = ReversePatcher.resourcePath + "\\filtersdata.xml";
        public static string m_FilesListXMLPath = ReversePatcher.resourcePath + "\\filesList.xml";
        public static List<string> m_CachedFiltersData = new List<string>();

        private static string commonFilesString = ConfigurationManager.AppSettings["Commonfiles"];
        public static List<string> Commonfiles = commonFilesString.Split(new[] { "," }, StringSplitOptions.None).ToList();

        //new List<string> { "oaidl.idl", "ocidl.idl", "atlbase.h", "atlcom.h", "statreg.h", "wtypes.idl", "comdef.h",
        //                                                            "math.h", "initguid.h", "objbase.h", "share.h", "olectl.h", "oledb.h", "OLEDBERR.h",
        //                                                            "activscp.h", "adoint.h", "afxdisp.h", "afxres.h", "afxwin.h", "assert.h", "ATLBASE.h",
        //                                                            "atlcomcli.h", "atlconv.h", "atlctl.h", "atldbcli.h", "atldbsch.h", "atlpath.h", "atlsafe.h",
        //                                                            "atlstr.h", "COMDEF.H", "comip.h", "comsvcs.h", "Comutil.h", "crtdbg.h", "ctype.h", "float.h",
        //                                                            "guiddef.h", "INITGUID.H", "inttypes.h", "limits.h", "locale.h", "malloc.h", "Math.h", "msxml6.h",
        //                                                            "oaidl.h", "Objbase.h", "ocidl.h", "ole2.h"};
        private static Dictionary<string, List<string>> fileContents = new Dictionary<string, List<string>>();
        #endregion

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
            string clonedRepo = ConfigurationManager.AppSettings["ClonedRepo"];

            return clonedRepo;
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


            return resolvedList;
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
        #endregion

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
