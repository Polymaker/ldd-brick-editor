using LDD.BrickEditor.Models.Navigation;
using LDD.Modding.Editing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LDD.BrickEditor.ProjectHandling
{
    public interface IProjectDocument
    {
        PartProject Project { get; }

        ProjectTreeNodeCollection NavigationTreeNodes { get; }

        bool ShowPartModels { get; }

        bool ShowCollisions { get; }

        bool ShowConnections { get; }

        void RebuildNavigationTree();

        void RefreshNavigationNode(ProjectTreeNode node);
    }
}
