﻿using ObjectTK.Shaders.Variables;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Graphics;
using ObjectTK.Shaders.Sources;

namespace LDD.BrickEditor.Rendering.Shaders
{
    [SourceFile("LDD.BrickEditor.Resources.Shaders.GridShader.glsl", Embedded = true, SourceName = "GridShader")]
    [VertexShaderSource("GridShader.Vertex")]
    [FragmentShaderSource("GridShader.Fragment")]
    public class GridShaderProgram : ObjectTK.Shaders.Program
    {
        [VertexAttrib(3, VertexAttribPointerType.Float)]
        public VertexAttrib Position { get; protected set; }

        public StructUniform<GridLineInfo> MajorGridLine { get; protected set; }

        public StructUniform<GridLineInfo> MinorGridLine { get; protected set; }

        public Uniform<float> FadeDistance { get; protected set; }

        public Uniform<Matrix4> MVMatrix { get; protected set; }

        public Uniform<Matrix4> PMatrix { get; protected set; }

        public struct GridLineInfo
        {
            public float Spacing;
            public Color4 Color;
            public float Thickness;
            public bool OffCenter;

            public GridLineInfo(float spacing, Color4 color, float thickness, bool offCenter)
            {
                Spacing = spacing;
                Color = color;
                Thickness = thickness;
                OffCenter = offCenter;
            }
        }
    }
}
