using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace LDD.BrickEditor.UI.Controls
{
    [Designer(typeof(ControlLabelDesigner))]
    public partial class ControlLabel : Control, ISupportInitialize
    {
        const string DefaultText = "<Empty>";

        private Size LabelMinSize;
        private Size ControlMinSize;

        private Size TotalMinimumSize => new Size(
                Padding.Horizontal + LabelBounds.Width + ChildControlBounds.Width,
                Padding.Vertical + LabelBounds.Height + ChildControlBounds.Height
            );

        private Rectangle LabelBounds;
        private Rectangle ChildControlBounds;

        private int _LabelWidth;
        private int _MaxLabelWidth;
        private bool _AutoSizeHeight;
        private bool _AutoSizeWidth;
        private bool _MatchSiblingLabels;
        private bool hasInitialized;

        private ContentAlignment _ControlAlignment;
        private ContentAlignment _LabelAlignment;
        private ArrowDirection _LabelPosition;


        [DefaultValue(-1), RefreshProperties(RefreshProperties.Repaint)]
        public int LabelWidth
        {
            get => _LabelWidth;
            set
            {
                if ((value >= 20 || value == -1) && _LabelWidth != value)
                {
                    AdjustSize(value, false, false, AdjustSpecified.LabelWidth);
                }
            }
        }

        [DefaultValue(0), RefreshProperties(RefreshProperties.Repaint)]
        public int MaxLabelWidth
        {
            get => _MaxLabelWidth;
            set
            {
                if ((value >= 20 || value == 0) && _MaxLabelWidth != value)
                {
                    _MaxLabelWidth = value;
                    AdjustSize(0, false, false, AdjustSpecified.LabelDisplay);
                }
            }
        }

        [Browsable(false)]
        public bool AutoSizeLabel => LabelWidth == -1;

        [DefaultValue(false), RefreshProperties(RefreshProperties.Repaint)]
        public bool MatchSiblingLabels
        {
            get => _MatchSiblingLabels;
            set
            {
                if (_MatchSiblingLabels != value)
                {
                    _MatchSiblingLabels = value;
                    if (AutoSizeLabel)
                        AdjustSize(0, false, false, AdjustSpecified.LabelDisplay | AdjustSpecified.MatchSiblingLabel);

                    NotifyLabelWidthChanged();
                }
            }
        }

        [DefaultValue(ContentAlignment.MiddleLeft), RefreshProperties(RefreshProperties.Repaint)]
        public ContentAlignment LabelAlignment
        {
            get => _LabelAlignment;
            set
            {
                if (_LabelAlignment != value)
                {
                    _LabelAlignment = value;
                    AdjustSize(0, false, false, AdjustSpecified.LabelDisplay);
                }
            }
        }

        [DefaultValue(ArrowDirection.Left), RefreshProperties(RefreshProperties.Repaint)]
        public ArrowDirection LabelPosition
        {
            get => _LabelPosition;
            set
            {
                if (_LabelPosition != value)
                {
                    _LabelPosition = value;
                    AdjustSize(0, false, false, AdjustSpecified.LabelDisplay);
                }
            }
        }

        [DefaultValue(ContentAlignment.TopLeft), RefreshProperties(RefreshProperties.Repaint)]
        public ContentAlignment ControlAlignment
        {
            get => _ControlAlignment;
            set
            {
                if (_ControlAlignment != value)
                {
                    _ControlAlignment = value;
                    UpdateChildControlBounds();
                }
            }
        }



        [DefaultValue(true)]
        public bool AutoSizeHeight
        {
            get => _AutoSizeHeight;
            set
            {
                if (_AutoSizeHeight != value)
                {
                    AdjustSize(0, false, value, AdjustSpecified.AutoHeight);
                    //_AutoSizeHeight = value;
                    //AdjustControlSize();
                }
            }
        }

        [DefaultValue(false)]
        public bool AutoSizeWidth
        {
            get => _AutoSizeWidth;
            set
            {
                if (_AutoSizeWidth != value)
                {
                    AdjustSize(0, value, false, AdjustSpecified.AutoWidth);
                    //_AutoSizeWidth = value;
                    //AdjustControlSize();
                }
            }
        }

        [Browsable(false)]
        public bool AutoSizeAll => AutoSizeWidth && AutoSizeHeight;


        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Control Control => Controls.Count > 0 ? Controls[0] : null;

        public override Rectangle DisplayRectangle => 
            ChildControlBounds.IsEmpty ? base.DisplayRectangle : ChildControlBounds;


        public ControlLabel()
        {
            InitializeComponent();
            _LabelWidth = -1;
            _AutoSizeHeight = true;
            _LabelAlignment = ContentAlignment.MiddleLeft;
            _LabelPosition = ArrowDirection.Left;
            _ControlAlignment = ContentAlignment.TopLeft;

            SetStyle(ControlStyles.ResizeRedraw | ControlStyles.OptimizedDoubleBuffer, true);
        }
        

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            if (!hasInitialized)
            {
                hasInitialized = true;
            }
            CalculateChildControlMinSize(); 
            CalculateLabelSize();
            UpdateLabelBounds();
            UpdateChildControlBounds();
        }

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            if (hasInitialized)
                AdjustSize(0, false, false, AdjustSpecified.LabelDisplay);
        }

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            if (hasInitialized)
                AdjustSize(0, false, false, AdjustSpecified.LabelDisplay);
        }

        protected override void OnPaddingChanged(EventArgs e)
        {
            base.OnPaddingChanged(e);
            if ((AutoSizeWidth || AutoSizeHeight) && hasInitialized)
            {
                AdjustSize(0, false, false, AdjustSpecified.LabelDisplay);
            }
        }

        #region Size & Bounds calculation

        protected bool LabelWidthMatters => LabelPosition == ArrowDirection.Left || LabelPosition == ArrowDirection.Right;

        protected void CalculateLabelSize()
        {
            string textToMeasure = string.IsNullOrEmpty(Text) ? DefaultText : Text;
            int maxWidth = MaxLabelWidth == 0 || !LabelWidthMatters ? 99999 : MaxLabelWidth;

            var testSize = new Size(LabelWidth > 0 ? LabelWidth : maxWidth, Height - Padding.Vertical);
            testSize.Width -= Padding.Left;

            LabelMinSize = TextRenderer.MeasureText(textToMeasure, Font, testSize, GetLabelFormatFlags());

            if (LabelWidth > 0 && LabelWidthMatters)
                LabelMinSize.Width = LabelWidth;

            if (MatchSiblingLabels && AutoSizeLabel && LabelWidthMatters)
                LabelMinSize.Width = Math.Max(LabelMinSize.Width, CalculateMaxSiblingWidth());

        }

        public int CalculateLabelWidth()
        {
            string textToMeasure = string.IsNullOrEmpty(Text) ? DefaultText : Text;
            int maxWidth = MaxLabelWidth == 0 || !LabelWidthMatters ? 99999 : MaxLabelWidth;
            var testSize = new Size(maxWidth, Height - Padding.Vertical);
            //testSize.Width -= Padding.Left;

            var labelSize = TextRenderer.MeasureText(textToMeasure, Font, testSize, GetLabelFormatFlags());
            return labelSize.Width;
        }

        private void UpdateLabelBounds()
        {
            switch (LabelPosition)
            {
                case ArrowDirection.Up:
                    LabelBounds = new Rectangle(
                        Padding.Left,
                        Padding.Top,
                        Width - Padding.Horizontal,
                        LabelMinSize.Height
                    );
                    break;
                case ArrowDirection.Down:
                    LabelBounds = new Rectangle(
                        Padding.Left,
                        Height - Padding.Bottom - LabelMinSize.Height,
                        Width - Padding.Horizontal,
                        LabelMinSize.Height
                    );
                    break;
                case ArrowDirection.Left:
                    LabelBounds = new Rectangle(
                        Padding.Left,
                        Padding.Top,
                        LabelMinSize.Width,
                        Height - Padding.Vertical
                    );
                    break;
                case ArrowDirection.Right:
                    LabelBounds = new Rectangle(
                        Width - Padding.Right - LabelMinSize.Width,
                        Padding.Top,
                        LabelMinSize.Width,
                        Height - Padding.Vertical
                    );
                    break;
            }

        }

        private void CalculateChildControlMinSize()
        {
            if (Control != null)
            {
                if (AutoSizeWidth && AutoSizeHeight)
                {
                    ControlMinSize = Control.Size;
                    ControlMinSize.Width += Control.Margin.Horizontal;
                    ControlMinSize.Height += Control.Margin.Vertical;
                }
                else
                {
                    ControlMinSize = new Size(
                        AutoSizeWidth ? (Control.Width + Control.Margin.Horizontal) : Font.Height,
                        AutoSizeHeight ? (Control.Height + Control.Margin.Vertical) : Font.Height);
                    //int remainingWidth = Width - Minimum_Size.Width;
                    //if (remainingWidth < 0)
                    //    remainingWidth = 0;
                    //var ctrlMinSize = Control.GetPreferredSize(new Size(remainingWidth, Height));
                    //if (AutoSizeWidth)
                    //    ctrlMinSize.Width = Control.Width;
                    //if (AutoSizeHeight)
                    //    ctrlMinSize.Height = Control.Height;
                    //ctrlMinSize.Width += Control.Margin.Horizontal;
                    //ctrlMinSize.Height += Control.Margin.Vertical;
                    //ControlMinSize = ctrlMinSize;
                }

                UpdateChildControlBounds();
            }
            else
            {
                ControlMinSize = new Size(Font.Height, Font.Height);
            }
        }

        private void UpdateChildControlBounds()
        {
            if (LabelPosition == ArrowDirection.Up || LabelPosition == ArrowDirection.Down)
            {
                ChildControlBounds = new Rectangle(
                    Padding.Left,
                    (LabelPosition == ArrowDirection.Up) ? Padding.Top + LabelMinSize.Height : Padding.Top,
                    Width - Padding.Horizontal,
                    Height - Padding.Vertical - LabelMinSize.Height
                );
            }
            else
            {
                ChildControlBounds = new Rectangle(
                    (LabelPosition == ArrowDirection.Left) ? LabelMinSize.Width + Padding.Left : Padding.Left,
                    Padding.Top,
                    Width - LabelMinSize.Width - Padding.Horizontal,
                    Height - Padding.Vertical
                );
            }

            PositionChildControl();
        }

        private Size GetMinimumSize()
        {
            var minimumSize = new Size(Padding.Horizontal, Padding.Vertical);
            if (LabelPosition == ArrowDirection.Left || LabelPosition == ArrowDirection.Right)
                minimumSize.Width += LabelMinSize.Width + ControlMinSize.Width;
            else
                minimumSize.Width += Math.Max(LabelMinSize.Width, ControlMinSize.Width);

            if (LabelPosition == ArrowDirection.Up || LabelPosition == ArrowDirection.Down)
                minimumSize.Height += LabelMinSize.Height + ControlMinSize.Height;
            else
                minimumSize.Height += Math.Max(LabelMinSize.Height, ControlMinSize.Height);

            return minimumSize;
        }


        #endregion

        private TextFormatFlags GetLabelFormatFlags()
        {
            var flags = TextFormatFlags.Default;

            var alignStr = LabelAlignment.ToString();
            if (alignStr.Contains("Top"))
                flags |= TextFormatFlags.Top;
            else if (alignStr.Contains("Bottom"))
                flags |= TextFormatFlags.Bottom;
            else if (alignStr.Contains("Middle"))
                flags |= TextFormatFlags.VerticalCenter;

            if (alignStr.Contains("Left"))
                flags |= TextFormatFlags.Left;
            else if (alignStr.Contains("Right"))
                flags |= TextFormatFlags.Right;
            else if (alignStr.Contains("Center"))
                flags |= TextFormatFlags.HorizontalCenter;

            return flags | TextFormatFlags.WordBreak | TextFormatFlags.EndEllipsis;
        }

        protected override void OnControlAdded(ControlEventArgs e)
        {
            base.OnControlAdded(e);
            OnControlAssigned();

            e.Control.SizeChanged += Control_SizeChanged;
            e.Control.MarginChanged += Control_MarginChanged;
        }

        protected override void OnControlRemoved(ControlEventArgs e)
        {
            base.OnControlRemoved(e);
            e.Control.SizeChanged -= Control_SizeChanged;
            e.Control.MarginChanged -= Control_MarginChanged;

            AdjustSize(0, false, false, AdjustSpecified.ControlSize);
            //if (AutoSizeHeight || AutoSizeWidth)
            //    AdjustControlSize();
        }

        private void Control_SizeChanged(object sender, EventArgs e)
        {
            AdjustSize(0, false, false, AdjustSpecified.ControlSize);
        }

        private void Control_MarginChanged(object sender, EventArgs e)
        {
            AdjustSize(0, false, false, AdjustSpecified.ControlSize);
        }

        #region Size Synchronization

        private void NotifyLabelWidthChanged()
        {
            if (Parent == null)
                return;

            foreach (var otherLabel in Parent.Controls.OfType<ControlLabel>())
            {
                if (otherLabel != this && otherLabel.MatchSiblingLabels && otherLabel.AutoSizeLabel)
                    otherLabel.AdjustSize(-1, false, false, AdjustSpecified.LabelWidth | AdjustSpecified.MatchSiblingLabel);
            }
        }

        private int CalculateMaxSiblingWidth()
        {
            int maxLabelWidth = 0;

            if (Parent != null)
            {
                foreach (var otherLabel in Parent.Controls.OfType<ControlLabel>())
                {
                    if (otherLabel != this && otherLabel.Visible && otherLabel.MatchSiblingLabels/* && otherLabel.LabelWidthMatters*/)
                        maxLabelWidth = Math.Max(maxLabelWidth, otherLabel.CalculateLabelWidth());
                }
            }

            return maxLabelWidth;
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            NotifyLabelWidthChanged();
        }

        #endregion

        private enum AdjustSpecified
        {
            None = 0,
            LabelWidth = 1,
            LabelDisplay = 2,
            AutoWidth = 4,
            AutoHeight = 8,
            AutoSize = AutoWidth | AutoHeight,
            ControlSize = 16,
            MatchSiblingLabel = 32
        }

        private void AdjustSize(int labelW, bool autoW, bool autoH, AdjustSpecified specified)
        {
            bool autoWChanged = /*autoW != _AutoSizeWidth && */specified.HasFlag(AdjustSpecified.AutoWidth);
            bool autoHChanged = /*autoH != _AutoSizeHeight && */specified.HasFlag(AdjustSpecified.AutoHeight);
            bool labelWChanged = /*labelW != _LabelWidth && */specified.HasFlag(AdjustSpecified.LabelWidth);


            if (labelWChanged || specified.HasFlag(AdjustSpecified.LabelDisplay))
            {
                var oldSize = LabelMinSize;
                if (labelWChanged)
                    _LabelWidth = labelW;

                if (hasInitialized)
                {
                    CalculateLabelSize();
                    UpdateLabelBounds();

                    if (oldSize.Width != LabelMinSize.Width && !specified.HasFlag(AdjustSpecified.MatchSiblingLabel))
                        NotifyLabelWidthChanged();

                }

            }

            if (autoWChanged)
                _AutoSizeWidth = autoW;

            if (autoHChanged)
                _AutoSizeHeight = autoH;

            if (!hasInitialized)
                return;

            if (autoWChanged || autoHChanged || specified.HasFlag(AdjustSpecified.ControlSize))
            {
                CalculateChildControlMinSize();
            }

            if (AutoSizeAll)
                SetBounds(0, 0, 0, 0, BoundsSpecified.Size);
            else if (AutoSizeHeight)
                SetBounds(0, 0, 0, 0, BoundsSpecified.Height);
            else if (AutoSizeWidth)
                SetBounds(0, 0, 0, 0, BoundsSpecified.Width);
            else
                ResizeIfNeeded();

            UpdateChildControlBounds();

            Invalidate();
        }

        protected void OnControlAssigned()
        {
            CalculateChildControlMinSize();

            if (hasInitialized)
            {
                if (AutoSizeAll)
                    SetBounds(0, 0, 0, 0, BoundsSpecified.Size); // Force resize
                else
                    ResizeIfNeeded();
            }
        }

        private void ResizeIfNeeded()
        {
            var minSize = GetMinimumSize();

            if (Width < minSize.Width || Height < minSize.Height)
            {
                SetBounds(0, 0,
                    Math.Max(minSize.Width, Width),
                    Math.Max(minSize.Height, Height),
                    BoundsSpecified.Size);
            }
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            if (!hasInitialized)
                return;

            if (levent.AffectedControl == Control && !IsPositioningChildControl)
                PositionChildControl();

            base.OnLayout(levent);

            if (levent.AffectedControl == this && levent.AffectedProperty == "Bounds" && hasInitialized)
            {
                UpdateChildControlBounds();
            }
        }

        private bool IsPositioningChildControl;

        private void PositionChildControl()
        {
            if (Control == null)
                return;

            IsPositioningChildControl = true;

            if (Control.Dock != DockStyle.None)
            {
                switch (Control.Dock)
                {
                    case DockStyle.Top:
                        Control.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
                        break;
                    case DockStyle.Bottom:
                        Control.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
                        break;
                    case DockStyle.Left:
                        Control.Anchor = AnchorStyles.Top | AnchorStyles.Left;
                        break;
                    case DockStyle.Right:
                        Control.Anchor = AnchorStyles.Top | AnchorStyles.Right;
                        break;
                    case DockStyle.Fill:
                        Control.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
                        break;
                }

                Control.Dock = DockStyle.None;
            }


            Size ctrlSize = Control.Size;

            if (Control.Anchor.HasFlag(AnchorStyles.Left) && Control.Anchor.HasFlag(AnchorStyles.Right) && !AutoSizeWidth)
            {
                ctrlSize.Width = ChildControlBounds.Width - Control.Margin.Horizontal;
            }
            if (Control.Anchor.HasFlag(AnchorStyles.Top) && Control.Anchor.HasFlag(AnchorStyles.Bottom) && !AutoSizeHeight)
            {
                ctrlSize.Height = ChildControlBounds.Height - Control.Margin.Vertical;
            }

            Point ctrlPos = GetPositionForControl(ctrlSize, Control.Margin, ChildControlBounds, Control.Anchor);

            Control.Location = ctrlPos;
            Control.Size = ctrlSize;

            var finalSize = Control.Size;

            if (finalSize != ctrlSize)
            {
                var adjustedPos = GetPositionForControl(finalSize, Control.Margin, ChildControlBounds, Control.Anchor);
                Control.Location = adjustedPos;
            }

            IsPositioningChildControl = false;

        }

        private Point GetPositionForControl(Size controlSize, Padding margin, Rectangle bounds, AnchorStyles anchor)
        {
            Point ctrlPos = bounds.Location;
            //if (Control.Anchor.HasFlag(AnchorStyles.Left) && !Control.Anchor.HasFlag(AnchorStyles.Right))
            if (ControlAlignment == ContentAlignment.TopLeft ||
                ControlAlignment == ContentAlignment.MiddleLeft ||
                ControlAlignment == ContentAlignment.BottomLeft)
            {
                ctrlPos.X += margin.Left;
            }
            //else if (Control.Anchor.HasFlag(AnchorStyles.Right) && !Control.Anchor.HasFlag(AnchorStyles.Left))
            else if (ControlAlignment == ContentAlignment.TopRight ||
                ControlAlignment == ContentAlignment.MiddleRight ||
                ControlAlignment == ContentAlignment.BottomRight)
            {
                ctrlPos.X = bounds.Right - controlSize.Width - margin.Right;
            }
            else
            {
                ctrlPos.X = (bounds.Width - (controlSize.Width + margin.Horizontal)) / 2;
                ctrlPos.X += bounds.Left + margin.Left;
            }

            if (ControlAlignment == ContentAlignment.TopLeft ||
                ControlAlignment == ContentAlignment.TopCenter ||
                ControlAlignment == ContentAlignment.TopRight)
            {
                ctrlPos.Y += margin.Top;
            }
            else if (ControlAlignment == ContentAlignment.BottomLeft ||
                ControlAlignment == ContentAlignment.BottomCenter ||
                ControlAlignment == ContentAlignment.BottomRight)
            {
                ctrlPos.Y = bounds.Bottom - controlSize.Height - margin.Bottom;
            }
            else
            {
                ctrlPos.Y = (bounds.Height - (controlSize.Height + margin.Vertical)) / 2;
                ctrlPos.Y += bounds.Top + margin.Top;
            }

            //if (Control.Anchor.HasFlag(AnchorStyles.Top) && !Control.Anchor.HasFlag(AnchorStyles.Bottom))
            //{
            //    ctrlPos.Y += margin.Top;
            //}
            //else if (Control.Anchor.HasFlag(AnchorStyles.Bottom) && !Control.Anchor.HasFlag(AnchorStyles.Top))
            //{
            //    ctrlPos.Y = bounds.Bottom - controlSize.Height - margin.Bottom;
            //}
            //else
            //{
            //    ctrlPos.Y = (bounds.Height - (controlSize.Height + margin.Vertical)) / 2;
            //    ctrlPos.Y += bounds.Top + margin.Top;
            //}

            return ctrlPos;

        }

        public override Size GetPreferredSize(Size proposedSize)
        {
            var minSize = new Size(
                    LabelMinSize.Width + ControlMinSize.Width,
                    Math.Max(LabelMinSize.Height, ControlMinSize.Height)
                );

            if (!MinimumSize.IsEmpty)
            {
                minSize.Width = Math.Max(minSize.Width, MinimumSize.Width);
                minSize.Height = Math.Max(minSize.Height, MinimumSize.Height);
            }

            return minSize;
        }

        protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
        {
            if (!hasInitialized)
            {
                base.SetBoundsCore(x, y, width, height, specified);
                return;
            }

            bool sizeAssigned = false;

            if (specified.HasFlag(BoundsSpecified.Width) ||
                specified.HasFlag(BoundsSpecified.Height) ||
                specified.HasFlag(BoundsSpecified.Size))
            {
                sizeAssigned = true;
            }

            var minSize = GetMinimumSize();

            if (width < minSize.Width || AutoSizeWidth)
                width = minSize.Width;
            if (height < minSize.Height || AutoSizeHeight)
                height = minSize.Height;

            base.SetBoundsCore(x, y, width, height, specified);


            if (sizeAssigned )
            {
                UpdateLabelBounds();
                UpdateChildControlBounds();
            }
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            base.OnPaint(pe);
            string textToDraw = string.IsNullOrEmpty(Text) ? DefaultText : Text;
            TextRenderer.DrawText(pe.Graphics, textToDraw, Font, 
                LabelBounds, ForeColor,
                GetLabelFormatFlags());

            if (DesignMode && !ChildControlBounds.IsEmpty)
            {
                ControlPaint.DrawFocusRectangle(pe.Graphics, ChildControlBounds);
            }
        }

        public void BeginInit()
        {
            
        }

        public void EndInit()
        {
            hasInitialized = true;
        }
    }

    internal class ControlLabelDesigner : ParentControlDesigner
    {
        //private DesignerActionListCollection _ActionList;
        //private ISelectionService SelectionService;
        //private bool PassThrough;

        protected override bool AllowControlLasso => false;

        private ControlLabel LabelControl => Control as ControlLabel;

        public override SelectionRules SelectionRules
        {
            get
            {
                var rules = base.SelectionRules;
  
                if (LabelControl.AutoSizeHeight)
                    rules &= ~(SelectionRules.TopSizeable | SelectionRules.BottomSizeable);
                if (LabelControl.AutoSizeWidth)
                    rules &= ~(SelectionRules.LeftSizeable | SelectionRules.RightSizeable);

                return rules;
            }
        }

        public override void Initialize(IComponent component)
        {
            base.Initialize(component);

            //EnableDesignMode((Control as CollapsiblePanel).ContentPanel, "ContentPanel");
            //SelectionService = (ISelectionService)GetService(typeof(ISelectionService));
            //Panel.MouseClick += Panel_MouseClick;
        }

        public override bool CanParent(Control control)
        {
            if (Control.Controls.Count == 0)
                return true;
            else
                return false;
            //return base.CanParent(control);
        }
    }
}
