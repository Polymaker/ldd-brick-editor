﻿using ObjectTK.Buffers;
using ObjectTK.Shaders.Variables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;
using OpenTK;

namespace LDD.BrickEditor.Rendering
{
    public interface IVertexBuffer
    {
        Buffer<int> IndexBuffer { get; }

        Vector3 GetVertex(int index);

        void Bind();

        void BindAttribute(VertexAttrib attribute, int offset);

        void UnbindAttribute(VertexAttrib attribute);

        void DrawElements(PrimitiveType drawMode);

        void DrawElementsBaseVertex(PrimitiveType mode, int baseVertex, int count, int offset = 0);
    }
}
