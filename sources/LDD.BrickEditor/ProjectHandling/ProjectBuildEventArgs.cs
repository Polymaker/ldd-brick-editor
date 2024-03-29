﻿using LDD.Core.Parts;
using LDD.Modding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LDD.BrickEditor.ProjectHandling
{
    public class ProjectBuildEventArgs : EventArgs
    {
        public PartWrapper Result { get; }
        public bool Successful { get; }
        public IEnumerable<ValidationMessage> Messages { get; }

        public ProjectBuildEventArgs(PartWrapper result, bool successful, IEnumerable<ValidationMessage> messages)
        {
            Result = result;
            Successful = successful;
            Messages = messages.ToList().AsReadOnly(); 
        }
    }
}
