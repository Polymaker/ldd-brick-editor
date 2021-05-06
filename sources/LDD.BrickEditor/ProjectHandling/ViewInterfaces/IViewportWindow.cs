using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LDD.BrickEditor.ProjectHandling.ViewInterfaces
{
    public interface IViewportWindow
    {
        void RebuildModels();
        void ForceRender();
        void InvalidateBones();
    }
}
