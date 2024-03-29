﻿using LDD.BrickEditor.ProjectHandling;
using LDD.BrickEditor.Resources;
using LDD.Modding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LDD.BrickEditor.Models.Navigation
{
    public class ProjectTreeNode
    {
        public string NodeID { get; set; }

        public string Text { get; set; }

        private IProjectManager _Manager;
        public IProjectManager Manager
        {

            get => _Manager;
            set
            {
                if (_Manager != value)
                {
                    _Manager = value;
                    OnManagerAssigned();
                }
            }
        }
        //public IProjectDocument Document { get; set; }


        public ProjectTreeNode Parent { get; set; }

        public ProjectTreeNode RootNode => Parent == null ? this : Parent.RootNode;

        public bool IsRoot => Parent == null;

        public int TreeLevel => Parent == null ? 0 : Parent.TreeLevel + 1;

        public ProjectTreeNodeCollection Nodes { get; private set; }

        public string ImageKey { get; set; }

        public string VisibilityImageKey { get; set; }

        protected bool IsVisibilityDirty { get; set; }

        private VisibilityState _VisibilityState;
        public VisibilityState VisibilityState
        {
            get
            {
                if (IsVisibilityDirty)
                    RecalculateVisibility();
                return _VisibilityState;
            }
        }


        public ProjectTreeNode()
        {
            NodeID = GetHashCode().ToString();
            Nodes = new ProjectTreeNodeCollection(this);
            nodesDirty = true;
            IsVisibilityDirty = true;
        }

        public ProjectTreeNode(string text) : this()
        {
            Text = text;
        }

        internal void AssignParent(ProjectTreeNode node)
        {
            Parent = node;

            if (node?.Manager != null)
                Manager = node?.Manager;
        }

        protected virtual void OnManagerAssigned()
        {

        }

        public virtual void FreeObjects()
        {
            foreach (var child in Nodes)
                child.FreeObjects();
            Manager = null;
        }

        #region Children Handling

        protected bool nodesDirty;

        public virtual void InvalidateChildrens()
        {
            nodesDirty = true;
            foreach (var childNode in Nodes)
            {
                childNode.nodesDirty = true;
            }
        }

        public bool HasChildrens()
        {
            if (nodesDirty)
                RebuildChildrensCore();

            return Nodes.Any();
        }

        private void RebuildChildrensCore()
        {
            RebuildChildrens();
            nodesDirty = false;
        }

        protected virtual void RebuildChildrens()
        {
            foreach (var child in Nodes)
                child.FreeObjects();
            Nodes.Clear();
        }

        public IEnumerable<ProjectTreeNode> GetChildHierarchy(bool includeSelf = false)
        {
            if (includeSelf)
                yield return this;

            if (HasChildrens())
            {
                foreach (var directChild in Nodes)
                {
                    foreach (var subChild in directChild.GetChildHierarchy(true))
                        yield return subChild;
                }
            }
        }

        public IEnumerable<ProjectTreeNode> GetParents(bool includeSelf = false)
        {
            if (includeSelf)
                yield return this;

            if (Parent != null)
            {
                yield return Parent;
                foreach (var other in Parent.GetParents())
                    yield return other;
            }
        }

        protected void AutoGroupElements(IEnumerable<PartElement> elements, string groupTitle, int groupWhen, int maxGroupSize, bool groupOnSameLevel = false)
        {
            int totalElements = elements.Count();

            if (totalElements > groupWhen)
            {
                ProjectTreeNode groupNode = this;

                if (!groupOnSameLevel)
                {
                    groupNode = new ProjectTreeNode(groupTitle)
                    {
                        nodesDirty = false,
                        NodeID = $"{NodeID}_{groupTitle}"
                    };
                    Nodes.Add(groupNode);
                }

                //var parentNode = groupNode.Parent;

                if (totalElements > maxGroupSize)
                {
                    int remaining = totalElements;
                    int currIdx = 0;

                    while (remaining > 0)
                    {
                        int takeCount = Math.Min(remaining, maxGroupSize);

                        string rangeText = string.Empty;
                        if (takeCount / (double)maxGroupSize < 0.5)
                            rangeText = string.Format(ModelLocalizations.NodeRangeFormat2, currIdx + 1);
                        else
                            rangeText = string.Format(ModelLocalizations.NodeRangeFormat1, currIdx + 1, currIdx + takeCount);

                        var rangeNode = new ElementGroupNode(rangeText);
                        rangeNode.NodeID = $"{NodeID}_{groupTitle}_{currIdx + 1}";
                        if (groupOnSameLevel)
                            rangeNode.Text = groupTitle + " " + rangeNode.Text;

                        rangeNode.Elements.AddRange(elements.Skip(currIdx).Take(maxGroupSize));
                        groupNode.Nodes.Add(rangeNode);
                        currIdx += takeCount;
                        remaining -= takeCount;
                    }
                }
                else
                {
                    foreach (var elem in elements)
                        groupNode.Nodes.Add(ProjectElementNode.CreateDefault(elem));
                }
            }
            else
            {
                foreach (var elem in elements)
                    Nodes.Add(ProjectElementNode.CreateDefault(elem));
            }
        }

        #endregion

        public virtual bool CanToggleVisibility()
        {
            return CanToggleVisibilityCore();
        }

        protected virtual bool CanToggleVisibilityCore()
        {
            return Nodes.Any(x => x.CanToggleVisibility());
        }

        private readonly object VisibilityLock = new object();

        private void RecalculateVisibility()
        {
            try
            {
                if (Monitor.TryEnter(VisibilityLock))
                {
                    _VisibilityState = GetVisibilityState();
                    IsVisibilityDirty = false;
                }
            }
            catch { 
            }
            finally
            {
                Monitor.Exit(VisibilityLock);
            }
            
        }

        protected bool TryGetVisibility(out VisibilityState state)
        {
            if (!IsVisibilityDirty)
            {
                state = _VisibilityState;
                return true;
            }

            try
            {
                if (Monitor.TryEnter(VisibilityLock))
                {
                    _VisibilityState = GetVisibilityState();
                    state = _VisibilityState;
                    IsVisibilityDirty = false;
                    return true;
                }
            }
            catch
            {
            }
            finally
            {
                Monitor.Exit(VisibilityLock);
            }

            state = VisibilityState.None;
            return false;
        }

        public void InvalidateVisibility()
        {
            ProjectTreeNode curNode = this;
            while(curNode != null)
            {
                curNode.IsVisibilityDirty = true;
                curNode = curNode.Parent;
            }
        }

        protected virtual bool IsHiddenCore()
        {
            return false;
        }

        public bool IsHidden()
        {
            return IsHiddenCore();
        }

        protected virtual VisibilityState GetVisibilityState()
        {
            bool isHidden = IsHiddenCore();

            if (IsRoot)
            {
                return isHidden ? VisibilityState.Hidden : VisibilityState.Visible;
            }

            bool parentIsHidden = Parent.IsHiddenCore() || RootNode.IsHiddenCore();

            if (Nodes.Count > 0)
            {
                var allChilds = GetChildHierarchy();
                var nodeStates = Nodes.Select(x => x.VisibilityState);
                int hiddenCount = allChilds.Count(x => x.IsHiddenCore());
                int visibleCount = allChilds.Count() - hiddenCount;

                if (hiddenCount > visibleCount)
                {
                    return parentIsHidden ? VisibilityState.HiddenNotVisible :  VisibilityState.Hidden;
                }
            }

            return parentIsHidden ? VisibilityState.NotVisible : VisibilityState.Visible;
        }

        protected int GetHiddenElementCount()
        {
            var childModels = Nodes.OfType<ProjectElementNode>().Select(x => x.Element.GetExtension<ModelElementExtension>()).Where(x => x != null).ToList();
            return childModels.Count(x => x.IsHidden);
        }


        public void ToggleVisibility()
        {
            //if (!CanToggleVisibility())
            //    return;

            ToggleVisibilityCore();
            IsVisibilityDirty = true;
        }

        protected virtual void ToggleVisibilityCore()
        {
            if (Nodes.All(x => x is ElementGroupNode))
            {
                var elements = GetChildHierarchy().OfType<ProjectElementNode>().Select(x => x.Element);

                var nodeStates = Nodes.Select(x => x.VisibilityState);

                int hiddenCount = nodeStates.Count(x => x != VisibilityState.Visible);
                int visibleCount = nodeStates.Count() - hiddenCount;

                bool hideElements = visibleCount > hiddenCount;

                Manager.SetElementsHidden(elements, hideElements);
            }
            else if (Nodes.All(x => x is ProjectElementNode))
            {
                var elements = GetChildHierarchy().OfType<ProjectElementNode>().Select(x => x.Element);

                var nodeStates = Nodes.Select(x => x.VisibilityState);

                int hiddenCount = nodeStates.Count(x => x == VisibilityState.Hidden || x == VisibilityState.HiddenNotVisible);
                int visibleCount = nodeStates.Count() - hiddenCount;

                bool hideElements = visibleCount > hiddenCount;

                Manager.SetElementsHidden(elements, hideElements);
            }
        }

        public virtual void UpdateVisibilityIcon()
        {
            VisibilityImageKey = string.Empty;

            switch (VisibilityState)
            {
                case VisibilityState.Visible:
                    VisibilityImageKey = "Visible";
                    break;
                case VisibilityState.Hidden:
                    VisibilityImageKey = "Hidden";
                    break;
                case VisibilityState.HiddenNotVisible:
                    VisibilityImageKey = "Hidden2";
                    break;
                case VisibilityState.NotVisible:
                    VisibilityImageKey = "NotVisible";
                    break;
            }
        }

        #region Drag & Drop

        public virtual bool CanDragDrop()
        {
            return false;
        }

        public virtual bool CanDropOn(ProjectTreeNode node)
        {
            return false;
        }

        public virtual bool CanDropBefore(ProjectTreeNode node)
        {
            return false;
        }

        public virtual bool CanDropAfter(ProjectTreeNode node)
        {
            return false;
        }

        #endregion
    
    }
}
