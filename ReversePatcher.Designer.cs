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
            toolsToolStripMenuItem = new ToolStripMenuItem();
            selectFromFiltersToolStripMenuItem = new ToolStripMenuItem();
            inputFilesInTextToolStripMenuItem = new ToolStripMenuItem();
            addNewFilesToolStripMenuItem = new ToolStripMenuItem();
            removeFilesToolStripMenuItem = new ToolStripMenuItem();
            generatePrerequisiteFilesToolStripMenuItem = new ToolStripMenuItem();
            showReferencesToolStripMenuItem = new ToolStripMenuItem();
            helpToolStripMenuItem = new ToolStripMenuItem();
            aboutReversePatcherToolStripMenuItem = new ToolStripMenuItem();
            FilterCombo = new ComboBox();
            DependenciesList = new ListBox();
            GetDependenciesBtn = new Button();
            CopyList = new Button();
            ProjectsTreeView = new TreeView();
            SelectedFilesListBox = new ListBox();
            Recompute = new CheckBox();
            FilesList = new Label();
            DependencyListLabel = new Label();
            SelectedFilesLabel = new Label();
            DependenciesLabel = new Label();
            DependenciesTree = new TreeView();
            SelectedFilesBtn = new Button();
            FilterLabel = new Label();
            AddFilesRichTextBox = new RichTextBox();
            RPProgressBar = new ProgressBar();
            RemoveFilesBtn = new Button();
            menuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // menuStrip1
            // 
            menuStrip1.GripStyle = ToolStripGripStyle.Visible;
            menuStrip1.Items.AddRange(new ToolStripItem[] { toolsToolStripMenuItem, helpToolStripMenuItem });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(1380, 24);
            menuStrip1.TabIndex = 0;
            menuStrip1.Text = "menuStrip1";
            // 
            // toolsToolStripMenuItem
            // 
            toolsToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { selectFromFiltersToolStripMenuItem, inputFilesInTextToolStripMenuItem, addNewFilesToolStripMenuItem, removeFilesToolStripMenuItem, generatePrerequisiteFilesToolStripMenuItem, showReferencesToolStripMenuItem });
            toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
            toolsToolStripMenuItem.Size = new Size(46, 20);
            toolsToolStripMenuItem.Text = "Tools";
            // 
            // selectFromFiltersToolStripMenuItem
            // 
            selectFromFiltersToolStripMenuItem.Name = "selectFromFiltersToolStripMenuItem";
            selectFromFiltersToolStripMenuItem.Size = new Size(217, 22);
            selectFromFiltersToolStripMenuItem.Text = "Select files from filters";
            selectFromFiltersToolStripMenuItem.Click += selectFromFiltersToolStripMenuItem_Click;
            // 
            // inputFilesInTextToolStripMenuItem
            // 
            inputFilesInTextToolStripMenuItem.Name = "inputFilesInTextToolStripMenuItem";
            inputFilesInTextToolStripMenuItem.Size = new Size(217, 22);
            inputFilesInTextToolStripMenuItem.Text = "Input files in Text";
            inputFilesInTextToolStripMenuItem.Click += inputFilesInTextToolStripMenuItem_Click;
            // 
            // addNewFilesToolStripMenuItem
            // 
            addNewFilesToolStripMenuItem.Name = "addNewFilesToolStripMenuItem";
            addNewFilesToolStripMenuItem.Size = new Size(217, 22);
            addNewFilesToolStripMenuItem.Text = "Add new files";
            addNewFilesToolStripMenuItem.Click += addNewFilesToolStripMenuItem_Click;
            // 
            // removeFilesToolStripMenuItem
            // 
            removeFilesToolStripMenuItem.Name = "removeFilesToolStripMenuItem";
            removeFilesToolStripMenuItem.Size = new Size(217, 22);
            removeFilesToolStripMenuItem.Text = "Remove files";
            removeFilesToolStripMenuItem.Click += removeFilesToolStripMenuItem_Click;
            // 
            // generatePrerequisiteFilesToolStripMenuItem
            // 
            generatePrerequisiteFilesToolStripMenuItem.Name = "generatePrerequisiteFilesToolStripMenuItem";
            generatePrerequisiteFilesToolStripMenuItem.Size = new Size(217, 22);
            generatePrerequisiteFilesToolStripMenuItem.Text = "Generate Pre-requisite Files";
            generatePrerequisiteFilesToolStripMenuItem.Click += generatePrerequisiteFilesToolStripMenuItem_Click;
            // 
            // showReferencesToolStripMenuItem
            // 
            showReferencesToolStripMenuItem.Name = "showReferencesToolStripMenuItem";
            showReferencesToolStripMenuItem.Size = new Size(217, 22);
            showReferencesToolStripMenuItem.Text = "Show References";
            showReferencesToolStripMenuItem.Click += showReferencesToolStripMenuItem_Click;
            // 
            // helpToolStripMenuItem
            // 
            helpToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { aboutReversePatcherToolStripMenuItem });
            helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            helpToolStripMenuItem.Size = new Size(44, 20);
            helpToolStripMenuItem.Text = "Help";
            // 
            // aboutReversePatcherToolStripMenuItem
            // 
            aboutReversePatcherToolStripMenuItem.Name = "aboutReversePatcherToolStripMenuItem";
            aboutReversePatcherToolStripMenuItem.Size = new Size(190, 22);
            aboutReversePatcherToolStripMenuItem.Text = "About ReversePatcher";
            aboutReversePatcherToolStripMenuItem.Click += aboutReversePatcherToolStripMenuItem_Click;
            // 
            // FilterCombo
            // 
            FilterCombo.FormattingEnabled = true;
            FilterCombo.Location = new Point(120, 25);
            FilterCombo.Name = "FilterCombo";
            FilterCombo.Size = new Size(213, 22);
            FilterCombo.TabIndex = 1;
            FilterCombo.SelectedIndexChanged += FilterCombo_SelectedIndexChanged;
            // 
            // DependenciesList
            // 
            DependenciesList.FormattingEnabled = true;
            DependenciesList.ItemHeight = 14;
            DependenciesList.Location = new Point(12, 464);
            DependenciesList.Name = "DependenciesList";
            DependenciesList.SelectionMode = SelectionMode.MultiExtended;
            DependenciesList.Size = new Size(528, 326);
            DependenciesList.TabIndex = 4;
            // 
            // GetDependenciesBtn
            // 
            GetDependenciesBtn.Enabled = false;
            GetDependenciesBtn.Location = new Point(883, 51);
            GetDependenciesBtn.Name = "GetDependenciesBtn";
            GetDependenciesBtn.Size = new Size(125, 27);
            GetDependenciesBtn.TabIndex = 6;
            GetDependenciesBtn.Text = "Get Dependencies";
            GetDependenciesBtn.UseVisualStyleBackColor = true;
            GetDependenciesBtn.Click += GetDependencies_Click;
            // 
            // CopyList
            // 
            CopyList.Enabled = false;
            CopyList.Location = new Point(1164, 50);
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
            ProjectsTreeView.Size = new Size(528, 361);
            ProjectsTreeView.TabIndex = 9;
            ProjectsTreeView.AfterCheck += ProjectsTreeView_AfterCheck;
            // 
            // SelectedFilesListBox
            // 
            SelectedFilesListBox.FormattingEnabled = true;
            SelectedFilesListBox.ItemHeight = 14;
            SelectedFilesListBox.Location = new Point(572, 110);
            SelectedFilesListBox.Name = "SelectedFilesListBox";
            SelectedFilesListBox.Size = new Size(778, 214);
            SelectedFilesListBox.TabIndex = 10;
            // 
            // Recompute
            // 
            Recompute.AutoSize = true;
            Recompute.Location = new Point(339, 28);
            Recompute.Name = "Recompute";
            Recompute.Size = new Size(173, 18);
            Recompute.TabIndex = 11;
            Recompute.Text = "Re-compute Dependencies";
            Recompute.UseVisualStyleBackColor = true;
            // 
            // FilesList
            // 
            FilesList.AutoSize = true;
            FilesList.Location = new Point(12, 52);
            FilesList.Name = "FilesList";
            FilesList.Size = new Size(194, 14);
            FilesList.TabIndex = 12;
            FilesList.Tag = "";
            FilesList.Text = "Select the files from the below list";
            // 
            // DependencyListLabel
            // 
            DependencyListLabel.AutoSize = true;
            DependencyListLabel.Location = new Point(11, 440);
            DependencyListLabel.Name = "DependencyListLabel";
            DependencyListLabel.Size = new Size(151, 14);
            DependencyListLabel.TabIndex = 13;
            DependencyListLabel.Text = "Complete Dependency List";
            // 
            // SelectedFilesLabel
            // 
            SelectedFilesLabel.AutoSize = true;
            SelectedFilesLabel.Location = new Point(572, 92);
            SelectedFilesLabel.Name = "SelectedFilesLabel";
            SelectedFilesLabel.Size = new Size(84, 14);
            SelectedFilesLabel.TabIndex = 14;
            SelectedFilesLabel.Text = "Selected Files";
            // 
            // DependenciesLabel
            // 
            DependenciesLabel.AutoSize = true;
            DependenciesLabel.Location = new Point(572, 330);
            DependenciesLabel.Name = "DependenciesLabel";
            DependenciesLabel.Size = new Size(112, 14);
            DependenciesLabel.TabIndex = 15;
            DependenciesLabel.Text = "Dependencies Tree";
            // 
            // DependenciesTree
            // 
            DependenciesTree.Location = new Point(572, 352);
            DependenciesTree.Name = "DependenciesTree";
            DependenciesTree.Size = new Size(778, 446);
            DependenciesTree.TabIndex = 16;
            // 
            // SelectedFilesBtn
            // 
            SelectedFilesBtn.Location = new Point(572, 50);
            SelectedFilesBtn.Name = "SelectedFilesBtn";
            SelectedFilesBtn.Size = new Size(140, 27);
            SelectedFilesBtn.TabIndex = 17;
            SelectedFilesBtn.Text = "Show Selected Files";
            SelectedFilesBtn.UseVisualStyleBackColor = true;
            SelectedFilesBtn.Click += SelectedFilesBtn_Click;
            // 
            // FilterLabel
            // 
            FilterLabel.AutoSize = true;
            FilterLabel.Location = new Point(18, 29);
            FilterLabel.Name = "FilterLabel";
            FilterLabel.Size = new Size(89, 14);
            FilterLabel.TabIndex = 18;
            FilterLabel.Text = "Select the filter";
            // 
            // AddFilesRichTextBox
            // 
            AddFilesRichTextBox.Location = new Point(12, 69);
            AddFilesRichTextBox.Name = "AddFilesRichTextBox";
            AddFilesRichTextBox.Size = new Size(529, 368);
            AddFilesRichTextBox.TabIndex = 19;
            AddFilesRichTextBox.Text = "";
            AddFilesRichTextBox.Visible = false;
            // 
            // RPProgressBar
            // 
            RPProgressBar.Location = new Point(48, 822);
            RPProgressBar.Name = "RPProgressBar";
            RPProgressBar.Size = new Size(1269, 23);
            RPProgressBar.TabIndex = 20;
            RPProgressBar.Visible = false;
            // 
            // RemoveFilesBtn
            // 
            RemoveFilesBtn.Location = new Point(718, 50);
            RemoveFilesBtn.Name = "RemoveFilesBtn";
            RemoveFilesBtn.Size = new Size(125, 26);
            RemoveFilesBtn.TabIndex = 21;
            RemoveFilesBtn.Text = "Remove Files";
            RemoveFilesBtn.UseVisualStyleBackColor = true;
            RemoveFilesBtn.Click += RemoveFilesBtn_Click;
            // 
            // ReversePatcher
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            AutoScroll = true;
            AutoSize = true;
            BackColor = SystemColors.ButtonFace;
            ClientSize = new Size(1380, 859);
            Controls.Add(RemoveFilesBtn);
            Controls.Add(RPProgressBar);
            Controls.Add(AddFilesRichTextBox);
            Controls.Add(FilterLabel);
            Controls.Add(SelectedFilesBtn);
            Controls.Add(DependenciesTree);
            Controls.Add(DependenciesLabel);
            Controls.Add(SelectedFilesLabel);
            Controls.Add(DependencyListLabel);
            Controls.Add(FilesList);
            Controls.Add(Recompute);
            Controls.Add(SelectedFilesListBox);
            Controls.Add(ProjectsTreeView);
            Controls.Add(CopyList);
            Controls.Add(GetDependenciesBtn);
            Controls.Add(DependenciesList);
            Controls.Add(FilterCombo);
            Controls.Add(menuStrip1);
            Font = new Font("Calibri", 9F, FontStyle.Regular, GraphicsUnit.Point);
            FormBorderStyle = FormBorderStyle.Fixed3D;
            MainMenuStrip = menuStrip1;
            MaximizeBox = false;
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





        #endregion

        private MenuStrip menuStrip1;
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
        private Label FilesList;
        private Label DependencyListLabel;
        private Label SelectedFilesLabel;
        private Label DependenciesLabel;
        private TreeView DependenciesTree;
        private Button SelectedFilesBtn;
        private Label FilterLabel;
        private ToolStripMenuItem addNewFilesToolStripMenuItem;
        private RichTextBox AddFilesRichTextBox;
        private ProgressBar RPProgressBar;
        private ToolStripMenuItem removeFilesToolStripMenuItem;
        private ToolStripMenuItem selectFromFiltersToolStripMenuItem;
        private ToolStripMenuItem inputFilesInTextToolStripMenuItem;
        private Button RemoveFilesBtn;
        private ToolStripMenuItem aboutReversePatcherToolStripMenuItem;
        private ToolStripMenuItem showReferencesToolStripMenuItem;
    }
}