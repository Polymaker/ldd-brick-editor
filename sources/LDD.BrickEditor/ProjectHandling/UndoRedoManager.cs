﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LDD.BrickEditor.ProjectHandling
{
    public class UndoRedoManager
    {
        public ProjectManager ProjectManager { get; }

        //public ProjectDocument Document { get; private set; }

        public long CurrentChangeID { get; private set; }

        public int MaxHistory { get; set; }

        public bool ExecutingUndoRedo { get; private set; }

        public bool HistoryLimitExceeded { get; private set; }

        public bool ProjectIsModified { get; private set; }

        public bool NonReversibleActionPerformed { get; set; }

        private List<ChangeAction> UndoHistory { get; }

        private List<ChangeAction> RedoHistory { get; }

        public bool CanUndo => UndoHistory.Any();

        public bool CanRedo => RedoHistory.Any();

        private List<ChangeAction> BatchChanges;

        private int BatchNesting;

        public bool IsInBatch { get; private set; }

        public event EventHandler UndoHistoryChanged;

        public event EventHandler BeginUndoRedo;

        public event EventHandler EndUndoRedo;

        public UndoRedoManager(ProjectManager projectManager)
        {
            ProjectManager = projectManager;
            //ProjectManager.ProjectChanged += ProjectManager_ProjectChanged;
            MaxHistory = 15;

            UndoHistory = new List<ChangeAction>();
            RedoHistory = new List<ChangeAction>();
            BatchChanges = new List<ChangeAction>();
        }

        public void ClearHistory()
        {
            UndoHistory.Clear();
            RedoHistory.Clear();
            HistoryLimitExceeded = false;
            ProjectIsModified = false;
            CurrentChangeID = 0;
        }

        internal void ProcessProjectCollectionChanged(CollectionChangedEventArgs e)
        {
            if (ExecutingUndoRedo)
                return;

            var action = new CollectionChangeAction(e);
            AddActionCore(action);
        }

        internal void ProcessElementPropertyChanged(ObjectPropertyChangedEventArgs e)
        {
            if (ExecutingUndoRedo)
                return;

            var action = new PropertyChangeAction(e);
            AddActionCore(action);
        }

        public void StartBatchChanges()
        {
            BatchNesting++;
            IsInBatch = true;
        }

        public bool EndBatchChanges()
        {
            BatchNesting--;
            if (BatchNesting == 0)
            {
                IsInBatch = false;
                if (BatchChanges.Any())
                    AddAction(CombineBatchChanges(BatchChanges));

                BatchChanges.Clear();
                return true;
            }
            return false;
        }

        private BatchChangeAction CombineBatchChanges(IEnumerable<ChangeAction> actions)
        {
            var combinedChanges = new List<ChangeAction>();
            CollectionChangeAction prevColChange = null;

            foreach (var action in actions)
            {
                if (action is CollectionChangeAction colChange)
                {
                    if (prevColChange != null)
                    {
                        if (colChange.Data.Collection == prevColChange.Data.Collection)
                        {
                            prevColChange = new CollectionChangeAction(
                                new CollectionChangedEventArgs(
                                    colChange.Data.Collection,
                                    prevColChange.Data.ChangedItems.Concat(colChange.Data.ChangedItems)
                                    ));
                        }
                        else
                        {
                            combinedChanges.Add(prevColChange);
                            prevColChange = colChange;
                        }
                    }
                    else
                    {
                        prevColChange = colChange;
                    }
                }
                else
                {
                    if (prevColChange != null)
                    {
                        combinedChanges.Add(prevColChange);
                        prevColChange = null;
                    }

                    combinedChanges.Add(action);
                }
            }

            if (prevColChange != null)
                combinedChanges.Add(prevColChange);

            return new BatchChangeAction(combinedChanges);
        }

        public void AddEditorAction(EditorAction action)
        {
            AddActionCore(action);
        }

        private void AddActionCore(ChangeAction action)
        {
            bool createdBatch = false;

            if (!ProjectIsModified && !(action is EditorAction))
            {
                ProjectIsModified = true;
                if (!IsInBatch)
                {
                    StartBatchChanges();
                    createdBatch = true;
                }
                ProjectManager.CurrentProject.ProjectInfo.LastModification = DateTime.Now;
            }

            if (IsInBatch)
                BatchChanges.Add(action);
            else
                AddAction(action);

            if (createdBatch)
                EndBatchChanges();
        }

        private void AddAction(ChangeAction action)
        {
            RedoHistory.Clear();

            UndoHistory.Add(action);
            action.ChangeID = ++CurrentChangeID;

            if (UndoHistory.Count > MaxHistory)
            {
                HistoryLimitExceeded = true;
                UndoHistory.RemoveAt(0);
            }

            UndoHistoryChanged?.Invoke(this, EventArgs.Empty);
        }

        public void Undo()
        {
            if (UndoHistory.Any())
            {
                BeginUndoRedo?.Invoke(this, EventArgs.Empty);

                var lastAction = UndoHistory.Last();
                CurrentChangeID = lastAction.ChangeID - 1;

                UndoHistory.Remove(lastAction);
                RedoHistory.Add(lastAction);

                ExecutingUndoRedo = true;
                lastAction.Undo();
                ExecutingUndoRedo = false;

                EndUndoRedo?.Invoke(this, EventArgs.Empty);
                UndoHistoryChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public void Redo()
        {
            if (RedoHistory.Any())
            {
                BeginUndoRedo?.Invoke(this, EventArgs.Empty);

                var lastAction = RedoHistory.Last();
                CurrentChangeID = lastAction.ChangeID;

                RedoHistory.Remove(lastAction);
                UndoHistory.Add(lastAction);

                ExecutingUndoRedo = true;
                lastAction.Redo();
                ExecutingUndoRedo = false;

                EndUndoRedo?.Invoke(this, EventArgs.Empty);
                UndoHistoryChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
