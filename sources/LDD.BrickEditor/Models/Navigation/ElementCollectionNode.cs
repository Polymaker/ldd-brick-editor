using LDD.BrickEditor.ProjectHandling;
using LDD.Modding;
using LDD.Modding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LDD.BrickEditor.Models.Navigation
{
    public class ElementCollectionNode : ProjectTreeNode
    {
        //public override PartProject Project => Element.Project;

        public PartElement Element { get;}

        public IElementCollection Collection { get; }

        public Type CollectionType => Collection.ElementType;

        public string CollectionName { get; set; }

        public ElementCollectionNode(PartElement element, IElementCollection collection, string text)
        {
            Element = element;
            Collection = collection;
            NodeID = collection.GetHashCode().ToString();
            Text = text;
        }

        protected override void RebuildChildrens()
        {
            base.RebuildChildrens();

            AutoGroupElements(Collection.GetElements(), null, 10, 20, true);
            //foreach (var elem in Collection.GetElements())
            //    Nodes.Add(ProjectElementNode.CreateDefault(elem));
        }

        //public override void UpdateVisibilityIcon()
        //{
        //    base.UpdateVisibilityIcon();

        //    var femaleModelExt = Element.GetExtension<FemaleStudModelExtension>();

        //    if (femaleModelExt != null)
        //    {
        //        bool isAlternate = (Element as FemaleStudModel).ReplacementMeshes == Collection;
        //        bool isVisible = isAlternate == femaleModelExt.ShowAlternateModels;

        //        if (!femaleModelExt.IsVisible)
        //            VisibilityImageKey = isVisible ? "NotVisible" : "Hidden2";
        //        else
        //            VisibilityImageKey = isVisible ? "Visible" : "Hidden";
        //    }
        //    else
        //    {
        //        //var childElems = Nodes.Select(x=> x.el)
        //    }
        //}


        protected override VisibilityState GetVisibilityState2()
        {
            var femaleModelExt = Element.GetExtension<FemaleStudModelExtension>();

            if (femaleModelExt != null)
            {
                bool isAlternate = (Element as FemaleStudModel).ReplacementMeshes == Collection;
                bool isVisible = isAlternate == femaleModelExt.ShowAlternateModels;

                if (!femaleModelExt.IsVisible)
                    return isVisible ? VisibilityState.NotVisible : VisibilityState.HiddenNotVisible;
                else
                    return isVisible ? VisibilityState.Visible : VisibilityState.Hidden;
            }
            else if (Element is PartBone bone)
            {
                bool isCollectionVisible = false;
                if (Collection == bone.Connections)
                    isCollectionVisible = Manager?.ShowConnections ?? false;
                else if (Collection == bone.Collisions)
                    isCollectionVisible = Manager?.ShowCollisions ?? false;

                var baseVisibility = base.GetVisibilityState2();


                if (!isCollectionVisible)
                {
                    if (baseVisibility == VisibilityState.Hidden)
                        return VisibilityState.HiddenNotVisible;
                    else
                        return VisibilityState.NotVisible;
                }
                return baseVisibility;
            }
            return base.GetVisibilityState2();
        }

        protected override bool CanToggleVisibilityCore()
        {
            var femaleModelExt = Element.GetExtension<FemaleStudModelExtension>();

            return femaleModelExt != null;
        }

        protected override void ToggleVisibilityCore()
        {
            var femaleModelExt = Element.GetExtension<FemaleStudModelExtension>();
            if (femaleModelExt != null)
            {
                femaleModelExt.ShowAlternateModels = !femaleModelExt.ShowAlternateModels;
                return;
            }
            else if (Element is PartBone bone && Manager != null)
            {
                var baseVisibility = GetVisibilityState2();
                
                if (Collection == bone.Connections && !Manager.ShowConnections)
                {
                    Manager.ShowConnections = true;
                    if (baseVisibility == VisibilityState.NotVisible)
                        return;
                }
                else if (Collection == bone.Collisions && !Manager.ShowCollisions)
                {
                    Manager.ShowCollisions = true;
                    if (baseVisibility == VisibilityState.NotVisible)
                        return;
                }

            }
            base.ToggleVisibilityCore();
        }
    }
}
