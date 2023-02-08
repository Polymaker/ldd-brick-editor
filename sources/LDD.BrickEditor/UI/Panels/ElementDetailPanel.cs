using LDD.BrickEditor.Models.Navigation;
using LDD.BrickEditor.ProjectHandling;
using LDD.BrickEditor.Resources;
using LDD.Core.Primitives.Connectors;
using LDD.Modding;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LDD.BrickEditor.UI.Panels
{
    public partial class ElementDetailPanel : ProjectDocumentPanel
    {
        private PartElement _SelectedElement;

        public PartElement SelectedElement
        {
            get => _SelectedElement;
            set => SetCurrentObject(value, false);
        }


        private SortableBindingList<PartElement> Elements;

        public ElementDetailPanel()
        {
            InitializeComponent();
            CloseButtonVisible = false;
            CloseButton = false;
            Elements = new SortableBindingList<PartElement>();
        }

        public ElementDetailPanel(ProjectManager projectManager) : base(projectManager)
        {
            InitializeComponent();
            CloseButtonVisible = false;
            CloseButton = false;
            Elements = new SortableBindingList<PartElement>();
            DockAreas ^= WeifenLuo.WinFormsUI.Docking.DockAreas.Document;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            ElementsComboBox.ComboBox.DrawMode = DrawMode.OwnerDrawFixed;
            ElementsComboBox.ComboBox.DrawItem += ComboBox_DrawItem;


            ElementsComboBox.ComboBox.DataSource = Elements;
            ElementsComboBox.ComboBox.DisplayMember = "Name";
            ElementsComboBox.ComboBox.ValueMember = "ID";
            ElementsComboBox.ComboBox.DropDownHeight = 200;
            SetCurrentObject(null, false);
        }

        protected override void OnProjectChanged()
        {
            base.OnProjectChanged();
            UpdateElementList(true);
        }

        protected override void OnElementSelectionChanged()
        {
            base.OnElementSelectionChanged();
            //Trace.WriteLine($"SelectedElement => {ProjectManager.SelectedElement}");
            ExecuteOnThread(() =>
            {
                SetCurrentObject(ProjectManager.SelectedElement, false);
            });
        }

        #region MyRegion

        private void SetCurrentObject(PartElement model, bool fromComboBox)
        {
            using (FlagManager.UseFlag(nameof(SetCurrentObject)))
            {
                if (SelectedElement != null)
                {
                    _SelectedElement.PropertyValueChanged -= SelectedElement_PropertyChanged;
                    _SelectedElement = null;
                }

                _SelectedElement = model;

                using (FlagManager.UseFlag("FillSelectionDetails"))
                {
                    FillElementProperties(model);
                }

                if (SelectedElement != null)
                {
                    _SelectedElement.PropertyValueChanged += SelectedElement_PropertyChanged;

                    if (ProjectManager.SelectedElement != _SelectedElement)
                        ProjectManager.SelectElement(_SelectedElement);
                }

                if (!fromComboBox && ElementsComboBox.SelectedItem != SelectedElement)
                {
                    ElementsComboBox.SelectedItem = SelectedElement;
                }
            }


        }

        #endregion

        #region Elements combobox

        private void UpdateElementList(bool rebuild)
        {

            string prevSelectedID = ElementsComboBox.ComboBox.SelectedValue as string;

            using (FlagManager.UseFlag(nameof(UpdateElementList)))
            {
                if (rebuild || CurrentProject == null)
                    Elements.Clear();

                if (CurrentProject != null)
                {
                    var allNodes = ProjectManager.NavigationTreeNodes.SelectMany(x => x.GetChildHierarchy(true));
                    var elementNodes = allNodes.OfType<ProjectElementNode>().ToList();

                    var validElements = CurrentProject.GetAllElements()
                        .Where(x => !(x is ModelMesh || x is StudReference));

                    if (rebuild)
                        Elements.AddRange(validElements);
                    else
                        Elements.SyncItems(validElements);

                    Elements.SortItems(x =>
                    {
                        int baseNum = 0;
                        var elemNode = elementNodes.FirstOrDefault(y => y.Element == x);
                        if (elemNode != null)
                            baseNum = elementNodes.IndexOf(elemNode);

                        return baseNum;
                    });
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

            SetCurrentObject(ElementsComboBox.SelectedItem as PartElement, true);
        }

        private void ComboBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();

            if (e.Index >= 0)
            {
                var item = Elements[e.Index];
                var cellRect = e.Bounds;
                var offset = item.HierarchyLevel * 6;
                cellRect.X += offset;
                cellRect.Width -= offset;
                using(var brush = new SolidBrush(e.ForeColor))
                    e.Graphics.DrawString(item.Name, e.Font, brush, cellRect);
            }
        }

        #endregion


        protected override void OnProjectCollectionChanged(CollectionChangedEventArgs e)
        {
            base.OnProjectCollectionChanged(e);

            UpdateElementList(false);
        }

        private void SelectedElement_PropertyChanged(object sender, System.ComponentModel.PropertyValueChangedEventArgs e)
        {
            ExecuteOnThread(() =>
                {
                    var element = sender as PartElement;
                    using (FlagManager.UseFlag("UpdatePropertyValue"))
                    {
                        if (e.PropertyName == nameof(PartElement.Name))
                            NameTextBox.Text = element.Name;
                        if (e.PropertyName == nameof(PartSphereCollision.Radius))
                            CollisionRadiusBox.Value = (element as PartSphereCollision).Radius;
                        else if (e.PropertyName == nameof(PartBoxCollision.Size) && element is PartBoxCollision boxColl)
                            CollisionSizeEditor.Value = boxColl.Size * 2d;
                    }
                });
        }

        private string GetElementTypeName(PartElement element)
        {
            switch (element)
            {
                case PartSurface _:
                    return ModelLocalizations.Label_Surface;
                case PartBone _:
                    return ModelLocalizations.Label_Bone;
                case MaleStudModel _:
                    return ModelLocalizations.ModelComponentType_MaleStud;
                case FemaleStudModel _:
                    return ModelLocalizations.ModelComponentType_FemaleStud;
                case BrickTubeModel _:
                    return ModelLocalizations.ModelComponentType_BrickTube;
                case PartModel _:
                    return ModelLocalizations.ModelComponentType_Part;

                case ModelMeshReference _:
                    return ModelLocalizations.Label_Mesh;
                case PartConnection _:
                    return ModelLocalizations.Label_Connection;
                case PartCollision _:
                    return ModelLocalizations.Label_Collision;
            }

            return element.GetType().Name;
        }

        private string GetElementTypeName2(PartElement element)
        {
            if (element is PartSurface)
                return ModelLocalizations.Label_Surface;

            switch (element)
            {
                case PartSurface _:
                    return ModelLocalizations.Label_Surface;
                case PartBone _:
                    return ModelLocalizations.Label_Bone;
                case MaleStudModel _:
                    return ModelLocalizations.ModelComponentType_MaleStud;
                case FemaleStudModel _:
                    return ModelLocalizations.ModelComponentType_FemaleStud;
                case BrickTubeModel _:
                    return ModelLocalizations.ModelComponentType_BrickTube;
                case PartModel _:
                    return ModelLocalizations.ModelComponentType_Part;

                case ModelMeshReference _:
                    return ModelLocalizations.Label_Mesh;
                case PartConnection conn:
                    return $"{ModelLocalizations.Label_Connection} <{conn.ConnectorType.ToString()}>";
                //return ModelLocalizations.ResourceManager.GetString($"ConnectorType_{conn.ConnectorType}");
                case PartCollision coll:
                    string collType = ModelLocalizations.ResourceManager.GetString($"CollisionType_{coll.CollisionType}");
                    return $"{ModelLocalizations.Label_Collision} ({collType})";
            }

            return element.GetType().Name;
        }

       


        #region Main Properties Handling

        private void FillElementProperties(PartElement element)
        {
            ToggleControlsEnabled(element != null,
                NameLabel, SubMaterialIndexLabel, CollisionSizeLabel, CollisionRadiusLabel,
                BoneBoundsLabel, BonePhysPropertiesLabel,
                TransformEdit);

            BoundingEditor.DataBindings.Clear();
            MassNumberBox.DataBindings.Clear();
            CenterOfMassEditor.DataBindings.Clear();

            NameTextBox.Text = element?.Name ?? string.Empty;

            SubMaterialIndexLabel.Visible = element is PartSurface;
            //SubMaterialIndexBox.Visible = element is PartSurface;

            CollisionRadiusLabel.Visible = element is PartSphereCollision;
            //CollisionRadiusBox.Visible = element is PartSphereCollision;

            CollisionSizeLabel.Visible = element is PartBoxCollision;
            //CollisionSizeEditor.Visible = element is PartBoxCollision;
            BonePhysPropertiesLabel.Visible = element is PartBone;
            BoneBoundsLabel.Visible = element is PartBone;

            PatternRepetitionsLabel.Visible = element is RepetitionPattern;
            CircularPatternAngleLabel.Visible = element is CircularPattern;

            if (TransformEdit.Tag != null)
                TransformEdit.BindPhysicalElement(null);

            if (element != null)
            {
                SelectionInfoLabel.Text = GetElementTypeName(element);

                switch (element)
                {
                    case PartSurface surface:
                        SubMaterialIndexBox.Value = surface.SubMaterialIndex;
                        break;

                    case PartBoxCollision boxCollision:
                        CollisionSizeEditor.Value = boxCollision.Size * 2d;
                        break;

                    case PartSphereCollision sphereCollision:
                        CollisionRadiusBox.Value = sphereCollision.Radius;
                        break;

                    case PartBone bone:
                        BoundingEditor.DataBindings.Add(new Binding("Value",
                            bone, nameof(PartBone.Bounding),
                            false, DataSourceUpdateMode.OnPropertyChanged));
                        var matrixValues = bone.PhysicsAttributes.InertiaTensor.ToArray();
                        string matrixStr = string.Join("; ", matrixValues);
                        InertiaTensorTextBox.Text = matrixStr;

                        MassNumberBox.DataBindings.Add(new Binding("Value",
                            bone.PhysicsAttributes, nameof(PartProperties.PhysicsAttributes.Mass),
                            true, DataSourceUpdateMode.OnPropertyChanged));

                        CenterOfMassEditor.DataBindings.Add(new Binding("Value",
                            bone.PhysicsAttributes, nameof(PartProperties.PhysicsAttributes.CenterOfMass),
                            true, DataSourceUpdateMode.OnPropertyChanged));

                        FrictionCheckBox.Checked = bone.PhysicsAttributes.FrictionType == 1;
                        break;

                    case RepetitionPattern repetitionPattern:

                        break;
                }
            }
            else
            {
                if (ProjectManager.SelectedElements.Count > 1)
                    SelectionInfoLabel.Text = MultiSelectionMsg.Text;
                else
                    SelectionInfoLabel.Text = NoSelectionMsg.Text;
            }

            if (element is IPhysicalElement physicalElement)
            {
                TransformEdit.BindPhysicalElement(physicalElement);
                TransformEdit.Tag = physicalElement;
                TransformEdit.Enabled = true;
            }
            else
                TransformEdit.Enabled = false;

            int transformEditIndex = flowLayoutPanel1.Controls.IndexOf(TransformEdit);
            bool setFlowBreak = !(element is PartBone);
            for (int i = transformEditIndex - 1; i >= 0; i--)
            {
                bool isVisible = flowLayoutPanel1.Controls[i].Visible;
                flowLayoutPanel1.SetFlowBreak(flowLayoutPanel1.Controls[i], setFlowBreak && isVisible);

                if (isVisible)
                    setFlowBreak = false;
            }
        }




        #endregion

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                if (NameTextBox.ContainsFocus && NameTextBox.Modified)
                {
                    NameTextBox.Text = ProjectManager.SelectedElement?.Name ?? string.Empty;
                    return true;
                }
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }





        #region Editing handling

        private void CollisionRadiusBox_ValueChanged(object sender, EventArgs e)
        {
            if (FlagManager.IsSet("FillSelectionDetails") ||
                FlagManager.IsSet("UpdatePropertyValue"))
                return;

            if (_SelectedElement is PartSphereCollision sphereCollision)
            {
                sphereCollision.Radius = CollisionRadiusBox.Value;
            }
        }

        private void CollisionSizeEditor_ValueChanged(object sender, EventArgs e)
        {
            if (FlagManager.IsSet("FillSelectionDetails") ||
                FlagManager.IsSet("UpdatePropertyValue"))
                return;

            if (_SelectedElement is PartBoxCollision boxCollision)
            {
                boxCollision.Size = CollisionSizeEditor.Value / 2d;
            }
        }

        private void NameTextBox_Validating(object sender, CancelEventArgs e)
        {
            if (string.IsNullOrEmpty(NameTextBox.Text))
            {
                e.Cancel = true;
            }
        }

        private void NameTextBox_Validated(object sender, EventArgs e)
        {
            if (_SelectedElement == null)
                return;
            var newName = CurrentProject.RenameElement(ProjectManager.SelectedElement, NameTextBox.Text);
            NameTextBox.Text = newName;
        }


        private void SubMaterialIndexBox_ValueChanged(object sender, EventArgs e)
        {
            if (FlagManager.IsSet("FillSelectionDetails") ||
                FlagManager.IsSet("UpdatePropertyValue"))
                return;

            if (_SelectedElement is PartSurface surface)
            {
                surface.SubMaterialIndex = (int)SubMaterialIndexBox.Value;
            }
        }

        private void FrictionCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (FlagManager.IsSet(nameof(SetCurrentObject)))
                return;

            if (SelectedElement is PartBone bone)
                bone.PhysicsAttributes.FrictionType = FrictionCheckBox.Checked ? 1 : 0;
        }

        #endregion
    }
}
