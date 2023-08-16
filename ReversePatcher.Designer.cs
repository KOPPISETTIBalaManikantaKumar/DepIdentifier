using System.Xml;

namespace DepIdentifier
{
    partial class ReversePatcher
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        /// 

        private List<string> MainDirectoriesInfo = new List<string>();

        private static string m_CurrentDirectoryPath = System.IO.Path.GetFullPath(System.IO.Path.Combine(Directory.GetCurrentDirectory(), @"..\..\..\"));
        private string m_XMLSDirectoryPath = m_CurrentDirectoryPath + "resource";
        //private string m_XMLSResourceFileDirectoryPath = m_CurrentDirectoryPath + "resource" + @"\Res.xml";
        //public static string m_XMLSFilesListResourceFileDirectoryPath = m_CurrentDirectoryPath + "resource" + @"\FilesList.xml";
        private List<string> m_SelectedFiles = new List<string>();
        private static List<string> m_DependencyList = new List<string>();
        public List<string> m_failedFileList = new List<string>();
        private System.ComponentModel.IContainer components = null;
        private List<string> m_FiltersList = new List<string>();

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            menuStrip1 = new MenuStrip();
            fileToolStripMenuItem = new ToolStripMenuItem();
            toolsToolStripMenuItem = new ToolStripMenuItem();
            generatePrerequisiteFilesToolStripMenuItem = new ToolStripMenuItem();
            helpToolStripMenuItem = new ToolStripMenuItem();
            FilterCombo = new ComboBox();
            DependenciesList = new ListBox();
            GetDependenciesBtn = new Button();
            CopyList = new Button();
            ProjectsTreeView = new TreeView();
            SelectedFilesListBox = new ListBox();
            Recompute = new CheckBox();
            menuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // menuStrip1
            // 
            menuStrip1.GripStyle = ToolStripGripStyle.Visible;
            menuStrip1.Items.AddRange(new ToolStripItem[] { fileToolStripMenuItem, toolsToolStripMenuItem, helpToolStripMenuItem });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(1098, 24);
            menuStrip1.TabIndex = 0;
            menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            fileToolStripMenuItem.Size = new Size(37, 20);
            fileToolStripMenuItem.Text = "File";
            // 
            // toolsToolStripMenuItem
            // 
            toolsToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { generatePrerequisiteFilesToolStripMenuItem });
            toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
            toolsToolStripMenuItem.Size = new Size(46, 20);
            toolsToolStripMenuItem.Text = "Tools";
            // 
            // generatePrerequisiteFilesToolStripMenuItem
            // 
            generatePrerequisiteFilesToolStripMenuItem.Name = "generatePrerequisiteFilesToolStripMenuItem";
            generatePrerequisiteFilesToolStripMenuItem.Size = new Size(217, 22);
            generatePrerequisiteFilesToolStripMenuItem.Text = "Generate Pre-requisite Files";
            generatePrerequisiteFilesToolStripMenuItem.Click += generatePrerequisiteFilesToolStripMenuItem_Click;
            // 
            // helpToolStripMenuItem
            // 
            helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            helpToolStripMenuItem.Size = new Size(44, 20);
            helpToolStripMenuItem.Text = "Help";
            // 
            // FilterCombo
            // 
            FilterCombo.FormattingEnabled = true;
            FilterCombo.Location = new Point(12, 24);
            FilterCombo.Name = "FilterCombo";
            FilterCombo.Size = new Size(213, 23);
            FilterCombo.TabIndex = 1;
            FilterCombo.SelectedIndexChanged += FilterCombo_SelectedIndexChanged;
            // 
            // DependenciesList
            // 
            DependenciesList.FormattingEnabled = true;
            DependenciesList.ItemHeight = 15;
            DependenciesList.Location = new Point(573, 328);
            DependenciesList.Name = "DependenciesList";
            DependenciesList.SelectionMode = SelectionMode.MultiExtended;
            DependenciesList.Size = new Size(513, 409);
            DependenciesList.TabIndex = 4;
            // 
            // GetDependenciesBtn
            // 
            GetDependenciesBtn.Enabled = false;
            GetDependenciesBtn.Location = new Point(573, 52);
            GetDependenciesBtn.Name = "GetDependenciesBtn";
            GetDependenciesBtn.Size = new Size(125, 23);
            GetDependenciesBtn.TabIndex = 6;
            GetDependenciesBtn.Text = "Get Dependencies";
            GetDependenciesBtn.UseVisualStyleBackColor = true;
            GetDependenciesBtn.Click += GetDependencies_Click;
            // 
            // CopyList
            // 
            CopyList.Enabled = false;
            CopyList.Location = new Point(929, 50);
            CopyList.Name = "CopyList";
            CopyList.Size = new Size(121, 27);
            CopyList.TabIndex = 8;
            CopyList.Text = "Copy to Clipboard";
            CopyList.UseVisualStyleBackColor = true;
            CopyList.Visible = false;
            CopyList.Click += CopyList_Click;
            // 
            // ProjectsTreeView
            // 
            ProjectsTreeView.CheckBoxes = true;
            ProjectsTreeView.Location = new Point(12, 69);
            ProjectsTreeView.Name = "ProjectsTreeView";
            ProjectsTreeView.Size = new Size(528, 668);
            ProjectsTreeView.TabIndex = 9;
            ProjectsTreeView.AfterCheck += ProjectsTreeView_AfterCheck;
            // 
            // SelectedFilesListBox
            // 
            SelectedFilesListBox.FormattingEnabled = true;
            SelectedFilesListBox.ItemHeight = 15;
            SelectedFilesListBox.Location = new Point(573, 104);
            SelectedFilesListBox.Name = "SelectedFilesListBox";
            SelectedFilesListBox.Size = new Size(513, 214);
            SelectedFilesListBox.TabIndex = 10;
            // 
            // Recompute
            // 
            Recompute.AutoSize = true;
            Recompute.Location = new Point(252, 28);
            Recompute.Name = "Recompute";
            Recompute.Size = new Size(169, 19);
            Recompute.TabIndex = 11;
            Recompute.Text = "Re-compute Dependencies";
            Recompute.UseVisualStyleBackColor = true;
            // 
            // ReversePatcher
            // 
            AutoScaleMode = AutoScaleMode.None;
            AutoScroll = true;
            ClientSize = new Size(1098, 810);
            Controls.Add(Recompute);
            Controls.Add(SelectedFilesListBox);
            Controls.Add(ProjectsTreeView);
            Controls.Add(CopyList);
            Controls.Add(GetDependenciesBtn);
            Controls.Add(DependenciesList);
            Controls.Add(FilterCombo);
            Controls.Add(menuStrip1);
            MainMenuStrip = menuStrip1;
            Name = "ReversePatcher";
            Text = "ReversePatcher";
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        static bool AreListsEqualInAnyOrder(List<string> list1, List<string> list2)
        {
            HashSet<string> set1 = new HashSet<string>(list1);
            HashSet<string> set2 = new HashSet<string>(list2);
            return set1.SetEquals(set2);
        }

        private void ProjectsCheckedList_SelectedIndexChanged(object sender, EventArgs e)
        {
            List<string> selectedFiles = new List<string>();
            CheckedListBox checkedListBox = sender as CheckedListBox;
            if (checkedListBox.CheckedItems.Count > 0)
            {
                GetDependenciesBtn.Enabled = true;
            }
            foreach (var item in checkedListBox.CheckedItems)
            {
                selectedFiles.Add(item.ToString());
            }

            bool isSelectionSame = AreListsEqualInAnyOrder(selectedFiles, m_SelectedFiles);
            if (!isSelectionSame)
            {
                m_DependencyList.Clear();
                DependenciesList.Items.Clear();
                m_SelectedFiles = selectedFiles;
            }
        }

        private void LoadFilters()
        {
            if (File.Exists(DepIdentifierUtils.m_FiltersXMLPath))
            {
                var list = MainDirectoriesInfo = GetXmlData(DepIdentifierUtils.m_FiltersXMLPath, "data/filters", "Name");
                foreach (var item in list)
                {
                    FilterCombo.Items.Add(item);
                    m_FiltersList.Add(item);
                }
            }
        }

        private List<string> GetXmlData(string xml, string node, string attribute)
        {
            List<string> filters = new List<string>();

            XmlDocument xmlDoc = new XmlDocument();
            if (File.Exists(xml))
            {
                xmlDoc.Load(xml);

                XmlNode xmlNode = xmlDoc.SelectSingleNode(node);
                XmlNodeList xmlNodeList = xmlNode.ChildNodes;

                foreach (XmlNode filterNode in xmlNodeList)
                {
                    string filterName = filterNode.Attributes[attribute].InnerXml;
                    filters.Add(filterName);
                }
            }
            else
            {
                throw new Exception("Xml file not found..!");
            }
            return filters;
        }

        #endregion

        private MenuStrip menuStrip1;
        private ToolStripMenuItem fileToolStripMenuItem;
        private ToolStripMenuItem toolsToolStripMenuItem;
        private ToolStripMenuItem helpToolStripMenuItem;
        private ComboBox FilterCombo;
        private ListBox DependenciesList;
        private Button GetDependenciesBtn;
        private Button CopyList;
        private TreeView ProjectsTreeView;
        private ListBox SelectedFilesListBox;
        private ToolStripMenuItem generatePrerequisiteFilesToolStripMenuItem;
        private CheckBox Recompute;
    }
}