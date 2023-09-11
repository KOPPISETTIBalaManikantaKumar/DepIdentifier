using System.Collections.Concurrent;
using System.Diagnostics;
using System.Windows.Forms;
using System.Xml;
using static System.Windows.Forms.LinkLabel;
using System.Reflection;
using System.Configuration;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System;
using System.Xml.Linq;

namespace DepIdentifier
{
    public partial class ReversePatcher : Form
    {
        bool m_AddNewFiles = false;
        bool m_InputFileFromText = false;
        bool m_ShowReferences = false;
        private static string assemblyLocation = Assembly.GetExecutingAssembly().Location;
        public static string resourcePath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(assemblyLocation), "..\\..\\..\\resources"));

        public static string m_logFilePath = Path.GetTempPath() + $"DependencyDataLog_{DateTime.Now:yyyyMMddHHmmss}.txt";
        private List<string> m_RootFilesList = new List<string>{resourcePath + "AllFilesInS3Dkroot.txt",
            resourcePath + "AllFilesInS3Dmroot.txt",
            resourcePath + "AllFilesInS3Drroot.txt",
            resourcePath + "AllFilesInS3Dsroot.txt",
            resourcePath + "AllFilesInS3Dtroot.txt",
            resourcePath + "AllFilesInS3Dxroot.txt",
            resourcePath + "AllFilesInS3Dyroot.txt",
            resourcePath + "AllFilesInS3Dyroot.txt" };

        private static string allowedExtensionsString = ConfigurationManager.AppSettings["AllowedExtensions"];
        public static List<string> m_AllowedExtensions = allowedExtensionsString.Split(new[] { "," }, StringSplitOptions.None).ToList();

        //CacheAllRootFiles
        public static List<string> cachedKrootFiles = new List<string>();
        public static List<string> cachedMrootFiles = new List<string>();
        public static List<string> cachedLrootFiles = new List<string>();
        public static List<string> cachedRrootFiles = new List<string>();
        public static List<string> cachedSrootFiles = new List<string>();
        public static List<string> cachedTrootFiles = new List<string>();
        public static List<string> cachedXrootFiles = new List<string>();
        public static List<string> cachedYrootFiles = new List<string>();
        public static bool isXMLSaved = false;


        public static Dictionary<string, List<string>> m_DependencyDictionary = new Dictionary<string, List<string>>();

        public static List<string> patcherDataLines = new List<string>();
        public static string m_PatcherFilePath = ConfigurationManager.AppSettings["PatcherFilePath"];
        public static string m_AllS3DDirectoriesFilePath = ReversePatcher.resourcePath;
        public static string m_FiltersXMLPath = ReversePatcher.resourcePath + "\\filtersdata.xml";
        public static string m_FilesListXMLPath = ReversePatcher.resourcePath + "\\filesList.xml";
        public static string m_HelpFilePath = ReversePatcher.resourcePath + "\\README.md";
        public static List<string> m_CachedFiltersData = new List<string>();

        private static string commonFilesString = ConfigurationManager.AppSettings["Commonfiles"];
        public static List<string> Commonfiles = commonFilesString.Split(new[] { "," }, StringSplitOptions.None).ToList();

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


        public static string m_selectedFilterPath = string.Empty;

        private static List<string> m_filesForWhichDependenciesNeedToBeIdentified = new List<string>();
        //[DllImport("kernel32.dll", SetLastError = true)]
        //[return: MarshalAs(UnmanagedType.Bool)]
        //static extern bool AllocConsole();

        public ReversePatcher()
        {
            InitializeComponent();
            //AllocConsole();
            CacheAllRootFiles();
            LoadFilters();
            //Recompute.Visible = false;
            RemoveFilesBtn.Visible = false;

        }

        public static void CacheAllRootFiles()
        {
            try
            {
                if (File.Exists(resourcePath + "\\AllFilesInS3Dkroot.txt"))
                    cachedKrootFiles = File.ReadAllLines(resourcePath + "\\AllFilesInS3Dkroot.txt").ToList();
                if (File.Exists(resourcePath + "\\AllFilesInS3Dmroot.txt"))
                    cachedMrootFiles = File.ReadAllLines(resourcePath + "\\AllFilesInS3Dmroot.txt").ToList();
                if (File.Exists(resourcePath + "\\AllFilesInS3Drroot.txt"))
                    cachedRrootFiles = File.ReadAllLines(resourcePath + "\\AllFilesInS3Drroot.txt").ToList();
                if (File.Exists(resourcePath + "\\AllFilesInS3Dsroot.txt"))
                    cachedSrootFiles = File.ReadAllLines(resourcePath + "\\AllFilesInS3Dsroot.txt").ToList();
                if (File.Exists(resourcePath + "\\AllFilesInS3Dtroot.txt"))
                    cachedTrootFiles = File.ReadAllLines(resourcePath + "\\AllFilesInS3Dtroot.txt").ToList();
                if (File.Exists(resourcePath + "\\AllFilesInS3Dxroot.txt"))
                    cachedXrootFiles = File.ReadAllLines(resourcePath + "\\AllFilesInS3Dxroot.txt").ToList();
                if (File.Exists(resourcePath + "\\AllFilesInS3Dyroot.txt"))
                    cachedYrootFiles = File.ReadAllLines(resourcePath + "\\AllFilesInS3Dyroot.txt").ToList();
                if (File.Exists(resourcePath + "\\AllFilesInS3Dlroot.txt"))
                    cachedLrootFiles = File.ReadAllLines(resourcePath + "\\AllFilesInS3Dlroot.txt").ToList();
            }
            catch
            {
                //Might be the files do not exist            
            }
        }

        private void LoadFilters()
        {
            if (File.Exists(m_FiltersXMLPath))
            {
                var list = MainDirectoriesInfo = XMLHelperAPIs.GetXmlData(m_FiltersXMLPath, "data/filters", "Name");
                foreach (var item in list)
                {
                    FilterCombo.Items.Add(item);
                    m_FiltersList.Add(item);
                }
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
        }

        #region TreeView related APIs

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

        #endregion

        #region event Handlers
        private void CopyList_Click(object sender, EventArgs e)
        {
            string joinedString = string.Join(Environment.NewLine, DependenciesList.Items.Cast<string>().ToList());
            Clipboard.SetText(joinedString);
            DepIdentifierUtils.WriteTextInLog("List of strings copied to clipboard.");
        }

        private async void generatePrerequisiteFilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Cursor.Current = Cursors.WaitCursor;

                Stopwatch stopWatch = Stopwatch.StartNew();
                stopWatch.Start();
                if (File.Exists(m_AllS3DDirectoriesFilePath + "AllFilesInS3Dmroot.txt") || File.Exists(m_FiltersXMLPath) || File.Exists(m_FilesListXMLPath))
                {
                    MessageBox.Show("The Prerequisite Files already exist.. ", "DepIdentifier");
                }
                else
                {
                    await PreRequisiteGenerator.GenerateAllS3DFilesListAndFiltersListFromPatFile();
                    PreRequisiteGenerator.CreateFilesListTemplateXML();
                }
                stopWatch.Stop();
                TimeSpan elapsed = stopWatch.Elapsed;
                MessageBox.Show("Elapsed Time: " + elapsed);
                LoadFilters();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception occurred while generating pre requisite files: " + ex.Message);
            }
            Cursor.Current = Cursors.Default;
        }

        private void SelectedFilesBtn_Click(object sender, EventArgs e)
        {
            m_filesForWhichDependenciesNeedToBeIdentified.Clear();
            if (ProjectsTreeView.Visible)
            {
                bool anyCheckBoxChecked = IsAnyCheckBoxChecked(ProjectsTreeView.Nodes);

                if (anyCheckBoxChecked)
                {
                    List<string> currentSelectedFilePaths = new List<string>();
                    GetCheckedFilePaths(ProjectsTreeView.Nodes, currentSelectedFilePaths);
                    SelectedFilesListBox.Items.Clear();
                    currentSelectedFilePaths.Sort();
                    SelectedFilesListBox.Items.AddRange(currentSelectedFilePaths.ToArray());
                    GetDependenciesBtn.Enabled = true;
                    MessageBox.Show($"{currentSelectedFilePaths.Count} files selected.");
                }
                else
                    GetDependenciesBtn.Enabled = false;
            }
            else if (m_AddNewFiles == true || m_InputFileFromText == true)
            {
                XMLHelperAPIs.CreateOrUpdateListXml(m_filesForWhichDependenciesNeedToBeIdentified, ReversePatcher.m_FilesListXMLPath, "filtersdata", "", "filepath");

                m_filesForWhichDependenciesNeedToBeIdentified = AddFilesRichTextBox.Text.Split("\n").ToList();
                if (m_filesForWhichDependenciesNeedToBeIdentified.Count > 0)
                {
                    List<string> resolvedFilePaths = new List<string>();
                    SelectedFilesListBox.Items.Clear();
                    foreach (var filePath in m_filesForWhichDependenciesNeedToBeIdentified)
                    {
                        string file = filePath;
                        if (!file.StartsWith("g:\\", StringComparison.OrdinalIgnoreCase))
                        {
                            file = DepIdentifierUtils.ChangeToClonedPathFromVirtual(filePath);
                            resolvedFilePaths.Add(file);
                        }
                        else
                            resolvedFilePaths.Add(file);
                    }
                    resolvedFilePaths.Sort();
                    SelectedFilesListBox.Items.AddRange(resolvedFilePaths.ToArray());
                    m_filesForWhichDependenciesNeedToBeIdentified = resolvedFilePaths;

                    GetDependenciesBtn.Enabled = true;
                    MessageBox.Show($"{m_filesForWhichDependenciesNeedToBeIdentified.Count} files selected.");
                }
                else
                    GetDependenciesBtn.Enabled = false;
            }
            else
                GetDependenciesBtn.Enabled = false;
        }

        private void GetDependencies_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            DepIdentifierUtils.WriteTextInLog($"Time start:{DateTime.Now}");
            DependenciesTree.Nodes.Clear();
            DependenciesList.Items.Clear();
            m_DependencyDictionary = new Dictionary<string, List<string>>();


            if (ProjectsTreeView.Visible == true)
            {
                List<string> currentSelectedFilePaths = new List<string>();
                GetCheckedFilePaths(ProjectsTreeView.Nodes, currentSelectedFilePaths);
                m_filesForWhichDependenciesNeedToBeIdentified = currentSelectedFilePaths;
            }
            if (m_filesForWhichDependenciesNeedToBeIdentified.Count == 0)
                return;

            RPProgressBar.Visible = true;
            RPProgressBar.Value = 0;

            RPProgressBar.Maximum = 0;
            RPProgressBar.Maximum = m_filesForWhichDependenciesNeedToBeIdentified.Count;

            DepIdentifierUtils.WriteTextInLog($"Selected files count: {m_filesForWhichDependenciesNeedToBeIdentified.Count}");
            int counter = 0;
            XmlDocument xmlDocument = XMLHelperAPIs.GetFilesListXmlDocument();

            if (m_ShowReferences == true)
            {
                foreach (var file in m_filesForWhichDependenciesNeedToBeIdentified)
                {
                    string references = XMLHelperAPIs.GetAttributeOfFilePathFromXML(xmlDocument, "Reference", file);
                    if (!string.IsNullOrEmpty(references))
                    {
                        m_DependencyDictionary.Add(file, references.Split(";").ToList());
                    }
                }
            }
            else
            {
                //progressForm.Show();

                //var progressBar = SetProgressBar(0, m_filesForWhichDependenciesNeedToBeIdentified.Count + 1);

                //Resolve the vcxproj files first if any.
                List<string> vcxProjFilesSelected = new List<string>();
                foreach (var file in m_filesForWhichDependenciesNeedToBeIdentified)
                {
                    if (string.Compare(".vcxproj", Path.GetExtension(file), StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        RPProgressBar.Increment(1);
                        DepIdentifierUtils.WriteTextInLog($"-->{counter}/{m_filesForWhichDependenciesNeedToBeIdentified.Count} Idenitying " +
                            $"{file} dependencies");
                        counter++;
                        vcxProjFilesSelected.Add(file);
                        //m_filesForWhichDependenciesNeedToBeIdentified.Remove(file);

                        List<string> dependenicesOfCurrentFile = new List<string>();
                        if (m_DependencyDictionary.Keys.Any(key => key.Equals(file, StringComparison.OrdinalIgnoreCase)))
                        {
                            continue;
                        }
                        else
                        {
                            FileDepIdentifier fileDepIdentifier = new FileDepIdentifier();
                            dependenicesOfCurrentFile = fileDepIdentifier.GetDependencyDataOfGivenFile(file, isRecompute: Recompute.Checked);
                            dependenicesOfCurrentFile = dependenicesOfCurrentFile.Select(item =>
                                                item.Contains("..") && Path.IsPathRooted(item) ?
                                                Path.GetFullPath(item) : item.ToLower())
                                                .Distinct()
                                                .ToList();
                            m_DependencyDictionary.Add(file, dependenicesOfCurrentFile);
                            fileDepIdentifier.GetFileDependenciesRecursively(dependenicesOfCurrentFile, isRecomuteChecked: Recompute.Checked);
                        }
                    }
                }

                m_filesForWhichDependenciesNeedToBeIdentified.RemoveAll(x => vcxProjFilesSelected.Contains(x) == true);


                foreach (var file in m_filesForWhichDependenciesNeedToBeIdentified)
                {
                    RPProgressBar.Increment(1);
                    counter++;
                    //Skip the other files for which we donot identify dependencies
                    if (!DepIdentifierUtils.IsFileExtensionAllowed(file))
                    {
                        //if (!m_DependencyDictionary.ContainsKey(file))
                        if (!m_DependencyDictionary.Keys.Any(key => key.Equals(file, StringComparison.OrdinalIgnoreCase)))
                            m_DependencyDictionary.Add(file, new List<string> { "No Dependencies" });
                        continue;
                    }

                    DepIdentifierUtils.WriteTextInLog($"-->{counter}/{m_filesForWhichDependenciesNeedToBeIdentified.Count}");
                    List<string> dependenicesOfCurrentFile = new List<string>();


                    if (m_DependencyDictionary.Keys.Any(key => key.Equals(file, StringComparison.OrdinalIgnoreCase)))
                    {
                        continue;
                    }
                    else
                    {
                        FileDepIdentifier fileDepIdentifier = new FileDepIdentifier();
                        dependenicesOfCurrentFile = fileDepIdentifier.GetDependencyDataOfGivenFile(file, isRecompute: Recompute.Checked);
                        dependenicesOfCurrentFile = dependenicesOfCurrentFile.Select(item =>
                                                item.Contains("..") && Path.IsPathRooted(item) ?
                                                Path.GetFullPath(item) : item.ToLower())
                                                .Distinct()
                                                .ToList();
                        m_DependencyDictionary.Add(file, dependenicesOfCurrentFile);
                        fileDepIdentifier.GetFileDependenciesRecursively(dependenicesOfCurrentFile, isRecomuteChecked: Recompute.Checked);
                    }
                }
            }
            RPProgressBar.Visible = false;

            //Display in Tree View

            //BuildDependencyTree(m_DependencyDictionary, DependenciesTree);
            List<string> dependencyListToDisplay = new List<string>();

            //foreach (var kvp in m_DependencyDictionary)
            //{
            //    if (!string.IsNullOrEmpty(kvp.Key))
            //    {
            //        TreeNode fileNode = new TreeNode(kvp.Key);
            //        foreach (string dependency in kvp.Value)
            //        {
            //            if ((!string.IsNullOrEmpty(dependency) && string.Compare(dependency, "No Dependencies", StringComparison.OrdinalIgnoreCase) != 0))
            //                fileNode.Nodes.Add(dependency);
            //        }
            //        DependenciesTree.Nodes.Add(fileNode);
            //    }
            //}

            PopulateTreeView();

            DepIdentifierUtils.WriteTextInLog($"Time End:{DateTime.Now}");

            //Display in List
            foreach (var keys in m_DependencyDictionary.Keys)
            {
                List<string> dependenciesOfCurrentKey = new List<string>();
                m_DependencyDictionary.TryGetValue(keys, out dependenciesOfCurrentKey);
                dependenciesOfCurrentKey.Sort();
                dependenciesOfCurrentKey.RemoveAll(dep => dep == "No Dependencies" || String.IsNullOrEmpty(dep));
                dependencyListToDisplay.AddRange(dependenciesOfCurrentKey);
            }
            if (dependencyListToDisplay.Count == 0)
                DependenciesList.Items.Add("No Dependencies");
            else
            {
                dependencyListToDisplay = dependencyListToDisplay.Select(item => item.ToLower())    // Convert strings to lowercase
                                                                .Distinct()                        // Remove duplicates
                                                                .ToList();
                dependencyListToDisplay.Sort();
                DependenciesList.Items.AddRange(dependencyListToDisplay.ToArray());
            }

            Cursor.Current = Cursors.Default;

            CopyList.Enabled = true;
            CopyList.Visible = true;
        }

        private void PopulateTreeView()
        {
            DependenciesTree.Nodes.Clear();

            foreach (var kvp in m_DependencyDictionary)
            {
                TreeNode mainNode = new TreeNode(kvp.Key);

                // Handle null values and exceptions safely
                if (kvp.Value != null)
                {
                    foreach (var subitem in kvp.Value)
                    {
                        if (string.Compare(subitem, "no dependencies", StringComparison.OrdinalIgnoreCase) != 0)
                            mainNode.Nodes.Add(subitem);
                    }
                }

                DependenciesTree.Nodes.Add(mainNode);
            }
        }

        public void BuildDependencyTree(Dictionary<string, List<string>> dependencies, System.Windows.Forms.TreeView treeView)
        {
            treeView.Nodes.Clear();

            // Create nodes for keys and store them in a dictionary
            Dictionary<string, TreeNode> nodeDictionary = new Dictionary<string, TreeNode>();
            foreach (var entry in dependencies)
            {
                TreeNode rootNode = new TreeNode(entry.Key);
                treeView.Nodes.Add(rootNode);
                nodeDictionary.Add(entry.Key, rootNode);
            }

            // Populate dependency nodes
            foreach (var entry in dependencies)
            {
                if (nodeDictionary.TryGetValue(entry.Key, out TreeNode rootNode))
                {
                    BuildDependencyNodes(entry.Value, rootNode, nodeDictionary);
                }
            }
        }

        private void BuildDependencyNodes(List<string> dependencies, TreeNode parentNode, Dictionary<string, TreeNode> nodeDictionary)
        {
            foreach (var dependency in dependencies)
            {
                if (string.Equals(dependency, "No Dependencies", StringComparison.OrdinalIgnoreCase))
                    continue;

                if (nodeDictionary.TryGetValue(dependency, out TreeNode node))
                {
                    TreeNode newNode = new TreeNode(node.Text);  // Create a new node with the same text
                    parentNode.Nodes.Add(newNode);
                    List<string> textList = new List<string>();

                    foreach (TreeNode childNode in node.Nodes)
                    {
                        textList.Add(childNode.Text);
                    }
                    BuildDependencyNodes(textList, newNode, nodeDictionary);  // Pass the new node's nodes
                }
            }
        }

        private async void FilterCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            string rootFilesPath = string.Empty;
            System.Windows.Forms.ComboBox comboBox = sender as System.Windows.Forms.ComboBox;
            if (comboBox.SelectedIndex >= 0)
            {
                if (m_selectedFilterPath != comboBox.SelectedItem.ToString())
                {
                    m_selectedFilterPath = comboBox.SelectedItem.ToString();
                    List<string> filesList = new List<string>();

                    filesList = DepIdentifierUtils.GetAllFilesFromSelectedRoot(DepIdentifierUtils.GetSpecificCachedRootList(m_selectedFilterPath), m_selectedFilterPath);

                    //Fill the tree nodes
                    LoadFiles(filesList);
                }
            }
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

            SelectedFilesListBox.Items.Clear();
            GetDependenciesBtn.Enabled = false;
        }

        public static System.Windows.Forms.ProgressBar SetProgressBar(int minimum, int maximum)
        {
            System.Windows.Forms.ProgressBar progressBar = new System.Windows.Forms.ProgressBar();
            progressBar.Minimum = minimum;
            progressBar.Maximum = maximum;
            progressBar.Location = new System.Drawing.Point(20, 50); // Set the location
            progressBar.Size = new System.Drawing.Size(200, 30); // Set the size
                                                                 //Controls.Add(progressBar);


            //progressBar.Value = progress;

            //System.Windows.Forms.ProgressBar progressBar = new System.Windows.Forms.ProgressBar();
            //progressBar.Location = new Point(32, 824);
            //progressBar.Name = "ProgressBar";
            //progressBar.Size = new Size(1287, 23);
            //progressBar.TabIndex = 18;

            //progressBar.Visible = true;
            //progressBar.Minimum = minimum;
            //progressBar.Maximum = maximum;
            //progressBar.Value = minimum;
            //Controls.Add(progressBar);
            return progressBar;
        }
        public static void IncrementProgressBar(System.Windows.Forms.ProgressBar progressBar, int incrementedValue)
        {
            if (incrementedValue >= progressBar.Minimum && incrementedValue <= progressBar.Maximum)
                progressBar.Value = incrementedValue;
        }
        public static void ProgressBarVisibility(System.Windows.Forms.ProgressBar progressBar, bool visible)
        {
            progressBar.Visible = visible;
        }
        #endregion

        private void addNewFilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            m_ShowReferences = false;
            m_AddNewFiles = true;
            m_InputFileFromText = false;
            ProjectsTreeView.Visible = false;
            AddFilesRichTextBox.Visible = true;
            GetDependenciesBtn.Visible = true;
            GetDependenciesBtn.Text = "Get Dependencies";
            RemoveFilesBtn.Visible = false;
            SelectedFilesBtn.Visible = true;
            SelectedFilesListBox.Visible = true;
        }

        private void selectFromFiltersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            m_ShowReferences = false;
            m_AddNewFiles = false;
            m_InputFileFromText = false;
            ProjectsTreeView.Visible = true;
            AddFilesRichTextBox.Visible = false;
            FilesList.Text = "Select the files from the below list";
            GetDependenciesBtn.Visible = true;
            GetDependenciesBtn.Text = "Get Dependencies";
            RemoveFilesBtn.Visible = false;
            SelectedFilesBtn.Visible = true;
            SelectedFilesListBox.Visible = true;
        }

        private void removeFilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            m_ShowReferences = false;
            m_AddNewFiles = false;
            m_InputFileFromText = false;
            AddFilesRichTextBox.Visible = true;
            ProjectsTreeView.Visible = false;
            FilesList.Text = "Add the files to be removed";
            SelectedFilesBtn.Enabled = false;
            GetDependenciesBtn.Visible = false;
            RemoveFilesBtn.Visible = true;
            SelectedFilesListBox.Visible = false;
        }

        private void inputFilesInTextToolStripMenuItem_Click(object sender, EventArgs e)
        {
            m_ShowReferences = false;
            m_InputFileFromText = true;
            m_AddNewFiles = false;
            AddFilesRichTextBox.Visible = true;
            ProjectsTreeView.Visible = false;
            FilesList.Text = "Add the files for which dependencies need to be identified";
            GetDependenciesBtn.Visible = true;
            GetDependenciesBtn.Text = "Get Dependencies";
            RemoveFilesBtn.Visible = false;
            SelectedFilesBtn.Visible = true;
            SelectedFilesListBox.Visible = true;
        }

        private void RemoveFilesBtn_Click(object sender, EventArgs e)
        {
            FilesListXMLModifier filesListXMLModifier = new FilesListXMLModifier();
            m_filesForWhichDependenciesNeedToBeIdentified.Clear();
            m_filesForWhichDependenciesNeedToBeIdentified = AddFilesRichTextBox.Text.Split("\n").ToList();
            filesListXMLModifier.ResolveDeletedFilesDependencies(m_filesForWhichDependenciesNeedToBeIdentified);
        }

        private void aboutReversePatcherToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HelpForm helpForm = new HelpForm();

            string textContent = File.ReadAllText(m_HelpFilePath);
            // Convert the plain text into RTF
            string rtfContent = ConvertPlainTextToRtf(textContent);
            // Set the rich text content in the HelpForm's RichTextBox
            helpForm.SetRichText(rtfContent);

            // Show the HelpForm as a dialog
            helpForm.ShowDialog();
        }

        private string ConvertPlainTextToRtf(string plainText)
        {
            // Create an RTF header with default font and formatting
            string rtfHeader = @"{\rtf1\ansi\deff0{\fonttbl{\f0 Times New Roman;}}";

            // Replace newlines with RTF line breaks and escape special characters
            string escapedText = plainText
                .Replace("\\", "\\\\")
                .Replace("{", "\\{")
                .Replace("}", "\\}")
                .Replace(Environment.NewLine, "\\par ");

            // Combine the RTF header and the escaped text
            string rtfContent = rtfHeader + escapedText + "}";

            return rtfContent;
        }

        private void showReferencesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            m_AddNewFiles = false;
            m_InputFileFromText = false;
            ProjectsTreeView.Visible = true;
            AddFilesRichTextBox.Visible = false;
            FilesList.Text = "Select the files from the below list";
            GetDependenciesBtn.Visible = true;
            RemoveFilesBtn.Visible = false;
            SelectedFilesBtn.Visible = true;
            SelectedFilesListBox.Visible = true;

            m_ShowReferences = true;
            GetDependenciesBtn.Text = "Show References";
        }
    }
}