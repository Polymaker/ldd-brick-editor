﻿using LDD.BrickEditor.Rendering.Models;
using LDD.BrickEditor.Resources;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LDD.BrickEditor.Rendering
{
    static class ModelManager
    {
        public static IndexedVertexBuffer<VertVN> GeneralMeshBuffer;

        public static IndexedVertexBuffer<Vector3> BoundingBoxBufffer;

        public static PartialModel CubeModel { get; private set; }

        public static PartialModel ConeModel { get; private set; }

        public static PartialModel SphereModel { get; private set; }

        public static PartialModel CrossAxleMaleModel { get; private set; }

        public static PartialModel CrossAxleFemaleModel { get; private set; }

        public static PartialModel TechnicPinFemaleModel { get; private set; }

        public static PartialModel BarFemaleModel { get; private set; }

        public static PartialModel CylinderModel { get; private set; }

        public static PartialModel CircleModel { get; private set; }

        public static List<PartialModel> LoadedModels { get; private set; }

        static ModelManager()
        {
            LoadedModels = new List<PartialModel>();
        }

        public static void InitializeResources()
        {
            GeneralMeshBuffer = new IndexedVertexBuffer<VertVN>();

            var loadedMesh = ResourceHelper.GetResourceModel("Models.Cube.obj", "obj").Meshes[0];
            CubeModel = AppendPartialMesh(loadedMesh);

            loadedMesh = ResourceHelper.GetResourceModel("Models.Cone.obj", "obj").Meshes[0];
            ConeModel = AppendPartialMesh(loadedMesh);

            loadedMesh = ResourceHelper.GetResourceModel("Models.Sphere.obj", "obj").Meshes[0];
            SphereModel = AppendPartialMesh(loadedMesh);

            loadedMesh = ResourceHelper.GetResourceModel("Models.Cylinder.obj", "obj").Meshes[0];
            CylinderModel = AppendPartialMesh(loadedMesh);

            loadedMesh = ResourceHelper.GetResourceModel("Models.CrossAxleMale.obj", "obj").Meshes[0];
            CrossAxleMaleModel = AppendPartialMesh(loadedMesh);

            loadedMesh = ResourceHelper.GetResourceModel("Models.CrossAxleFemale.obj", "obj").Meshes[0];
            CrossAxleFemaleModel = AppendPartialMesh(loadedMesh);

            loadedMesh = ResourceHelper.GetResourceModel("Models.TechnicPinFemale.obj", "obj").Meshes[0];
            TechnicPinFemaleModel = AppendPartialMesh(loadedMesh);

            loadedMesh = ResourceHelper.GetResourceModel("Models.BarFemale.obj", "obj").Meshes[0];
            BarFemaleModel = AppendPartialMesh(loadedMesh);

            CircleModel = GenerateCircleModel();

            InitializeBoundingBoxBuffer();
        }

        public static void InitializeBuffers()
        {
            GeneralMeshBuffer.CreateBuffers();
            BoundingBoxBufffer.CreateBuffers();
        }

        private static void InitializeBoundingBoxBuffer()
        {
            BoundingBoxBufffer = new IndexedVertexBuffer<Vector3>();
            var box = BBox.FromCenterSize(Vector3.Zero, Vector3.One);

            BoundingBoxBufffer.SetVertices(box.GetCorners());
            var bboxIndices = new List<int>();

            for (int i = 0; i < 4; i++)
            {
                bboxIndices.Add((i * 2));
                bboxIndices.Add((i * 2) + 1);
                bboxIndices.Add((i * 2));
                bboxIndices.Add(((i + 1) * 2) % 8);
                bboxIndices.Add((i * 2) + 1);
                bboxIndices.Add((((i + 1) * 2) + 1) % 8);
            }

            BoundingBoxBufffer.SetIndices(bboxIndices);
        }

        private static PartialModel AppendPartialMesh(Assimp.Mesh mesh)
        {
            var primitiveType = OpenTK.Graphics.OpenGL.PrimitiveType.Triangles;
            if (mesh.Faces[0].IndexCount == 4)
                primitiveType = OpenTK.Graphics.OpenGL.PrimitiveType.Quads;

            int curIdx = GeneralMeshBuffer.IndexCount;
            int curVert = GeneralMeshBuffer.VertexCount;
            GeneralMeshBuffer.LoadModelVertices(mesh, true);
            int idxCount = GeneralMeshBuffer.IndexCount - curIdx;
            var vertices = GeneralMeshBuffer.GetVertices().Skip(curVert);
            var vertexPositions = vertices.Select(x => x.Position).ToList();

            var bounding = BBox.FromVertices(vertexPositions);

            var model = new PartialModel(GeneralMeshBuffer, curIdx, curVert, idxCount, primitiveType);
            model.BoundingBox = bounding;
            model.Vertices = vertexPositions;
            LoadedModels.Add(model);
            //model.LoadVertices();
            //model.CalculateBoundingBox();
            return model;
        }

        private static PartialModel GenerateCircleModel()
        {
            float stepAngle = (float)Math.PI * 2f / 32f;

            var indices = new List<int>();
            var vertices = new List<VertVN>();

            //Cone vertices and indices
            for (int i = 0; i < 32; i++)
            {
                var pt = new Vector3((float)Math.Cos(stepAngle * i), 0f, (float)Math.Sin(stepAngle * i)) * 0.5f;
                vertices.Add(new VertVN(pt, pt.Normalized()));

                indices.Add((i + 1) % 32); indices.Add(i); //indices.Add(32);
            }

            int curIdx = GeneralMeshBuffer.IndexCount;
            int curVert = GeneralMeshBuffer.VertexCount;

            GeneralMeshBuffer.AppendVertices(vertices);
            GeneralMeshBuffer.AppendIndices(indices);

            int idxCount = GeneralMeshBuffer.IndexCount - curIdx;
            var vertexPositions = vertices.Select(x => x.Position).ToList();

            var bounding = BBox.FromVertices(vertexPositions);

            var model = new PartialModel(GeneralMeshBuffer, curIdx, curVert, idxCount, OpenTK.Graphics.OpenGL.PrimitiveType.Lines);
            model.BoundingBox = bounding;
            model.Vertices = vertexPositions;
            LoadedModels.Add(model);
            return model;
        }

        public static void ReleaseResources()
        {
            if (GeneralMeshBuffer != null)
            {
                GeneralMeshBuffer.Dispose();
                GeneralMeshBuffer = null;
            }

            if (BoundingBoxBufffer != null)
            {
                BoundingBoxBufffer.Dispose();
                BoundingBoxBufffer = null;
            }

            CubeModel = null;
            ConeModel = null;
            SphereModel = null;
            CrossAxleMaleModel = null;
            CrossAxleFemaleModel = null;
            TechnicPinFemaleModel = null;
            BarFemaleModel = null;
            CylinderModel = null;
            CircleModel = null;
            LoadedModels.Clear();
        }
    }
}
