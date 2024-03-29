﻿namespace LDD.BrickEditor.UI.Windows
{
    partial class AppSettingsWindow
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AppSettingsWindow));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.CategoryListView = new BrightIdeasSoftware.ObjectListView();
            this.displaySettingsPanel1 = new LDD.BrickEditor.UI.Settings.DisplaySettingsPanel();
            this.editorSettingsPanel1 = new LDD.BrickEditor.UI.Settings.EditorSettingsPanel();
            this.lddSettingsPanel1 = new LDD.BrickEditor.UI.Settings.LddSettingsPanel();
            this.CloseButton = new System.Windows.Forms.Button();
            this.SaveButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.CategoryListView)).BeginInit();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            resources.ApplyResources(this.splitContainer1, "splitContainer1");
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.CategoryListView);
            // 
            // splitContainer1.Panel2
            // 
            resources.ApplyResources(this.splitContainer1.Panel2, "splitContainer1.Panel2");
            this.splitContainer1.Panel2.Controls.Add(this.displaySettingsPanel1);
            this.splitContainer1.Panel2.Controls.Add(this.editorSettingsPanel1);
            this.splitContainer1.Panel2.Controls.Add(this.lddSettingsPanel1);
            // 
            // CategoryListView
            // 
            this.CategoryListView.CellEditUseWholeCell = false;
            this.CategoryListView.Cursor = System.Windows.Forms.Cursors.Default;
            resources.ApplyResources(this.CategoryListView, "CategoryListView");
            this.CategoryListView.FullRowSelect = true;
            this.CategoryListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.CategoryListView.HideSelection = false;
            this.CategoryListView.MultiSelect = false;
            this.CategoryListView.Name = "CategoryListView";
            this.CategoryListView.ShowGroups = false;
            this.CategoryListView.UseCompatibleStateImageBehavior = false;
            this.CategoryListView.UseHotItem = true;
            this.CategoryListView.View = System.Windows.Forms.View.Details;
            this.CategoryListView.SelectionChanged += new System.EventHandler(this.CategoryListView_SelectionChanged);
            // 
            // displaySettingsPanel1
            // 
            resources.ApplyResources(this.displaySettingsPanel1, "displaySettingsPanel1");
            this.displaySettingsPanel1.Name = "displaySettingsPanel1";
            // 
            // editorSettingsPanel1
            // 
            resources.ApplyResources(this.editorSettingsPanel1, "editorSettingsPanel1");
            this.editorSettingsPanel1.Name = "editorSettingsPanel1";
            // 
            // lddSettingsPanel1
            // 
            resources.ApplyResources(this.lddSettingsPanel1, "lddSettingsPanel1");
            this.lddSettingsPanel1.Name = "lddSettingsPanel1";
            // 
            // CloseButton
            // 
            resources.ApplyResources(this.CloseButton, "CloseButton");
            this.CloseButton.Name = "CloseButton";
            this.CloseButton.UseVisualStyleBackColor = true;
            this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
            // 
            // SaveButton
            // 
            resources.ApplyResources(this.SaveButton, "SaveButton");
            this.SaveButton.Name = "SaveButton";
            this.SaveButton.UseVisualStyleBackColor = true;
            this.SaveButton.Click += new System.EventHandler(this.SaveButton_Click);
            // 
            // AppSettingsWindow
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.CloseButton);
            this.Controls.Add(this.SaveButton);
            this.Controls.Add(this.splitContainer1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AppSettingsWindow";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.CategoryListView)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Button CloseButton;
        private System.Windows.Forms.Button SaveButton;
        private Settings.LddSettingsPanel lddSettingsPanel1;
        private BrightIdeasSoftware.ObjectListView CategoryListView;
        private Settings.EditorSettingsPanel editorSettingsPanel1;
        private Settings.DisplaySettingsPanel displaySettingsPanel1;
    }
}