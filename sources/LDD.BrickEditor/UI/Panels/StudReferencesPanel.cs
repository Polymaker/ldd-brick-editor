using LDD.BrickEditor.ProjectHandling;
using LDD.BrickEditor.UI.Editors;
using LDD.Modding;
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
    public partial class StudReferencesPanel : ProjectDocumentPanel
    {
        private SortableBindingList<PartCullingModel> Models;

        public PartCullingModel SelectedElement { get; private set; }

        private SortableBindingList<StudReferenceListModel> StudReferences { get; set; }

        private SortableBindingList<PartConnection> StudConnections { get; set; }

        private class StudReferenceListModel
        {
            public StudReference StudReference { get; set; }
            public string ConnectionName => StudReference.Connection?.Name;
            public string ConnectionID
            {
                get => StudReference.ConnectionID;
                set => StudReference.ConnectionID = value;
            }
            public int PositionX
            {
                get => StudReference.PositionX;
                set => StudReference.PositionX = value;
            }
            public int PositionY
            {
                get => StudReference.PositionY;
                set => StudReference.PositionY = value;
            }
            public int Value1
            {
                get => StudReference.Value1;
                set => StudReference.Value1 = value;
            }
            public int Value2
            {
                get => StudReference.Value2;
                set => StudReference.Value2 = value;
            }
            public string ButtonText { get; set; } = "X";
            public bool IsAdjacent { get; set; }

            public StudReferenceListModel(StudReference studReference)
            {
                StudReference = studReference;
                if (studReference.Parent is PartCullingModel cullingModel)
                    IsAdjacent = !cullingModel.ReferencedStuds.Contains(studReference);
            }
        }

        public StudReferencesPanel()
        {
            InitializeComponent();
            InitializeView();
            CloseButtonVisible = false;
            CloseButton = false;
        }

        public StudReferencesPanel(ProjectManager projectManager) : base(projectManager)
        {
            InitializeComponent();
            InitializeView();
            CloseButtonVisible = false;
            CloseButton = false;
        }

        private void InitializeView()
        {
            olvConnectionColumn.AspectGetter = (x) =>
            {
                if (x is StudReferenceListModel studRef)
                    return studRef.ConnectionName ?? "TODO";

                return string.Empty;
            };
            olvConnectionColumn.GroupKeyGetter = (rowObj) =>
            {
                if (rowObj is StudReferenceListModel model)
                    return model.IsAdjacent ? "2_ADJ_STUD" : "1_TUBE_STUD";
                return string.Empty;
            };
            olvConnectionColumn.GroupKeyToTitleConverter = (groupKey) =>
            {
                if (groupKey as string == "1_TUBE_STUD")
                    return "Tube stud";
                else if (groupKey as string == "2_ADJ_STUD")
                    return "Adjacent studs";
                return string.Empty;
            };
            olvPositionColumn.AspectGetter = (x) =>
            {
                if (x is StudReferenceListModel studRef)
                    return $"X:{studRef.PositionX} Y:{studRef.PositionY}";

                return string.Empty;
            };

            Models = new SortableBindingList<PartCullingModel>();
            StudConnections = new SortableBindingList<PartConnection>();
            StudReferences = new SortableBindingList<StudReferenceListModel>();
            StudReferences.ListChanged += StudReferences_ListChanged;
            StudRefListView.DataSource = StudReferences;
            StudRefListView.ShowGroups = false;
            
    
            ElementsComboBox.ComboBox.DataSource = Models;
            ElementsComboBox.ComboBox.DisplayMember = "Name";
            ElementsComboBox.ComboBox.ValueMember = "ID";

            AddStudButton.Enabled = false;
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            if (ProjectManager.IsProjectOpen && SelectedElement != null)
                DisplaySelectedStudInViewPort();
        }

        private void SetCurrentObject(PartCullingModel model, bool fromComboBox)
        {
            using (FlagManager.UseFlag(nameof(SetCurrentObject)))
            {
                if (SelectedElement != null)
                {
                    SelectedElement.ReferencedStuds.CollectionChanged -= ReferencedStuds_CollectionChanged;
                    if (SelectedElement is BrickTubeModel tubeModel)
                        tubeModel.AdjacentStuds.CollectionChanged -= ReferencedStuds_CollectionChanged;
                }

                StudReferences.Clear();

                SelectedElement = model;
                StudRefListView.ShowGroups = model is BrickTubeModel;
                
                if (SelectedElement != null)
                {
                    StudReferences.AddRange(SelectedElement.GetStudReferences().Select(x => new StudReferenceListModel(x)));
                    
                    SelectedElement.ReferencedStuds.CollectionChanged += ReferencedStuds_CollectionChanged;
                    if (SelectedElement is BrickTubeModel tubeModel)
                        tubeModel.AdjacentStuds.CollectionChanged += ReferencedStuds_CollectionChanged;
                }

                if (!fromComboBox && ElementsComboBox.SelectedItem != SelectedElement)
                {
                    ElementsComboBox.SelectedItem = SelectedElement;
                }
            }

            UpdateToolBarButtonStates();
        }

        private void UpdateToolBarButtonStates()
        {
            bool anyStudConn = StudConnections.Any();
            
            AddStudButton.Enabled = SelectedElement != null && anyStudConn;
            

            if (SelectedElement is BrickTubeModel tubeModel && anyStudConn)
            {
                AddStudButton.Visible = !(tubeModel.TubeStud != null);
                GenerateAdjStudsButton.Visible = (tubeModel.TubeStud != null);
                GenerateAdjStudsButton.Enabled = tubeModel.TubeStud.FieldNode != null;
            }
            else
            {
                AddStudButton.Visible = true;
                GenerateAdjStudsButton.Visible = false;
            }
        }

        #region Project events

        protected override void OnProjectClosed()
        {
            base.OnProjectClosed();
            SetCurrentObject(null, true);
        }

        protected override void OnProjectChanged()
        {
            base.OnProjectChanged();

            ReloadConnections();
            UpdateElementList(true);
            UpdateToolBarButtonStates();
        }

        protected override void OnElementPropertyChanged(ObjectPropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(e);

            if (e.Object is StudReference studReference)
            {
                var model = StudReferences.FirstOrDefault(x => x.StudReference == studReference);

                if (model != null)
                {
                    this.ExecuteOnThread(() =>
                    {
                        StudRefListView.RefreshObject(model);
                    });
                }
            }
        }


        protected override void OnProjectCollectionChanged(CollectionChangedEventArgs e)
        {
            base.OnProjectCollectionChanged(e);

            if (e.ChangedElements<PartCullingModel>().Any())
                UpdateElementList(false);
            else if (e.ChangedElements<PartConnection>().Any(c => c.ConnectorType == Core.Primitives.Connectors.ConnectorType.Custom2DField))
                ReloadConnections();
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

        #region Model Elements Combobox

        private void UpdateElementList(bool rebuild)
        {

            string prevSelectedID = ElementsComboBox.ComboBox.SelectedValue as string;

            using (FlagManager.UseFlag(nameof(UpdateElementList)))
            {
                if (rebuild || CurrentProject == null)
                    Models.Clear();

                if (CurrentProject != null)
                {
                    var cullingModels = CurrentProject.GetAllElements<PartCullingModel>();

                    if (rebuild)
                        Models.AddRange(cullingModels);
                    else
                        Models.SyncItems(cullingModels);
                }
            }

            var modelToSelect = Models.FirstOrDefault(x => x.ID == prevSelectedID);
            if (modelToSelect == null && Models.Count > 0)
                modelToSelect = Models[0];

            string currentSelectedID = ElementsComboBox.ComboBox.SelectedValue as string;

            SetCurrentObject(modelToSelect, false);
        }


        private void ElementsComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (FlagManager.IsSet(nameof(UpdateElementList)) ||
                FlagManager.IsSet(nameof(SetCurrentObject)))
                return;

            SetCurrentObject(ElementsComboBox.SelectedItem as PartCullingModel, true);
        }

        #endregion

        #region Selection Sync

        private void SyncToCurrentSelection()
        {
            var selectedModels = ProjectManager.GetSelectionHierarchy()
                .OfType<PartCullingModel>();

            if (selectedModels.Count() == 1)
            {
                using (FlagManager.UseFlag(nameof(SyncToCurrentSelection)))
                {
                    //ElementsComboBox.SelectedItem = selectedConnectors.FirstOrDefault();
                    SetCurrentObject(selectedModels.FirstOrDefault(), false);
                }
            }
        }

        private void SyncSelectionCheckBox_Click(object sender, EventArgs e)
        {
            if (SyncSelectionCheckBox.Checked)
                SyncToCurrentSelection();
        }

        #endregion

        #region Connections Combo

        private void ReloadConnections()
        {
            if (ProjectManager.IsProjectOpen)
            {
                var conns = CurrentProject.GetAllElements<PartConnection>()
                    .Where(x => x.ConnectorType == Core.Primitives.Connectors.ConnectorType.Custom2DField);

                StudConnections.SyncItems(conns);
                
                //ConnectionColumn.DataSource = conns.ToList();
                //ConnectionColumn.ValueMember = nameof(PartConnection.ID);
                //ConnectionColumn.DisplayMember = nameof(PartConnection.Name);
            }
            else
            {
                StudConnections.Clear();
                //ConnectionColumn.DataSource = new List<PartConnection>();
            }

            UpdateToolBarButtonStates();
        }

        #endregion

        #region Reference list sync


        private void StudReferences_ListChanged(object sender, ListChangedEventArgs e)
        {
            if (SelectedElement == null || FlagManager.IsSet(nameof(SetCurrentObject)))
                return;

            if (FlagManager.IsSet("SyncStuds"))
                return;

            using (FlagManager.UseFlag("SyncStuds"))
            {
                ProjectManager.StartBatchChanges();

                var normalStuds = StudReferences.Where(x => !x.IsAdjacent).Select(x => x.StudReference);
                var adjStuds = StudReferences.Where(x => x.IsAdjacent).Select(x => x.StudReference);
                SyncCurrentStudsToElement(normalStuds, SelectedElement.ReferencedStuds);

                if (SelectedElement is BrickTubeModel tubeModel)
                    SyncCurrentStudsToElement(adjStuds, tubeModel.AdjacentStuds);

                ProjectManager.EndBatchChanges();
            }
        }

        private void SyncCurrentStudsToElement(IEnumerable<StudReference> currentRefs, ElementCollection<StudReference> elementStuds)
        {
            var addedItems = currentRefs.Except(elementStuds).ToList();
            var removedItems = elementStuds.Except(currentRefs).ToList();

            if (addedItems.Count > 0)
                elementStuds.AddRange(addedItems);
            if (removedItems.Count > 0)
                elementStuds.RemoveRange(removedItems);
        }

        private void SyncCurrentStudsFromElement(IEnumerable<StudReference> currentRefs, ElementCollection<StudReference> elementStuds)
        {
            var addedItems = elementStuds.Except(currentRefs).ToList();
            var removedItems = currentRefs.Except(elementStuds).ToList();

            if (addedItems.Count > 0)
                StudReferences.AddRange(addedItems.Select(x => new StudReferenceListModel(x)));
            if (removedItems.Count > 0)
                StudReferences.RemoveAll(x => removedItems.Contains(x.StudReference));
        }

        private void ReferencedStuds_CollectionChanged(object sender, CollectionChangedEventArgs ccea)
        {
            if (FlagManager.IsSet("SyncStuds"))
                return;

            using (FlagManager.UseFlag("SyncStuds"))
            {
                var normalStuds = StudReferences.Where(x => !x.IsAdjacent).Select(x => x.StudReference);
                var adjStuds = StudReferences.Where(x => x.IsAdjacent).Select(x => x.StudReference);
                SyncCurrentStudsFromElement(normalStuds, SelectedElement.ReferencedStuds);

                if (SelectedElement is BrickTubeModel tubeModel)
                    SyncCurrentStudsFromElement(adjStuds, tubeModel.AdjacentStuds);
            }
        }

        #endregion

        #region Reference list handling

        private void StudRefListView_CellEditStarting(object sender, BrightIdeasSoftware.CellEditEventArgs e)
        {
            if (e.RowObject is StudReferenceListModel model)
            {
                if (e.Column == olvConnectionColumn)
                {
                    var connCbo = new ComboBox
                    {
                        DropDownStyle = ComboBoxStyle.DropDownList,
                        Bounds = e.CellBounds
                    };
                    connCbo.CreateControl();
                    connCbo.BindingContext = new BindingContext();
                    connCbo.DataSource = StudConnections;
                    connCbo.ValueMember = nameof(PartConnection.ID);
                    connCbo.DisplayMember = nameof(PartConnection.Name);
                    connCbo.Update();
                    if (!string.IsNullOrEmpty(model.ConnectionID))
                        connCbo.SelectedValue = model.ConnectionID;
                    //connCbo.SelectedIndex = StudConnections.IndexOf(model.StudReference.Connection);
                    e.Control = connCbo;
                }
                else if (e.Column == olvPositionColumn)
                {
                    var studConnector = model.StudReference.Connector;

                    if (studConnector == null)
                    {
                        e.Cancel = true;
                        return;
                    }

                    var cellRefPicker = new StudGridCellPicker
                    {
                        Bounds = e.CellBounds
                    };
                    cellRefPicker.CreateControl();
                    cellRefPicker.StudConnector = studConnector;
                    cellRefPicker.SelectedCell = new Point(model.PositionX, model.PositionY);
     
                    e.Control = cellRefPicker;
                    //this.InvokeWithDelay(40, () =>
                    //{
                    //    cellRefPicker.Select();
                    //    cellRefPicker.Focus();
                    //    cellRefPicker.ShowDropDown();
                    //});

                }
            }
        }

        private void StudRefListView_CellEditFinished(object sender, BrightIdeasSoftware.CellEditEventArgs e)
        {
            if (e.RowObject is StudReferenceListModel model)
            {
                if (e.Column == olvConnectionColumn)
                {
                    model.StudReference.ConnectionIndex = -1;
                    model.ConnectionID = e.NewValue as string;
                }
                else if (e.Column == olvPositionColumn && e.Control is StudGridCellPicker cellPicker)
                {
                    ProjectManager.StartBatchChanges();
                    model.PositionX = cellPicker.SelectedCell.X;
                    model.PositionY = cellPicker.SelectedCell.Y;
                    model.StudReference.FieldIndex = -1;
                    ProjectManager.EndBatchChanges();
                }
                UpdateToolBarButtonStates();
            }
        }

        private void StudRefListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (IsActivated)
                DisplaySelectedStudInViewPort();
        }


        #endregion


        private void DisplaySelectedStudInViewPort()
        {
            if (StudRefListView.SelectedItem?.RowObject is StudReferenceListModel model)
            {
                ProjectManager.ViewportWindow.SelectStudReference(model.StudReference);
            }
            else
                ProjectManager.ViewportWindow.SelectStudReference(null);
        }

        private void AddStudButton_Click(object sender, EventArgs e)
        {
            if (SelectedElement == null)
                return;
            
            var existingRef = SelectedElement.GetStudReferences().FirstOrDefault(x => x.Connection != null);
            string defaultConnectionID = existingRef?.ConnectionID ?? string.Empty;

            if (string.IsNullOrEmpty(defaultConnectionID))
            {
                if (SelectedElement is MaleStudModel)
                    defaultConnectionID = StudConnections.FirstOrDefault(x => x.SubType == 23)?.ID;
                else
                    defaultConnectionID = StudConnections.FirstOrDefault(x => x.SubType == 22)?.ID;
            }

            var newStudRef = new StudReference()
            {
                ConnectionID = defaultConnectionID
            };

            var model = new StudReferenceListModel(newStudRef);
            StudReferences.Add(model);
            StudRefListView.EditModel(model);

            UpdateToolBarButtonStates();
        }

        private void StudRefListView_ButtonClick(object sender, BrightIdeasSoftware.CellClickEventArgs e)
        {
            var modelToDelete = e.Model as StudReferenceListModel;

            if (modelToDelete != null)
                StudReferences.Remove(modelToDelete);

            if (ProjectManager.ViewportWindow.SelectedStudModel.Stud == modelToDelete.StudReference)
                ProjectManager.ViewportWindow.SelectStudReference(null);

            UpdateToolBarButtonStates();
        }

        private void GenerateAdjStudsButton_Click(object sender, EventArgs e)
        {
            if (SelectedElement is BrickTubeModel tubeModel)
            {
                ProjectManager.StartBatchChanges();
                tubeModel.AutoGenerateAdjacentStuds();
                ProjectManager.EndBatchChanges();
            }
        }
    }
}
