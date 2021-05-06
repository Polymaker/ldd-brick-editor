using LDD.BrickEditor.Models.Navigation;
using LDD.Modding;
using LDD.Modding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LDD.BrickEditor.ProjectHandling
{
    public interface IProjectManager
    {
        PartProject CurrentProject { get; }

        ProjectTreeNodeCollection NavigationTreeNodes { get; }

        bool ShowPartModels { get; set; }

        bool ShowCollisions { get; set; }

        bool ShowConnections { get; set; }

        bool ShowBones { get; set; }

        void RebuildNavigationTree();

        void RefreshNavigationNode(ProjectTreeNode node);

        void SetElementHidden(PartElement element, bool hidden);

        void SetElementsHidden(IEnumerable<PartElement> elements, bool hidden);
    }
}
