using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LDD.BrickEditor.ProjectHandling.ViewInterfaces
{
    public interface IViewportWindow
    {
        Rendering.Models.StudRefModel SelectedStudModel { get; }
        void RebuildModels();
        void ForceRender();
        void InvalidateBones();
        void SelectModel(Rendering.ModelBase model);

        void SelectStudReference(Modding.StudReference reference);
    }
}
