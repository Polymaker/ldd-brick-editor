using LDD.BrickEditor.ProjectHandling;
using LDD.BrickEditor.Resources;
using LDD.Modding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LDD.BrickEditor.Models.Navigation
{
    public class ProjectCollectionNode : ProjectTreeNode
    {
        //public override PartProject Project => Collection.Project;

        //public ProjectManager Manager { get; set; }

        public bool DoNotShowNodes { get; set; }

        public IElementCollection Collection { get; }

        public Type ElementType => Collection.ElementType;

        //public ProjectCollectionNode(IElementCollection collection)
        //{
        //    Collection = collection;
        //    NodeID = collection.GetHashCode().ToString();
        //}

        public ProjectCollectionNode(IElementCollection collection, string text) : base (text)
        {
            Collection = collection;
            NodeID = collection.GetHashCode().ToString();
            Text = text;
        }

        protected override void OnManagerAssigned()
        {
            base.OnManagerAssigned();
            if (Manager != null)
                Manager.ElementCollectionVisibilityChanged += Manager_ElementCollectionVisibilityChanged;
        }

        private void Manager_ElementCollectionVisibilityChanged(object sender, EventArgs e)
        {
            if (sender == Collection)
            {
                InvalidateVisibility();
                Manager.RefreshNavigationNode(this);
            }
        }

        public override void FreeObjects()
        {
            base.FreeObjects();
            if (Manager != null)
                Manager.ElementCollectionVisibilityChanged -= Manager_ElementCollectionVisibilityChanged;
        }

        protected override void RebuildChildrens()
        {
            base.RebuildChildrens();

            if (DoNotShowNodes)
                return;

            if (Collection.ElementType == typeof(PartConnection))
            {
                foreach (var elemGroup in Collection.GetElements().OfType<PartConnection>().GroupBy(x => x.ConnectorType))
                {
                    string groupTitle = ModelLocalizations.ResourceManager.GetString($"Label_{elemGroup.Key}Connectors");
                    
                    AutoGroupElements(elemGroup, groupTitle, 4, 10);
                }
            }
            else if (Collection.ElementType == typeof(PartCollision))
            {
                foreach (var elemGroup in Collection.GetElements().OfType<PartCollision>().GroupBy(x => x.CollisionType))
                {
                    string groupTitle = elemGroup.Key == Core.Primitives.Collisions.CollisionType.Box ?
                        ModelLocalizations.Label_CollisionBoxes : ModelLocalizations.Label_CollisionSpheres;
                    
                    AutoGroupElements(elemGroup, groupTitle, 10, 10, true);
                }
            }
            else
            {
                foreach (var elem in Collection.GetElements())
                    Nodes.Add(ProjectElementNode.CreateDefault(elem));
            }
        }

        protected override VisibilityState GetVisibilityState()
        {
            if (Collection == Manager.CurrentProject.Surfaces)
                return Manager.ShowPartModels ? VisibilityState.Visible : VisibilityState.Hidden;
            if (Collection == Manager.CurrentProject.Collisions)
                return Manager.ShowCollisions ? VisibilityState.Visible : VisibilityState.Hidden;
            if (Collection == Manager.CurrentProject.Connections)
                return Manager.ShowConnections ? VisibilityState.Visible : VisibilityState.Hidden;
            if (Collection == Manager.CurrentProject.Bones)
                return Manager.ShowBones ? VisibilityState.Visible : VisibilityState.Hidden;
            return VisibilityState.None;
        }

        protected override bool IsHiddenCore()
        {
            if (Collection == Manager.CurrentProject.Surfaces)
                return !Manager.ShowPartModels;
            if (Collection == Manager.CurrentProject.Collisions)
                return !Manager.ShowCollisions;
            if (Collection == Manager.CurrentProject.Connections)
                return !Manager.ShowConnections;
            return false;
        }

        protected override bool CanToggleVisibilityCore()
        {
            return Collection == Manager.CurrentProject.Surfaces ||
                Collection == Manager.CurrentProject.Collisions ||
                Collection == Manager.CurrentProject.Connections;
        }

        protected override void ToggleVisibilityCore()
        {
            if (Collection == Manager.CurrentProject.Surfaces)
                Manager.ShowPartModels = !Manager.ShowPartModels;
            else if (Collection == Manager.CurrentProject.Collisions)
                Manager.ShowCollisions = !Manager.ShowCollisions;
            else if (Collection == Manager.CurrentProject.Connections)
                Manager.ShowConnections = !Manager.ShowConnections;
            else if (Collection == Manager.CurrentProject.Bones)
                Manager.ShowBones = !Manager.ShowBones;
        }
    }
}
