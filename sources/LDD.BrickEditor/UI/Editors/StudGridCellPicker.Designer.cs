namespace LDD.BrickEditor.UI.Editors
{
    partial class StudGridCellPicker
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
            this.PosXBox = new LDD.BrickEditor.UI.Controls.NumberTextBox();
            this.PosYBox = new LDD.BrickEditor.UI.Controls.NumberTextBox();
            this.SuspendLayout();
            // 
            // PosXBox
            // 
            this.PosXBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.PosXBox.Location = new System.Drawing.Point(0, 4);
            this.PosXBox.MinimumValue = -1D;
            this.PosXBox.Name = "PosXBox";
            this.PosXBox.Size = new System.Drawing.Size(40, 13);
            this.PosXBox.TabIndex = 0;
            this.PosXBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.PosXBox.ValueChanged += new System.EventHandler(this.PosXBox_ValueChanged);
            // 
            // PosYBox
            // 
            this.PosYBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.PosYBox.Location = new System.Drawing.Point(40, 4);
            this.PosYBox.MinimumValue = -1D;
            this.PosYBox.Name = "PosYBox";
            this.PosYBox.Size = new System.Drawing.Size(40, 13);
            this.PosYBox.TabIndex = 1;
            this.PosYBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.PosYBox.ValueChanged += new System.EventHandler(this.PosYBox_ValueChanged);
            // 
            // StudGridCellPicker
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.PosXBox);
            this.Controls.Add(this.PosYBox);
            this.Name = "StudGridCellPicker";
            this.Size = new System.Drawing.Size(100, 21);
            this.MouseClick += new System.Windows.Forms.MouseEventHandler(this.StudGridCellPicker_MouseClick);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Controls.NumberTextBox PosYBox;
        private Controls.NumberTextBox PosXBox;
    }
}
