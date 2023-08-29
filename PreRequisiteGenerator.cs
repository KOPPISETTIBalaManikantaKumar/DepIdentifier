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
                if(!File.Exists(DepIdentifierUtils.m_PatcherFilePath))
                {
                    MessageBox.Show($"Unabe to find the patcher in the location {DepIdentifierUtils.m_PatcherFilePath}");
                }
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
            ReversePatcher.CacheAllRootFiles();
            try
            {
                //mroot_drawingsisometric
                //mroot_projectmgmt
                //mroot_reports
                //sroot_tribontranslator
                //xroot_mathkernel

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
                    using (DynamicProgressBar progressForm = new DynamicProgressBar())
                    {
                        progressForm.SetMinAndMax(0, DepIdentifierUtils.m_CachedFiltersData.Count);
                        //progressForm.Show();

                        // ProgressBar progressBar = ReversePatcher.SetProgressBar(0, DepIdentifierUtils.m_CachedFiltersData.Count);
                        int counter = 0;
                        List<string> files = new List<string> { "mroot_drawingsisometric", "mroot_projectmgmt", "mroot_reports", "sroot_tribontranslator", "xroot_mathkernel" };

                        //foreach (var filterPath in DepIdentifierUtils.m_CachedFiltersData)
                        foreach (var filterPath in files)
                        {
                            counter++;
                            progressForm.UpdateProgress(counter);
                            //Get Filters Data from the Res file and later use it to add the xmlDirectoryPath
                            List<string> filesToAddInXML = DepIdentifierUtils.GetAllFilesFromSelectedRoot(DepIdentifierUtils.GetSpecificCachedRootList(filterPath), filterPath);
                            // filesToAddInXML = DepIdentifierUtils.FilterFilePathsByExtensions(filesToAddInXML, DepIdentifierUtils.IncludedExtensions);

                            XMLHelperAPIs.CreateOrUpdateListXml(filesToAddInXML, DepIdentifierUtils.m_FilesListXMLPath, "filtersdata", filterPath.Replace("\\", "_"), "filepath");
                        }
                        //progressForm.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error occurred while creating files list template xml with exception: {ex.Message}");
            }
        }

    }
}
