namespace LDD.BrickEditor.UI.Windows
{
    partial class ConfigureOutlinesDialog
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ConfigureOutlinesDialog));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.AddGroupButton = new System.Windows.Forms.ToolStripButton();
            this.RemoveGroupButton = new System.Windows.Forms.ToolStripButton();
            this.controlLabel2 = new LDD.BrickEditor.UI.Controls.ControlLabel();
            this.ThicknessBox = new LDD.BrickEditor.UI.Controls.NumberTextBox();
            this.NoOutlineCheckBox = new System.Windows.Forms.CheckBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.AvailableMeshList = new BrightIdeasSoftware.ObjectListView();
            this.GroupMeshList = new BrightIdeasSoftware.ObjectListView();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.BreakAngleLabel = new LDD.BrickEditor.UI.Controls.ControlLabel();
            this.BreakAngleBox = new LDD.BrickEditor.UI.Controls.NumberTextBox();
            this.controlLabel1 = new LDD.BrickEditor.UI.Controls.ControlLabel();
            this.GroupNameBox = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.controlLabel2)).BeginInit();
            this.controlLabel2.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.AvailableMeshList)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.GroupMeshList)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.BreakAngleLabel)).BeginInit();
            this.BreakAngleLabel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.controlLabel1)).BeginInit();
            this.controlLabel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            resources.ApplyResources(this.splitContainer1, "splitContainer1");
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.toolStrip1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.controlLabel2);
            this.splitContainer1.Panel2.Controls.Add(this.NoOutlineCheckBox);
            this.splitContainer1.Panel2.Controls.Add(this.tableLayoutPanel1);
            this.splitContainer1.Panel2.Controls.Add(this.BreakAngleLabel);
            this.splitContainer1.Panel2.Controls.Add(this.controlLabel1);
            // 
            // toolStrip1
            // 
            this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.AddGroupButton,
            this.RemoveGroupButton});
            resources.ApplyResources(this.toolStrip1, "toolStrip1");
            this.toolStrip1.Name = "toolStrip1";
            // 
            // AddGroupButton
            // 
            this.AddGroupButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            resources.ApplyResources(this.AddGroupButton, "AddGroupButton");
            this.AddGroupButton.Name = "AddGroupButton";
            // 
            // RemoveGroupButton
            // 
            this.RemoveGroupButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            resources.ApplyResources(this.RemoveGroupButton, "RemoveGroupButton");
            this.RemoveGroupButton.Name = "RemoveGroupButton";
            // 
            // controlLabel2
            // 
            this.controlLabel2.Controls.Add(this.ThicknessBox);
            resources.ApplyResources(this.controlLabel2, "controlLabel2");
            this.controlLabel2.MatchSiblingLabels = true;
            this.controlLabel2.Name = "controlLabel2";
            // 
            // ThicknessBox
            // 
            resources.ApplyResources(this.ThicknessBox, "ThicknessBox");
            this.ThicknessBox.MaximumValue = 2D;
            this.ThicknessBox.MinimumValue = 0.1D;
            this.ThicknessBox.Name = "ThicknessBox";
            this.ThicknessBox.Value = 1D;
            // 
            // NoOutlineCheckBox
            // 
            resources.ApplyResources(this.NoOutlineCheckBox, "NoOutlineCheckBox");
            this.NoOutlineCheckBox.Name = "NoOutlineCheckBox";
            this.NoOutlineCheckBox.UseVisualStyleBackColor = true;
            this.NoOutlineCheckBox.CheckedChanged += new System.EventHandler(this.NoOutlineCheckBox_CheckedChanged);
            // 
            // tableLayoutPanel1
            // 
            resources.ApplyResources(this.tableLayoutPanel1, "tableLayoutPanel1");
            this.tableLayoutPanel1.Controls.Add(this.AvailableMeshList, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.GroupMeshList, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.label2, 1, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            // 
            // AvailableMeshList
            // 
            this.AvailableMeshList.CellEditUseWholeCell = false;
            this.AvailableMeshList.Cursor = System.Windows.Forms.Cursors.Default;
            resources.ApplyResources(this.AvailableMeshList, "AvailableMeshList");
            this.AvailableMeshList.FullRowSelect = true;
            this.AvailableMeshList.HideSelection = false;
            this.AvailableMeshList.IsSimpleDragSource = true;
            this.AvailableMeshList.Name = "AvailableMeshList";
            this.AvailableMeshList.UseCompatibleStateImageBehavior = false;
            this.AvailableMeshList.View = System.Windows.Forms.View.Details;
            this.AvailableMeshList.FormatRow += new System.EventHandler<BrightIdeasSoftware.FormatRowEventArgs>(this.AvailableMeshList_FormatRow);
            // 
            // GroupMeshList
            // 
            this.GroupMeshList.CellEditUseWholeCell = false;
            this.GroupMeshList.Cursor = System.Windows.Forms.Cursors.Default;
            resources.ApplyResources(this.GroupMeshList, "GroupMeshList");
            this.GroupMeshList.FullRowSelect = true;
            this.GroupMeshList.HideSelection = false;
            this.GroupMeshList.IsSimpleDragSource = true;
            this.GroupMeshList.IsSimpleDropSink = true;
            this.GroupMeshList.Name = "GroupMeshList";
            this.GroupMeshList.UseCompatibleStateImageBehavior = false;
            this.GroupMeshList.View = System.Windows.Forms.View.Details;
            this.GroupMeshList.FormatRow += new System.EventHandler<BrightIdeasSoftware.FormatRowEventArgs>(this.GroupMeshList_FormatRow);
            this.GroupMeshList.ModelCanDrop += new System.EventHandler<BrightIdeasSoftware.ModelDropEventArgs>(this.GroupMeshList_ModelCanDrop);
            this.GroupMeshList.ModelDropped += new System.EventHandler<BrightIdeasSoftware.ModelDropEventArgs>(this.GroupMeshList_ModelDropped);
            this.GroupMeshList.KeyUp += new System.Windows.Forms.KeyEventHandler(this.GroupMeshList_KeyUp);
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // BreakAngleLabel
            // 
            this.BreakAngleLabel.Controls.Add(this.BreakAngleBox);
            resources.ApplyResources(this.BreakAngleLabel, "BreakAngleLabel");
            this.BreakAngleLabel.MatchSiblingLabels = true;
            this.BreakAngleLabel.Name = "BreakAngleLabel";
            // 
            // BreakAngleBox
            // 
            resources.ApplyResources(this.BreakAngleBox, "BreakAngleBox");
            this.BreakAngleBox.MaximumValue = 90D;
            this.BreakAngleBox.MinimumValue = 10D;
            this.BreakAngleBox.Name = "BreakAngleBox";
            this.BreakAngleBox.Value = 35D;
            // 
            // controlLabel1
            // 
            this.controlLabel1.Controls.Add(this.GroupNameBox);
            resources.ApplyResources(this.controlLabel1, "controlLabel1");
            this.controlLabel1.MatchSiblingLabels = true;
            this.controlLabel1.MaxLabelWidth = 100;
            this.controlLabel1.Name = "controlLabel1";
            // 
            // GroupNameBox
            // 
            resources.ApplyResources(this.GroupNameBox, "GroupNameBox");
            this.GroupNameBox.Name = "GroupNameBox";
            // 
            // ConfigureOutlinesDialog
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ConfigureOutlinesDialog";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.controlLabel2)).EndInit();
            this.controlLabel2.ResumeLayout(false);
            this.controlLabel2.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.AvailableMeshList)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.GroupMeshList)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.BreakAngleLabel)).EndInit();
            this.BreakAngleLabel.ResumeLayout(false);
            this.BreakAngleLabel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.controlLabel1)).EndInit();
            this.controlLabel1.ResumeLayout(false);
            this.controlLabel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton AddGroupButton;
        private Controls.ControlLabel BreakAngleLabel;
        private Controls.NumberTextBox BreakAngleBox;
        private Controls.ControlLabel controlLabel1;
        private System.Windows.Forms.TextBox GroupNameBox;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private BrightIdeasSoftware.ObjectListView AvailableMeshList;
        private BrightIdeasSoftware.ObjectListView GroupMeshList;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox NoOutlineCheckBox;
        private System.Windows.Forms.ToolStripButton RemoveGroupButton;
        private Controls.ControlLabel controlLabel2;
        private Controls.NumberTextBox ThicknessBox;
    }
}