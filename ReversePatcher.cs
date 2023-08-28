using System.Collections.Concurrent;
using System.Diagnostics;
using System.Windows.Forms;
using System.Xml;
using static System.Windows.Forms.LinkLabel;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Reflection;
using System.Configuration;

namespace DepIdentifier
{
    public partial class ReversePatcher : Form
    {

        private static string assemblyLocation = Assembly.GetExecutingAssembly().Location;
        public static string resourcePath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(assemblyLocation), "..\\..\\..\\resources"));

        public static string m_logFilePath = Path.GetTempPath() + $"DependencyDataLog_{DateTime.Now:yyyyMMddHHmmss}.txt";
        private List<string> m_RootFilesList = new List<string>{resourcePath + "AllFilesInS3Dkroot.txt",
            resourcePath + "AllFilesInS3Dmroot.txt",
            resourcePath + "AllFilesInS3Drroot.txt",
            resourcePath + "AllFilesInS3Dsroot.txt",
            resourcePath + "AllFilesInS3Dtroot.txt",
            resourcePath + "AllFilesInS3Dxroot.txt",
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


        public static Dictionary<string, List<string>> m_DependencyDictionary = new Dictionary<string, List<string>>();

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
            if (File.Exists(DepIdentifierUtils.m_FiltersXMLPath))
            {
                var list = MainDirectoriesInfo = XMLHelperAPIs.GetXmlData(DepIdentifierUtils.m_FiltersXMLPath, "data/filters", "Name");
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
                DialogResult result = DialogResult.Yes;
                if (File.Exists(DepIdentifierUtils.m_AllS3DDirectoriesFilePath + "AllFilesInS3Dmroot.txt") || File.Exists(DepIdentifierUtils.m_FiltersXMLPath) || File.Exists(DepIdentifierUtils.m_FilesListXMLPath))
                {
                    result = MessageBox.Show("The Prerequisite Files already exist.. Do you want to continue?", "DepIdentifier", MessageBoxButtons.YesNo);
                }
                if (result == DialogResult.Yes)
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

        private void GetDependencies_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            DepIdentifierUtils.WriteTextInLog($"Time start:{DateTime.Now}");
            DependenciesTree.Nodes.Clear();
            DependenciesList.Items.Clear();
            m_DependencyDictionary = new Dictionary<string, List<string>>();
            m_filesForWhichDependenciesNeedToBeIdentified.Clear();
            List<string> currentSelectedFilePaths = new List<string>();
            GetCheckedFilePaths(ProjectsTreeView.Nodes, currentSelectedFilePaths);

            m_filesForWhichDependenciesNeedToBeIdentified = currentSelectedFilePaths;

            DepIdentifierUtils.WriteTextInLog($"Selected files count: {m_filesForWhichDependenciesNeedToBeIdentified.Count}");
            int counter = 0;
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(DepIdentifierUtils.m_FilesListXMLPath);

            ProgressBar.Minimum = 0;
            ProgressBar.Maximum = m_filesForWhichDependenciesNeedToBeIdentified.Count;

            ProgressBar.Visible = true;

            //Resolve the vcxproj files first if any.
            List<string> vcxProjFilesSelected = new List<string>();
            foreach (var file in m_filesForWhichDependenciesNeedToBeIdentified)
            {
                if (string.Compare(".vcxproj", Path.GetExtension(file), StringComparison.OrdinalIgnoreCase) == 0)
                {
                    vcxProjFilesSelected.Add(file);
                    //m_filesForWhichDependenciesNeedToBeIdentified.Remove(file);

                    List<string> dependenicesOfCurrentFile = new List<string>();
                    if (m_DependencyDictionary.ContainsKey(file))
                    {
                        continue;
                    }
                    else
                    {
                        dependenicesOfCurrentFile = FileDepIdentifier.GetDependencyDataOfGivenFile(file, xmlDocument, isRecompute: Recompute.Checked);
                        m_DependencyDictionary.Add(file, dependenicesOfCurrentFile);
                        FileDepIdentifier.GetFileDependenciesRecursively(dependenicesOfCurrentFile, xmlDocument);
                    }
                }
            }

            m_filesForWhichDependenciesNeedToBeIdentified.RemoveAll(x => vcxProjFilesSelected.Contains(x) == true);

            foreach (var file in m_filesForWhichDependenciesNeedToBeIdentified)
            {
                counter++;
                if (counter != 1)
                    ProgressBar.Increment(1);

                //Skip the other files for which we donot identify dependencies
                if (!DepIdentifierUtils.IsFileExtensionAllowed(file))
                {
                    if (!m_DependencyDictionary.ContainsKey(file))
                        m_DependencyDictionary.Add(file, new List<string> { "No Dependencies" });
                    continue;
                }

                DepIdentifierUtils.WriteTextInLog($"-->{counter}/{m_filesForWhichDependenciesNeedToBeIdentified.Count}");
                List<string> dependenicesOfCurrentFile = new List<string>();


                if (m_DependencyDictionary.ContainsKey(file))
                {
                    continue;
                }
                else
                {
                    dependenicesOfCurrentFile = FileDepIdentifier.GetDependencyDataOfGivenFile(file, xmlDocument, isRecompute: Recompute.Checked);
                    m_DependencyDictionary.Add(file, dependenicesOfCurrentFile);
                    FileDepIdentifier.GetFileDependenciesRecursively(dependenicesOfCurrentFile, xmlDocument);
                }
            }

            //Display in Tree View
            List<string> dependencyListToDisplay = new List<string>();

            foreach (var kvp in m_DependencyDictionary)
            {
                if (!string.IsNullOrEmpty(kvp.Key))
                {
                    TreeNode fileNode = new TreeNode(kvp.Key);
                    foreach (string dependency in kvp.Value)
                    {
                        if ((!string.IsNullOrEmpty(dependency) && string.Compare(dependency, "No Dependencies", StringComparison.OrdinalIgnoreCase) != 0))
                            fileNode.Nodes.Add(dependency);
                    }
                    DependenciesTree.Nodes.Add(fileNode);
                }
            }

            DepIdentifierUtils.WriteTextInLog($"Time End:{DateTime.Now}");

            //Display in Tree View
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
                dependencyListToDisplay = dependencyListToDisplay.Distinct().ToList();
                dependencyListToDisplay.Sort();
                DependenciesList.Items.AddRange(dependencyListToDisplay.ToArray());
            }

            Cursor.Current = Cursors.Default;

            CopyList.Enabled = true;
            CopyList.Visible = true;
            ProgressBar.Visible = false;
        }

        private async void FilterCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            string rootFilesPath = string.Empty;
            System.Windows.Forms.ComboBox comboBox = sender as System.Windows.Forms.ComboBox;

            if (m_selectedFilterPath != comboBox.SelectedItem.ToString())
            {
                m_selectedFilterPath = comboBox.SelectedItem.ToString();
                List<string> filesList = new List<string>();

                filesList = DepIdentifierUtils.GetAllFilesFromSelectedRoot(DepIdentifierUtils.GetSpecificCachedRootList(m_selectedFilterPath), m_selectedFilterPath);

                //Fill the tree nodes
                LoadFiles(filesList);
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

        public static void SetProgressBar(int minimum, int maximum) 
        {
            ProgressBar.Visible = true;
            ProgressBar.Minimum = minimum;
            ProgressBar.Maximum = maximum;
            ProgressBar.Value = minimum;
        }
        public static void IncrementProgressBar(int incrementedValue) 
        {
            ProgressBar.Value = incrementedValue;
        }
        public static void ProgressBarVisibility(bool visible)
        {
            ProgressBar.Visible = visible;
        }
        #endregion
    }
}