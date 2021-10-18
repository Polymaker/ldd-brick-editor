using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using System.Windows.Forms.Layout;
using LDD.Core.Primitives.Connectors;

namespace LDD.BrickEditor.UI.Editors
{
    public partial class StudGridCellPicker : UserControl
    {
        private class SGCPLayoutEngine : LayoutEngine
        {
            internal static readonly SGCPLayoutEngine Instance = new SGCPLayoutEngine();
            public SGCPLayoutEngine()
            {
            }

            public override bool Layout(object container, LayoutEventArgs layoutEventArgs)
            {
                var cellPicker = container as StudGridCellPicker;
                Rectangle parentDisplayRectangle = cellPicker.DisplayRectangle;
                int boxWidth = (parentDisplayRectangle.Width - 20) / 2;
                
                cellPicker.PosXBox.SetBounds(3, cellPicker.PosXBox.Top, boxWidth - 6, cellPicker.PosXBox.Height);
                cellPicker.PosYBox.SetBounds(boxWidth + 3, cellPicker.PosYBox.Top, boxWidth - 6, cellPicker.PosYBox.Height);
                return false;
            }
        }

        private ToolStripDropDown PopupDropDown;
        private ToolStripControlHost PopupControlHost;
        private Point _SelectedCell;
        private bool isAssigningSelectedCell;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public StudGridControl GridControl { get; private set; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Custom2DFieldConnector StudConnector
        {
            get => GridControl.StudConnector;
            set => GridControl.StudConnector = value;
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Point SelectedCell
        {
            get => _SelectedCell;
            set => SetSelectedCell(value);
        }

        [Browsable(false)]
        public bool IsDropDown => PopupDropDown?.IsDropDown ?? false;

        public override LayoutEngine LayoutEngine => SGCPLayoutEngine.Instance;

        public event EventHandler SelectedCellChanged;

        public StudGridCellPicker()
        {
            InitializeComponent();
            
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | 
                ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw |
                ControlStyles.Selectable, true);
            
            GridControl = new StudGridControl();
            //GridControl.Dock = DockStyle.Fill;

            GridControl.ConnectorChanged += GridControl_ConnectorChanged;
            GridControl.FocusedCellChanged += GridControl_FocusedCellChanged;
            PopupDropDown = new ToolStripDropDown();
            PopupControlHost = new ToolStripControlHost(GridControl);

            PopupControlHost.AutoSize = false;
            PopupControlHost.Size = new Size(200, 200);
            PopupDropDown = new ToolStripDropDown();
            PopupDropDown.Items.Add(PopupControlHost);
            PopupDropDown.Width = 200;
            PosXBox.GotFocus += TextBoxes_FocusChanged;
            PosYBox.GotFocus += TextBoxes_FocusChanged;
            PosXBox.LostFocus += TextBoxes_FocusChanged;
            PosYBox.LostFocus += TextBoxes_FocusChanged;
            CausesValidation = true;
        }

        private void GridControl_FocusedCellChanged(object sender, EventArgs e)
        {
            if (IsDropDown)
            {
                SelectedCell = GridControl.FocusedCell ?? new Point(-1,-1);
                CloseDropDown();
            }
        }

        private void TextBoxes_FocusChanged(object sender, EventArgs e)
        {
            Invalidate();
        }

        private void GridControl_ConnectorChanged(object sender, EventArgs e)
        {
            if (StudConnector != null)
            {
                PosXBox.MaximumValue = StudConnector.ArrayWidth - 1;
                PosYBox.MaximumValue = StudConnector.ArrayHeight - 1;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (VisualStyleRenderer.IsSupported)
            {
                var txtState = VisualStyleElement.TextBox.TextEdit.Normal;

                if (PosXBox.Focused || PosYBox.Focused)
                    txtState = VisualStyleElement.TextBox.TextEdit.Selected;

                var vsr = new VisualStyleRenderer(txtState);

                vsr.DrawBackground(e.Graphics, ClientRectangle);
                vsr = new VisualStyleRenderer(VisualStyleElement.ComboBox.DropDownButton.Normal);
                vsr.DrawBackground(e.Graphics, new Rectangle(Width - 20, 0, 20, Height));

            }
        }

        private void StudGridCellPicker_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && e.X >= Width - 20)
                ShowDropDown();
        }

        protected override void Select(bool directed, bool forward)
        {
            PosXBox.Select();
        }

        public void ShowDropDown()
        {
            var dropdownSize = GridControl.GetPreferredSize(new Size(600, 400));
            PopupControlHost.Size = dropdownSize;
            GridControl.FocusedCell = SelectedCell;
            PopupDropDown.Show(this, new Point(0, Height));
            GridControl.Focus();
        }

        public void CloseDropDown()
        {
            PopupDropDown.Close();
        }

        private void SetSelectedCell(Point cell)
        {
            if (_SelectedCell != cell)
            {
                isAssigningSelectedCell = true;
                PosXBox.Value = cell.X;
                PosYBox.Value = cell.Y;
                _SelectedCell = cell;
                isAssigningSelectedCell = false;
                OnSelectedCellChanged();
            }
        }

        private void PosYBox_ValueChanged(object sender, EventArgs e)
        {
            if (isAssigningSelectedCell)
                return;

            _SelectedCell.Y = (int)PosYBox.Value;
            OnSelectedCellChanged();
        }

        private void PosXBox_ValueChanged(object sender, EventArgs e)
        {
            if (isAssigningSelectedCell)
                return;

            _SelectedCell.X = (int)PosXBox.Value;
            OnSelectedCellChanged();
        }

        private void OnSelectedCellChanged()
        {
            SelectedCellChanged?.Invoke(this, EventArgs.Empty);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Tab)
            {
                if (PosYBox.ContainsFocus)
                    PosYBox.PerformEndEdit();
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
