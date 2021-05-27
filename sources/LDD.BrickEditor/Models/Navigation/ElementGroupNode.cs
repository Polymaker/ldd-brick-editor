using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LDD.BrickEditor.ProjectHandling;
using LDD.Modding;

namespace LDD.BrickEditor.Models.Navigation
{
    public class ElementGroupNode : ProjectTreeNode
    {
        public List<PartElement> Elements { get; set; }

        public ElementGroupNode()
        {
            Elements = new List<PartElement>();
        }

        public ElementGroupNode(string text) : base(text)
        {
            Elements = new List<PartElement>();
        }

        protected override void RebuildChildrens()
        {
            base.RebuildChildrens();

            foreach (var elem in Elements)
                Nodes.Add(ProjectElementNode.CreateDefault(elem));
        }

        protected override VisibilityState GetVisibilityState()
        {
            if (Elements.Count > 0 && Nodes.Count == 0)
            {
                var childModels = Elements.Select(x => x.GetExtension<ModelElementExtension>()).Where(x => x != null).ToList();
                if (childModels.Count > 0)
                {
                    int hiddenModels = childModels.Count(x => x.IsHidden);
                    int visibleModels = childModels.Count - hiddenModels;
                    bool hideElements = visibleModels > hiddenModels;
                }
            }
            return base.GetVisibilityState();
        }

        //public override void ToggleVisibility()
        //{
        //    var childModels = Nodes.OfType<ProjectElementNode>().Select(x => x.Element.GetExtension<ModelElementExtension>()).Where(x => x != null).ToList();

        //    if (childModels.Any())
        //    {
        //        int hiddenModels = childModels.Count(x => x.IsHidden);
        //        int visibleModels = childModels.Count - hiddenModels;
        //        bool hideElements = visibleModels > hiddenModels;
        //        Manager.SetElementsHidden(childModels.Select(x => x.Element), hideElements);
        //    }
        //}
    }
}
