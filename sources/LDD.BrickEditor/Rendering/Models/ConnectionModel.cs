﻿using LDD.BrickEditor.Rendering.Models;
using LDD.Core.Primitives.Connectors;
using LDD.Modding;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using QuickFont;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LDD.BrickEditor.Rendering
{
    public class ConnectionModel : PartElementModel
    {
        public PartConnection Connection { get; set; }

        public PartialModel RenderingModel { get; set; }

        public Matrix4 ModelTransform { get; set; }

        public Matrix4 ParentTransform { get; set; }

        private bool _DisplayInvertedGender;


        public bool DisplayInvertedGender
        {
            get => _DisplayInvertedGender;
            set
            {
                if (value != _DisplayInvertedGender)
                {
                    _DisplayInvertedGender = value;
                    UpdateRenderingModel();
                }
            }
        }

        public ConnectionModel(PartConnection connection) : base (connection)
        {
            Connection = connection;

            SetTransformFromElement();

            UpdateRenderingModel();

            if (connection.Connector is Custom2DFieldConnector studConn)
            {
                studConn.SizeChanged += StudConn_SizeChanged;
            }
            connection.Connector.PropertyChanged += Connector_PropertyChanged;
        }

        

        private void StudConn_SizeChanged(object sender, EventArgs e)
        {
            StudTextDirty = true;
            UpdateRenderingModel();
        }

        protected override void OnElementPropertyChanged(PropertyValueChangedEventArgs e)
        {
            base.OnElementPropertyChanged(e);

            switch (e.PropertyName)
            {
                case nameof(PartElement.ID):
                case nameof(PartElement.Name):
                case nameof(PartConnection.Transform):
                    break;


                default:
                    UpdateRenderingModel();
                    break;
            }
        }

        private void Connector_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Connector.Transform))
                return;

            if (!Connection.IsAssigningConnectorProperties)
                UpdateRenderingModel();
        }

        private void UpdateRenderingModel()
        {
            RenderingModel = null;
            ModelTransform = Matrix4.Identity;
            BoundingBox = BBox.Empty;
            ModelMaterial = RenderHelper.ConnectionMaterial;

            if (Connection.SubType < 1000)
            {
                if (Connection.SubType % 2 == 1)
                    ModelMaterial = RenderHelper.MaleConnectorMaterial;
                else
                    ModelMaterial = RenderHelper.FemaleConnectorMaterial;
            }

            if (Connection.Connector is AxelConnector axelConnector)
            {
                CreateAxleModel(axelConnector);
            }
            else if (Connection.Connector is Custom2DFieldConnector custom2DField)
            {
                var studsSize = new Vector3(custom2DField.StudWidth * 0.8f, 0.2f, custom2DField.StudHeight * 0.8f);
                BoundingBox = BBox.FromCenterSize(studsSize * new Vector3(0.5f,0,0.5f), studsSize);
            }
            else if (Connection.Connector is BallConnector ball)
            {
                //var studsSize = new Vector3(custom2DField.StudWidth * 0.8f, 0.2f, custom2DField.StudHeight * 0.8f);
                //BoundingBox = BBox.FromCenterSize(studsSize * new Vector3(0.5f, 0, 0.5f), studsSize);
                float ballSize = 0f;
                switch (ball.SubType)
                {
                    case 2:
                    case 3:
                        ballSize = 0.64f;
                        break;
                    case 4:
                    case 5:
                        ballSize = 1f;
                        break;
                    case 6:
                    case 7:
                        ballSize = 0.22f;
                        break;
                    case 14:
                    case 15:
                        ballSize = 1.96f;
                        break;
                    case 16:
                    case 17:
                        ballSize = 1.65f;
                        break;
                    case 18:
                    case 19:
                        ballSize = 2.35f;
                        break;
                }

                if (ballSize != 0f)
                {
                    RenderingModel = ModelManager.SphereModel;
                    ModelTransform = Matrix4.CreateScale(ballSize);
                }
            }

            if (RenderingModel != null)
            {
                var vertices = RenderingModel.BoundingBox.GetCorners();
                vertices = vertices.Select(x => Vector3.TransformPosition(x, ModelTransform)).ToArray();
                BoundingBox = BBox.FromVertices(vertices);
            }
        }

        private void CreateAxleModel(AxelConnector axelConnector)
        {
            if (axelConnector.Length > 0)
            {
                int renderType = axelConnector.SubType;
                if (DisplayInvertedGender && renderType < 1000)
                    renderType += (renderType % 2 == 0) ? 1 : -1;

                switch (renderType)
                {
                    case 2:
                        RenderingModel = ModelManager.TechnicPinFemaleModel;
                        ModelTransform = Matrix4.CreateScale(1f, axelConnector.Length, 1f);
                        break;

                    case 3:
                        RenderingModel = ModelManager.CylinderModel;
                        ModelTransform = Matrix4.CreateScale(0.48f, axelConnector.Length, 0.48f);
                        break;

                    case 4:
                        RenderingModel = ModelManager.CrossAxleFemaleModel;
                        ModelTransform = Matrix4.CreateScale(1f, axelConnector.Length, 1f);
                        break;

                    case 5:
                        RenderingModel = ModelManager.CrossAxleMaleModel;
                        ModelTransform = Matrix4.CreateScale(1f, axelConnector.Length, 1f);
                        break;

                    case 6:
                        RenderingModel = ModelManager.BarFemaleModel;
                        ModelTransform = Matrix4.CreateScale(1f, axelConnector.Length, 1f);
                        break;

                    case 7:
                        RenderingModel = ModelManager.CylinderModel;
                        ModelTransform = Matrix4.CreateScale(0.32f, axelConnector.Length, 0.32f);
                        break;

                    case 14:
                        RenderingModel = ModelManager.BarFemaleModel;
                        ModelTransform = Matrix4.CreateScale(0.46875f, axelConnector.Length, 0.46875f);
                        break;

                    case 15:
                        RenderingModel = ModelManager.CylinderModel;
                        ModelTransform = Matrix4.CreateScale(0.15f, axelConnector.Length, 0.15f);
                        break;
                }
                
            }
        }

        public override void RenderModel(Camera camera, MeshRenderMode mode = MeshRenderMode.Solid)
        {
            switch (Connection.ConnectorType)
            {
                case ConnectorType.Axel:
                    RenderTechnicAxle(Connection.GetConnector<AxelConnector>());
                    break;
                case ConnectorType.Ball:
                    break;
                case ConnectorType.Custom2DField:
                    RenderCustom2DField(Connection.GetConnector<Custom2DFieldConnector>());
                    break;
                case ConnectorType.Fixed:
                    break;
                case ConnectorType.Gear:
                    break;
                case ConnectorType.Hinge:
                    break;
                case ConnectorType.Rail:
                    break;
                case ConnectorType.Slider:
                    break;
            }

            if (RenderingModel != null)
            {
                var finalTransform = ModelTransform * Transform;

                RenderHelper.RenderWithStencil(
                    () =>
                    {
                        RenderHelper.BeginDrawModel(RenderingModel, finalTransform, ModelMaterial);

                        RenderHelper.ModelShader.IsSelected.Set(IsSelected);
                        RenderingModel.DrawElements();

                        RenderHelper.EndDrawModel(RenderingModel);
                    },
                    () =>
                    {
                        var wireColor = IsSelected ? RenderHelper.SelectionOutlineColor : RenderHelper.WireframeColor;
                        RenderHelper.BeginDrawWireframe(RenderingModel.VertexBuffer, finalTransform, 
                            IsSelected ? 4f : 2f, wireColor);

                        RenderingModel.DrawElements();

                        RenderHelper.EndDrawWireframe(RenderingModel.VertexBuffer);
                    });

                if (IsSelected && !BoundingBox.IsEmpty)
                {
                    var selectionBox = BoundingBox;
                    selectionBox.Size += new Vector3(0.1f);
                    RenderHelper.DrawBoundingBox(Transform,
                        selectionBox,
                        new Vector4(0f, 1f, 1f, 1f), 1.5f);
                }
            }
            else
            {
                RenderHelper.DrawGizmoAxes(Transform, 0.5f, IsSelected);
            }
        }

        private QFontDrawingPrimitive StudText;
        private bool StudTextDirty;

        private void RenderCustom2DField(Custom2DFieldConnector connector)
        {
            RenderHelper.DrawStudConnectorGrid(Transform, connector);

            var color = IsSelected ? new Vector4(1f) : new Vector4(0, 0, 0, 1);
            RenderHelper.DrawRectangle(Transform,
                new Vector2(connector.StudWidth * 0.8f, connector.StudHeight * 0.8f),
                color, 3f);
            
            using (OpenTKHelper.TempEnable(EnableCap.Texture2D))
            {
                RebuildStudText(connector);

                if (!UIRenderHelper.TextRenderer.DrawingPrimitives.Contains(StudText))
                    UIRenderHelper.TextRenderer.DrawingPrimitives.Add(StudText);
            }
        }

        private void RebuildStudText(Custom2DFieldConnector connector)
        {
            float textScale = 0.007f;
            float depthOffset = connector.SubType == 23 ? 0.0005f : -0.0005f;
            depthOffset /= textScale;

            if (StudText == null || StudTextDirty)
                StudText = RenderHelper.Create3DTextPrimitive(UIRenderHelper.MonoFont, Transform, Color.Black, textScale);
            else
            {
                StudText.ModelViewMatrix = RenderHelper.Get3dTextMatrix(Transform, textScale);
                return;
            }
            
            for (int y = 0; y < connector.ArrayHeight; y++)
            {
                for (int x = 0; x < connector.ArrayWidth; x++)
                {
                    float curPosX = 0.4f * x;
                    float curPosY = (0.4f * y) * -1f;
                    var hAling = StringAlignment.Center;
                    var vAling = StringAlignment.Center;

                    if (x == 0)
                        curPosX += 0.08f;
                    else if (x == connector.ArrayWidth - 1)
                        curPosX -= 0.08f;
                    if (y == 0)
                        curPosY -= 0.08f;
                    else if (y == connector.ArrayHeight - 1)
                        curPosY += 0.08f;
                    StudText.AddText($"{x},{y}", new Vector2(curPosX / textScale, curPosY / textScale),
                        vAling, hAling, depthOffset);
                }
            }

            StudTextDirty = false;
        }   

        private void RenderTechnicAxle(AxelConnector axel)
        {
            if (RenderingModel == null && axel.Length > 0)
            {
                RenderHelper.DrawLine(Transform, RenderHelper.DefaultAxisColors[1],
                    Vector3.Zero, Vector3.UnitY * axel.Length, 2f);
            }
        }

        public override bool RayIntersectsBoundingBox(Ray ray, out float distance)
        {
            if (BoundingBox.IsEmpty)
            {
                var bsphere = new BSphere(Origin, 0.5f);
                return Ray.IntersectsSphere(ray, bsphere, out distance);
            }

            return base.RayIntersectsBoundingBox(ray, out distance);
        }

        public override bool RayIntersects(Ray ray, out float distance)
        {
            distance = float.NaN;

            if (RenderingModel != null)
            {
                var modelRay = Ray.Transform(ray, Transform.Inverted());
                return RenderingModel.RayIntersects(modelRay, ModelTransform, out distance);
                //return RayIntersectsBoundingBox(ray, out distance);
            }

            var localRay = Ray.Transform(ray, Transform.Inverted());

            if (Connection.ConnectorType == ConnectorType.Custom2DField)
            {
                var studConnector = Connection.GetConnector<Custom2DFieldConnector>();
                if (Ray.IntersectsPlane(localRay, new Plane(Vector3.Zero, Vector3.UnitY, 0f), out distance))
                {
                    var hitPos = localRay.Origin + localRay.Direction * distance;
                    if (hitPos.X < 0 || hitPos.X > studConnector.StudWidth * 0.8f
                        || hitPos.Z < 0 || hitPos.Z > studConnector.StudHeight * 0.8f)
                    {
                        return false;
                    }
                    return true;
                }
            }

            for (int i = 0; i < 3; i++)
            {
                var axe = new Vector3(0.08f);
                axe[i] = 0.5f;
                var center = Vector3.Zero;
                center[i] = axe[i] / 2f;
                var axeBox = BBox.FromCenterSize(center, axe);
                if (Ray.IntersectsBox(localRay, axeBox, out float hitDist))
                    distance = float.IsNaN(distance) ? hitDist : Math.Min(hitDist, distance);
            }

            return !float.IsNaN(distance);

            //var bsphere = new BSphere(Vector3.Zero, 0.5f);
            //return Ray.IntersectsSphere(localRay, bsphere, out distance);

        }

    }
}
