namespace LDD.BrickEditor.UI.Settings
{
    partial class EditorSettingsPanel
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EditorSettingsPanel));
            this.BuildConfigsGroupBox = new System.Windows.Forms.GroupBox();
            this.BuildCfgSplitContainer = new System.Windows.Forms.SplitContainer();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.DelBuildCfgBtn = new System.Windows.Forms.Button();
            this.AddBuildCfgBtn = new System.Windows.Forms.Button();
            this.BuildConfigListView = new System.Windows.Forms.ListView();
            this.BuildCfgNameColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.BuildCfgNameLabel = new LDD.BrickEditor.UI.Controls.ControlLabel();
            this.BuildCfg_NameBox = new System.Windows.Forms.TextBox();
            this.SaveBuildCfgBtn = new System.Windows.Forms.Button();
            this.CancelBuildCfgBtn = new System.Windows.Forms.Button();
            this.BuildCfg_Lod0Chk = new System.Windows.Forms.CheckBox();
            this.BuildCfg_PathBox = new LDD.BrickEditor.UI.Controls.BrowseTextBox();
            this.BuildCfgPathLabel = new System.Windows.Forms.Label();
            this.BuildCfg_OverwriteChk = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.LanguageNoteLabel = new System.Windows.Forms.Label();
            this.LanguageLabel = new System.Windows.Forms.Label();
            this.LanguageCombo = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.BackupIntervalBox = new System.Windows.Forms.NumericUpDown();
            this.UsernameOptionalLabel = new System.Windows.Forms.Label();
            this.UsernameLabel = new System.Windows.Forms.Label();
            this.UsernameTextbox = new System.Windows.Forms.TextBox();
            this.WorkspaceBrowseBox = new LDD.BrickEditor.UI.Controls.BrowseTextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.BuildConfigsGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.BuildCfgSplitContainer)).BeginInit();
            this.BuildCfgSplitContainer.Panel1.SuspendLayout();
            this.BuildCfgSplitContainer.Panel2.SuspendLayout();
            this.BuildCfgSplitContainer.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.BuildCfgNameLabel)).BeginInit();
            this.BuildCfgNameLabel.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.BackupIntervalBox)).BeginInit();
            this.SuspendLayout();
            // 
            // BuildConfigsGroupBox
            // 
            resources.ApplyResources(this.BuildConfigsGroupBox, "BuildConfigsGroupBox");
            this.BuildConfigsGroupBox.Controls.Add(this.BuildCfgSplitContainer);
            this.BuildConfigsGroupBox.Name = "BuildConfigsGroupBox";
            this.BuildConfigsGroupBox.TabStop = false;
            // 
            // BuildCfgSplitContainer
            // 
            resources.ApplyResources(this.BuildCfgSplitContainer, "BuildCfgSplitContainer");
            this.BuildCfgSplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.BuildCfgSplitContainer.Name = "BuildCfgSplitContainer";
            // 
            // BuildCfgSplitContainer.Panel1
            // 
            this.BuildCfgSplitContainer.Panel1.Controls.Add(this.tableLayoutPanel1);
            // 
            // BuildCfgSplitContainer.Panel2
            // 
            this.BuildCfgSplitContainer.Panel2.Controls.Add(this.BuildCfgNameLabel);
            this.BuildCfgSplitContainer.Panel2.Controls.Add(this.SaveBuildCfgBtn);
            this.BuildCfgSplitContainer.Panel2.Controls.Add(this.CancelBuildCfgBtn);
            this.BuildCfgSplitContainer.Panel2.Controls.Add(this.BuildCfg_Lod0Chk);
            this.BuildCfgSplitContainer.Panel2.Controls.Add(this.BuildCfg_PathBox);
            this.BuildCfgSplitContainer.Panel2.Controls.Add(this.BuildCfgPathLabel);
            this.BuildCfgSplitContainer.Panel2.Controls.Add(this.BuildCfg_OverwriteChk);
            // 
            // tableLayoutPanel1
            // 
            resources.ApplyResources(this.tableLayoutPanel1, "tableLayoutPanel1");
            this.tableLayoutPanel1.Controls.Add(this.DelBuildCfgBtn, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.AddBuildCfgBtn, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.BuildConfigListView, 0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            // 
            // DelBuildCfgBtn
            // 
            resources.ApplyResources(this.DelBuildCfgBtn, "DelBuildCfgBtn");
            this.DelBuildCfgBtn.Name = "DelBuildCfgBtn";
            this.DelBuildCfgBtn.UseVisualStyleBackColor = true;
            this.DelBuildCfgBtn.Click += new System.EventHandler(this.DelBuildCfgBtn_Click);
            // 
            // AddBuildCfgBtn
            // 
            resources.ApplyResources(this.AddBuildCfgBtn, "AddBuildCfgBtn");
            this.AddBuildCfgBtn.Name = "AddBuildCfgBtn";
            this.AddBuildCfgBtn.UseVisualStyleBackColor = true;
            this.AddBuildCfgBtn.Click += new System.EventHandler(this.AddBuildCfgBtn_Click);
            // 
            // BuildConfigListView
            // 
            resources.ApplyResources(this.BuildConfigListView, "BuildConfigListView");
            this.BuildConfigListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.BuildCfgNameColumn});
            this.tableLayoutPanel1.SetColumnSpan(this.BuildConfigListView, 2);
            this.BuildConfigListView.FullRowSelect = true;
            this.BuildConfigListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.BuildConfigListView.HideSelection = false;
            this.BuildConfigListView.MultiSelect = false;
            this.BuildConfigListView.Name = "BuildConfigListView";
            this.BuildConfigListView.UseCompatibleStateImageBehavior = false;
            this.BuildConfigListView.View = System.Windows.Forms.View.Details;
            this.BuildConfigListView.SelectedIndexChanged += new System.EventHandler(this.BuildConfigListView_SelectedIndexChanged);
            // 
            // BuildCfgNameColumn
            // 
            resources.ApplyResources(this.BuildCfgNameColumn, "BuildCfgNameColumn");
            // 
            // BuildCfgNameLabel
            // 
            resources.ApplyResources(this.BuildCfgNameLabel, "BuildCfgNameLabel");
            this.BuildCfgNameLabel.Controls.Add(this.BuildCfg_NameBox);
            this.BuildCfgNameLabel.Name = "BuildCfgNameLabel";
            // 
            // BuildCfg_NameBox
            // 
            resources.ApplyResources(this.BuildCfg_NameBox, "BuildCfg_NameBox");
            this.BuildCfg_NameBox.Name = "BuildCfg_NameBox";
            this.BuildCfg_NameBox.TextChanged += new System.EventHandler(this.BuildCfgProperty_ValueChanged);
            // 
            // SaveBuildCfgBtn
            // 
            resources.ApplyResources(this.SaveBuildCfgBtn, "SaveBuildCfgBtn");
            this.SaveBuildCfgBtn.Name = "SaveBuildCfgBtn";
            this.SaveBuildCfgBtn.UseVisualStyleBackColor = true;
            this.SaveBuildCfgBtn.Click += new System.EventHandler(this.SaveBuildCfgBtn_Click);
            // 
            // CancelBuildCfgBtn
            // 
            resources.ApplyResources(this.CancelBuildCfgBtn, "CancelBuildCfgBtn");
            this.CancelBuildCfgBtn.Name = "CancelBuildCfgBtn";
            this.CancelBuildCfgBtn.UseVisualStyleBackColor = true;
            this.CancelBuildCfgBtn.Click += new System.EventHandler(this.CancelBuildCfgBtn_Click);
            // 
            // BuildCfg_Lod0Chk
            // 
            resources.ApplyResources(this.BuildCfg_Lod0Chk, "BuildCfg_Lod0Chk");
            this.BuildCfg_Lod0Chk.Name = "BuildCfg_Lod0Chk";
            this.BuildCfg_Lod0Chk.UseVisualStyleBackColor = true;
            this.BuildCfg_Lod0Chk.CheckedChanged += new System.EventHandler(this.BuildCfgProperty_ValueChanged);
            // 
            // BuildCfg_PathBox
            // 
            resources.ApplyResources(this.BuildCfg_PathBox, "BuildCfg_PathBox");
            this.BuildCfg_PathBox.AutoSizeButton = true;
            this.BuildCfg_PathBox.ButtonWidth = 26;
            this.BuildCfg_PathBox.Name = "BuildCfg_PathBox";
            this.BuildCfg_PathBox.ReadOnly = true;
            this.BuildCfg_PathBox.Value = "";
            this.BuildCfg_PathBox.BrowseButtonClicked += new System.EventHandler(this.BuildCfg_PathBox_BrowseButtonClicked);
            this.BuildCfg_PathBox.ValueChanged += new System.EventHandler(this.BuildCfgProperty_ValueChanged);
            // 
            // BuildCfgPathLabel
            // 
            resources.ApplyResources(this.BuildCfgPathLabel, "BuildCfgPathLabel");
            this.BuildCfgPathLabel.Name = "BuildCfgPathLabel";
            // 
            // BuildCfg_OverwriteChk
            // 
            resources.ApplyResources(this.BuildCfg_OverwriteChk, "BuildCfg_OverwriteChk");
            this.BuildCfg_OverwriteChk.Name = "BuildCfg_OverwriteChk";
            this.BuildCfg_OverwriteChk.UseVisualStyleBackColor = true;
            this.BuildCfg_OverwriteChk.CheckStateChanged += new System.EventHandler(this.BuildCfgProperty_ValueChanged);
            // 
            // groupBox1
            // 
            resources.ApplyResources(this.groupBox1, "groupBox1");
            this.groupBox1.Controls.Add(this.LanguageNoteLabel);
            this.groupBox1.Controls.Add(this.LanguageLabel);
            this.groupBox1.Controls.Add(this.LanguageCombo);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.BackupIntervalBox);
            this.groupBox1.Controls.Add(this.UsernameOptionalLabel);
            this.groupBox1.Controls.Add(this.UsernameLabel);
            this.groupBox1.Controls.Add(this.UsernameTextbox);
            this.groupBox1.Controls.Add(this.WorkspaceBrowseBox);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.TabStop = false;
            // 
            // LanguageNoteLabel
            // 
            resources.ApplyResources(this.LanguageNoteLabel, "LanguageNoteLabel");
            this.LanguageNoteLabel.Name = "LanguageNoteLabel";
            // 
            // LanguageLabel
            // 
            resources.ApplyResources(this.LanguageLabel, "LanguageLabel");
            this.LanguageLabel.Name = "LanguageLabel";
            // 
            // LanguageCombo
            // 
            this.LanguageCombo.FormattingEnabled = true;
            resources.ApplyResources(this.LanguageCombo, "LanguageCombo");
            this.LanguageCombo.Name = "LanguageCombo";
            this.LanguageCombo.SelectedIndexChanged += new System.EventHandler(this.LanguageCombo_SelectedIndexChanged);
            // 
            // label6
            // 
            resources.ApplyResources(this.label6, "label6");
            this.label6.Name = "label6";
            // 
            // label5
            // 
            resources.ApplyResources(this.label5, "label5");
            this.label5.Name = "label5";
            // 
            // BackupIntervalBox
            // 
            resources.ApplyResources(this.BackupIntervalBox, "BackupIntervalBox");
            this.BackupIntervalBox.Name = "BackupIntervalBox";
            this.BackupIntervalBox.Value = new decimal(new int[] {
            60,
            0,
            0,
            0});
            // 
            // UsernameOptionalLabel
            // 
            resources.ApplyResources(this.UsernameOptionalLabel, "UsernameOptionalLabel");
            this.UsernameOptionalLabel.Name = "UsernameOptionalLabel";
            // 
            // UsernameLabel
            // 
            resources.ApplyResources(this.UsernameLabel, "UsernameLabel");
            this.UsernameLabel.Name = "UsernameLabel";
            // 
            // UsernameTextbox
            // 
            resources.ApplyResources(this.UsernameTextbox, "UsernameTextbox");
            this.UsernameTextbox.Name = "UsernameTextbox";
            // 
            // WorkspaceBrowseBox
            // 
            resources.ApplyResources(this.WorkspaceBrowseBox, "WorkspaceBrowseBox");
            this.WorkspaceBrowseBox.AutoSizeButton = true;
            this.WorkspaceBrowseBox.ButtonWidth = 26;
            this.WorkspaceBrowseBox.Name = "WorkspaceBrowseBox";
            this.WorkspaceBrowseBox.Value = "";
            this.WorkspaceBrowseBox.BrowseButtonClicked += new System.EventHandler(this.WorkspaceBrowseBox_BrowseButtonClicked);
            // 
            // label3
            // 
            resources.ApplyResources(this.label3, "label3");
            this.label3.Name = "label3";
            // 
            // EditorSettingsPanel
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.BuildConfigsGroupBox);
            this.Controls.Add(this.groupBox1);
            this.Name = "EditorSettingsPanel";
            this.BuildConfigsGroupBox.ResumeLayout(false);
            this.BuildCfgSplitContainer.Panel1.ResumeLayout(false);
            this.BuildCfgSplitContainer.Panel2.ResumeLayout(false);
            this.BuildCfgSplitContainer.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.BuildCfgSplitContainer)).EndInit();
            this.BuildCfgSplitContainer.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.BuildCfgNameLabel)).EndInit();
            this.BuildCfgNameLabel.ResumeLayout(false);
            this.BuildCfgNameLabel.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.BackupIntervalBox)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox BuildConfigsGroupBox;
        private System.Windows.Forms.SplitContainer BuildCfgSplitContainer;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Button DelBuildCfgBtn;
        private System.Windows.Forms.Button AddBuildCfgBtn;
        private System.Windows.Forms.ListView BuildConfigListView;
        private System.Windows.Forms.ColumnHeader BuildCfgNameColumn;
        private System.Windows.Forms.Button CancelBuildCfgBtn;
        private System.Windows.Forms.CheckBox BuildCfg_OverwriteChk;
        private System.Windows.Forms.Button SaveBuildCfgBtn;
        private Controls.BrowseTextBox BuildCfg_PathBox;
        private System.Windows.Forms.TextBox BuildCfg_NameBox;
        private System.Windows.Forms.CheckBox BuildCfg_Lod0Chk;
        private System.Windows.Forms.Label BuildCfgPathLabel;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.NumericUpDown BackupIntervalBox;
        private System.Windows.Forms.Label UsernameOptionalLabel;
        private System.Windows.Forms.Label UsernameLabel;
        private System.Windows.Forms.TextBox UsernameTextbox;
        private Controls.BrowseTextBox WorkspaceBrowseBox;
        private System.Windows.Forms.Label label3;
        private Controls.ControlLabel BuildCfgNameLabel;
        private System.Windows.Forms.Label LanguageLabel;
        private System.Windows.Forms.ComboBox LanguageCombo;
        private System.Windows.Forms.Label LanguageNoteLabel;
    }
}
