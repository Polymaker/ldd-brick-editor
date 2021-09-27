using LDD.BrickEditor.ProjectHandling;
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
        }

        public StudReferencesPanel(ProjectManager projectManager) : base(projectManager)
        {
            InitializeComponent();
            InitializeView();
        }

        private void InitializeView()
        {
            olvConnectionColumn.AspectGetter = (x) =>
            {
                if (x is StudReferenceListModel studRef)
                    return studRef.ConnectionName ?? "TODO";

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
            ElementsComboBox.ComboBox.DataSource = Models;
            ElementsComboBox.ComboBox.DisplayMember = "Name";
            ElementsComboBox.ComboBox.ValueMember = "ID";

        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            if (ProjectManager.IsProjectOpen && SelectedElement != null)
                DsiplaySelectedStudInViewPort();
        }

        private void SetCurrentObject(PartCullingModel model, bool fromComboBox)
        {
            using (FlagManager.UseFlag(nameof(SetCurrentObject)))
            {
                if (SelectedElement != null)
                {
                    SelectedElement.ReferencedStuds.CollectionChanged -= ReferencedStuds_CollectionChanged;
                }

                StudReferences.Clear();

                SelectedElement = model;
                StudRefListView.ShowGroups = model is BrickTubeModel;
                AddStudButton.Enabled = model != null;

                if (SelectedElement != null)
                {
                    StudReferences.AddRange(SelectedElement.GetStudReferences().Select(x => new StudReferenceListModel(x)));
                    
                    SelectedElement.ReferencedStuds.CollectionChanged += ReferencedStuds_CollectionChanged;
                }

                if (!fromComboBox && ElementsComboBox.SelectedItem != SelectedElement)
                {
                    ElementsComboBox.SelectedItem = SelectedElement;
                }
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

            if (e.ElementType == typeof(PartCullingModel))
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
        }

        #endregion


        #region Reference list sync


        private void StudReferences_ListChanged(object sender, ListChangedEventArgs e)
        {
            if (SelectedElement == null || FlagManager.IsSet(nameof(SetCurrentObject)))
                return;

            using (FlagManager.UseFlag("SyncStuds"))
            {
                var normalStuds = StudReferences.Where(x => !x.IsAdjacent).Select(x => x.StudReference);
                var adjStuds = StudReferences.Where(x => x.IsAdjacent).Select(x => x.StudReference);
                SynchronizeElementStuds(normalStuds, SelectedElement.ReferencedStuds);

                if (SelectedElement is BrickTubeModel tubeModel)
                    SynchronizeElementStuds(adjStuds, tubeModel.AdjacentStuds);
            }
        }

        private void SynchronizeElementStuds(IEnumerable<StudReference> currentRefs, ElementCollection<StudReference> elementStuds)
        {
            var addedItems = currentRefs.Except(elementStuds).ToList();
            var removedItems = elementStuds.Except(currentRefs).ToList();

            if (addedItems.Count > 0)
                elementStuds.AddRange(addedItems);
            if (removedItems.Count > 0)
                elementStuds.RemoveRange(removedItems);
        }

        private void ReferencedStuds_CollectionChanged(object sender, CollectionChangedEventArgs ccea)
        {
            if (FlagManager.IsSet("SyncStuds"))
                return;


        }

        #endregion

        private void StudRefListView_CellEditStarting(object sender, BrightIdeasSoftware.CellEditEventArgs e)
        {
            if (e.RowObject is StudReferenceListModel model)
            {
                if (e.Column == olvConnectionColumn)
                {
                    var connCbo = new ComboBox
                    {
                        DataSource = StudConnections,
                        ValueMember = nameof(PartConnection.ID),
                        DisplayMember = nameof(PartConnection.Name),
                        DropDownStyle = ComboBoxStyle.DropDownList,
                        SelectedValue = model.ConnectionID,
                        Bounds = e.CellBounds
                    };
                    e.Control = connCbo;
                }
            }
           
        }

        private void StudRefListView_CellEditFinished(object sender, BrightIdeasSoftware.CellEditEventArgs e)
        {
            if (e.RowObject is StudReferenceListModel model)
            {
                if (e.Column == olvConnectionColumn)
                {
                    model.ConnectionID = e.NewValue as string;
                    //StudRefListView.RefreshObject(model);
                }
            }
        }

        private void StudRefListView_SelectedIndexChanged(object sender, EventArgs e)
        {

            if (IsActivated)
                DsiplaySelectedStudInViewPort();
        }

        private void DsiplaySelectedStudInViewPort()
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
            var newStudRef = new StudReference();
            var model = new StudReferenceListModel(newStudRef);
            StudReferences.Add(model);
            StudRefListView.EditModel(model);
        }
    }
}
