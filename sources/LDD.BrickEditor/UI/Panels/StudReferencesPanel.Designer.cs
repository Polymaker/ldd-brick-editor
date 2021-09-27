namespace LDD.BrickEditor.UI.Panels
{
    partial class StudReferencesPanel
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(StudReferencesPanel));
            this.SelectionToolStrip = new System.Windows.Forms.ToolStrip();
            this.CurrentSelectionLabel = new System.Windows.Forms.ToolStripLabel();
            this.ElementsComboBox = new System.Windows.Forms.ToolStripComboBox();
            this.SyncSelectionCheckBox = new LDD.BrickEditor.UI.Controls.ToolStripCheckBox();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.AddStudButton = new System.Windows.Forms.ToolStripButton();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.StudRefListView = new BrightIdeasSoftware.DataListView();
            this.olvConnectionColumn = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvPositionColumn = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvValue1Column = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvValue2Column = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.SelectionToolStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.StudRefListView)).BeginInit();
            this.SuspendLayout();
            // 
            // SelectionToolStrip
            // 
            this.SelectionToolStrip.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(249)))), ((int)(((byte)(250)))));
            this.SelectionToolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.SelectionToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.CurrentSelectionLabel,
            this.ElementsComboBox,
            this.SyncSelectionCheckBox,
            this.toolStripSeparator1,
            this.AddStudButton});
            resources.ApplyResources(this.SelectionToolStrip, "SelectionToolStrip");
            this.SelectionToolStrip.Name = "SelectionToolStrip";
            // 
            // CurrentSelectionLabel
            // 
            this.CurrentSelectionLabel.Name = "CurrentSelectionLabel";
            resources.ApplyResources(this.CurrentSelectionLabel, "CurrentSelectionLabel");
            // 
            // ElementsComboBox
            // 
            this.ElementsComboBox.Name = "ElementsComboBox";
            resources.ApplyResources(this.ElementsComboBox, "ElementsComboBox");
            this.ElementsComboBox.SelectedIndexChanged += new System.EventHandler(this.ElementsComboBox_SelectedIndexChanged);
            // 
            // SyncSelectionCheckBox
            // 
            this.SyncSelectionCheckBox.Margin = new System.Windows.Forms.Padding(6, 1, 0, 2);
            this.SyncSelectionCheckBox.Name = "SyncSelectionCheckBox";
            resources.ApplyResources(this.SyncSelectionCheckBox, "SyncSelectionCheckBox");
            this.SyncSelectionCheckBox.Click += new System.EventHandler(this.SyncSelectionCheckBox_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            resources.ApplyResources(this.toolStripSeparator1, "toolStripSeparator1");
            // 
            // AddStudButton
            // 
            this.AddStudButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            resources.ApplyResources(this.AddStudButton, "AddStudButton");
            this.AddStudButton.Name = "AddStudButton";
            this.AddStudButton.Click += new System.EventHandler(this.AddStudButton_Click);
            // 
            // StudRefListView
            // 
            this.StudRefListView.AllColumns.Add(this.olvConnectionColumn);
            this.StudRefListView.AllColumns.Add(this.olvPositionColumn);
            this.StudRefListView.AllColumns.Add(this.olvValue1Column);
            this.StudRefListView.AllColumns.Add(this.olvValue2Column);
            resources.ApplyResources(this.StudRefListView, "StudRefListView");
            this.StudRefListView.AutoGenerateColumns = false;
            this.StudRefListView.CellEditActivation = BrightIdeasSoftware.ObjectListView.CellEditActivateMode.DoubleClick;
            this.StudRefListView.CellEditUseWholeCell = false;
            this.StudRefListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvConnectionColumn,
            this.olvPositionColumn,
            this.olvValue1Column,
            this.olvValue2Column});
            this.StudRefListView.Cursor = System.Windows.Forms.Cursors.Default;
            this.StudRefListView.DataSource = null;
            this.StudRefListView.FullRowSelect = true;
            this.StudRefListView.HideSelection = false;
            this.StudRefListView.Name = "StudRefListView";
            this.StudRefListView.RowHeight = 21;
            this.StudRefListView.UseCompatibleStateImageBehavior = false;
            this.StudRefListView.View = System.Windows.Forms.View.Details;
            this.StudRefListView.CellEditFinished += new BrightIdeasSoftware.CellEditEventHandler(this.StudRefListView_CellEditFinished);
            this.StudRefListView.CellEditStarting += new BrightIdeasSoftware.CellEditEventHandler(this.StudRefListView_CellEditStarting);
            this.StudRefListView.SelectedIndexChanged += new System.EventHandler(this.StudRefListView_SelectedIndexChanged);
            // 
            // olvConnectionColumn
            // 
            this.olvConnectionColumn.AspectName = "";
            this.olvConnectionColumn.FillsFreeSpace = true;
            resources.ApplyResources(this.olvConnectionColumn, "olvConnectionColumn");
            // 
            // olvPositionColumn
            // 
            this.olvPositionColumn.Groupable = false;
            resources.ApplyResources(this.olvPositionColumn, "olvPositionColumn");
            // 
            // olvValue1Column
            // 
            this.olvValue1Column.AspectName = "Value1";
            this.olvValue1Column.Groupable = false;
            resources.ApplyResources(this.olvValue1Column, "olvValue1Column");
            // 
            // olvValue2Column
            // 
            this.olvValue2Column.AspectName = "Value2";
            this.olvValue2Column.Groupable = false;
            resources.ApplyResources(this.olvValue2Column, "olvValue2Column");
            // 
            // StudReferencesPanel
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.StudRefListView);
            this.Controls.Add(this.SelectionToolStrip);
            this.Name = "StudReferencesPanel";
            this.SelectionToolStrip.ResumeLayout(false);
            this.SelectionToolStrip.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.StudRefListView)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip SelectionToolStrip;
        private System.Windows.Forms.ToolStripLabel CurrentSelectionLabel;
        private System.Windows.Forms.ToolStripComboBox ElementsComboBox;
        private Controls.ToolStripCheckBox SyncSelectionCheckBox;
        private System.Windows.Forms.ToolTip toolTip1;
        private BrightIdeasSoftware.DataListView StudRefListView;
        private BrightIdeasSoftware.OLVColumn olvConnectionColumn;
        private BrightIdeasSoftware.OLVColumn olvPositionColumn;
        private BrightIdeasSoftware.OLVColumn olvValue1Column;
        private BrightIdeasSoftware.OLVColumn olvValue2Column;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton AddStudButton;
    }
}