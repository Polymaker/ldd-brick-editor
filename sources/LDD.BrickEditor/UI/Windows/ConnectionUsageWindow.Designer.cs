namespace LDD.BrickEditor.UI.Windows
{
    partial class ConnectionUsageWindow
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ConnectionUsageWindow));
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.SearchButton = new System.Windows.Forms.Button();
            this.ExportButton = new System.Windows.Forms.Button();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.ConnTypeColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ConnSubTypeColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.RefCountColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.PartListColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.localizableStringList1 = new LDD.BrickEditor.Localization.LocalizableStringList(this.components);
            this.StartAnalysingLabel = ((LDD.BrickEditor.Localization.LocalizableString)(new LDD.BrickEditor.Localization.LocalizableString()));
            this.CancelAnalysingLabel = ((LDD.BrickEditor.Localization.LocalizableString)(new LDD.BrickEditor.Localization.LocalizableString()));
            this.CancelingLabel = ((LDD.BrickEditor.Localization.LocalizableString)(new LDD.BrickEditor.Localization.LocalizableString()));
            this.ExcelFileFilterLabel = ((LDD.BrickEditor.Localization.LocalizableString)(new LDD.BrickEditor.Localization.LocalizableString()));
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.SuspendLayout();
            // 
            // dataGridView1
            // 
            resources.ApplyResources(this.dataGridView1, "dataGridView1");
            this.dataGridView1.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ConnTypeColumn,
            this.ConnSubTypeColumn,
            this.RefCountColumn,
            this.PartListColumn});
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.ReadOnly = true;
            this.dataGridView1.RowHeadersVisible = false;
            // 
            // SearchButton
            // 
            resources.ApplyResources(this.SearchButton, "SearchButton");
            this.SearchButton.Name = "SearchButton";
            this.SearchButton.UseVisualStyleBackColor = true;
            this.SearchButton.Click += new System.EventHandler(this.SearchButton_Click);
            // 
            // ExportButton
            // 
            resources.ApplyResources(this.ExportButton, "ExportButton");
            this.ExportButton.Name = "ExportButton";
            this.ExportButton.UseVisualStyleBackColor = true;
            this.ExportButton.Click += new System.EventHandler(this.ExportButton_Click);
            // 
            // progressBar1
            // 
            resources.ApplyResources(this.progressBar1, "progressBar1");
            this.progressBar1.Name = "progressBar1";
            // 
            // ConnTypeColumn
            // 
            this.ConnTypeColumn.DataPropertyName = "ConnectionType";
            this.ConnTypeColumn.FillWeight = 60F;
            resources.ApplyResources(this.ConnTypeColumn, "ConnTypeColumn");
            this.ConnTypeColumn.Name = "ConnTypeColumn";
            this.ConnTypeColumn.ReadOnly = true;
            // 
            // ConnSubTypeColumn
            // 
            this.ConnSubTypeColumn.DataPropertyName = "SubType";
            this.ConnSubTypeColumn.FillWeight = 60F;
            resources.ApplyResources(this.ConnSubTypeColumn, "ConnSubTypeColumn");
            this.ConnSubTypeColumn.Name = "ConnSubTypeColumn";
            this.ConnSubTypeColumn.ReadOnly = true;
            this.ConnSubTypeColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // RefCountColumn
            // 
            this.RefCountColumn.DataPropertyName = "RefCount";
            this.RefCountColumn.FillWeight = 40F;
            resources.ApplyResources(this.RefCountColumn, "RefCountColumn");
            this.RefCountColumn.Name = "RefCountColumn";
            this.RefCountColumn.ReadOnly = true;
            this.RefCountColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // PartListColumn
            // 
            this.PartListColumn.DataPropertyName = "Parts";
            resources.ApplyResources(this.PartListColumn, "PartListColumn");
            this.PartListColumn.Name = "PartListColumn";
            this.PartListColumn.ReadOnly = true;
            this.PartListColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // localizableStringList1
            // 
            this.localizableStringList1.Items.AddRange(new LDD.BrickEditor.Localization.LocalizableString[] {
            this.StartAnalysingLabel,
            this.CancelAnalysingLabel,
            this.CancelingLabel,
            this.ExcelFileFilterLabel});
            // 
            // StartAnalysingLabel
            // 
            resources.ApplyResources(this.StartAnalysingLabel, "StartAnalysingLabel");
            // 
            // CancelAnalysingLabel
            // 
            resources.ApplyResources(this.CancelAnalysingLabel, "CancelAnalysingLabel");
            // 
            // CancelingLabel
            // 
            resources.ApplyResources(this.CancelingLabel, "CancelingLabel");
            // 
            // ExcelFileFilterLabel
            // 
            resources.ApplyResources(this.ExcelFileFilterLabel, "ExcelFileFilterLabel");
            // 
            // ConnectionUsageWindow
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.ExportButton);
            this.Controls.Add(this.SearchButton);
            this.Controls.Add(this.dataGridView1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ConnectionUsageWindow";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.Button SearchButton;
        private System.Windows.Forms.Button ExportButton;
        private System.Windows.Forms.ProgressBar progressBar1;
        private Localization.LocalizableStringList localizableStringList1;
        private Localization.LocalizableString StartAnalysingLabel;
        private Localization.LocalizableString CancelAnalysingLabel;
        private Localization.LocalizableString CancelingLabel;
        private System.Windows.Forms.DataGridViewTextBoxColumn ConnTypeColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn ConnSubTypeColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn RefCountColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn PartListColumn;
        private Localization.LocalizableString ExcelFileFilterLabel;
    }
}