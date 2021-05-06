using LDD.BrickEditor.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WeifenLuo.WinFormsUI.ThemeVS2015;

namespace LDD.BrickEditor.UI
{
    class CustomDockTheme : VS2015ThemeBase
    {
        public CustomDockTheme() : base(ResourceHelper.GetResourceBytes("CustomTheme.vstheme"))
        {
        }
    }
}
