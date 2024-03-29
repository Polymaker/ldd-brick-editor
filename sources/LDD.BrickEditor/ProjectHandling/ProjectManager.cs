﻿using LDD.BrickEditor.Models.Navigation;
using LDD.BrickEditor.ProjectHandling.ViewInterfaces;
using LDD.BrickEditor.Rendering;
using LDD.BrickEditor.Resources;
using LDD.BrickEditor.Settings;
using LDD.BrickEditor.UI.Windows;
using LDD.Core;
using LDD.Core.Parts;
using LDD.Core.Primitives.Collisions;
using LDD.Core.Primitives.Connectors;
using LDD.Modding;
using LDD.Common.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace LDD.BrickEditor.ProjectHandling
{
    public class ProjectManager : IProjectManager
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetLogger("Project Manager");

        private List<PartElement> _SelectedElements;
        private List<ValidationMessage> _ValidationMessages;
        private long LastValidation;
        private long LastSavedChange;

        public PartProject CurrentProject { get; private set; }

        public UndoRedoManager UndoRedoManager { get; }

        public string CurrentProjectPath { get; private set; }

        public string TemporaryProjectPath { get; private set; }

        public bool IsProjectOpen => CurrentProject != null;

        //public bool IsNewProject => IsProjectOpen && string.IsNullOrEmpty(CurrentProject.ProjectPath);
        public bool IsNewProject => IsProjectOpen && string.IsNullOrEmpty(CurrentProjectPath);

        public bool IsModified => LastSavedChange != UndoRedoManager.CurrentChangeID || UndoRedoManager.NonReversibleActionPerformed;

        public event EventHandler UndoHistoryChanged
        {
            add => UndoRedoManager.UndoHistoryChanged += value;
            remove => UndoRedoManager.UndoHistoryChanged -= value;
        }

        public PartElement SelectedElement
        {
            get => _SelectedElements.FirstOrDefault();
            set => SelectElement(value);
        }

        public IList<PartElement> SelectedElements => _SelectedElements.AsReadOnly();

        public ProjectTreeNodeCollection NavigationTreeNodes { get; private set; }

        #region Windows

        public IMainWindow MainWindow { get; set; }
        public IViewportWindow ViewportWindow { get; set; }
        public INavigationWindow NavigationWindow { get; set; }

        #endregion

        #region Events

        public event EventHandler SelectionChanged;

        public event EventHandler ProjectClosed;

        public event EventHandler ProjectChanged;

        public event EventHandler ProjectModified;

        /// <summary>
        /// Raised when a collection in the project has changed.
        /// </summary>
        public event CollectionChangedEventHandler ProjectCollectionChanged;

        public event EventHandler<ObjectPropertyChangedEventArgs> ObjectPropertyChanged;

        /// <summary>
        /// Raised when one or more collections in the project has changed. <br/>
        /// Raised only after all changes are applied
        /// </summary>
        public event EventHandler ProjectElementsChanged;

        #endregion

        private bool ElementsChanged;
        private bool PreventProjectChange;
        private bool IsProjectAttached;
        private bool PreventCollectionEvents;

        static ProjectManager()
        {
            ElementExtenderFactory.RegisterExtension(typeof(PartSurface), typeof(ModelElementExtension));
            ElementExtenderFactory.RegisterExtension(typeof(SurfaceComponent), typeof(ModelElementExtension));
            ElementExtenderFactory.RegisterExtension(typeof(FemaleStudModel), typeof(FemaleStudModelExtension));
            ElementExtenderFactory.RegisterExtension(typeof(ModelMeshReference), typeof(ModelElementExtension));
            
            ElementExtenderFactory.RegisterExtension(typeof(PartBone), typeof(BoneElementExtension));

            ElementExtenderFactory.RegisterExtension(typeof(PartCollision), typeof(ModelElementExtension));
            ElementExtenderFactory.RegisterExtension(typeof(PartConnection), typeof(ModelElementExtension));

            ElementExtenderFactory.RegisterExtension(typeof(ClonePattern), typeof(ModelElementExtension));
        }

        public ProjectManager()
        {
            _SelectedElements = new List<PartElement>();
            _ValidationMessages = new List<ValidationMessage>();
            NavigationTreeNodes = new ProjectTreeNodeCollection(this);
            
            UndoRedoManager = new UndoRedoManager(this);
            UndoRedoManager.BeginUndoRedo += UndoRedoManager_BeginUndoRedo;
            UndoRedoManager.EndUndoRedo += UndoRedoManager_EndUndoRedo;
            UndoRedoManager.UndoHistoryChanged += UndoRedoManager_UndoHistoryChanged;

            _ShowPartModels = true;
            _ShowGrid = true;
            _ShowBones = true;
            _PartRenderMode = MeshRenderMode.SolidWireframe;
        }

        #region Project Loading/Closing

        public void SetCurrentProject(PartProject project, string tempPath = null)
        {
            if (CurrentProject != project)
            {
                PreventProjectChange = true;
                CloseCurrentProject();
                PreventProjectChange = false;

                CurrentProject = project;
                CurrentProjectPath = project.ProjectPath;

                if (!string.IsNullOrEmpty(tempPath))
                {
                    LastSavedChange = -1;
                    if (File.Exists(tempPath))
                        TemporaryProjectPath = tempPath;
                }

                SaveWorkingProject();

                SettingsManager.AddOpenedFile(GetCurrentProjectInfo());

                if (project != null)
                    AttachPartProject(project);

                ProjectChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public void CloseCurrentProject()
        {
            if (CurrentProject != null)
            {
                if (!string.IsNullOrEmpty(TemporaryProjectPath))
                    DeleteTemporaryProject(TemporaryProjectPath);
                else
                    SettingsManager.RemoveOpenedFile(CurrentProjectPath);

                DettachPartProject(CurrentProject);

                ProjectClosed?.Invoke(this, EventArgs.Empty);

                CurrentProject = null;
                UndoRedoManager.ClearHistory();

                if (!PreventProjectChange)
                    ProjectChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public void DeleteTemporaryProject(string path)
        {
            string directoryPath = path;
            if ((Path.GetFileName(path) ?? string.Empty) != string.Empty && File.Exists(path))
                directoryPath = Path.GetDirectoryName(path);

            if (Directory.Exists(directoryPath))
            {
                Task.Factory.StartNew(() => FileHelper.DeleteFileOrFolder(directoryPath, true, true));
            }

            SettingsManager.RemoveOpenedFile(path);
        }

        private void AttachPartProject(PartProject project)
        {
            project.ProjectCollectionChanged += Project_ProjectCollectionChanged;
            project.ProjectObjectChanged += Project_ProjectObjectChanged;
            //project.ElementCollectionChanged += Project_ElementCollectionChanged;
            //project.ElementPropertyChanged += Project_ElementPropertyChanged;
            project.UpdateModelStatistics();

            //LoadUserProperties();

            LastValidation = -1;
            IsProjectAttached = true;

            InitializeElementExtensions();
        }

        

        private void DettachPartProject(PartProject project)
        {
            //project.ElementCollectionChanged -= Project_ElementCollectionChanged;
            //project.ElementPropertyChanged -= Project_ElementPropertyChanged;
            project.ProjectCollectionChanged -= Project_ProjectCollectionChanged;
            project.ProjectObjectChanged -= Project_ProjectObjectChanged;

            NavigationTreeNodes.Clear();
            _SelectedElements.Clear();
            _ValidationMessages.Clear();
            LastSavedChange = 0;
            LastValidation = -1;
            IsProjectAttached = false;
            CurrentProjectPath = null;
            TemporaryProjectPath = null;
        }

        #endregion

        public void SaveProject(string targetPath)
        {
            if (IsProjectOpen)
            {
                string oldPath = CurrentProjectPath;

                //SaveUserProperties();

                StartBatchChanges();
                
                CurrentProject.RemoveUnreferencedMeshes();
                CurrentProject.ProjectPath = TemporaryProjectPath;
                CurrentProject.LoadProjectXml();
                foreach (var mesh in CurrentProject.Meshes)
                {
                    if (!mesh.IsModelLoaded)
                        mesh.ReloadModelFromXml();
                }
                CurrentProject.UnloadProjectXml();

                EndBatchChanges();

                CurrentProject.ProjectPath = targetPath;
                CurrentProject.Save(targetPath);

                if (CurrentProject.ErrorMessages.Any())
                    MessageBoxEX.ShowDetails(Messages.Caption_UnexpectedError, Messages.Caption_UnexpectedError, string.Join(Environment.NewLine, CurrentProject.ErrorMessages));

                CurrentProjectPath = targetPath;
                
                LastSavedChange = UndoRedoManager.CurrentChangeID;
                UndoRedoManager.NonReversibleActionPerformed = false;

                if (oldPath != CurrentProjectPath)
                    SettingsManager.UpdateOpenedFile(TemporaryProjectPath, CurrentProjectPath);

                SettingsManager.AddRecentProject(GetCurrentProjectInfo(), false);
                ProjectModified?.Invoke(this, EventArgs.Empty);

                foreach (var mesh in CurrentProject.Meshes)
                    mesh.UnloadModel();
            }
        }

        public void SaveWorkingProject()
        {
            if (IsProjectOpen)
            {
                if (string.IsNullOrEmpty(TemporaryProjectPath))
                {
                    string tempDir = FileHelper.GetTempDirectory(16);
                    string tempFile = Path.Combine(tempDir, PartProject.ProjectFileName);
                    TemporaryProjectPath = tempFile;
                }

                CurrentProject.LoadProjectXml();
                foreach (var mesh in CurrentProject.Meshes)
                {
                    if (!mesh.IsModelLoaded)
                        mesh.ReloadModelFromXml();
                }
                CurrentProject.UnloadProjectXml();

                CurrentProject.ProjectPath = TemporaryProjectPath;
                CurrentProject.Save(TemporaryProjectPath);
                
                foreach (var mesh in CurrentProject.Meshes)
                    mesh.UnloadModel();
            }
        }

        public RecentFileInfo GetCurrentProjectInfo()
        {
            if (!IsProjectOpen)
                return null;
            return new RecentFileInfo()
            {
                PartID = CurrentProject.PartID,
                PartName = CurrentProject.PartDescription,
                ProjectFile = CurrentProjectPath,
                TemporaryPath = TemporaryProjectPath
            };
        }

        #region Project User Properties


        //private void LoadUserProperties()
        //{
        //    if (CurrentProject.TryGetProperty("ShowModels", out bool showModels))
        //        ShowPartModels = showModels;

        //    if (CurrentProject.TryGetProperty("ShowCollisions", out showModels))
        //        ShowCollisions = showModels;

        //    if (CurrentProject.TryGetProperty("ShowConnections", out showModels))
        //        ShowConnections = showModels;

        //    if (CurrentProject.TryGetProperty("PartRenderMode", out MeshRenderMode renderMode))
        //        PartRenderMode = renderMode;

        //    foreach (var elem in CurrentProject.GetAllElements())
        //    {
        //        var elemExt = elem.GetExtension<ModelElementExtension>();
        //        if (elemExt == null)
        //            continue;

        //        string elemCfg = string.Empty;
        //        string elemKey = GetElemKey(elem);

        //        if (!string.IsNullOrEmpty(elemKey) && 
        //            CurrentProject.ProjectProperties.ContainsKey(elemKey))
        //        {
        //            elemCfg = CurrentProject.ProjectProperties[elemKey];
        //        }

        //        if (elemCfg.EqualsIC("Hidden"))
        //            elemExt.IsHidden = true;
        //    }
        //}

        //private void SaveUserProperties()
        //{
        //    CurrentProject.ProjectProperties.Clear();
        //    CurrentProject.ProjectProperties["ShowModels"] = ShowPartModels.ToString();
        //    CurrentProject.ProjectProperties["ShowCollisions"] = ShowCollisions.ToString();
        //    CurrentProject.ProjectProperties["ShowConnections"] = ShowConnections.ToString();
        //    CurrentProject.ProjectProperties["PartRenderMode"] = PartRenderMode.ToString();

        //    foreach (var elem in CurrentProject.GetAllElements())
        //    {
        //        var elemExt = elem.GetExtension<ModelElementExtension>();

        //        if (elemExt != null && elemExt.IsHidden)
        //        {
        //            string elemKey = GetElemKey(elem);
        //            CurrentProject.ProjectProperties[elemKey] = "Hidden";
        //        }
        //    }
        //}

        //private string GetElemKey(PartElement element)
        //{
        //    if (element is PartSurface)
        //        return element.Name;
        //    else
        //        return $"Elem_{element.ID}";
        //}

        #endregion

        #region Project Events

        //private void Project_ElementCollectionChanged(object sender, ElementCollectionChangedEventArgs e)
        //{
        //    if (e.Action == System.ComponentModel.CollectionChangeAction.Remove)
        //    {
        //        int count = _SelectedElements.RemoveAll(x => e.RemovedElements.Contains(x));
        //        if (count > 0)
        //            SelectionChanged?.Invoke(this, EventArgs.Empty);
        //    }
        //    else
        //    {
        //        if (IsProjectAttached)
        //            InitializeElementExtensions();
        //    }

        //    if (!PreventCollectionEvents)
        //        ElementCollectionChanged?.Invoke(this, e);

        //    UndoRedoManager.ProcessProjectElementsChanged(e);

        //    if (IsExecutingUndoRedo || IsExecutingBatchChanges)
        //        ElementsChanged = true;
        //    else
        //        ProjectElementsChanged?.Invoke(this, EventArgs.Empty);
        //}

        //private void Project_ElementPropertyChanged(object sender, ElementValueChangedEventArgs e)
        //{
        //    if (!PreventCollectionEvents)
        //        ElementPropertyChanged?.Invoke(this, e);

        //    UndoRedoManager.ProcessElementPropertyChanged(e);
        //}

        private void Project_ProjectObjectChanged(object sender, ObjectPropertyChangedEventArgs e)
        {
            if (!PreventCollectionEvents)
                ObjectPropertyChanged?.Invoke(this, e);

            UndoRedoManager.ProcessElementPropertyChanged(e);
        }

        private void Project_ProjectCollectionChanged(object sender, System.ComponentModel.CollectionChangedEventArgs ccea)
        {
            var removedElements = ccea.RemovedElements<PartElement>();
            int count = _SelectedElements.RemoveAll(x => removedElements.Contains(x));
            if (count > 0)
                SelectionChanged?.Invoke(this, EventArgs.Empty);

            if (ccea.AddedElements<PartElement>().Any() && IsProjectAttached)
                InitializeElementExtensions();

            if (!PreventCollectionEvents)
                ProjectCollectionChanged?.Invoke(this, ccea);

            UndoRedoManager.ProcessProjectCollectionChanged(ccea);

            if (IsExecutingUndoRedo || IsExecutingBatchChanges)
                ElementsChanged = true;
            else
                ProjectElementsChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Localizations and Display


        public string GetProjectDisplayName()
        {
            if (CurrentProject != null)
            {
                bool isNewProject = string.IsNullOrEmpty(CurrentProjectPath);
                bool hasPartId = CurrentProject.PartID > 0;
                bool hasDescription = !string.IsNullOrEmpty(CurrentProject.PartDescription);

                if (isNewProject)
                {
                    if (hasPartId)
                        return $"{ModelLocalizations.Label_NewPartProject} - {CurrentProject.PartID}";
                    return ModelLocalizations.Label_NewPartProject;
                }
                else
                {
                    string fileName = Path.GetFileName(CurrentProjectPath);

                    if (Path.GetFileNameWithoutExtension(CurrentProjectPath) != CurrentProject.PartID.ToString())
                    {
                        string extraInfo = string.Empty;
                        if (hasPartId && !fileName.Contains(CurrentProject.PartID.ToString()))
                            extraInfo = CurrentProject.PartID.ToString();

                        if (hasDescription)
                            extraInfo += " " + CurrentProject.PartDescription;

                        if (!string.IsNullOrEmpty(extraInfo))
                            fileName += $" - {extraInfo.Trim()}";
                        return fileName;
                    }
                    else if (hasPartId && hasDescription)
                        return $"{CurrentProject.PartID} - {CurrentProject.PartDescription}";
                    else if (hasPartId)
                        return $"{ModelLocalizations.Label_Part} {CurrentProject.PartID}";
                    else
                        return ModelLocalizations.Label_NewPartProject;
                }
            }

            return ModelLocalizations.Label_NoActiveProject;
        }


        public static string GetSurfaceName(PartSurface surface)
        {
            if (surface.SurfaceID == 0)
                return ModelLocalizations.Label_MainSurface;

            return string.Format(ModelLocalizations.Label_DecorationSurfaceNumber, surface.SurfaceID);
        }

        #endregion

        #region Viewport Display Handling

        private bool _ShowGrid;
        private bool _ShowBones;
        private bool _ShowPartModels;
        private bool _ShowCollisions;
        private bool _ShowConnections;
        private bool _Show3dCursor;
        private MeshRenderMode _PartRenderMode;
        private bool BatchChangingVisibility;

        public bool ShowGrid
        {
            get => _ShowGrid;
            set => SetGridVisibility(value);
        }

        public bool ShowBones
        {
            get => _ShowBones;
            set => SetBonesVisibility(value);
        }

        public bool ShowPartModels
        {
            get => _ShowPartModels;
            set => SetPartModelsVisibility(value);
        }

        public bool ShowCollisions
        {
            get => _ShowCollisions;
            set => SetCollisionsVisibility(value);
        }

        public bool ShowConnections
        {
            get => _ShowConnections;
            set => SetConnectionsVisibility(value);
        }

        public bool Show3dCursor
        {
            get => _Show3dCursor;
            set => Set3dCursorVisibility(value);
        }

        public MeshRenderMode PartRenderMode
        {
            get => _PartRenderMode;
            set => SetPartRenderMode(value);
        }

        public event EventHandler GridVisibilityChanged;

        public event EventHandler BonesVisibilityChanged;

        public event EventHandler CursorVisibilityChanged;

        public event EventHandler PartModelsVisibilityChanged;

        public event EventHandler CollisionsVisibilityChanged;

        public event EventHandler ConnectionsVisibilityChanged;

        public event EventHandler ElementCollectionVisibilityChanged;

        public event EventHandler PartRenderModeChanged;

        private void SetGridVisibility(bool visible)
        {
            if (visible != _ShowGrid)
            {
                Trace.WriteLine($"{nameof(SetGridVisibility)}( visible => {visible} )");

                _ShowGrid = visible;

                GridVisibilityChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private void SetBonesVisibility(bool visible)
        {
            if (visible != _ShowBones)
            {
                Trace.WriteLine($"{nameof(SetBonesVisibility)}( visible => {visible} )");

                _ShowBones = visible;

                //if (IsProjectOpen && IsProjectAttached)
                //{
                //    InvalidateElementsVisibility(CurrentProject.Bones);
                //    RefreshAllNavigation();
                //}

                BonesVisibilityChanged?.Invoke(this, EventArgs.Empty);
                ElementCollectionVisibilityChanged?.Invoke(CurrentProject.Bones, EventArgs.Empty);
            }
        }

        private void Set3dCursorVisibility(bool visible)
        {
            if (visible != _Show3dCursor)
            {
                Trace.WriteLine($"{nameof(Set3dCursorVisibility)}( visible => {visible} )");

                _Show3dCursor = visible;

                CursorVisibilityChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private void SetPartModelsVisibility(bool visible)
        {
            if (visible != ShowPartModels)
            {
                Trace.WriteLine($"{nameof(SetPartModelsVisibility)}( visible => {visible} )");

                _ShowPartModels = visible;

                if (IsProjectOpen && IsProjectAttached)
                {
                    //CurrentProject.ProjectProperties["ShowModels"] = visible.ToString();
                    InvalidateElementsVisibility(CurrentProject.Surfaces);
                    RefreshAllNavigation();
                }

                PartModelsVisibilityChanged?.Invoke(this, EventArgs.Empty);
                ElementCollectionVisibilityChanged?.Invoke(CurrentProject.Surfaces, EventArgs.Empty);
            }
        }

        private void SetCollisionsVisibility(bool visible)
        {
            if (visible != ShowCollisions)
            {
                Trace.WriteLine($"{nameof(SetCollisionsVisibility)}( visible => {visible} )");

                _ShowCollisions = visible;

                if (IsProjectOpen && IsProjectAttached)
                {
                    InvalidateElementsVisibility(CurrentProject.GetAllElements<PartCollision>());
                    RefreshAllNavigation();
                }

                CollisionsVisibilityChanged?.Invoke(this, EventArgs.Empty);
                ElementCollectionVisibilityChanged?.Invoke(CurrentProject.Collisions, EventArgs.Empty);
            }
        }

        private void SetConnectionsVisibility(bool visible)
        {
            if (visible != ShowConnections)
            {
                Trace.WriteLine($"{nameof(SetConnectionsVisibility)}( visible => {visible} )");

                _ShowConnections = visible;

                if (IsProjectOpen && IsProjectAttached)
                {
                    InvalidateElementsVisibility(CurrentProject.GetAllElements<PartConnection>());
                    RefreshAllNavigation();
                }

                ConnectionsVisibilityChanged?.Invoke(this, EventArgs.Empty);
                ElementCollectionVisibilityChanged?.Invoke(CurrentProject.Connections, EventArgs.Empty);
            }
        }

        private void SetPartRenderMode(MeshRenderMode renderMode)
        {
            if (renderMode != _PartRenderMode)
            {
                _PartRenderMode = renderMode;
                PartRenderModeChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private void InvalidateElementsVisibility(IEnumerable<PartElement> elements)
        {
            BatchChangingVisibility = true;
            var elemExtensions = elements.Select(x => x.GetExtension<ModelElementExtension>()).ToList();
            elemExtensions.RemoveAll(e => e == null);

            foreach (var elemExt in elemExtensions)
                elemExt.InvalidateVisibility();

            foreach (var elemExt in elemExtensions)
                elemExt.CalculateVisibility();

            var test = elemExtensions.Where(x => x.IsVisibilityDirty);
            BatchChangingVisibility = false;
        }

        #endregion

        public void InitializeElementExtensions()
        {
            if (IsProjectOpen)
            {
                foreach (var elem in CurrentProject.GetAllElements())
                {
                    var modelExt = elem.GetExtension<ModelElementExtension>();
                    if (modelExt != null && modelExt.Manager == null)
                        modelExt.AssignManager(this);
                }
            }
        }

        #region Selection Management

        public void ClearSelection()
        {
            if (_SelectedElements.Any())
            {
                _SelectedElements.Clear();
                SelectionChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public void SelectElement(PartElement element)
        {
            Logger.Trace($"SelectElement (element: {element?.ID ?? "null"})");

            if (element == null)
            {
                ClearSelection();
            }
            else if (!(SelectedElement == element && _SelectedElements.Count == 1))
            {
                _SelectedElements.Clear();
                _SelectedElements.Add(element);
                SelectionChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public void SetSelected(PartElement element, bool selected)
        {
            Logger.Trace($"Set Selected (element: {element.ID}, selected: {selected})");

            bool isSelected = SelectedElements.Contains(element);
            if (selected != isSelected)
            {
                if (selected)
                    _SelectedElements.Add(element);
                else
                    _SelectedElements.Remove(element);
                SelectionChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public void SelectElements(IEnumerable<PartElement> elements)
        {
            _SelectedElements.Clear();
            _SelectedElements.AddRange(elements);
            SelectionChanged?.Invoke(this, EventArgs.Empty);
        }

        public IEnumerable<PartElement> GetSelectionHierarchy()
        {
            return SelectedElements.SelectMany(x => x.GetChildsHierarchy(true)).Distinct();
        }

        public bool IsSelected(PartElement element)
        {
            return SelectedElements.Contains(element);
        }

        public bool IsContainedInSelection(PartElement element)
        {
            var allChilds = SelectedElements.SelectMany(x => x.GetChildsHierarchy(true));
            return allChilds.Contains(element);
        }

        public int GetSelectionIndex(PartElement element)
        {
            for (int i = 0; i < _SelectedElements.Count; i++)
            {
                var allChilds = _SelectedElements[i].GetChildsHierarchy(true);
                if (allChilds.Contains(element))
                    return i;
            }
            return -1;
        }

        #endregion

        #region Undo/Redo

        public bool IsExecutingUndoRedo => UndoRedoManager.ExecutingUndoRedo;

        public bool IsExecutingBatchChanges => UndoRedoManager.IsInBatch;

        public bool CanUndo => UndoRedoManager.CanUndo;

        public bool CanRedo => UndoRedoManager.CanRedo;

        public void Undo()
        {
            UndoRedoManager.Undo();
        }

        public void Redo()
        {
            UndoRedoManager.Redo();
        }

        public void StartBatchChanges()
        {
            UndoRedoManager.StartBatchChanges();
        }

        public void StartBatchChanges(string description)
        {
            //TODO
            Trace.WriteLine($"Executing: {description}");
            UndoRedoManager.StartBatchChanges();
        }

        public void EndBatchChanges()
        {
            if (UndoRedoManager.EndBatchChanges() && ElementsChanged)
            {
                ElementsChanged = false;
                ProjectElementsChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private void UndoRedoManager_UndoHistoryChanged(object sender, EventArgs e)
        {
            ProjectModified?.Invoke(this, EventArgs.Empty);
        }

        private void UndoRedoManager_BeginUndoRedo(object sender, EventArgs e)
        {

        }

        private void UndoRedoManager_EndUndoRedo(object sender, EventArgs e)
        {
            if (ElementsChanged)
            {
                ElementsChanged = false;
                ProjectElementsChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        #endregion

        #region Part Validation Handling

        public bool IsValidatingProject { get; private set; }

        public IList<ValidationMessage> ValidationMessages => _ValidationMessages.AsReadOnly();

        public bool IsPartValidated => IsProjectOpen && LastValidation == UndoRedoManager.CurrentChangeID;

        public bool IsPartValid => IsPartValidated &&
            !ValidationMessages.Any(x => x.Level == ValidationLevel.Error);

        public event EventHandler ValidationStarted;

        public event EventHandler ValidationFinished;

        public void ValidateProject()
        {
            if (IsProjectOpen)
            {
                IsValidatingProject = true;
                ValidationStarted?.Invoke(this, EventArgs.Empty);

                _ValidationMessages.Clear();

                try
                {
                    _ValidationMessages.AddRange(CurrentProject.ValidatePart());
                }
                catch (Exception ex)
                {
                    _ValidationMessages.Add(new ValidationMessage("PROJECT", "UNHANDLED_EXCEPTION", ValidationLevel.Error)
                    {
                        Message = ex.ToString()
                    });
                }

                IsValidatingProject = false;
                LastValidation = UndoRedoManager.CurrentChangeID;
                
                ValidationFinished?.Invoke(this, EventArgs.Empty);
            }
        }

        #endregion

        #region Editing Validation

        public bool ValidateResizeStud(Custom2DFieldConnector connector, int newWidth, int newHeight)
        {
            //var connection = CurrentProject.GetAllElements<PartConnection>(x => x.Connector == connector).FirstOrDefault();
            var studRefs = CurrentProject.GetAllElements<StudReference>(x => x.Connector == connector).ToList();
            
            if (studRefs.Any(x => x.FieldNode != null && (x.PositionX > newWidth || x.PositionY > newHeight)))
            {
                var dlgResult = MessageBox.Show(
                    Messages.Message_ConfirmResizeStuds, 
                    Messages.Caption_Confirmation, 
                    MessageBoxButtons.YesNo);
                return dlgResult == DialogResult.Yes;
            }

            return true;
        }



        #endregion

        #region Element handling

        

        #endregion

        #region LDD File Generation Handling

        public bool IsGeneratingFiles { get; private set; }

        //todo: move to GenerationFinished eventargs
        public bool GenerationSuccessful { get; private set; }

        public event EventHandler GenerationStarted;

        public event EventHandler<ProjectBuildEventArgs> GenerationFinished;

        public PartWrapper GenerateLddFiles()
        {
            if (!IsProjectOpen)
                return null;

            IsGeneratingFiles = true;
            GenerationSuccessful = false;
            GenerationStarted?.Invoke(this, EventArgs.Empty);
            PartWrapper generatedPart = null;
            var messages = new List<ValidationMessage>();

            try
            {
                generatedPart = CurrentProject.GenerateLddPart();
                GenerationSuccessful = true;
            }
            catch (Exception ex)
            {
                messages.Add(new ValidationMessage("PROJECT", "UNHANDLED_EXCEPTION", ValidationLevel.Error)
                {
                    Message = ex.ToString()
                });
            }

            IsGeneratingFiles = false;

            GenerationFinished?.Invoke(this, 
                new ProjectBuildEventArgs(
                    generatedPart, 
                GenerationSuccessful,
                messages
                )
            );

            return generatedPart;
        }

        public string ExpandVariablePath(string path)
        {
            string result = path;

            if (result.Contains("$(LddAppData)"))
            {
                if (string.IsNullOrEmpty(LDDEnvironment.Current?.ApplicationDataPath) || 
                    !Directory.Exists(LDDEnvironment.Current.ApplicationDataPath))
                    throw new ArgumentException("Could not find LDD AppData directory");
                result = result.Replace("$(LddAppData)", LDDEnvironment.Current.ApplicationDataPath);
            }

            if (result.Contains("$(ProjectDir)"))
            {
                if (!IsProjectOpen || !CurrentProject.IsLoadedFromDisk)
                    throw new ArgumentException("Project is not saved to disk.");
                result = result.Replace("$(ProjectDir)", Path.GetDirectoryName(CurrentProject.ProjectPath));
            }

            if (result.Contains("$(WorkspaceDir)"))
            {
                if (!SettingsManager.IsWorkspaceDefined)
                    throw new ArgumentException("Workspace not defined!");
                result = result.Replace("$(WorkspaceDir)", SettingsManager.Current.EditorSettings.ProjectWorkspace);
            }
            if (result.Contains("$(PartID)"))
            {
                if (CurrentProject.PartID == 0)
                    throw new ArgumentException("Part ID is not defined");
                result = result.Replace("$(PartID)", CurrentProject.PartID.ToString());
            }

            return result;
        }

        public void SaveGeneratedPart(PartWrapper part, BuildConfiguration buildConfig)
        {
            string targetPath = buildConfig.OutputPath;

            if (targetPath.Contains("$"))
            {
                targetPath = ExpandVariablePath(targetPath);
            }

            if (buildConfig.InternalFlag == BuildConfiguration.MANUAL_FLAG ||
                string.IsNullOrEmpty(buildConfig.OutputPath))
            {
                using (var sfd = new SaveFileDialog())
                {
                    if (!string.IsNullOrEmpty(targetPath) &&
                        FileHelper.IsValidDirectory(targetPath))
                        sfd.InitialDirectory = targetPath;

                    sfd.FileName = part.PartID.ToString();

                    if (sfd.ShowDialog() != DialogResult.OK)
                    {
                        //show canceled message
                        return;
                    }

                    targetPath = Path.GetDirectoryName(sfd.FileName);
                }
            }

            var targetDir = new DirectoryInfo(targetPath);
            string meshDirectory = targetPath;
            if (buildConfig.LOD0Subdirectory)
                meshDirectory = Path.Combine(targetPath, "LOD0");

            bool filesExists = part.CheckFilesExists(targetPath, meshDirectory);

            if (buildConfig.ConfirmOverwrite && filesExists)
            {
                var msgResult = MessageBoxEX.Show(MainWindow,
                    Messages.Message_ConfirmOverwritePartFiles, 
                    Messages.Caption_LddPartGeneration, MessageBoxButtons.YesNo);
                if (msgResult != DialogResult.Yes)
                    return;
            }

            part.SaveToDirectory(targetPath, meshDirectory);

            var generatedFiles = new List<string>() { part.Filepath };
            generatedFiles.AddRange(part.Surfaces.Select(s => s.Filepath));

            if (targetDir.Parent != null)
            {
                for (int i = 0; i < generatedFiles.Count; i++)
                {
                    generatedFiles[i] = generatedFiles[i].Substring(targetDir.Parent.FullName.Length);
                }
            }
            

            MessageBoxEX.ShowDetails(MainWindow, 
                Messages.Message_LddFilesGenerated, 
                Messages.Caption_LddPartGeneration, 
                string.Join(Environment.NewLine, generatedFiles), 
                MessageBoxButtons.OK, 
                MessageBoxIcon.Information);
        }

        #endregion

        #region Navigation Tree

        public void RebuildNavigationTree()
        {
            foreach (var node in NavigationTreeNodes)
                node.FreeObjects();
            NavigationTreeNodes.Clear();

            NavigationTreeNodes.Add(new ProjectCollectionNode(
                CurrentProject.Surfaces,
                ModelLocalizations.Label_Surfaces));

            NavigationTreeNodes.Add(new ProjectCollectionNode(
                   CurrentProject.Collisions,
                   ModelLocalizations.Label_Collisions)
            {
                DoNotShowNodes = CurrentProject.Properties.Flexible
            });

            NavigationTreeNodes.Add(new ProjectCollectionNode(
                CurrentProject.Connections,
                ModelLocalizations.Label_Connections)
            {
                DoNotShowNodes = CurrentProject.Properties.Flexible
            });

            if (CurrentProject.Properties.Flexible)
            {
                NavigationTreeNodes.Add(new ProjectCollectionNode(
                    CurrentProject.Bones,
                    ModelLocalizations.Label_Bones));
            }

            NavigationTreeNodes.Add(new ProjectCollectionNode(
                   CurrentProject.ClonePatterns,
                   ModelLocalizations.Label_ClonePatterns));
        }

        public void RefreshNavigationNode(ProjectTreeNode node)
        {
            if (!BatchChangingVisibility)
                NavigationWindow?.RefreshNavigationNode(node);
        }

        public void RefreshAllNavigation()
        {
            NavigationWindow.RefreshAllNavigation();
        }

        #endregion

        #region Editor Actions

        #region Element Editing

        public PartSurface AddSurface()
        {
            StartBatchChanges($"{nameof(AddSurface)}");
            var newSurface = CurrentProject.AddSurface();
            EndBatchChanges();

            SelectElement(newSurface);

            return newSurface;
        }

        public PartConnection AddConnection(ConnectorType type, PartBone parent)
        {
            if (CurrentProject == null)
                return null;

            StartBatchChanges($"{nameof(AddConnection)}( type => {type}, parent => {parent?.Name ?? "null"})");

            var newConnection = PartConnection.Create(type);
            if (parent != null)
                parent.Connections.Add(newConnection);
            else
                CurrentProject.Connections.Add(newConnection);

            EndBatchChanges();

            if (!ShowConnections)
                ShowConnections = true;

            SelectElement(newConnection);

            return newConnection;
        }

        public PartCollision AddCollision(CollisionType type, PartBone parent)
        {
            if (CurrentProject == null)
                return null;

            StartBatchChanges($"{nameof(AddCollision)}( type => {type}, parent => {parent?.Name ?? "null"})");

            var newCollision = PartCollision.Create(type);
            if (parent != null)
                parent.Collisions.Add(newCollision);
            else
                CurrentProject.Collisions.Add(newCollision);

            EndBatchChanges();

            if (!ShowCollisions)
                ShowCollisions = true;

            SelectElement(newCollision);

            return newCollision;
        }

        public ClonePattern AddClonePattern(ClonePatternType patternType, IEnumerable<PartElement> elements = null)
        {
            if (CurrentProject == null)
                return null;

            StartBatchChanges($"{nameof(AddClonePattern)}( type => {patternType}");

            ClonePattern pattern = null;
            switch (patternType)
            {
                case ClonePatternType.Mirror:
                    pattern = new MirrorPattern();
                    break;
                case ClonePatternType.Linear:
                    pattern = new LinearPattern();
                    break;
                case ClonePatternType.Circular:
                    pattern = new CircularPattern();
                    break;
            }

            if (elements != null && elements.Any())
                pattern.Elements.AddRange(elements);

            CurrentProject.ClonePatterns.Add(pattern);
            EndBatchChanges();
            SelectElement(pattern);

            return pattern;
        }

        public void DisolvePattern(ClonePattern pattern)
        {
            if (CurrentProject == null)
                return;

            StartBatchChanges($"{nameof(DisolvePattern)}( pattern => {pattern.Name}");

            foreach (var elemRef in pattern.Elements)
            {
                var elemToClone = elemRef.Element;
                var parentBone = elemToClone.Parent as PartBone;
                var elemClones = pattern.GetElementClones(elemToClone);

                if (elemToClone is PartConnection)
                {
                    var clonedConnectors = elemClones.OfType<PartConnection>();
                    if (parentBone != null)
                        parentBone.Connections.AddRange(clonedConnectors);
                    else
                        CurrentProject.Connections.AddRange(clonedConnectors);
                }
                else if (elemToClone is PartCollision)
                {
                    var clonedCollisions = elemClones.OfType<PartCollision>();
                    if (parentBone != null)
                        parentBone.Collisions.AddRange(clonedCollisions);
                    else
                        CurrentProject.Collisions.AddRange(clonedCollisions);
                }

                //for (int r = 0; r < pattern.Repetitions; r++)
                //{
                //    var clonedElem = pattern.GetClonedElement(elemToClone, r + 1);
                //    if (clonedElem is PartConnection conn)
                //    {
                //        if (parentBone != null)
                //            parentBone.Connections.Add(conn);
                //        else
                //            CurrentProject.Connections.Add(conn);
                //    }
                //    else if (clonedElem is PartCollision coll)
                //    {
                //        if (parentBone != null)
                //            parentBone.Collisions.Add(coll);
                //        else
                //            CurrentProject.Collisions.Add(coll);
                //    }
                //}
            }

            CurrentProject.ClonePatterns.Remove(pattern);

            EndBatchChanges();
        }

        public PartElement DuplicateElement(PartElement element)
        {
            PartElement newElement = null;

            StartBatchChanges($"{nameof(DuplicateElement)}( element => {element.Name} )");

            if (element is PartConnection connection)
            {
                newElement = connection.Clone();
                var collection = (element.Parent as PartBone)?.Connections ?? CurrentProject.Connections;
                collection.Add(newElement);
                CurrentProject.RenameElement(newElement, element.Name);
                if (!ShowConnections)
                    ShowConnections = true;
            }
            else if (element is PartCollision collision)
            {
                newElement = collision.Clone();
                var collection = (element.Parent as PartBone)?.Collisions ?? CurrentProject.Collisions;
                collection.Add(newElement);
                CurrentProject.RenameElement(newElement, element.Name);
                if (!ShowCollisions)
                    ShowCollisions = true;
            }

            EndBatchChanges();

            if (newElement != null)
                SelectElement(newElement);


            return newElement;
        }

        public void DeleteElements(IEnumerable<PartElement> elements)
        {
            var elemsToDelete = new List<PartElement>();
            int elemCount = elements.Count();

            foreach (var elem in elements)
            {
                var dlgResult = ConfirmCanDelete(elem, elemCount == 1);

                if (dlgResult == DialogResult.Cancel)
                {
                    elemsToDelete.Clear();
                    break;
                }

                if (dlgResult == DialogResult.Yes)
                    elemsToDelete.Add(elem);
            }

            if (elemsToDelete.Any())
            {
                StartBatchChanges(nameof(DeleteElements));

                ClearSelection();

                var removedElements = elemsToDelete.Where(x => x.TryRemove()).ToList();
                foreach (var elem in removedElements)
                    CurrentProject.RemoveReferences(elem);
                //if (removedElements.OfType<ModelMeshReference>().Any())
                //    CurrentProject.RemoveUnreferencedMeshes();

                EndBatchChanges();
            }
        }

        private DialogResult ConfirmCanDelete(PartElement element, bool onlyOneElement = true)
        {
            var msgButtons = onlyOneElement ? MessageBoxButtons.YesNo : MessageBoxButtons.YesNoCancel;

            if (element is PartConnection conn)
            {
                if (conn.ConnectorType == ConnectorType.Custom2DField)
                {
                    var allStudRefs = CurrentProject.GetAllElements<StudReference>();
                    if (allStudRefs.Any(x => x.ConnectionID == conn.ID))
                    {
                        return MessageBox.Show(
                            string.Format(Messages.Message_ConfirmDeleteStudConnection, conn.Name), 
                            Messages.Caption_DeleteConfirmation,
                            msgButtons);
                    }
                }
                else if (conn.ConnectorType == ConnectorType.Ball || 
                    conn.ConnectorType == ConnectorType.Fixed)
                {
                    if (CurrentProject.Bones.Any(x => 
                        x.SourceConnectionID == conn.ID ||
                        x.TargetConnectionID == conn.ID))
                    {
                        return MessageBox.Show(
                            string.Format(Messages.Message_ConfirmDeleteBoneConnection, conn.Name), 
                            Messages.Caption_DeleteConfirmation,
                            msgButtons);
                    }
                }
            }

            if (CurrentProject.ClonePatterns.Any(p => p.Elements.Contains(element)))
            {
                return MessageBox.Show(
                            string.Format(Messages.Message_ConfirmDeleteReferencedElement, element.Name),
                            Messages.Caption_DeleteConfirmation,
                            msgButtons);
            }
            
            return DialogResult.Yes;
        }

        public void CopySelectedElementsToClipboard()
        {
            if (!IsProjectOpen)
                return;

            string clipboardText = string.Empty;
            foreach (var elem in GetSelectionHierarchy())
            {
                if (elem is PartCollision collision)
                {
                    var colXml = collision.GenerateLDD().SerializeToXml();
                    if (!string.IsNullOrEmpty(clipboardText))
                        clipboardText += Environment.NewLine;
                    clipboardText += colXml.ToString();
                }
                else if (elem is PartConnection connection)
                {

                    var colXml = connection.GenerateLDD().SerializeToXml();
                    if (!string.IsNullOrEmpty(clipboardText))
                        clipboardText += Environment.NewLine;
                    clipboardText += colXml.ToString();
                }
            }

            if (!string.IsNullOrEmpty(clipboardText))
            {
                Clipboard.SetText(clipboardText, TextDataFormat.Text);
            }
        }

        public void HandlePasteFromClipboard()
        {
            if (!IsProjectOpen)
                return;

            var clipboardText = Clipboard.GetText(TextDataFormat.Text);
            if (string.IsNullOrEmpty(clipboardText))
                return;

            clipboardText = $"<Root>{clipboardText}</Root>";
            try
            {
                var tmpXml = XElement.Parse(clipboardText);
                var pastedElements = new List<PartElement>();

                foreach (var elem in tmpXml.Elements())
                {
                    var conn = Connector.DeserializeConnector(elem);
                    if (conn != null)
                    {
                        var connElem = new PartConnection(conn);
                        pastedElements.Add(connElem);
                        continue;
                    }

                    var coll = Collision.DeserializeCollision(elem);
                    if (coll != null)
                    {
                        var collElem = PartCollision.FromLDD(coll);
                        pastedElements.Add(collElem);
                        continue;
                    }
                }

                if (pastedElements.Any())
                {
                    StartBatchChanges(nameof(HandlePasteFromClipboard));
                    var newCollisions = pastedElements.OfType<PartCollision>();
                    var newConnections = pastedElements.OfType<PartConnection>();

                    if (newCollisions.Any() && !ShowCollisions)
                        ShowCollisions = true;

                    if (newConnections.Any() && !ShowConnections)
                        ShowConnections = true;

                    if (SelectedElement is PartBone selectedBone)
                    {
                        if (newCollisions.Any())
                            selectedBone.Collisions.AddRange(newCollisions);
                        if (newConnections.Any())
                            selectedBone.Connections.AddRange(newConnections);
                    }
                    else
                    {
                        if (newCollisions.Any())
                            CurrentProject.Collisions.AddRange(newCollisions);
                        if (newConnections.Any())
                            CurrentProject.Connections.AddRange(newConnections);
                    }

                    EndBatchChanges();

                    SelectElements(pastedElements);
                }
                
            }
            catch { }
        }

        public void ClearBoneConnections()
        {
            if (CurrentProject == null || !CurrentProject.Flexible)
                return;
            StartBatchChanges(nameof(ClearBoneConnections));
            PreventCollectionEvents = true;
            foreach (var bone in CurrentProject.Bones)
            {
                bone.Connections.RemoveAll(x => x.SubType >= 999000);
                bone.TargetConnectionID = string.Empty;
                bone.TargetConnectionIndex = -1;
                bone.SourceConnectionID = string.Empty;
                bone.SourceConnectionIndex = -1;
            }

            PreventCollectionEvents = false;
            ViewportWindow.InvalidateBones();
            EndBatchChanges();
        }

        public void RebuildBoneConnections(double[] flexAttributes)
        {
            if (CurrentProject == null || !CurrentProject.Flexible)
                return;

            if (CurrentProject.Bones.Count == 0)
                return;

            StartBatchChanges(nameof(RebuildBoneConnections));
            PreventCollectionEvents = true;
            foreach (var bone in CurrentProject.Bones)
            {
                bone.Connections.RemoveAll(x => x.SubType >= 999000);
                bone.TargetConnectionID = string.Empty;
                bone.TargetConnectionIndex = -1;
                bone.SourceConnectionID = string.Empty;
                bone.SourceConnectionIndex = -1;
            }

            int lastBoneId = CurrentProject.Bones.Max(x => x.BoneID);

            int curConnType = 0;
            int totalConnection = 0;

            foreach (var bone in CurrentProject.Bones.OrderBy(x => x.BoneID))
            {
                foreach (var linkedBone in CurrentProject.Bones.Where(x => x.TargetBoneID == bone.BoneID))
                {
                    PartConnection targetConn = PartConnection.Create(ConnectorType.Ball);

                    if (linkedBone.BoneID == lastBoneId)
                    {
                        targetConn = PartConnection.Create(ConnectorType.Fixed);
                        var fixConn = targetConn.GetConnector<FixedConnector>();
                        fixConn.SubType = 999000 + curConnType;
                    }
                    else
                    {
                        targetConn = PartConnection.Create(ConnectorType.Ball);
                        var ballConn = targetConn.GetConnector<BallConnector>();
                        ballConn.SubType = 999000 + curConnType;
                    }

                    curConnType = (++curConnType) % 4;

                    var posOffset = linkedBone.Transform.Position - bone.Transform.Position;
                    posOffset = bone.Transform.ToMatrixD().Inverted().TransformVector(posOffset);
                    targetConn.Transform.Position = posOffset;

                    bone.Connections.Add(targetConn);
                    CurrentProject.RenameElement(targetConn, $"FlexConn{totalConnection++}");

                    PartConnection sourceConn = null;
                    if (linkedBone.BoneID == lastBoneId)
                    {
                        sourceConn = PartConnection.Create(ConnectorType.Fixed);
                        var fixConn = sourceConn.GetConnector<FixedConnector>();
                        fixConn.SubType = 999000 + curConnType;
                    }
                    else
                    {
                        sourceConn = PartConnection.Create(ConnectorType.Ball);
                        var ballConn = sourceConn.GetConnector<BallConnector>();
                        ballConn.SubType = 999000 + curConnType;
                        ballConn.FlexAttributes = flexAttributes;
                    }

                    curConnType = (++curConnType) % 4;
                    linkedBone.Connections.Add(sourceConn);
                    CurrentProject.RenameElement(sourceConn, $"FlexConn{totalConnection++}");

                    linkedBone.TargetConnectionID = targetConn.ID;
                    linkedBone.SourceConnectionID = sourceConn.ID;

                }
            }
            
            CurrentProject.UpdateBoneReferencesIndices();

            PreventCollectionEvents = false;
            ViewportWindow.InvalidateBones();
            EndBatchChanges();
        }

        public void RemoveOutlines()
        {
            if (!IsProjectOpen)
                return;

            CurrentProject.ClearEdgeOutlines();

            UndoRedoManager.NonReversibleActionPerformed = true;
            ProjectModified?.Invoke(this, EventArgs.Empty);
        }

        public void RecalculateOutlines()
        {
            if (!IsProjectOpen)
                return;

            CurrentProject.ComputeEdgeOutlines();

            UndoRedoManager.NonReversibleActionPerformed = true;
            ProjectModified?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Visibility Handling

        public void SetElementHidden(PartElement element, bool hidden)
        {
            var elementExt = element.GetExtension<ModelElementExtension>();

            if (elementExt != null && elementExt.IsHidden != hidden)
            {
                elementExt.IsHidden = hidden;
                elementExt.CalculateVisibility();

                UndoRedoManager.AddEditorAction(new HideElementAction(
                    nameof(SetElementHidden),
                    new PartElement[] { element },
                    hidden));
            }
        }

        public void SetElementsHidden(IEnumerable<PartElement> elements, bool hidden)
        {
            var changedElems = new List<PartElement>();

            foreach (var elem in elements)
            {
                var elementExt = elem.GetExtension<ModelElementExtension>();

                if (elementExt != null && elementExt.IsHidden != hidden)
                {
                    elementExt.IsHidden = hidden;
                    changedElems.Add(elem);
                    elementExt.CalculateVisibility();
                }
            }

            if (changedElems.Any())
            {
                UndoRedoManager.AddEditorAction(new HideElementAction(nameof(SetElementsHidden), changedElems, hidden));
            }
        }

        public void HideSelectedElements()
        {
            var hideableElements = SelectedElements.Where(x => x.GetExtension<ModelElementExtension>() != null);
            var hiddenElems = new List<PartElement>();

            foreach (var elem in hideableElements)
            {
                var elementExt = elem.GetExtension<ModelElementExtension>();
                if (elementExt != null && !elementExt.IsHidden)
                {
                    hiddenElems.Add(elem);
                    elementExt.IsHidden = true;
                    elementExt.CalculateVisibility();
                }
            }

            if (hiddenElems.Any())
            {
                UndoRedoManager.AddEditorAction(new HideElementAction(nameof(HideSelectedElements), hiddenElems, true));
            }
        }

        public void UnhideSelectedElements()
        {
            var hideableElements = SelectedElements.Where(x => x.GetExtension<ModelElementExtension>() != null);
            var changedElems = new List<PartElement>();

            foreach (var elem in hideableElements)
            {
                var elementExt = elem.GetExtension<ModelElementExtension>();
                if (elementExt != null && elementExt.IsHidden)
                {
                    changedElems.Add(elem);
                    elementExt.IsHidden = false;
                    elementExt.CalculateVisibility();
                }
            }

            if (changedElems.Any())
            {
                UndoRedoManager.AddEditorAction(new HideElementAction(nameof(UnhideSelectedElements), changedElems, false));
            }
        }

        public void HideUnselectedElements()
        {
            var selectedElements = SelectedElements.Select(x => x.GetExtension<ModelElementExtension>()).Where(x => x != null);
            if (!selectedElements.Any(x => x.IsVisible))
                return;

            var hideableElements = CurrentProject.GetAllElements(x => x.GetExtension<ModelElementExtension>() != null);
            var hiddenElems = new List<PartElement>();

            foreach (var elem in hideableElements)
            {
                if (IsContainedInSelection(elem))
                    continue;

                if (elem.GetChildsHierarchy().Any(x => IsSelected(x)))
                    continue;

                var elementExt = elem.GetExtension<ModelElementExtension>();

                if (elementExt != null && !elementExt.IsHidden)
                {
                    hiddenElems.Add(elem);
                    elementExt.IsHidden = true;
                    elementExt.CalculateVisibility();
                }
            }

            if (hiddenElems.Any())
            {
                UndoRedoManager.AddEditorAction(new HideElementAction(nameof(HideSelectedElements), hiddenElems, true));
            }
        }

        public void UnhideEverything()
        {
            var hideableElements = CurrentProject.GetAllElements(x => x.GetExtension<ModelElementExtension>() != null);
            var hiddenElems = new List<PartElement>();

            foreach (var elem in hideableElements)
            {
                var elementExt = elem.GetExtension<ModelElementExtension>();
                if (elementExt != null && elementExt.IsHidden)
                {
                    hiddenElems.Add(elem);
                    elementExt.IsHidden = false;
                    elementExt.CalculateVisibility();
                }
            }

            if (hiddenElems.Any())
            {
                UndoRedoManager.AddEditorAction(new HideElementAction(nameof(HideSelectedElements), hiddenElems, false));
            }
        }

        #endregion

        #region Selection

        public void SelectAllVisible()
        {
            var hideableElements = CurrentProject.GetAllElements(x => x.GetExtension<ModelElementExtension>() != null);
            var elemsToSelect = new List<PartElement>();
            foreach (var elem in CurrentProject.GetAllElements())
            {
                var elementExt = elem.GetExtension<ModelElementExtension>();
                if (elementExt != null && elementExt.IsVisible)
                {
                    if (elem is PartBone && !ShowBones)
                        continue;
                    elemsToSelect.Add(elem);
                }
            }
            SelectElements(elemsToSelect);
        }

        #endregion

        #region Dialogs

        public void ShowCopyBoneDataDialog()
        {
            if (CurrentProject == null)
                return;

            Trace.WriteLine($"Executing: {nameof(ShowCopyBoneDataDialog)}");

            using (var dlg = new BoneDataCopyDialog(this))
            {
                StartBatchChanges(nameof(ShowCopyBoneDataDialog));
                dlg.ShowDialog();
                EndBatchChanges();
            }
        }

        public void ShowLinkBonesDialog()
        {
            if (CurrentProject == null)
                return;

            Trace.WriteLine($"Executing: {nameof(ShowLinkBonesDialog)}");

            using (var dlg = new BoneLinkDialog(this))
            {
                StartBatchChanges(nameof(ShowLinkBonesDialog));
                dlg.ShowDialog();
                EndBatchChanges();
            }
        }

        public void ShowOutlinesConfigDialog()
        {
            if (CurrentProject == null)
                return;

            Trace.WriteLine($"Executing: {nameof(ShowOutlinesConfigDialog)}");

            using (var dlg = new ConfigureOutlinesDialog(this))
            {
                StartBatchChanges(nameof(ShowOutlinesConfigDialog));
                dlg.ShowDialog();
                EndBatchChanges();
            }
        }

        #endregion

        #region Other

        public void OpenPartInLDD()
        {
            if (!IsProjectOpen)
                return;

            Logger.Info("Oppening current part in LDD...");

            if (CurrentProject.PartID == 0 || !PartWrapper.PartExists(LDDEnvironment.Current, CurrentProject.PartID))
            {
                Logger.Warn("Part was not found in LDD primitive folder");
                MessageBox.Show(Messages.Message_CouldNotFindPartInLDD, Messages.Caption_OpeningPartInLdd);
                return;
            }

            Logger.Info("Creating temporary LDD model for part");

            var emptyModel = new Core.Models.Model
            {
                FileVersion = new Core.Data.VersionInfo(5, 0),
                ApplicationVersion = new Core.Data.VersionInfo(4, 3),
                BrickSetVersion = 2670,
                Brand = Core.Data.Brand.LDDExtended,
                ModelName = CurrentProject.PartDescription
            };
            emptyModel.Bricks.Add(new Core.Models.Brick
            {
                DesignID = CurrentProject.PartID,
                Part = new Core.Models.Part
                {
                    DesignID = CurrentProject.PartID,
                    Materials = CurrentProject.Surfaces.Select(s => 21).ToList(),
                    Bone = new Core.Models.Bone()
                }
            });
            emptyModel.RigidSystems.Add(new Core.Models.RigidSystem()
            {
                RigidItems = new List<Core.Models.RigidItem>
                {
                    new Core.Models.RigidItem
                    {
                        BoneRefs = new List<int> { 0 }
                    }
                }
            });
            
            string tmpDir = Path.GetTempPath();
            string tmpFile = Path.Combine(tmpDir, Guid.NewGuid().ToString() + ".lxfml");
            Logger.Debug($"Model path: {tmpFile}");
            emptyModel.Save(tmpFile);
            Logger.Info("Launching LDD...");
            Process.Start(tmpFile);
        }

        #endregion

        #endregion
    }
}
