using System.Collections.Concurrent;
using System.Diagnostics;
using System.Windows.Forms;
using System.Xml;
using static System.Windows.Forms.LinkLabel;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace DepIdentifier
{
    public partial class ReversePatcher : Form
    {
        private const string resourcePath = "G:\\xroot\\Bldtools\\DepIdentifier\\resources\\";
        private List<string> m_RootFilesList = new List<string>{resourcePath + "AllFilesInS3Dkroot.txt",
            resourcePath + "AllFilesInS3Dmroot.txt",
            resourcePath + "AllFilesInS3Drroot.txt",
            resourcePath + "AllFilesInS3Dsroot.txt",
            resourcePath + "AllFilesInS3Dtroot.txt",
            resourcePath + "AllFilesInS3Dxroot.txt",
            resourcePath + "AllFilesInS3Dyroot.txt" };

        private List<string> m_ExtensionsList = new List<string> { ".rc", ".cpp", ".vcxproj", "vbproj", ".props", ".csproj", ".vbp", ".wixproj", ".wxs", ".lst", ".h" };
        private bool isShowIDLChecked;
        private bool isShowDotHChecked;
        private bool isShowAllChecked;

        //CacheAllRootFiles
        public static List<string> cachedKrootFiles = new List<string>();
        public static List<string> cachedMrootFiles = new List<string>();
        public static List<string> cachedLrootFiles = new List<string>();
        public static List<string> cachedRrootFiles = new List<string>();
        public static List<string> cachedSrootFiles = new List<string>();
        public static List<string> cachedTrootFiles = new List<string>();
        public static List<string> cachedXrootFiles = new List<string>();
        public static List<string> cachedYrootFiles = new List<string>();


        public Dictionary<string, List<string>> m_DependencyDictionary = new Dictionary<string, List<string>>();

        public static List<string> GetCachedKrootFiles()
        {
            return cachedKrootFiles;
        }
        public static List<string> GetCachedMrootFiles()
        {
            return cachedMrootFiles;
        }   
        public static List<string> GetCachedLrootFiles()
        {
            return cachedLrootFiles;
        }      
        public static List<string> GetCachedRrootFiles()
        {
            return cachedRrootFiles;
        }         
        public static List<string> GetCachedSrootFiles()
        {
            return cachedSrootFiles;
        }           
        public static List<string> GetCachedTrootFiles()
        {
            return cachedTrootFiles;
        }           
        public static List<string> GetCachedXrootFiles()
        {
            return cachedXrootFiles;
        }             
        public static List<string> GetCachedYrootFiles()
        {
            return cachedYrootFiles;
        }   


        private static string m_selectedFilterPath = string.Empty;

        private static List<string> m_filesForWhichDependenciesNeedToBeIdentified = new List<string>();

        public ReversePatcher()
        {
            InitializeComponent();
            CacheAllRootFiles();
            LoadFilters();

            Cursor = Cursors.WaitCursor;
            //_ = DepIdentifierUtils.ComputeDependenciesForAllFilesAsync(m_FiltersList, m_XMLSDirectoryPath);
            Cursor = Cursors.Default;
        }

        public static void CacheAllRootFiles()
        {
            try
            {
                if (File.Exists(resourcePath + "AllFilesInS3Dkroot.txt"))
                    cachedKrootFiles = File.ReadAllLines(resourcePath + "AllFilesInS3Dkroot.txt").ToList();
                if(File.Exists(resourcePath + "AllFilesInS3Dmroot.txt"))
                    cachedMrootFiles = File.ReadAllLines(resourcePath + "AllFilesInS3Dmroot.txt").ToList();
                if (File.Exists(resourcePath + "AllFilesInS3Drroot.txt"))
                    cachedRrootFiles = File.ReadAllLines(resourcePath + "AllFilesInS3Drroot.txt").ToList();
                if (File.Exists(resourcePath + "AllFilesInS3Dsroot.txt"))
                    cachedSrootFiles = File.ReadAllLines(resourcePath + "AllFilesInS3Dsroot.txt").ToList();
                if (File.Exists(resourcePath + "AllFilesInS3Dtroot.txt"))
                    cachedTrootFiles = File.ReadAllLines(resourcePath + "AllFilesInS3Dtroot.txt").ToList();
                if (File.Exists(resourcePath + "AllFilesInS3Dxroot.txt"))
                    cachedXrootFiles = File.ReadAllLines(resourcePath + "AllFilesInS3Dxroot.txt").ToList();
                if (File.Exists(resourcePath + "AllFilesInS3Dyroot.txt"))
                    cachedYrootFiles = File.ReadAllLines(resourcePath + "AllFilesInS3Dyroot.txt").ToList();
                if (File.Exists(resourcePath + "AllFilesInS3Dlroot.txt"))
                    cachedLrootFiles = File.ReadAllLines(resourcePath + "AllFilesInS3Dlroot.txt").ToList();
            }
            catch
            {
                //Might be the files do not exist            
            }
        }


        private void ShowDependencies_SelectedIndexChanged(object sender, EventArgs e)
        {
            CheckedListBox checkedListBox = sender as CheckedListBox;
            foreach (var item in checkedListBox.CheckedItems)
            {
                string value = item.ToString();
                if (value == "Show IDL")
                    isShowIDLChecked = true;
                if (value == "Show .h")
                    isShowDotHChecked = true;
                if (value == "Show all")
                    isShowIDLChecked = true;
            }
        }

        private async void FilterCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            string rootFilesPath = string.Empty;
            System.Windows.Forms.ComboBox comboBox = sender as System.Windows.Forms.ComboBox;

            if (m_selectedFilterPath != comboBox.SelectedItem.ToString())
            {
                m_selectedFilterPath = comboBox.SelectedItem.ToString();
                //To get the filters file names
                //foreach(var rootFile in m_RootFilesList)
                //{
                //    if(Path.GetFileName(rootFile).Contains(comboBox.SelectedItem.ToString().Split("\\")[0]))
                //    {
                //        rootFilesPath = rootFile;
                //        break;
                //    }
                //}
                List<string> filesList = new List<string>();

                filesList = GetAllFilesFromSelectedRoot(DepIdentifierUtils.GetSpecificCachedRootList(m_selectedFilterPath), m_selectedFilterPath);


                //Fill the tree nodes
                LoadFiles(filesList);


                //m_DependencyList.Clear();
                //DependenciesList.Items.Clear();


                //if (m_selectedFilterPath != null)
                //{
                //    SelectAll.Visible = true;
                //    SelectAll.Enabled = true;

                //    List<string> filesUnderSelectedRoot = new List<string>();
                //    bool ifXMLFileExist = File.Exists(m_XMLSDirectoryPath + @"\FilesList.xml");
                //    try
                //    {
                //        if (ifXMLFileExist)
                //        {
                //            filesUnderSelectedRoot = GetXmlData(m_XMLSDirectoryPath + @"\FilesList.xml", "FiltersData/" + m_selectedFilterPath.Replace("\\", "_"), "Name");
                //        }
                //    }
                //    catch (Exception ex)
                //    {
                //        //xml might not contain the data
                //    }
                //    if (filesUnderSelectedRoot.Count == 0)
                //    {
                //        filesUnderSelectedRoot = Utilities.FindPatternFilesInDirectory(Utilities.GetClonedRepo() + m_selectedFilterPath, "*.*");
                //        filesUnderSelectedRoot.Sort();
                //        if (!ifXMLFileExist)
                //            await DepIdentifierUtils.WriteListToXmlAsync(filesUnderSelectedRoot, m_XMLSDirectoryPath + @"\FilesList.xml", m_selectedFilterPath.Replace("\\", "_"), "FilePath", true);
                //        else
                //        {
                //            await DepIdentifierUtils.UpdateXmlWithDataAsync(filesUnderSelectedRoot, m_XMLSDirectoryPath + @"\FilesList.xml", m_selectedFilterPath.Replace("\\", "_"), "FilePath", true);
                //        }
                //    }

                //    ProjectsCheckedList.Items.Clear();

                //    foreach (var file in filesUnderSelectedRoot)
                //        ProjectsCheckedList.Items.Add(file);
            }
        }

        private void LoadFiles(List<string> lines)
        {
            ProjectsTreeView.Nodes.Clear();


            foreach (string line in lines)
            {
                string fullPath = line.Trim();
                string[] pathComponents = fullPath.Split('\\');

                TreeNode currentNode = null;
                TreeNodeCollection nodes = ProjectsTreeView.Nodes;

                foreach (string component in pathComponents)
                {
                    if (currentNode == null)
                    {
                        TreeNode existingNode = FindNodeByText(nodes, component);
                        if (existingNode == null)
                        {
                            currentNode = nodes.Add(component);
                        }
                        else
                        {
                            currentNode = existingNode;
                        }
                    }
                    else
                    {
                        TreeNode existingNode = FindNodeByText(currentNode.Nodes, component);
                        if (existingNode == null)
                        {
                            currentNode = currentNode.Nodes.Add(component);
                        }
                        else
                        {
                            currentNode = existingNode;
                        }
                    }
                }
            }

            //ProjectsTreeView.ExpandAll();
        }

        private TreeNode FindNodeByText(TreeNodeCollection nodes, string text)
        {
            foreach (TreeNode node in nodes)
            {
                if (node.Text == text)
                {
                    return node;
                }
            }
            return null;
        }


        public static List<string> GetAllFilesFromSelectedRoot(List<string> textFilesPath, string rootFolder)
        {

            List<string> filteredFiles = new List<string>();

            rootFolder = Utilities.GetClonedRepo() + rootFolder.Replace("_", "\\");
            foreach (string filePath in textFilesPath)
            {
                if (filePath.StartsWith(rootFolder, StringComparison.OrdinalIgnoreCase))
                {
                    filteredFiles.Add(filePath);
                }
            }

            return filteredFiles;
        }

        //private void GetSelectedFilesButton_Click(object sender, EventArgs e)
        //{
        //    TreeNode selectedNode = ProjectsTreeView.SelectedNode;
        //    if (selectedNode != null)
        //    {
        //        if (selectedNode.Checked)
        //        {
        //            var selectedFilePaths = CollectFilePaths(selectedNode);
        //            foreach (var filePath in selectedFilePaths)
        //            {
        //                Console.WriteLine(filePath);
        //            }
        //        }
        //    }
        //}

        // Recursive function to check/uncheck child nodes
        private void CheckChildNodes(TreeNode node, bool isChecked)
        {
            foreach (TreeNode childNode in node.Nodes)
            {
                childNode.Checked = isChecked;

                if (childNode.Nodes.Count > 0)
                {
                    CheckChildNodes(childNode, isChecked);
                }
            }
        }

        // Recursive function to uncheck all ancestors
        private void UncheckAncestors(TreeNode node)
        {
            if (node == null)
                return;

            node.Checked = false;

            UncheckAncestors(node.Parent);
        }

        private void ProjectsTreeView_AfterCheck(object sender, TreeViewEventArgs e)
        {

            if (e.Action != TreeViewAction.Unknown)
            {
                // Check/uncheck child nodes based on the parent's state
                CheckChildNodes(e.Node, e.Node.Checked);
                // If the parent node is unchecked, uncheck all its ancestors
                if (!e.Node.Checked)
                {
                    UncheckAncestors(e.Node.Parent);
                }
            }

            bool anyCheckBoxChecked = IsAnyCheckBoxChecked(ProjectsTreeView.Nodes);
            if (anyCheckBoxChecked) { GetDependenciesBtn.Enabled = true; }
            else { GetDependenciesBtn.Enabled = false; }
        }

        private bool IsAnyCheckBoxChecked(TreeNodeCollection nodes)
        {
            foreach (TreeNode node in nodes)
            {
                if (node.Checked)
                    return true;

                if (node.Nodes.Count > 0)
                {
                    if (IsAnyCheckBoxChecked(node.Nodes))
                        return true;
                }
            }
            return false;
        }

        private string[] CollectFilePaths(TreeNode node)
        {
            var filePaths = new List<string>();
            if (node.Nodes.Count == 0)
            {
                filePaths.Add(node.FullPath);
            }
            else
            {
                foreach (TreeNode childNode in node.Nodes)
                {
                    filePaths.AddRange(CollectFilePaths(childNode));
                }
            }
            return filePaths.ToArray();
        }

        private void GetCheckedFilePaths(TreeNodeCollection nodes, List<string> selectedFilePaths)
        {
            foreach (TreeNode node in nodes)
            {
                if (node.Checked)
                {
                    selectedFilePaths.AddRange(CollectFilePaths(node));
                }
                else
                {
                    GetCheckedFilePaths(node.Nodes, selectedFilePaths);
                }
            }
        }

        /// <summary>
        /// As of now not returning anything and the GetTheFileDependencies is writing the data to xml
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="filesUnderSelectedRoot"></param>
        public static void GetDependenciesOfFilesList(string folder, List<string> filesUnderSelectedRoot)
        {
            foreach (var file in filesUnderSelectedRoot)
            {
                DepIdentifierUtils.GetTheFileDependencies(file, folder);
            }
        }

        private void GetDependencies_Click(object sender, EventArgs e)
        {
            m_DependencyDictionary = new Dictionary<string, List<string>>();
            m_filesForWhichDependenciesNeedToBeIdentified.Clear();
            List<string> currentSelectedFilePaths = new List<string>();
            GetCheckedFilePaths(ProjectsTreeView.Nodes, currentSelectedFilePaths);

            m_filesForWhichDependenciesNeedToBeIdentified = currentSelectedFilePaths;

            SelectedFilesListBox.Items.AddRange(m_filesForWhichDependenciesNeedToBeIdentified.ToArray());

            foreach (var file in m_filesForWhichDependenciesNeedToBeIdentified)
            {
                List<string> dependenicesOfCurrentFile = new List<string>();
                Cursor.Current = Cursors.WaitCursor;
                string dependentList = string.Empty;
                if (File.Exists(DepIdentifierUtils.m_FilesListXMLPath))
                {
                    var xmlDoc = new XmlDocument();
                    xmlDoc.Load(DepIdentifierUtils.m_FilesListXMLPath);
                    string elementName = "filepath";
                    string attributeNameToSearch = "name";
                    string attributeValueToSearch = file.ToLower();
                    //dependentList = Utilities.GetNameAttributeValue(xmlDoc, m_selectedFilterPath.Replace("\\", "_") + "/FilePath", "Name", file);
                    dependentList = Utilities.GetNameAttributeValue(xmlDoc, m_selectedFilterPath.Replace("\\", "_"), elementName, attributeNameToSearch, attributeValueToSearch);
                    //dependentList = Utilities.GetNameAttributeValue(xmlDoc, m_selectedFilterPath.Replace("\\", "_") + "/FilePath", "Name", file);
                }
                if (String.IsNullOrEmpty(dependentList))
                {
                    dependenicesOfCurrentFile = DepIdentifierUtils.GetTheFileDependencies(file, m_selectedFilterPath);

                    //Update the xml accordingly
                }
                else
                {
                    string[] splittedStrings = dependentList.Split(new[] { ";" }, StringSplitOptions.None);
                    m_DependencyList.AddRange(splittedStrings);
                    m_DependencyDictionary.Add(file, splittedStrings.ToList());
                }
            }

            DependenciesList.Items.Clear();
            if (m_DependencyList.Count == 0)
            {
                DependenciesList.Items.Add("No Dependencies.");
            }
            else
            {
                foreach (var dependentFile in m_DependencyList)
                {
                    DependenciesList.Items.Add(dependentFile);
                }
            }

            Cursor.Current = Cursors.Default;

            CopyList.Enabled = true;
            CopyList.Visible = true;
        }

        private void GetFileDependenciesRecursively()
        {
            foreach (var file in m_filesForWhichDependenciesNeedToBeIdentified)
            {
                List<string> dependenicesOfCurrentFile = new List<string>();
                Cursor.Current = Cursors.WaitCursor;
                string dependentList = GetDependencyDataOfFilesFromXML(file);
                if (String.IsNullOrEmpty(dependentList))
                {
                    string[] filter = file.Split("\\");
                    string currentFileFilter = filter[1] + "_" + filter[2];
                    dependenicesOfCurrentFile = DepIdentifierUtils.GetTheFileDependencies(file, currentFileFilter);

                    //Update the xml accordingly
                }
                else
                {
                    string[] splittedStrings = dependentList.Split(new[] { ";" }, StringSplitOptions.None);
                    dependenicesOfCurrentFile.AddRange(splittedStrings);
                    m_DependencyDictionary.Add(file, dependenicesOfCurrentFile);
                }
            }
        }

        private static string GetDependencyDataOfFilesFromXML(string file)
        {
            string dependentListSemiColonSeperated = string.Empty;
            try
            {
                if (File.Exists(DepIdentifierUtils.m_FilesListXMLPath))
                {
                    var xmlDoc = new XmlDocument();
                    xmlDoc.Load(DepIdentifierUtils.m_FilesListXMLPath);
                    string elementName = "filepath";
                    string attributeNameToSearch = "name";
                    string attributeValueToSearch = file.ToLower();
                    //dependentList = Utilities.GetNameAttributeValue(xmlDoc, m_selectedFilterPath.Replace("\\", "_") + "/FilePath", "Name", file);
                    dependentListSemiColonSeperated = Utilities.GetNameAttributeValue(xmlDoc, m_selectedFilterPath.Replace("\\", "_"), elementName, attributeNameToSearch, attributeValueToSearch);
                    //dependentList = Utilities.GetNameAttributeValue(xmlDoc, m_selectedFilterPath.Replace("\\", "_") + "/FilePath", "Name", file);
                }
            }
            catch(Exception ex)
            {
                throw new Exception("GetDependencyDataOfFilsFromXML failed with exception " + ex.Message);
            }
            return dependentListSemiColonSeperated;
        }

        //private void IdentifyIDLDependencies(string idlFileName)
        //{
        //    if (idlFileName.Contains(".idl"))
        //    {
        //        if (idlFileName != null && idlFileName != string.Empty)
        //        {
        //            List<string> parsedIdlFilePaths = Utilities.ExtractImportedFilesAndResolvePathsFromFile(idlFileName);
        //            List<string> updatedParsedIdlFilePaths = new List<string>();
        //            updatedParsedIdlFilePaths.AddRange(parsedIdlFilePaths);
        //            m_DependencyList.AddRange(parsedIdlFilePaths);
        //            UpdateTheXmlAttributeIDLPath(idlFileName, updatedParsedIdlFilePaths);
        //            foreach (string idlFile in parsedIdlFilePaths)
        //            {
        //                List<string> extractedFilePaths = Utilities.ExtractImportedFilesAndResolvePathsFromFile(idlFile);
        //                extractedFilePaths = extractedFilePaths.Distinct().ToList();
        //                UpdateTheXmlAttributeIDLPath(idlFile, extractedFilePaths);
        //                updatedParsedIdlFilePaths.AddRange(extractedFilePaths);
        //                updatedParsedIdlFilePaths = updatedParsedIdlFilePaths.Distinct().ToList();
        //            }
        //            List<string> itemsToAdd = new List<string>();

        //            while (true)
        //            {
        //                itemsToAdd.Clear();

        //                foreach (string idlFile in updatedParsedIdlFilePaths)
        //                {
        //                    if (!m_DependencyList.Contains(idlFile))
        //                    {
        //                        m_DependencyList.Add(idlFile);
        //                        List<string> extractedFilePaths = Utilities.ExtractImportedFilesAndResolvePathsFromFile(idlFile);
        //                        extractedFilePaths = extractedFilePaths.Distinct().ToList();
        //                        UpdateTheXmlAttributeIDLPath(idlFile, extractedFilePaths);
        //                        itemsToAdd.AddRange(extractedFilePaths);
        //                    }
        //                }

        //                if (itemsToAdd.Count == 0)  //If all the dependencies are already found then break here..
        //                    break;
        //                else //Else add the dependencies identified in the above level to updated list.
        //                {
        //                    updatedParsedIdlFilePaths.AddRange(itemsToAdd);
        //                    updatedParsedIdlFilePaths = updatedParsedIdlFilePaths.Distinct().ToList();
        //                }
        //            }

        //            m_DependencyList.AddRange(updatedParsedIdlFilePaths.ToList());
        //            m_DependencyList = m_DependencyList.Distinct().ToList();
        //            m_DependencyList.Sort();

        //            //UpdateTheXmlAttributeIDLPath(idlFileName, updatedParsedIdlFilePaths);
        //            //GetFullDependentsList
        //        }
        //    }
        //}

        private async Task IdentifyIDLDependencies(string idlFileName)
        {
            if (!IsValidIdlFileName(idlFileName))
            {
                return;
            }

            List<string> parsedIdlFilePaths = await DepIdentifierUtils.GetParsedIdlFilePathsAsync(idlFileName, m_selectedFilterPath);
            List<string> updatedParsedIdlFilePaths = new List<string>(parsedIdlFilePaths);
            m_DependencyList.AddRange(parsedIdlFilePaths);
            await DepIdentifierUtils.UpdateTheXmlAttributeIDLPathAsync(idlFileName, updatedParsedIdlFilePaths, m_selectedFilterPath);

            await UpdateDependenciesRecursiveAsync(updatedParsedIdlFilePaths);

            m_DependencyList = m_DependencyList.Distinct().ToList();
            m_DependencyList.Sort();

            // GetFullDependentsList
        }

        private async Task UpdateDependenciesRecursiveAsync(List<string> updatedParsedIdlFilePaths)
        {
            List<string> itemsToAdd = new List<string>();

            while (true)
            {
                itemsToAdd.Clear();

                await Parallel.ForEachAsync(updatedParsedIdlFilePaths, async (idlFile, cancellationToken) =>
                {
                    bool addedToDependencyList = false;
                    lock (m_DependencyList)
                    {
                        if (!m_DependencyList.Contains(idlFile))
                        {
                            m_DependencyList.Add(idlFile);
                            addedToDependencyList = true;
                        }
                    }

                    if (addedToDependencyList)
                    {
                        List<string> extractedFilePaths = await DepIdentifierUtils.GetParsedIdlFilePathsAsync(idlFile, m_selectedFilterPath);
                        lock (m_DependencyList)
                        {
                            itemsToAdd.AddRange(extractedFilePaths);
                        }
                    }
                });

                if (itemsToAdd.Count == 0)
                {
                    break;
                }
                else
                {
                    updatedParsedIdlFilePaths.AddRange(itemsToAdd);
                    updatedParsedIdlFilePaths = updatedParsedIdlFilePaths.Distinct().ToList();
                }
            }
        }






        //private List<string> GetParsedIdlFilePaths(string idlFileName)
        //{
        //    List<string> parsedIdlFilePaths = Utilities.ExtractImportedFilesAndResolvePathsFromFile(idlFileName);
        //    parsedIdlFilePaths = parsedIdlFilePaths.Distinct().ToList();

        //    // Update the XML attribute with IDL path information for the current idlFileName
        //    UpdateTheXmlAttributeIDLPath(idlFileName, parsedIdlFilePaths);

        //    return parsedIdlFilePaths;
        //}

        //private async Task<List<string>> GetParsedIdlFilePathsAsync(string idlFileName)
        //{
        //    List<string> parsedIdlFilePaths = await Task.Run(() => Utilities.ExtractImportedFilesAndResolvePathsFromFile(idlFileName));
        //    parsedIdlFilePaths = parsedIdlFilePaths.Distinct().ToList();

        //    await UpdateTheXmlAttributeIDLPathAsync(idlFileName, parsedIdlFilePaths);

        //    return parsedIdlFilePaths;
        //}

        //private async Task UpdateTheXmlAttributeIDLPathAsync(string idlFileName, List<string> updatedParsedIdlFilePaths)
        //{
        //    // Update the XML attribute with IDL path information asynchronously
        //    if (System.IO.File.Exists(m_XMLSFilesListResourceFileDirectoryPath))
        //    {
        //        var xmlDoc = new XmlDocument();
        //        xmlDoc.Load(m_XMLSFilesListResourceFileDirectoryPath);

        //        //Utilities.AppendNewAttribute(xmlDoc, m_selectedFilterPath.Replace("\\", "_") + "/FilePath", "IDL", string.Join(";", m_DependencyList));
        //        Utilities.UpdateTheXmlAttribute(xmlDoc, m_selectedFilterPath.Replace("\\", "_") + "/FilePath", "Name", idlFileName, "IDL", string.Join(";", idlFilePaths));
        //        Utilities.SaveXmlToFile(xmlDoc, m_XMLSFilesListResourceFileDirectoryPath);
        //    }
        //}

        private bool IsValidIdlFileName(string fileName)
        {
            return !string.IsNullOrEmpty(fileName) && fileName.Contains(".idl");
        }


        //private void UpdateDependenciesRecursive(List<string> updatedParsedIdlFilePaths)
        //{
        //    List<string> itemsToAdd = new List<string>();

        //    while (true)
        //    {
        //        itemsToAdd.Clear();

        //        foreach (string idlFile in updatedParsedIdlFilePaths)
        //        {
        //            if (!m_DependencyList.Contains(idlFile))
        //            {
        //                m_DependencyList.Add(idlFile);
        //                List<string> extractedFilePaths = GetParsedIdlFilePaths(idlFile);
        //                itemsToAdd.AddRange(extractedFilePaths);
        //            }
        //        }

        //        if (itemsToAdd.Count == 0)
        //        {
        //            break;
        //        }
        //        else
        //        {
        //            updatedParsedIdlFilePaths.AddRange(itemsToAdd);
        //            updatedParsedIdlFilePaths = updatedParsedIdlFilePaths.Distinct().ToList();
        //        }
        //    }
        //}

        private async Task UpdateTheXmlAttributeIDLPathAsync(string idlFileName, List<string> idlFilePaths)
        {
            if (System.IO.File.Exists(DepIdentifierUtils.m_FilesListXMLPath))
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.Load(DepIdentifierUtils.m_FilesListXMLPath);

                //Utilities.AppendNewAttribute(xmlDoc, m_selectedFilterPath.Replace("\\", "_") + "/FilePath", "IDL", string.Join(";", m_DependencyList));
                await DepIdentifierUtils.UpdateTheXmlAttributeAsync(xmlDoc, m_selectedFilterPath.Replace("\\", "_") + "/FilePath", "Name", idlFileName, "IDL", string.Join(";", idlFilePaths));
                await Utilities.SaveXmlToFile(xmlDoc, DepIdentifierUtils.m_FilesListXMLPath);
            }
        }

        //private void SelectAll_CheckedChanged(object sender, EventArgs e)
        //{
        //    CheckBox selectAllCheckBox = sender as CheckBox;
        //    if (selectAllCheckBox.Checked)
        //    {
        //        for (int i = 0; i < ProjectsCheckedList.Items.Count; i++)
        //        {
        //            m_SelectedFiles.Add(ProjectsCheckedList.Items[i].ToString());
        //            ProjectsCheckedList.SetItemChecked(i, true);
        //        }
        //        GetDependenciesBtn.Enabled = true;
        //    }
        //    else
        //    {
        //        GetDependenciesBtn.Enabled = false;
        //    }
        //}

        private void CopyList_Click(object sender, EventArgs e)
        {
            string joinedString = string.Join(Environment.NewLine, m_DependencyList);
            Clipboard.SetText(joinedString);
            Console.WriteLine("List of strings copied to clipboard.");
        }

        private async void generatePrerequisiteFilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Cursor.Current = Cursors.WaitCursor;
                Stopwatch stopWatch = Stopwatch.StartNew();
                stopWatch.Start();
                DialogResult result = DialogResult.Yes;
                if (File.Exists(DepIdentifierUtils.m_AllS3DDirectoriesFilePath + "AllFilesInS3Dmroot.txt") || File.Exists(DepIdentifierUtils.m_FiltersXMLPath) || File.Exists(DepIdentifierUtils.m_FilesListXMLPath))
                {
                    result = MessageBox.Show("The Prerequisite Files already exist.. Do you want to continue?", "DepIdentifier", MessageBoxButtons.YesNo);
                }
                if (result == DialogResult.Yes)
                {
                    await DepIdentifierUtils.GenerateAllS3DFilesListAndFiltersListFromPatFile();
                    DepIdentifierUtils.CreateFilesListTemplateXML();
                }
                stopWatch.Stop();
                TimeSpan elapsed = stopWatch.Elapsed;
                MessageBox.Show("Elapsed Time: " + elapsed);
                LoadFilters();
            }
            catch(Exception ex)
            {
                MessageBox.Show("Exception occurred while generating pre requisite files: " + ex.Message);
            }
            Cursor.Current = Cursors.Default;
        }
    }
}