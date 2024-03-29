﻿using LDD.BrickEditor.ProjectHandling;
using LDD.BrickEditor.Resources;
using LDD.Core.Primitives.Connectors;
using LDD.Modding;
using LDD.Common.Simple3D;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LDD.BrickEditor.UI.Panels
{
    public partial class ConnectionEditorPanel : ProjectDocumentPanel
    {
        private SortableBindingList<PartConnection> Connections;
        private SortableBindingList<ConnectorInfo> SubTypeList { get; set; }
        private List<ControlConnType> EditControlHelpers;
   
        //private PartConnection _SelectedElement;

        public PartConnection SelectedElement { get; private set; }

        public ConnectionEditorPanel()
        {
            InitializeComponent();
            Initialize();
        }

        public ConnectionEditorPanel(ProjectManager projectManager) : base(projectManager)
        {
            InitializeComponent();
            Initialize();
        }

        #region Initialization

        private void Initialize()
        {
            Connections = new SortableBindingList<PartConnection>();
            SubTypeList = new SortableBindingList<ConnectorInfo>();
            SubTypeList.ApplySort("SubType", ListSortDirection.Ascending);
            
            ElementsComboBox.ComboBox.DataSource = Connections;
            ElementsComboBox.ComboBox.DisplayMember = "Name";
            ElementsComboBox.ComboBox.ValueMember = "ID";
            CloseButtonVisible = false;

            TypeValueLabel.Text = string.Empty;
            HingeLayoutPanel.Visible = false;

            EditControlHelpers = new List<ControlConnType>()
            {
                new ControlConnType(HingeLayoutPanel, ConnectorType.Hinge),
                new ControlConnType(GearLayoutPanel, ConnectorType.Gear),
                new ControlConnType(TagControlLabel,
                    ConnectorType.Hinge, ConnectorType.Fixed, ConnectorType.Slider),
                new ControlConnType(LengthControlLabel, LengthBox,
                    ConnectorType.Axel, ConnectorType.Slider, ConnectorType.Rail),
                new ControlConnType(SpringPanel, CylindricalCheckBox, ConnectorType.Slider),
                new ControlConnType(AxesControlLabel, ConnectorType.Fixed),
                new ControlConnType(GrabbingLayoutPanel, ConnectorType.Axel),
                new ControlConnType(CapLayoutPanel, ConnectorType.Axel, ConnectorType.Slider),
                new ControlConnType(FlexControlLabel, ConnectorType.Ball)
            };
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            SetCurrentObject(null, false);
            AddConnectionDropDown.Enabled = false;
            BuildAddMenu();
            EditControlHelpers.ForEach(x => x.SetVisibility(false));
        }

        private void BuildAddMenu()
        {
            foreach(ConnectorType connectorType in Enum.GetValues(typeof(ConnectorType)))
            {
                //if (connectorType == ConnectorType.Custom2DField)
                //    continue;

                string menuText = ModelLocalizations.ResourceManager.GetString($"ConnectorType_{connectorType}");
                menuText = menuText.Replace("&", "&&");
                var addMenuItem = AddConnectionDropDown.DropDownItems.Add(menuText);
                addMenuItem.Tag = connectorType;
                addMenuItem.Click += AddMenuItem_Click;
            }
        }

        private void AddMenuItem_Click(object sender, EventArgs e)
        {
            var menu = sender as ToolStripItem;
            var newConnection = ProjectManager.AddConnection((ConnectorType)menu.Tag, null);
            SetCurrentObject(newConnection, false);
        }

        #endregion

        #region Project events

        protected override void OnProjectChanged()
        {
            base.OnProjectChanged();
            UpdateElementList(true);
            AddConnectionDropDown.Enabled = CurrentProject != null;
        }

        protected override void OnProjectCollectionChanged(CollectionChangedEventArgs e)
        {
            base.OnProjectCollectionChanged(e);

            if (e.ElementType == typeof(PartConnection))
                UpdateElementList(false);
        }

        protected override void OnElementSelectionChanged()
        {
            base.OnElementSelectionChanged();
            ExecuteOnThread(() =>
            {
                if (SyncSelectionCheckBox.Checked)
                    SyncToCurrentSelection();
            });
        }

        #endregion

        #region Elements Combobox

        private void UpdateElementList(bool rebuild)
        {

            string prevSelectedID = ElementsComboBox.ComboBox.SelectedValue as string;

            using (FlagManager.UseFlag(nameof(UpdateElementList)))
            {
                if (rebuild || CurrentProject == null)
                    Connections.Clear();

                if (CurrentProject != null)
                {
                    var connectionElems = CurrentProject.GetAllElements<PartConnection>();

                    if (rebuild)
                        Connections.AddRange(connectionElems);
                    else
                        Connections.SyncItems(connectionElems);
                }
            }
            
            string currentSelectedID = ElementsComboBox.ComboBox.SelectedValue as string;
            if (prevSelectedID != currentSelectedID)
                SetCurrentObject(ElementsComboBox.SelectedItem as PartConnection, false);
        }


        private void ElementsComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (FlagManager.IsSet(nameof(UpdateElementList)) || 
                FlagManager.IsSet(nameof(SetCurrentObject)))
                return;

            SetCurrentObject(ElementsComboBox.SelectedItem as PartConnection, true);
        }

        #endregion

        #region Connection Subtypes Combobox

        private void FillSubTypeComboBox(ConnectorType connectorType)
        {
            ConnectionSubTypeCombo.DataSource = null;
            SubTypeList.Clear();

            SubTypeList.AddRange(ResourceHelper.Connectors
                .Where(x => x.Type == connectorType));
            ConnectionSubTypeCombo.DataSource = SubTypeList;
            ConnectionSubTypeCombo.DisplayMember = "SubTypeDisplayText";
            ConnectionSubTypeCombo.ValueMember = "SubType";
        }

        private void ConnectionSubTypeCombo_Validating(object sender, CancelEventArgs e)
        {
            if (ConnectionSubTypeCombo.SelectedItem == null)
            {
                if (!int.TryParse(ConnectionSubTypeCombo.Text, out _))
                    e.Cancel = true;
            }
        }

        private void ConnectionSubTypeCombo_Validated(object sender, EventArgs e)
        {
            if (ConnectionSubTypeCombo.SelectedItem == null &&
                int.TryParse(ConnectionSubTypeCombo.Text, out int subTypeID))
            {
                SetSubTypeComboValue(subTypeID);
            }
        }

        private void SetSubTypeComboValue(int subTypeID)
        {
            var connInfo = SubTypeList.FirstOrDefault(x => x.SubType == subTypeID);
            if (connInfo == null)
            {
                connInfo = new ConnectorInfo()
                {
                    SubType = subTypeID,
                    //Type = 
                };
                SubTypeList.AddSorted(connInfo);
            }
            ConnectionSubTypeCombo.SelectedItem = connInfo;
        }


        private void ConnectionSubTypeCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (FlagManager.IsSet(nameof(SetCurrentObject)))
                return;

            if (SelectedElement != null &&
                ConnectionSubTypeCombo.SelectedItem is ConnectorInfo connInfo)
            {
                if (SelectedElement.SubType != connInfo.SubType)
                    SelectedElement.SubType = connInfo.SubType;
                if (SelectedElement.ConnectorType == ConnectorType.Ball)
                    UpdateFlexEditorVisibility();
            }
        }

        #endregion

        #region MyRegion

        private class ControlConnType
        {
            public Control[] Controls { get; set; }
            public ConnectorType[] ConnectorTypes { get; set; }
            public bool Visible { get; private set; }

            public ControlConnType(Control control, ConnectorType connectorType)
            {
                Controls = new Control[] { control };
                ConnectorTypes = new ConnectorType[] { connectorType };
                Visible = true;
            }

            public ControlConnType(Control[] controls, params ConnectorType[] connectorTypes)
            {
                Controls = controls;
                ConnectorTypes = connectorTypes;
                Visible = true;
            }

            public ControlConnType(Control control, params ConnectorType[] connectorTypes)
            {
                Controls = new Control[] { control };
                ConnectorTypes = connectorTypes;
                Visible = true;
            }

            public ControlConnType(Control ctrl1, Control ctrl2, params ConnectorType[] connectorTypes)
            {
                Controls = new Control[] { ctrl1, ctrl2 };
                ConnectorTypes = connectorTypes;
                Visible = true;
            }

            public ControlConnType(ConnectorType connectorType, params Control[] controls)
            {
                Controls = controls;
                ConnectorTypes = new ConnectorType[] { connectorType };
                Visible = true;
            }

            public ControlConnType(ConnectorType type1, ConnectorType type2, params Control[] controls)
            {
                Controls = controls;
                ConnectorTypes = new ConnectorType[] { type1, type2 };
                Visible = true;
            }

            public void UpdateVisibility(ConnectorType connectorType)
            {
                Visible = ConnectorTypes.Contains(connectorType);
                foreach (Control ctrl in Controls)
                    ctrl.Visible = Visible;
            }

            public void SetVisibility(bool isVisible)
            {
                Visible = isVisible;
                foreach (Control ctrl in Controls)
                    ctrl.Visible = isVisible;
            }
        }

        private void SetCurrentObject(PartConnection connection, bool fromComboBox)
        {
            foreach (var ctrl in GetAllEditControl())
                ctrl.DataBindings.Clear();

            using (FlagManager.UseFlag(nameof(SetCurrentObject)))
            {
                if (SelectedElement != null)
                {
                    SelectedElement.PropertyValueChanged -= SelectedElement_PropertyChanged;
                    SelectedElement = null;
                }

                SelectedElement = connection;
                
                if (SelectedElement != null)
                {
                    ElementNameTextBox.Enabled = true;
                    ConnectionSubTypeCombo.Enabled = true;
                    TransformEdit.Enabled = true;

                    TypeValueLabel.Text = connection.ConnectorType.ToString();
                    ElementNameTextBox.DataBindings.Add(new Binding(
                        "Text",
                        connection,
                        nameof(connection.Name), true, 
                        DataSourceUpdateMode.OnValidation));

                    EditControlHelpers.ForEach(x => x.UpdateVisibility(SelectedElement.ConnectorType));
                    
                    TransformEdit.BindPhysicalElement(connection);

                    switch (SelectedElement.ConnectorType)
                    {

                        case ConnectorType.Hinge:
                            {

                                OrientedCheckBox.DataBindings.Add(new Binding(
                                    "Checked",
                                    connection.Connector,
                                    nameof(HingeConnector.Oriented), true,
                                    DataSourceUpdateMode.OnPropertyChanged));
                                LimitMinBox.DataBindings.Add(new Binding(
                                    "Value",
                                    connection.Connector,
                                    nameof(HingeConnector.LimitMin), true,
                                    DataSourceUpdateMode.OnPropertyChanged));
                                LimitMaxBox.DataBindings.Add(new Binding(
                                    "Value",
                                    connection.Connector,
                                    nameof(HingeConnector.LimitMax), true,
                                    DataSourceUpdateMode.OnPropertyChanged));
                                FlipLimitMinBox.DataBindings.Add(new Binding(
                                    "Value",
                                    connection.Connector,
                                    nameof(HingeConnector.FlipLimitMin), true,
                                    DataSourceUpdateMode.OnPropertyChanged));
                                FlipLimitMaxBox.DataBindings.Add(new Binding(
                                    "Value",
                                    connection.Connector,
                                    nameof(HingeConnector.FlipLimitMax), true,
                                    DataSourceUpdateMode.OnPropertyChanged));
                            }
                            break;

                        case ConnectorType.Axel:
                            {
                                LengthBox.DataBindings.Add(new Binding(
                                    "Value",
                                    connection.Connector,
                                    nameof(AxelConnector.Length), true,
                                    DataSourceUpdateMode.OnPropertyChanged));

                                StartCappedCheckBox.DataBindings.Add(new Binding(
                                    "Checked",
                                    connection.Connector,
                                    nameof(AxelConnector.StartCapped), true,
                                    DataSourceUpdateMode.OnPropertyChanged));
                                EndCappedCheckBox.DataBindings.Add(new Binding(
                                    "Checked",
                                    connection.Connector,
                                    nameof(AxelConnector.EndCapped), true,
                                    DataSourceUpdateMode.OnPropertyChanged));

                                GrabbingCheckBox.DataBindings.Add(new Binding(
                                    "Checked",
                                    connection.Connector,
                                    nameof(AxelConnector.Grabbing), true,
                                    DataSourceUpdateMode.OnPropertyChanged));
                                GrabbingRequiredCheckBox.DataBindings.Add(new Binding(
                                    "Checked",
                                    connection.Connector,
                                    nameof(AxelConnector.IsGrabbingRequired), true,
                                    DataSourceUpdateMode.OnPropertyChanged));
                            }
                            break;

                        case ConnectorType.Gear:
                            {
                                ToothNumBox.DataBindings.Add(new Binding(
                                    "Value",
                                    connection.Connector,
                                    nameof(GearConnector.ToothCount), true,
                                    DataSourceUpdateMode.OnPropertyChanged));
                                RadiusNumBox.DataBindings.Add(new Binding(
                                    "Value",
                                    connection.Connector,
                                    nameof(GearConnector.Radius), true,
                                    DataSourceUpdateMode.OnPropertyChanged));
                            }
                            break;

                        case ConnectorType.Slider:
                            {
                                var sliderConn = connection.Connector as SliderConnector;
                                LengthBox.DataBindings.Add(new Binding(
                                    "Value",
                                    connection.Connector,
                                    nameof(SliderConnector.Length), true,
                                    DataSourceUpdateMode.OnPropertyChanged));

                                CylindricalCheckBox.DataBindings.Add(new Binding(
                                    "Checked",
                                    connection.Connector,
                                    nameof(SliderConnector.Cylindrical), true,
                                    DataSourceUpdateMode.OnPropertyChanged));

                                StartCappedCheckBox.DataBindings.Add(new Binding(
                                    "Checked",
                                    connection.Connector,
                                    nameof(SliderConnector.StartCapped), true,
                                    DataSourceUpdateMode.OnPropertyChanged));
                                EndCappedCheckBox.DataBindings.Add(new Binding(
                                    "Checked",
                                    connection.Connector,
                                    nameof(SliderConnector.EndCapped), true,
                                    DataSourceUpdateMode.OnPropertyChanged));
                                UpdateSliderSpringValue();
                                
                            }
                            break;
                        case ConnectorType.Rail:
                            {
                                LengthBox.DataBindings.Add(new Binding(
                                    "Value",
                                    connection.Connector,
                                    nameof(RailConnector.Length), true,
                                    DataSourceUpdateMode.OnPropertyChanged));
                            }
                            break;

                        case ConnectorType.Fixed:
                            {
                                AxesNumberBox.DataBindings.Add(new Binding(
                                    "Value",
                                    connection.Connector,
                                    nameof(FixedConnector.Axes), true,
                                    DataSourceUpdateMode.OnPropertyChanged));
                            }
                            break;

                        case ConnectorType.Ball:
                            {
                                UpdateFlexEditorVisibility();

                                FillFlexValues();
                            }
                            break;
                    }

                    FillSubTypeComboBox(SelectedElement.ConnectorType);
                    SetSubTypeComboValue(SelectedElement.SubType);

                    SelectedElement.PropertyValueChanged += SelectedElement_PropertyChanged;

                    if (SyncSelectionCheckBox.Checked && fromComboBox
                        && !FlagManager.IsSet(nameof(SyncToCurrentSelection)))
                    {
                        ProjectManager.SelectElement(connection);
                    }
                }
                else
                {
                    TypeValueLabel.Text = string.Empty;
                    //FlexTextBox.Text = string.Empty;
                    EditControlHelpers.ForEach(x => x.SetVisibility(false));
                    ElementNameTextBox.Enabled = false;
                    ConnectionSubTypeCombo.Enabled = false;
                    TransformEdit.Enabled = false;
                    TransformEdit.BindPhysicalElement(null);

                    //HingeLayoutPanel.Visible = false;
                }

                if (!fromComboBox && ElementsComboBox.SelectedItem != SelectedElement)
                {
                    ElementsComboBox.SelectedItem = SelectedElement;
                }
            }
        }

        private void SelectedElement_PropertyChanged(object sender, System.ComponentModel.PropertyValueChangedEventArgs e)
        {
            if (InvokeRequired)
            {
                BeginInvoke((Action)(() =>
                {
                    SelectedElement_PropertyChanged(sender, e);
                }));
                return;
            }

            using (FlagManager.UseFlag(nameof(SelectedElement_PropertyChanged)))
            {
                if (e.PropertyName == nameof(PartConnection.SubType) && SelectedElement != null)
                    SetSubTypeComboValue(SelectedElement.SubType);

                if (e.PropertyName == nameof(BallConnector.FlexAttributes) && SelectedElement != null)
                    FillFlexValues();

                foreach (Control ctrl in GetAllEditControl())
                {
                    var binding = ctrl.DataBindings[0];

                    if (binding.DataSource == sender &&
                        binding.BindingMemberInfo.BindingMember == e.PropertyName)
                    {
                        binding.ReadValue();
                        break;
                    }
                }

                if (e.PropertyName == nameof(SliderConnector.Spring))
                    UpdateSliderSpringValue();
            }
        }

        #endregion

        #region Selection Sync

        private void SyncToCurrentSelection()
        {
            var selectedConnectors = ProjectManager.GetSelectionHierarchy()
                .OfType<PartConnection>();

            if (selectedConnectors.Count() == 1)
            {
                using (FlagManager.UseFlag(nameof(SyncToCurrentSelection)))
                {
                    //ElementsComboBox.SelectedItem = selectedConnectors.FirstOrDefault();
                    SetCurrentObject(selectedConnectors.FirstOrDefault(), false);
                }
            }
        }

        private void SyncSelectionCheckBox_Click(object sender, EventArgs e)
        {
            if (SyncSelectionCheckBox.Checked)
                SyncToCurrentSelection();
        }

        #endregion

        private IEnumerable<Control> GetAllEditControl(Control.ControlCollection controlCollection = null)
        {
            if (controlCollection == null)
                controlCollection = flowLayoutPanel1.Controls;

            foreach (Control ctrl in controlCollection)
            {
                if (ctrl.DataBindings.Count > 0)
                    yield return ctrl;

                if (ctrl.Controls.Count > 0)
                {
                    foreach (var subCtrl in GetAllEditControl(ctrl.Controls))
                        yield return subCtrl;
                }
            }
        }


        private void UpdateSliderSpringValue()
        {
            if (SelectedElement?.Connector is SliderConnector slider)
            {
                SpringCheckBox.Checked = slider.Spring.HasValue;
                if (slider.Spring.HasValue)
                    SpringEditor.Value = slider.Spring.Value;
                else
                    SpringEditor.Value = Vector3d.Zero;
            }
        }

        private void SpringCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SpringEditor.Enabled = SpringCheckBox.Checked;

            if (FlagManager.IsSet(nameof(SelectedElement_PropertyChanged)) ||
                FlagManager.IsSet(nameof(SetCurrentObject)))
                return;

            if (SelectedElement?.Connector is SliderConnector slider)
            {
                slider.Spring = SpringCheckBox.Checked ? (Vector3d?)SpringEditor.Value : null;
            }  
        }

        private void SpringEditor_ValueChanged(object sender, EventArgs e)
        {
            if (FlagManager.IsSet(nameof(SelectedElement_PropertyChanged)) ||
                FlagManager.IsSet(nameof(SetCurrentObject)))
                return;

            if (SpringCheckBox.Checked && SelectedElement?.Connector is SliderConnector slider)
                slider.Spring = SpringEditor.Value;
        }

        private void SpringEditor_SizeChanged(object sender, EventArgs e)
        {
            SpringPanel.Height = SpringEditor.Bottom + 3;
        }

        private void UpdateFlexEditorVisibility()
        {
            var flexEditor = EditControlHelpers.First(x => x.Controls.Contains(FlexControlLabel));
            if (SelectedElement.ConnectorType == ConnectorType.Ball)
            {
                if (!CurrentProject.Flexible && flexEditor.Visible)
                    flexEditor.SetVisibility(false);
                else
                    flexEditor.SetVisibility(SelectedElement.SubType > 1000);
            }
            else if (flexEditor.Visible)
                flexEditor.SetVisibility(false);
        }

        private string ValidateFlexValues()
        {

            return string.Empty;
        }


        private void FillFlexValues()
        {
            using (FlagManager.UseFlag(nameof(FillFlexValues)))
            {
                FlexTextBox.Text = string.Empty;
                if (SelectedElement?.Connector is BallConnector ballConnector)
                {
                    if (ballConnector.FlexAttributes != null)
                        FlexTextBox.Text = string.Join("; ", ballConnector.FlexAttributes);
                }
            }

        }

        private void FlexTextBox_Validating(object sender, CancelEventArgs e)
        {
            if (FlagManager.IsSet(nameof(FillFlexValues)))
                return;

            if (!string.IsNullOrEmpty(FlexTextBox.Text))
            {
                var matValues = FlexTextBox.Text.Split(';', ',');

                if (matValues.Length != 5)
                {
                    MessageBox.Show("Invalid number of values. Must have 5 numbers.");
                    e.Cancel = true;
                    return;
                }
                var invalidValues = new List<string>();
                for (int i = 0; i < matValues.Length; i++)
                {
                    if (!Utilities.NumberHelper.SmartTryParse(matValues[i].Trim(), out _))
                        invalidValues.Add(matValues[i]);
                }
                if (invalidValues.Any())
                {
                    MessageBox.Show($"Invalid values: {string.Join(", ", invalidValues)}");
                    e.Cancel = true;
                    return;
                }
            }
        }

        

        private void FlexTextBox_Validated(object sender, EventArgs e)
        {
            if (FlagManager.IsSet(nameof(FillFlexValues)))
                return;


            if (!string.IsNullOrEmpty(FlexTextBox.Text) && 
                BallConnector.TryParseFlexAttributes(FlexTextBox.Text.Replace(";", ","), out double[] values))
            {
                //var matValues = FlexTextBox.Text.Split(';', ',');
                //var values = new double[5];
                //for (int i = 0; i < matValues.Length; i++)
                //{
                //    if (Utilities.NumberHelper.SmartTryParse(matValues[i].Trim(), out double fV))
                //        values[i] = fV;
                //}
                (SelectedElement.Connector as BallConnector).FlexAttributes = values;
                //(SelectedElement.Connector as BallConnector).SetFlexValues(values);
            }
            else if (string.IsNullOrEmpty(FlexTextBox.Text))
                (SelectedElement.Connector as BallConnector).FlexAttributes = null;
        }

        private void flowLayoutPanel1_Layout(object sender, LayoutEventArgs e)
        {
            AdjustFlowBreak();
        }

        private void AdjustFlowBreak()
        {

            Control currentFlowBreak = null;
            Control ctrlBeforeTransform = null;
            foreach (Control ctrl in flowLayoutPanel1.Controls)
            {
                bool isFlowBreak = flowLayoutPanel1.GetFlowBreak(ctrl);
                if (isFlowBreak)
                    currentFlowBreak = ctrl;


                if (ctrl == TransformEdit)
                    break;

                if (ctrl.Visible)
                    ctrlBeforeTransform = ctrl;
            }

            if (Width >= NameControlLabel.Width * 1.75)
            {
                if (ctrlBeforeTransform != null && ctrlBeforeTransform != currentFlowBreak)
                {
                    if (currentFlowBreak != null)
                        flowLayoutPanel1.SetFlowBreak(currentFlowBreak, false);

                    //only flow break if the last control is in the first column
                    if (ctrlBeforeTransform.Left <= 50) 
                        flowLayoutPanel1.SetFlowBreak(ctrlBeforeTransform, true);
                }
                else if (currentFlowBreak != null && ctrlBeforeTransform == null)
                    flowLayoutPanel1.SetFlowBreak(currentFlowBreak, false);
                else if (currentFlowBreak != null && currentFlowBreak.Left > 50)
                    flowLayoutPanel1.SetFlowBreak(currentFlowBreak, false);
            }
            else if (currentFlowBreak != null)
            {
                flowLayoutPanel1.SetFlowBreak(currentFlowBreak, false);
            }
        }

        
    }
}
