﻿using LDD.BrickEditor.Native;
using LDD.BrickEditor.ProjectHandling;
using LDD.BrickEditor.ProjectHandling.ViewInterfaces;
using LDD.BrickEditor.Rendering;
using LDD.BrickEditor.Resources;
using LDD.BrickEditor.Settings;
using LDD.BrickEditor.UI.Panels;
using LDD.BrickEditor.Utilities;
using LDD.Core;
using LDD.Core.Parts;
using LDD.Modding;
using LDD.Common.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace LDD.BrickEditor.UI.Windows
{
    public partial class BrickEditorWindow : Form, IMainWindow
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetLogger("Main Window");

        public ProjectManager ProjectManager { get; private set; }

        public PartProject CurrentProject => ProjectManager.CurrentProject;

        protected FlagManager FlagManager { get; }

        private bool IsInitializing;

        public BrickEditorWindow()
        {
            InitializeComponent();

            DockPanelControl.Theme = new CustomDockTheme();

            visualStudioToolStripExtender1.SetStyle(MainMenu, 
                VisualStudioToolStripExtender.VsVersion.Vs2015,
                DockPanelControl.Theme);

            DockPanelControl.Theme.Extender.DockPaneStripFactory = new VS2015DockPaneStripFactory();
            DockPanelControl.Theme.Extender.DockPaneCaptionFactory = new VS2015DockPaneCaptionFactory();

            FlagManager = new FlagManager();

            Icon = Properties.Resources.BrickStudioIcon;

        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            MultiInstanceManager.Initialize(this);

            RestoreSavedPosition();
            InitializeProjectManager();
            MainMenu.Enabled = false;
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            Logger.Info("Main window shown");

            IsInitializing = true;
            
            UpdateMenuItemStates();
            UpdateWindowTitle();

            this.InvokeWithDelay(200, BeginLoadingUI);

        }

        private void RestoreSavedPosition()
        {
            if (SettingsManager.Current.DisplaySettings.LastPosition != default)
            {
                var savedBounds = SettingsManager.Current.DisplaySettings.LastPosition;
                var tlCorner = SettingsManager.Current.DisplaySettings.LastPosition.Location;
                var trCorner = new Point(savedBounds.Right, savedBounds.Top);
                var blCorner = new Point(savedBounds.Left, savedBounds.Bottom);
                var brCorner = new Point(savedBounds.Right, savedBounds.Bottom);
                var corners = new Point[] { tlCorner, trCorner, blCorner, brCorner };

                var screen = Screen.FromPoint(tlCorner);

                if (screen != null && corners.Any(x => screen.Bounds.Contains(x)))
                {
                    Bounds = savedBounds;
                    if (SettingsManager.Current.DisplaySettings.IsMaximized)
                        WindowState = FormWindowState.Maximized;
                }
            }
        }

        private void LoadAndValidateSettings()
        {
            if (!SettingsManager.HasInitialized)
                SettingsManager.Initialize();

            if (LDDEnvironment.Current == null || !LDDEnvironment.Current.IsValidInstall)
            {
                if (!LDDEnvironment.IsInstalled)
                    MessageBox.Show(Messages.LddInstallNotFound, Messages.Caption_StartupValidations, MessageBoxButtons.OK);
                else
                    MessageBox.Show(Messages.LddConfigInvalid, Messages.Caption_StartupValidations, MessageBoxButtons.OK);
            }
            else
            {
                if (!Models.BrickListCache.IsInitialized || Models.BrickListCache.CheckIfConfigurationChanged())
                {
                    Task.Factory.StartNew(() => Models.BrickListCache.Initialize());
                }
            }

            AutoSaveTimer.Interval = SettingsManager.Current.EditorSettings.BackupInterval * 1000;
        }

        private void UpdateWindowTitle()
        {
            if (InvokeRequired)
            {
                BeginInvoke(new MethodInvoker(UpdateWindowTitle));
                return;
            }

            if (CurrentProject != null)
            {
                string projectDesc = ProjectManager.GetProjectDisplayName();
                
                Text = $"{WindowTitle.Text} - {projectDesc}";
            }
            else
                Text = WindowTitle.Text;
        }

        #region UI Layout

        public NavigationPanel NavigationPanel { get; private set; }
        public ViewportPanel ViewportPanel { get; private set; }
        public ValidationPanel ValidationPanel { get; private set; }
        public PartPropertiesPanel PropertiesPanel { get; private set; }
        public ElementDetailPanel DetailPanel { get; private set; }
        public StudConnectionPanel StudConnectionPanel { get; private set; }
        public ConnectionEditorPanel ConnectionPanel { get; private set; }
        public StudReferencesPanel StudReferencesPanel { get; private set; }
        public ProjectInfoPanel InfoPanel { get; private set; }
        public ProgressPopupWindow WaitPopup { get; private set; }

        private IDockContent[] DockPanels => new IDockContent[]
        {
            NavigationPanel,
            ViewportPanel,
            ValidationPanel,
            PropertiesPanel,
            DetailPanel,
            StudConnectionPanel,
            ConnectionPanel,
            InfoPanel,
            StudReferencesPanel
        };

        private void InitializePanels()
        {
            Logger.Info("Initializing Panels");

            int totalPanels = DockPanels.Length;
            int currentPanel = 0;
            WaitPopup.UpdateProgress(0, totalPanels);

            NavigationPanel = new NavigationPanel(ProjectManager);
            WaitPopup.UpdateProgress(++currentPanel, totalPanels);

            ViewportPanel = new ViewportPanel(ProjectManager);
            WaitPopup.UpdateProgress(++currentPanel, totalPanels);

            ValidationPanel = new ValidationPanel(ProjectManager);
            WaitPopup.UpdateProgress(++currentPanel, totalPanels);

            PropertiesPanel = new PartPropertiesPanel(ProjectManager);
            WaitPopup.UpdateProgress(++currentPanel, totalPanels);

            DetailPanel = new ElementDetailPanel(ProjectManager);
            WaitPopup.UpdateProgress(++currentPanel, totalPanels);

            StudConnectionPanel = new StudConnectionPanel(ProjectManager);
            WaitPopup.UpdateProgress(++currentPanel, totalPanels);

            StudReferencesPanel = new StudReferencesPanel(ProjectManager);
            WaitPopup.UpdateProgress(++currentPanel, totalPanels);

            ConnectionPanel = new ConnectionEditorPanel(ProjectManager);
            WaitPopup.UpdateProgress(++currentPanel, totalPanels);

            InfoPanel = new ProjectInfoPanel(ProjectManager);
            WaitPopup.UpdateProgress(++currentPanel, totalPanels);

        }

        private void LayoutDockPanels()
        {
            Logger.Info("Loading Panels");

            if (WaitPopup != null && WaitPopup.Visible)
                WaitPopup.UpdateProgress(0, 2);

            var savedLayout = SettingsManager.GetSavedUserLayout();

            if (!(savedLayout != null && LoadCustomLayout(savedLayout)))
                LoadDefaultLayout();
            
            if (WaitPopup != null && WaitPopup.Visible)
                WaitPopup.UpdateProgress(1, 2);

            ValidateAllPanelsLoaded();

            if (WaitPopup != null && WaitPopup.Visible)
                WaitPopup.UpdateProgress(1, 2);

            foreach (IDockContent dockPanel in DockPanelControl.Contents)
            {
                if (dockPanel is ProjectDocumentPanel documentPanel)
                    documentPanel.Enabled = false;
            }
        }

        private void ValidateAllPanelsLoaded()
        {
            var panels = DockPanels;
            var loadedPanels = DockPanelControl.Contents.OfType<ProjectDocumentPanel>().ToList();

            var pane = PropertiesPanel?.Pane;
            if (pane == null)
            {
                Logger.Warn("PropertiesPanel.Pane is NULL!");
                pane = loadedPanels.FirstOrDefault()?.Pane;
            }

            if (pane == null)
            {
                Logger.Warn("Could not find DockPane! Loading Default Layout...");
                LoadDefaultLayout();
            }
            else
            {
                for (int i = 0; i < panels.Length; i++)
                {
                    if (!loadedPanels.Contains(panels[i]))
                    {
                        Logger.Warn($"Panel '{panels[i].GetType().Name}' was not loaded");
                        (panels[i] as DockContent).Show(pane, null);
                    }
                }
            }


            foreach(var dockPane in DockPanelControl.Panes)
            {
                var activeContent = dockPane.ActiveContent;
                foreach(var content in dockPane.Contents.OfType<DockContent>())
                    content.Show();
                dockPane.ActiveContent = activeContent;
            }
        }

        private IDockContent DockContentLoadingHandler(string str)
        {
            var panels = DockPanels;

            string panelClass = str;
            string layoutArgs = null;

            if (panelClass.Contains(":"))
            {
                panelClass = str.Substring(0, str.IndexOf(":"));
                layoutArgs = str.Substring(str.IndexOf(":") + 1);
            }

            if (panelClass.Contains("LDDModder."))
                panelClass = panelClass.Replace("LDDModder.", "LDD.");

            for (int i = 0; i < panels.Length; i++)
            {
                if (panels[i].GetType().FullName == panelClass)
                {
                    if (!string.IsNullOrEmpty(layoutArgs) && 
                        panels[i] is ProjectDocumentPanel documentPanel)
                    {
                        Task.Factory.StartNew(() =>
                        {
                            Thread.Sleep(100);
                            BeginInvoke((Action)(() =>
                            {
                                documentPanel.ApplyLayoutArgs(layoutArgs);
                            }));
                        });
                    }

                    return panels[i];
                }
            }
            return null;
        }

        private bool LoadCustomLayout(UserUILayout layout)
        {
            if (!File.Exists(layout.Path))
                return false;

            Logger.Info("Loading custom layout");

            MemoryStream tmpMs = null;

            if (DockPanelControl.Contents.Count > 0)
            {
                tmpMs = new MemoryStream();
                DockPanelControl.SaveAsXml(tmpMs, Encoding.UTF8);
            }

            foreach (var content in DockPanelControl.Contents.ToArray())
                content.DockHandler.DockPanel = null;

            try
            {
                DockPanelControl.LoadFromXml(layout.Path, DockContentLoadingHandler);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error while loading custom layout xml");
            }

            if (tmpMs != null)
            {
                DockPanelControl.LoadFromXml(tmpMs, DockContentLoadingHandler);
            }

            return false;
        }

        private void LoadDefaultLayout()
        {
            foreach (var content in DockPanelControl.Contents.ToArray())
                content.DockHandler.DockPanel = null;

            try
            {
                var layoutStream = ResourceHelper.GetResourceStream("DefaultLayout.xml");
                if (layoutStream != null)
                    DockPanelControl.LoadFromXml(layoutStream, DockContentLoadingHandler);
                return;
            }
            catch { }

            ViewportPanel.Show(DockPanelControl, DockState.Document);

            StudConnectionPanel.Show(DockPanelControl, DockState.Document);

            ViewportPanel.Activate();

            DockPanelControl.DockLeftPortion = 250;

            NavigationPanel.Show(DockPanelControl, DockState.DockLeft);

            DockPanelControl.DockWindows[DockState.DockBottom].BringToFront();
            DockPanelControl.DockBottomPortion = 250;

            PropertiesPanel.Show(DockPanelControl, DockState.DockBottom);

            DetailPanel.Show(PropertiesPanel.Pane, null);

            ConnectionPanel.Show(PropertiesPanel.Pane, null);

            ValidationPanel.Show(PropertiesPanel.Pane, null);

            InfoPanel.Show(PropertiesPanel.Pane, null);
        }

        private void BeginLoadingUI()
        {
            WaitPopup = new ProgressPopupWindow();
            WaitPopup.Message = Messages.Message_InitializingUI;
            WaitPopup.Shown += OnInitializationPopupLoaded;
            WaitPopup.ShowCenter(this);

            //var test = new AsyncInvoker(this);
            //test.WhenShown(WaitPopup, () =>
            //{

            //}).Then(() =>
            //{

            //});
        }

        private void OnInitializationPopupLoaded(object sender, EventArgs e)
        {
            WaitPopup.Shown -= OnInitializationPopupLoaded;
            
            this.InvokeWithDelay(100, ()=>
            {
                try
                {
                    InitializePanels();
                    LayoutDockPanels();
                    InitializeAfterShown();
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Error while initializing UI");
                    MessageBoxEX.ShowException(this, Messages.Caption_UnexpectedError, Messages.Caption_UnexpectedError, ex);
                }
                
            });
            
            //Task.Factory.StartNew(() =>
            //{
            //    Thread.Sleep(100);
            //    BeginInvoke((Action)(() =>
            //    {
            //        FlagManager.Set("OnLoadAsync");
            //        AutoSaveTimer.Interval = 500;
            //        AutoSaveTimer.Start();
            //        WaitPopup.Shown -= OnInitializationPopupLoaded;
            //        InitializePanels();
            //        LayoutDockPanels();
            //    }));
            //});
        }

        private void InitializeAfterShown()
        {
            WaitPopup.Message = Messages.Message_InitializingResources;
            WaitPopup.UpdateProgress(0, 0);
            
            Application.DoEvents();

            ResourceHelper.LoadResources();

            LDDEnvironment.Initialize();
            SettingsManager.ValidateLddPaths();

            var documentPanels = DockPanelControl.Contents.OfType<ProjectDocumentPanel>().ToList();

            foreach (var documentPanel in documentPanels)
            {
                documentPanel.Enabled = true;
                documentPanel.DefferedInitialization();
                documentPanel.HasViewInitialized = true;
            }

            if (!UIRenderHelper.LoadFreetype6())
            {
                MessageBoxEX.Show(this, Messages.Message_CouldNotLoadFreetype6, 
                    Messages.Caption_UnexpectedError, 
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }

            ViewportPanel.Activate();//must be visible to properly init GL resource

            Task.Factory.StartNew(() =>
            {
                ViewportPanel.InitGlResourcesAsync();
                BeginInvoke(new MethodInvoker(OnInitializationFinished));
            });
        }

        private void OnInitializationFinished()
        {
            MainMenu.Enabled = true;
            WaitPopup.Hide();
            WaitPopup.Close();

            LoadAndValidateSettings();
            UpdateMenuItemStates();
            RebuildLayoutMenu();
            RebuildRecentFilesMenu();

            var documentPanels = DockPanelControl.Contents.OfType<ProjectDocumentPanel>().ToList();

            foreach (var documentPanel in documentPanels)
                documentPanel.OnInitializationFinished();

            IsInitializing = false;

            Task.Factory.StartNew(() =>
            {
                Thread.Sleep(200);
                Invoke(new MethodInvoker(CheckCanRecoverProject));
            });
        }

        public void ActivatePanel(ApplicationPanels panel)
        {
            switch (panel)
            {
                case ApplicationPanels.NavigationPanel:
                    NavigationPanel.Activate();
                    break;
                case ApplicationPanels.ViewportPanel:
                    ViewportPanel.Activate();
                    break;
                case ApplicationPanels.ValidationPanel:
                    ValidationPanel.Activate();
                    break;
                case ApplicationPanels.PropertiesPanel:
                    PropertiesPanel.Activate();
                    break;
                case ApplicationPanels.DetailPanel:
                    DetailPanel.Activate();
                    break;
                case ApplicationPanels.StudConnectionPanel:
                    StudConnectionPanel.Activate();
                    break;
                case ApplicationPanels.ConnectionPanel:
                    ConnectionPanel.Activate();
                    break;
                case ApplicationPanels.InfoPanel:
                    InfoPanel.Activate();
                    break;
                case ApplicationPanels.StudReferencesPanel:
                    StudReferencesPanel.Activate();
                    break;
            }
        }

        #endregion

        #region Project Handling

        private void InitializeProjectManager()
        {
            ProjectManager = new ProjectManager();
            ProjectManager.MainWindow = this;
            ProjectManager.ProjectChanged += ProjectManager_ProjectChanged;
            ProjectManager.UndoHistoryChanged += ProjectManager_UndoHistoryChanged;
            ProjectManager.ValidationFinished += ProjectManager_ValidationFinished;
            ProjectManager.GenerationFinished += ProjectManager_GenerationFinished;
            ProjectManager.ObjectPropertyChanged += ProjectManager_ObjectPropertyChanged;
        }

        private void ProjectManager_ProjectChanged(object sender, EventArgs e)
        {
            UpdateMenuItemStates();

            UpdateWindowTitle();
        }

        private void ProjectManager_ObjectPropertyChanged(object sender, ObjectPropertyChangedEventArgs e)
        {
            if (e.Object is PartProperties && 
                (e.PropertyName == nameof(PartProperties.ID) || e.PropertyName == nameof(PartProperties.Description))
                )
            {
                UpdateWindowTitle();
            }
        }

        private void ProjectManager_UndoHistoryChanged(object sender, EventArgs e)
        {
            if (InvokeRequired)
                BeginInvoke(new MethodInvoker(UpdateUndoRedoMenus));
            else
                UpdateUndoRedoMenus();
        }

        private bool OpenPartProjectFile(string projectFilePath)
        {
            if (!CloseCurrentProject())
                return false;

            if (MultiInstanceManager.InstanceCount > 1)
            {
                if (MultiInstanceManager.CheckFileIsOpen(projectFilePath))
                {
                    MessageBoxEX.ShowDetails(this,
                        Messages.Error_OpeningProject,
                        Messages.Caption_OpeningProject,
                        "The file is already opened in another instance."); //TODO: translate
                    return false;
                }
            }

            PartProject loadedProject = null;
            try
            {
                loadedProject = PartProject.Open(projectFilePath);
            }
            catch (Exception ex)
            {
                MessageBoxEX.ShowDetails(this, 
                    Messages.Error_OpeningProject, 
                    Messages.Caption_OpeningProject, ex.ToString());
            }

            if (loadedProject != null)
            {
                LoadPartProject(loadedProject);
                
            }

            return loadedProject != null;
        }

        private void OpenPartFromFiles(string lddFilePath)
        {
            string directory = Path.GetDirectoryName(lddFilePath);
            string filename = Path.GetFileNameWithoutExtension(lddFilePath);
            string fileType = Path.GetExtension(lddFilePath);

            if (!int.TryParse(filename, out int partID))
            {
                //TODO: Show message
                return;
            }

            Core.Primitives.Primitive primitive = null;

            string primitivePath = Path.Combine(directory, $"{partID}.xml");

            if (!File.Exists(primitivePath))
            {
                MessageBoxEX.ShowDetails(this,
                        Messages.Error_OpeningFile, //TODO: change
                        Messages.Caption_OpeningProject,
                        "Could not find primitive xml file."); //TODO: translate
                return;
            }

            try
            {
                primitive = Core.Primitives.Primitive.Load(primitivePath);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Could not read primitive '{primitivePath}'");

                MessageBoxEX.ShowDetails(this,
                    Messages.Error_OpeningFile, //TODO: change
                    Messages.Caption_OpeningProject,
                    "File is not an LDD Primitive."); //TODO: translate

                return;
            }

            string lod0Folder = Path.Combine(directory, "LOD0");

            string[] meshFiles = new string[0];

            if (Directory.Exists(lod0Folder))
                meshFiles = Directory.GetFiles(lod0Folder, primitive.ID + ".g*");
            if (meshFiles.Length == 0)
                meshFiles = Directory.GetFiles(directory, primitive.ID + ".g*");

            if (meshFiles.Length == 0)
            {
                var res = MessageBoxEX.Show(this,
                    Messages.Message_NoModelFoundOpenAnyway,
                    Messages.Caption_OpeningProject,
                    MessageBoxButtons.YesNo); //TODO: translate

                if (res != DialogResult.Yes)
                    return;
            }


            var partInfo = new PartWrapper(primitive);

            foreach (string meshPath in meshFiles)
            {
                if (PartSurfaceMesh.ParseSurfaceID(meshPath, out int surfaceID))
                {
                    try
                    {
                        var mesh = Core.Files.MeshFile.Read(meshPath);
                        partInfo.Surfaces.Add(new PartSurfaceMesh(partID, surfaceID, mesh)
                        {
                            Filepath = meshPath
                        });
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex, $"Could not read mesh '{meshPath}'");
                    }
                }
            }

            var tmpProject = PartProject.CreateFromLddPart(partInfo);
            LoadNewPartProject(tmpProject);
        }

        private void LoadNewPartProject(PartProject project)
        {
            try
            {
                
                if (!string.IsNullOrWhiteSpace(SettingsManager.Current.EditorSettings.Username))
                {
                    string username = SettingsManager.Current.EditorSettings.Username;
                    if (string.IsNullOrWhiteSpace(project.ProjectInfo.Authors)/* || 
                        !project.ProjectInfo.Authors.Contains(username)*/)
                    {
                        //if (!string.IsNullOrWhiteSpace(project.ProjectInfo.Authors))
                        //    username = ", " + username;

                        //project.ProjectInfo.Authors += username;
                        project.ProjectInfo.Authors = username;
                    }
                    
                }
                LoadPartProject(project);
            }
            catch (Exception ex)
            {
                MessageBoxEX.ShowDetails(this,
                    Messages.Error_CreatingProject,
                    Messages.Caption_OpeningProject, ex.ToString());
            }
        }

        private bool LoadPartProject(PartProject project, string tempPath = null)
        {
            if (!CloseCurrentProject())
                return false;

            if (!string.IsNullOrEmpty(project.ProjectPath))
                Logger.Info($"Loading project {project.ProjectPath}");
            else if(project.PartID != 0)
                Logger.Info($"Loading project from brick {project.PartID}");
            else
                Logger.Info($"Loading new empty project");

            ViewportPanel.ForceRender();
            ProjectManager.SetCurrentProject(project, tempPath);

            if (project != null)
            {
                if (!ProjectManager.IsNewProject && string.IsNullOrEmpty(tempPath))
                {
                    RebuildRecentFilesMenu();
                    SettingsManager.AddRecentProject(ProjectManager.GetCurrentProjectInfo(), true);
                }
                
                AutoSaveTimer.Start();
            }

            return project != null;
        }

        public bool CloseCurrentProject()
        {
            if (ProjectManager.IsProjectOpen)
            {
                Logger.Info("Closing current project");
                AutoSaveTimer.Stop();

                if (ProjectManager.IsModified || ProjectManager.IsNewProject)
                {
                    var messageText = ProjectManager.IsNewProject ? 
                        Messages.Message_SaveNewProject : 
                        Messages.Message_SaveChanges;

                    var result = MessageBox.Show(messageText, Messages.Caption_SaveBeforeClose, MessageBoxButtons.YesNoCancel);

                    if (result == DialogResult.Yes)
                        SaveProject();
                    else if (result == DialogResult.Cancel)
                    {
                        AutoSaveTimer.Start();
                        return false;
                    }
                }

                ProjectManager.CloseCurrentProject();
                
            }

            return true;
        }

        public void SaveProject(bool selectPath = false)
        {
            var project = ProjectManager.CurrentProject;
            string targetPath = ProjectManager.CurrentProjectPath;
            Logger.Info($"Saving current project");

            if (selectPath || ProjectManager.IsNewProject)
            {
                using (var sfd = new SaveFileDialog())
                {
                    if (!string.IsNullOrEmpty(targetPath))
                    {
                        sfd.InitialDirectory = Path.GetDirectoryName(targetPath);
                        sfd.FileName = Path.GetFileName(targetPath);
                    }
                    else
                    {
                        if (SettingsManager.IsWorkspaceDefined)
                            sfd.InitialDirectory = SettingsManager.Current.EditorSettings.ProjectWorkspace;

                        if (project.PartID > 0)
                            sfd.FileName = $"{project.PartID}.lpp";
                        else
                            sfd.FileName = $"New part.lpp";
                    }

                    sfd.Filter = "LDD Part Project|*.lpp|All Files|*.*";
                    sfd.DefaultExt = ".lpp";

                    if (sfd.ShowDialog() == DialogResult.OK)
                        targetPath = sfd.FileName;
                    else
                        return;
                }
            }

            string oldPath = ProjectManager.CurrentProjectPath;

            ProjectManager.SaveProject(targetPath);

            if (oldPath != targetPath)
                RebuildRecentFilesMenu();
        }

        private void CheckCanRecoverProject()
        {
            SettingsManager.CleanUpFilesHistory();

            if (SettingsManager.Current.OpenedProjects.Count > 0)
            {
                bool projectWasLoaded = false;

                foreach(var fileInfo in SettingsManager.Current.OpenedProjects.ToArray())
                {
                    //project was not correctly closed
                    if (File.Exists(fileInfo.TemporaryPath))
                    {
                        if (projectWasLoaded)
                            continue;

                        if (MultiInstanceManager.InstanceCount > 1)
                        {
                            bool isOpenInOtherInstance = MultiInstanceManager.CheckFileIsOpen(fileInfo.TemporaryPath);
                            if (isOpenInOtherInstance)
                                return;
                        }

                        bool projectRestored = false;

                        if (MessageBoxEX.Show(this,
                            Messages.Message_RecoverProject,
                            Messages.Caption_RecoverLastProject, 
                            MessageBoxButtons.YesNo) == DialogResult.Yes)
                        {
                            PartProject loadedProject = null;
                            try
                            {
                                loadedProject = PartProject.Open(fileInfo.TemporaryPath);
                                loadedProject.ProjectPath = fileInfo.ProjectFile;

                                if (LoadPartProject(loadedProject, fileInfo.TemporaryPath))
                                {
                                    projectRestored = true;
                                    projectWasLoaded = true;
                                }
                            }
                            catch (Exception ex)
                            {
                                MessageBoxEX.ShowDetails(this,
                                    Messages.Error_OpeningProject,
                                    Messages.Caption_OpeningProject, ex.ToString());
                                //exceptionThrown = true;
                            }
                            
                        }

                        if (!projectRestored)
                            ProjectManager.DeleteTemporaryProject(fileInfo.TemporaryPath);

                        break;
                    }
                }
            }
        }

        #endregion

        private void ImportMeshFile()
        {
            using (var imd = new ImportModelsDialog(ProjectManager))
            {
                imd.SelectFileOnStart = true;
                imd.ShowDialog();
            }
        }
        
        private void BrickEditorWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (IsInitializing)
            {
                e.Cancel = true;
                return;
            }

            SaveCurrentUILayout();

            if (CurrentProject != null)
            {
                e.Cancel = true;
                BeginInvoke(new MethodInvoker(TryCloseProjectAndExit));
                return;
            }

            Logger.Info("Closing Brick Studio");
            Logger.Debug($"FormClosing started at {DateTime.Now:HH:mm:ss.ff}");

            foreach (var form in DockPanelControl.Documents.OfType<DockContent>().ToList())
            {
                Logger.Debug($"Closing Form '{form.Text}' at {DateTime.Now:HH:mm:ss.ff}");
                form.Close();
                if (!form.IsDisposed)
                {
                    e.Cancel = true;
                    break;
                }
            }

            Logger.Debug($"FormClosing finished at {DateTime.Now:HH:mm:ss.ff}");
        }

        private void TryCloseProjectAndExit()
        {
            Logger.Debug($"TryCloseProjectAndExit at {DateTime.Now:HH:mm:ss.ff}");
            if (CloseCurrentProject())
            {
                ViewportPanel.StopRenderingLoop();
                ViewportPanel.UnloadModels();

                //Application.DoEvents();
                Task.Factory.StartNew(() =>
                {
                    Thread.Sleep(100);
                    Logger.Debug($"Invoke Close at {DateTime.Now:HH:mm:ss.ff}");
                    BeginInvoke(new MethodInvoker(Close));
                });
            }
            else
                Logger.Info("User canceled closing");
        }

        private void BrickEditorWindow_FormClosed(object sender, FormClosedEventArgs e)
        {
            Logger.Debug($"FormClosed at {DateTime.Now:HH:mm:ss.ff}");
        }

        private void AutoSaveTimer_Tick(object sender, EventArgs e)
        {
            if (ProjectManager.IsProjectOpen)
                ProjectManager.SaveWorkingProject();
        }

        public DockPanel GetDockPanelControl()
        {
            return DockPanelControl;
        }

        public bool IsFileOpen(string filepath)
        {
            if (ProjectManager.IsProjectOpen)
            {
                return ProjectManager.CurrentProjectPath == filepath || 
                    ProjectManager.TemporaryProjectPath == filepath;
            }
            return false;
        }

        protected override void WndProc(ref Message m)
        {
            if (!MultiInstanceManager.ProcessMessage(ref m))
                base.WndProc(ref m);
        }

        private void SaveCurrentUILayout()
        {
            SettingsManager.SaveCurrentUILayout(DockPanelControl);

            User32.WINDOWPLACEMENT posInfo = default;
            if (User32.GetWindowPlacement(Handle, ref posInfo))
            {
                SettingsManager.LoadSettings();
                SettingsManager.Current.DisplaySettings.LastPosition = posInfo.NormalPosition;
                SettingsManager.Current.DisplaySettings.IsMaximized = posInfo.ShowCmd.HasFlag(User32.ShowWindowCommands.Maximize);
                SettingsManager.SaveSettings();
            }
        }


    }
}
