using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DepIdentifier
{
    internal class PreRequisiteGenerator
    {
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
                List<string> patcherDataLines = File.ReadAllLines(DepIdentifierUtils.m_PatcherFilePath).ToList();

                List<string> filtersDataList = new List<string>();

                foreach (string line in patcherDataLines)
                {
                    if (line.StartsWith("Filename:", StringComparison.OrdinalIgnoreCase))
                    {
                        string path = line.Substring("Filename:".Length).Trim();
                        string virtualDrive = DepIdentifierUtils.GetReplacedVirtualDriveLetter(path);
                        if (virtualDrive == string.Empty)
                            continue;
                        path = DepIdentifierUtils.ChangeToClonedPathFromVirtual(path);
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
                        string virtualDriveLetter = DepIdentifierUtils.GetReplacedVirtualDriveLetter(path, forFilter: true);
                        if (virtualDriveLetter == string.Empty)
                            continue;
                        filtersDataList.Add(virtualDriveLetter + "_" + path.Substring(path.LastIndexOf("\\") + 1).Trim());
                    }
                }
                DepIdentifierUtils.m_CachedFiltersData = filtersDataList;
                XMLHelperAPIs.CreateOrUpdateListXml(filtersDataList, DepIdentifierUtils.m_FiltersXMLPath, "data", "filters", "filter");

                foreach (var (virtualDrive, stringBuilder) in virtualDriveFiles)
                {
                    string virtualDriveFilePath = Path.Combine(DepIdentifierUtils.m_AllS3DDirectoriesFilePath, $"AllFilesInS3D{DepIdentifierUtils.GetRootLetter(virtualDrive)}root.txt");
                    List<string> cachedRootList = DepIdentifierUtils.GetSpecificCachedRootList(virtualDrive + "root");
                    cachedRootList = DepIdentifierUtils.ConvertToStringList(stringBuilder.ToString());

                    if (File.Exists(virtualDriveFilePath))
                    {
                        File.Delete(virtualDriveFilePath);
                    }
                    await File.WriteAllTextAsync(virtualDriveFilePath, stringBuilder.ToString());
                }

                DepIdentifierUtils.WriteTextInLog("Paths copied to respective files successfully.");
            }
            catch (Exception ex)
            {
                DepIdentifierUtils.WriteTextInLog("An error occurred: " + ex.Message);
            }
        }

        private static void CreateFiltersInXML()
        {
            if (DepIdentifierUtils.m_CachedFiltersData.Count == 0)
            {
                MessageBox.Show("Filters Data is empty.");
                throw new Exception("Filters Data is empty.");
            }
            foreach (var filterPath in DepIdentifierUtils.m_CachedFiltersData)
            {
                XMLHelperAPIs.CreateOrUpdateListXml(new List<string>(), DepIdentifierUtils.m_FilesListXMLPath, "filtersdata", filterPath.Replace("//", "_"), "");
            }
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
                if (DepIdentifierUtils.m_CachedFiltersData.Count == 0)
                {
                    MessageBox.Show("Filters Data is empty.");
                    throw new Exception("Filters Data is empty.");
                }
                else
                {
                    if (!File.Exists(DepIdentifierUtils.m_FilesListXMLPath))
                    {
                        CreateFiltersInXML();
                    }

                    foreach (var filterPath in DepIdentifierUtils.m_CachedFiltersData)
                    {
                        if (filterPath == "sroot_civil")
                        {

                        }
                        //Get Filters Data from the Res file and later use it to add the xmlDirectoryPath
                        List<string> filesToAddInXML = DepIdentifierUtils.GetAllFilesFromSelectedRoot(DepIdentifierUtils.GetSpecificCachedRootList(filterPath), filterPath);
                        filesToAddInXML = DepIdentifierUtils.FilterFilePathsByExtensions(filesToAddInXML, DepIdentifierUtils.IncludedExtensions);

                        XMLHelperAPIs.CreateOrUpdateListXml(filesToAddInXML, DepIdentifierUtils.m_FilesListXMLPath, "filtersdata", filterPath.Replace("\\", "_"), "filepath");

                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Exception occurred while creating files list template xml");
            }
        }
    }
}
