namespace LDD.BrickEditor.UI.Panels
{
    partial class ElementDetailPanel
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ElementDetailPanel));
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.LabelInertiaTensor = new System.Windows.Forms.Label();
            this.LabelCenterOfMass = new System.Windows.Forms.Label();
            this.InertiaTensorTextBox = new System.Windows.Forms.TextBox();
            this.LabelMass = new System.Windows.Forms.Label();
            this.CenterOfMassEditor = new LDD.BrickEditor.UI.Editors.VectorEditor();
            this.MassNumberBox = new LDD.BrickEditor.UI.Controls.NumberTextBox();
            this.FrictionCheckBox = new System.Windows.Forms.CheckBox();
            this.BoundingEditor = new LDD.BrickEditor.UI.Controls.BoundingBoxEditor();
            this.NameTextBox = new System.Windows.Forms.TextBox();
            this.TransformEdit = new LDD.BrickEditor.UI.Controls.TransformEditor();
            this.SubMaterialIndexBox = new LDD.BrickEditor.UI.Controls.NumberTextBox();
            this.CollisionSizeEditor = new LDD.BrickEditor.UI.Editors.VectorEditor();
            this.CollisionRadiusBox = new LDD.BrickEditor.UI.Controls.NumberTextBox();
            this.SelectionInfoLabel = new System.Windows.Forms.Label();
            this.localizableStringList1 = new LDD.BrickEditor.Localization.LocalizableStringList(this.components);
            this.MultiSelectionMsg = ((LDD.BrickEditor.Localization.LocalizableString)(new LDD.BrickEditor.Localization.LocalizableString()));
            this.NoSelectionMsg = ((LDD.BrickEditor.Localization.LocalizableString)(new LDD.BrickEditor.Localization.LocalizableString()));
            this.TopStudsLabel = ((LDD.BrickEditor.Localization.LocalizableString)(new LDD.BrickEditor.Localization.LocalizableString()));
            this.BottomStudsLabel = ((LDD.BrickEditor.Localization.LocalizableString)(new LDD.BrickEditor.Localization.LocalizableString()));
            this.NoConnectorRefLabel = ((LDD.BrickEditor.Localization.LocalizableString)(new LDD.BrickEditor.Localization.LocalizableString()));
            this.SelectionToolStrip = new System.Windows.Forms.ToolStrip();
            this.CurrentSelectionLabel = new System.Windows.Forms.ToolStripLabel();
            this.ElementsComboBox = new System.Windows.Forms.ToolStripComboBox();
            this.NameLabel = new LDD.BrickEditor.UI.Controls.ControlLabel();
            this.SubMaterialIndexLabel = new LDD.BrickEditor.UI.Controls.ControlLabel();
            this.CollisionRadiusLabel = new LDD.BrickEditor.UI.Controls.ControlLabel();
            this.CollisionSizeLabel = new LDD.BrickEditor.UI.Controls.ControlLabel();
            this.controlLabel1 = new LDD.BrickEditor.UI.Controls.ControlLabel();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.BonePhysPropertiesLabel = new LDD.BrickEditor.UI.Controls.ControlLabel();
            this.BoneBoundsLabel = new LDD.BrickEditor.UI.Controls.ControlLabel();
            this.tableLayoutPanel3.SuspendLayout();
            this.SelectionToolStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.NameLabel)).BeginInit();
            this.NameLabel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.SubMaterialIndexLabel)).BeginInit();
            this.SubMaterialIndexLabel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.CollisionRadiusLabel)).BeginInit();
            this.CollisionRadiusLabel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.CollisionSizeLabel)).BeginInit();
            this.CollisionSizeLabel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.controlLabel1)).BeginInit();
            this.controlLabel1.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.BonePhysPropertiesLabel)).BeginInit();
            this.BonePhysPropertiesLabel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.BoneBoundsLabel)).BeginInit();
            this.BoneBoundsLabel.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel3
            // 
            resources.ApplyResources(this.tableLayoutPanel3, "tableLayoutPanel3");
            this.tableLayoutPanel3.Controls.Add(this.LabelInertiaTensor, 0, 0);
            this.tableLayoutPanel3.Controls.Add(this.LabelCenterOfMass, 0, 1);
            this.tableLayoutPanel3.Controls.Add(this.InertiaTensorTextBox, 1, 0);
            this.tableLayoutPanel3.Controls.Add(this.LabelMass, 0, 2);
            this.tableLayoutPanel3.Controls.Add(this.CenterOfMassEditor, 1, 1);
            this.tableLayoutPanel3.Controls.Add(this.MassNumberBox, 1, 2);
            this.tableLayoutPanel3.Controls.Add(this.FrictionCheckBox, 2, 2);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            // 
            // LabelInertiaTensor
            // 
            resources.ApplyResources(this.LabelInertiaTensor, "LabelInertiaTensor");
            this.LabelInertiaTensor.Name = "LabelInertiaTensor";
            // 
            // LabelCenterOfMass
            // 
            resources.ApplyResources(this.LabelCenterOfMass, "LabelCenterOfMass");
            this.LabelCenterOfMass.Name = "LabelCenterOfMass";
            // 
            // InertiaTensorTextBox
            // 
            resources.ApplyResources(this.InertiaTensorTextBox, "InertiaTensorTextBox");
            this.tableLayoutPanel3.SetColumnSpan(this.InertiaTensorTextBox, 2);
            this.InertiaTensorTextBox.Name = "InertiaTensorTextBox";
            // 
            // LabelMass
            // 
            resources.ApplyResources(this.LabelMass, "LabelMass");
            this.LabelMass.Name = "LabelMass";
            // 
            // CenterOfMassEditor
            // 
            resources.ApplyResources(this.CenterOfMassEditor, "CenterOfMassEditor");
            this.tableLayoutPanel3.SetColumnSpan(this.CenterOfMassEditor, 2);
            this.CenterOfMassEditor.Name = "CenterOfMassEditor";
            // 
            // MassNumberBox
            // 
            resources.ApplyResources(this.MassNumberBox, "MassNumberBox");
            this.MassNumberBox.MaximumValue = 99999D;
            this.MassNumberBox.Name = "MassNumberBox";
            // 
            // FrictionCheckBox
            // 
            resources.ApplyResources(this.FrictionCheckBox, "FrictionCheckBox");
            this.FrictionCheckBox.Name = "FrictionCheckBox";
            this.FrictionCheckBox.UseVisualStyleBackColor = true;
            this.FrictionCheckBox.CheckedChanged += new System.EventHandler(this.FrictionCheckBox_CheckedChanged);
            // 
            // BoundingEditor
            // 
            resources.ApplyResources(this.BoundingEditor, "BoundingEditor");
            this.BoundingEditor.Name = "BoundingEditor";
            // 
            // NameTextBox
            // 
            resources.ApplyResources(this.NameTextBox, "NameTextBox");
            this.NameTextBox.Name = "NameTextBox";
            this.NameTextBox.Validating += new System.ComponentModel.CancelEventHandler(this.NameTextBox_Validating);
            this.NameTextBox.Validated += new System.EventHandler(this.NameTextBox_Validated);
            // 
            // TransformEdit
            // 
            resources.ApplyResources(this.TransformEdit, "TransformEdit");
            this.TransformEdit.Name = "TransformEdit";
            // 
            // SubMaterialIndexBox
            // 
            resources.ApplyResources(this.SubMaterialIndexBox, "SubMaterialIndexBox");
            this.SubMaterialIndexBox.Name = "SubMaterialIndexBox";
            this.SubMaterialIndexBox.ValueChanged += new System.EventHandler(this.SubMaterialIndexBox_ValueChanged);
            // 
            // CollisionSizeEditor
            // 
            resources.ApplyResources(this.CollisionSizeEditor, "CollisionSizeEditor");
            this.CollisionSizeEditor.Name = "CollisionSizeEditor";
            this.CollisionSizeEditor.ValueChanged += new System.EventHandler(this.CollisionSizeEditor_ValueChanged);
            // 
            // CollisionRadiusBox
            // 
            resources.ApplyResources(this.CollisionRadiusBox, "CollisionRadiusBox");
            this.CollisionRadiusBox.Name = "CollisionRadiusBox";
            this.CollisionRadiusBox.ValueChanged += new System.EventHandler(this.CollisionRadiusBox_ValueChanged);
            // 
            // SelectionInfoLabel
            // 
            resources.ApplyResources(this.SelectionInfoLabel, "SelectionInfoLabel");
            this.SelectionInfoLabel.Name = "SelectionInfoLabel";
            // 
            // localizableStringList1
            // 
            this.localizableStringList1.Items.AddRange(new LDD.BrickEditor.Localization.LocalizableString[] {
            this.MultiSelectionMsg,
            this.NoSelectionMsg,
            this.TopStudsLabel,
            this.BottomStudsLabel,
            this.NoConnectorRefLabel});
            // 
            // MultiSelectionMsg
            // 
            resources.ApplyResources(this.MultiSelectionMsg, "MultiSelectionMsg");
            // 
            // NoSelectionMsg
            // 
            resources.ApplyResources(this.NoSelectionMsg, "NoSelectionMsg");
            // 
            // TopStudsLabel
            // 
            resources.ApplyResources(this.TopStudsLabel, "TopStudsLabel");
            // 
            // BottomStudsLabel
            // 
            resources.ApplyResources(this.BottomStudsLabel, "BottomStudsLabel");
            // 
            // NoConnectorRefLabel
            // 
            resources.ApplyResources(this.NoConnectorRefLabel, "NoConnectorRefLabel");
            // 
            // SelectionToolStrip
            // 
            this.SelectionToolStrip.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(249)))), ((int)(((byte)(250)))));
            this.SelectionToolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.SelectionToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.CurrentSelectionLabel,
            this.ElementsComboBox});
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
            // NameLabel
            // 
            this.NameLabel.Controls.Add(this.NameTextBox);
            resources.ApplyResources(this.NameLabel, "NameLabel");
            this.NameLabel.MatchSiblingLabels = true;
            this.NameLabel.Name = "NameLabel";
            // 
            // SubMaterialIndexLabel
            // 
            this.SubMaterialIndexLabel.AutoSizeWidth = true;
            this.SubMaterialIndexLabel.Controls.Add(this.SubMaterialIndexBox);
            resources.ApplyResources(this.SubMaterialIndexLabel, "SubMaterialIndexLabel");
            this.SubMaterialIndexLabel.MatchSiblingLabels = true;
            this.SubMaterialIndexLabel.Name = "SubMaterialIndexLabel";
            // 
            // CollisionRadiusLabel
            // 
            this.CollisionRadiusLabel.AutoSizeWidth = true;
            this.CollisionRadiusLabel.Controls.Add(this.CollisionRadiusBox);
            resources.ApplyResources(this.CollisionRadiusLabel, "CollisionRadiusLabel");
            this.CollisionRadiusLabel.MatchSiblingLabels = true;
            this.CollisionRadiusLabel.Name = "CollisionRadiusLabel";
            // 
            // CollisionSizeLabel
            // 
            this.CollisionSizeLabel.Controls.Add(this.CollisionSizeEditor);
            resources.ApplyResources(this.CollisionSizeLabel, "CollisionSizeLabel");
            this.CollisionSizeLabel.MatchSiblingLabels = true;
            this.CollisionSizeLabel.Name = "CollisionSizeLabel";
            // 
            // controlLabel1
            // 
            this.controlLabel1.Controls.Add(this.SelectionInfoLabel);
            resources.ApplyResources(this.controlLabel1, "controlLabel1");
            this.controlLabel1.MatchSiblingLabels = true;
            this.controlLabel1.Name = "controlLabel1";
            // 
            // flowLayoutPanel1
            // 
            resources.ApplyResources(this.flowLayoutPanel1, "flowLayoutPanel1");
            this.flowLayoutPanel1.Controls.Add(this.controlLabel1);
            this.flowLayoutPanel1.Controls.Add(this.NameLabel);
            this.flowLayoutPanel1.Controls.Add(this.SubMaterialIndexLabel);
            this.flowLayoutPanel1.Controls.Add(this.CollisionRadiusLabel);
            this.flowLayoutPanel1.Controls.Add(this.CollisionSizeLabel);
            this.flowLayoutPanel1.Controls.Add(this.BonePhysPropertiesLabel);
            this.flowLayoutPanel1.Controls.Add(this.BoneBoundsLabel);
            this.flowLayoutPanel1.Controls.Add(this.TransformEdit);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            // 
            // BonePhysPropertiesLabel
            // 
            this.BonePhysPropertiesLabel.Controls.Add(this.tableLayoutPanel3);
            this.BonePhysPropertiesLabel.LabelPosition = System.Windows.Forms.ArrowDirection.Up;
            resources.ApplyResources(this.BonePhysPropertiesLabel, "BonePhysPropertiesLabel");
            this.BonePhysPropertiesLabel.MatchSiblingLabels = true;
            this.BonePhysPropertiesLabel.Name = "BonePhysPropertiesLabel";
            // 
            // BoneBoundsLabel
            // 
            this.BoneBoundsLabel.Controls.Add(this.BoundingEditor);
            this.flowLayoutPanel1.SetFlowBreak(this.BoneBoundsLabel, true);
            this.BoneBoundsLabel.LabelPosition = System.Windows.Forms.ArrowDirection.Up;
            resources.ApplyResources(this.BoneBoundsLabel, "BoneBoundsLabel");
            this.BoneBoundsLabel.MatchSiblingLabels = true;
            this.BoneBoundsLabel.Name = "BoneBoundsLabel";
            // 
            // ElementDetailPanel
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.flowLayoutPanel1);
            this.Controls.Add(this.SelectionToolStrip);
            this.Name = "ElementDetailPanel";
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel3.PerformLayout();
            this.SelectionToolStrip.ResumeLayout(false);
            this.SelectionToolStrip.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.NameLabel)).EndInit();
            this.NameLabel.ResumeLayout(false);
            this.NameLabel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.SubMaterialIndexLabel)).EndInit();
            this.SubMaterialIndexLabel.ResumeLayout(false);
            this.SubMaterialIndexLabel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.CollisionRadiusLabel)).EndInit();
            this.CollisionRadiusLabel.ResumeLayout(false);
            this.CollisionRadiusLabel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.CollisionSizeLabel)).EndInit();
            this.CollisionSizeLabel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.controlLabel1)).EndInit();
            this.controlLabel1.ResumeLayout(false);
            this.controlLabel1.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.BonePhysPropertiesLabel)).EndInit();
            this.BonePhysPropertiesLabel.ResumeLayout(false);
            this.BonePhysPropertiesLabel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.BoneBoundsLabel)).EndInit();
            this.BoneBoundsLabel.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private Controls.TransformEditor TransformEdit;
        private System.Windows.Forms.TextBox NameTextBox;
        private Controls.NumberTextBox SubMaterialIndexBox;
        private Editors.VectorEditor CollisionSizeEditor;
        private Controls.NumberTextBox CollisionRadiusBox;
        private System.Windows.Forms.Label SelectionInfoLabel;
        private Localization.LocalizableStringList localizableStringList1;
        private Localization.LocalizableString MultiSelectionMsg;
        private Localization.LocalizableString NoSelectionMsg;
        private Localization.LocalizableString TopStudsLabel;
        private Localization.LocalizableString BottomStudsLabel;
        private Localization.LocalizableString NoConnectorRefLabel;
        private System.Windows.Forms.ToolStrip SelectionToolStrip;
        private System.Windows.Forms.ToolStripLabel CurrentSelectionLabel;
        private System.Windows.Forms.ToolStripComboBox ElementsComboBox;
        private Controls.ControlLabel NameLabel;
        private Controls.ControlLabel SubMaterialIndexLabel;
        private Controls.ControlLabel CollisionRadiusLabel;
        private Controls.ControlLabel CollisionSizeLabel;
        private Controls.ControlLabel controlLabel1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.Label LabelInertiaTensor;
        private System.Windows.Forms.Label LabelCenterOfMass;
        private System.Windows.Forms.TextBox InertiaTensorTextBox;
        private System.Windows.Forms.Label LabelMass;
        private Editors.VectorEditor CenterOfMassEditor;
        private Controls.NumberTextBox MassNumberBox;
        private System.Windows.Forms.CheckBox FrictionCheckBox;
        private Controls.BoundingBoxEditor BoundingEditor;
        private Controls.ControlLabel BoneBoundsLabel;
        private Controls.ControlLabel BonePhysPropertiesLabel;
    }
}