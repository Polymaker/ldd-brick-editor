﻿using LDD.Modding;
using OpenTK;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace LDD.BrickEditor.Rendering.Models
{
    public class ClonePatternModel : PartElementModel
    {
        public ClonePattern ClonePattern => Element as ClonePattern;
        public List<PartElementModel> ElementModels { get; set; }
        public bool ElementModelsDirty { get; private set; }

        private List<Vector3> RepetitionCenters;

        public ClonePatternModel(ClonePattern clonePattern) : base(clonePattern)
        {
            ClonePattern.PropertyValueChanged += ClonePattern_PropertyChanged;
            ElementModels = new List<PartElementModel>();
            ClonePattern.Elements.CollectionChanged += Elements_CollectionChanged;
            ElementModelsDirty = true;
            RepetitionCenters = new List<Vector3>();

            SetTransformFromElement();

            if (AltColors == null)
                GenerateAltColors();

            HasTransparency = true;
        }

        private void ClonePattern_PropertyChanged(object sender, PropertyValueChangedEventArgs e)
        {
            if (e.PropertyName == nameof(LinearPattern.Direction) ||
                e.PropertyName == nameof(MirrorPattern.Normal) ||
                e.PropertyName == nameof(CircularPattern.Axis) ||
                e.PropertyName == nameof(CircularPattern.Origin))
            {
                SetTransformFromElement();
            }
            else if(e.PropertyName == nameof(CircularPattern.Repetitions))
            {
                RecalculateBoundingBox();
            }
        }

        static Vector4[] AltColors { get; set; }

        private static void GenerateAltColors()
        {
            AltColors = new Vector4[16];

            for (int i = 0; i < 16; i++)
            {
                var color = OpenTK.Graphics.Color4.FromHsv(new Vector4(i / 15f, 1f, 1f, 1f));
                AltColors[i] = new Vector4(color.R, color.G, color.B, color.A);
            }
        }

        protected override Matrix4 GetElementTransform()
        {
            return ((LDD.Common.Simple3D.Matrix4)ClonePattern.GetPatternMatrix()).ToGL();
        }

        private void RecalculateBoundingBox()
        {
            BoundingBox = BBox.Empty;

            if (ElementModels == null || ElementModels.Count == 0)
                return;

            var allBBox = new List<BBox>();

            foreach (var elementRef in ClonePattern.Elements)
            {
                var elemModel = ElementModels.FirstOrDefault(x => x.Element.ID == elementRef.ElementID);
                if (elemModel == null)
                    continue;

                var originalTrans = elemModel.Transform;
                var baseTransform = ItemTransform.FromMatrix(elemModel.Transform.ToLDD());

                for (int i = 0; i <= ClonePattern.NumberOfInstances; i++)
                {
                    //if (ClonePattern.SkippedInstances.Contains(i))
                    //    continue;

                    var trans = ClonePattern.ApplyTransform(baseTransform, i).ToMatrix().ToGL();

                    elemModel.SetTemporaryTransform(trans);
                    var localBbox = elemModel.GetWorldBoundingBox();
                    allBBox.Add(BBox.Transform(Transform.Inverted(), localBbox));
                    elemModel.SetTemporaryTransform(null);
                }
            }

            if (allBBox.Count > 0)
            {
                BoundingBox = BBox.Combine(allBBox);
                //var worldBox = BBox.Combine(allBBox);
                //BoundingBox = BBox.Transform(Transform.Inverted(), worldBox);
            }
        }

        protected override void ApplyTransformToElement(Matrix4 transform)
        {
            base.ApplyTransformToElement(transform);

            var axis = Vector3.TransformVector(Vector3.UnitZ, transform);

            if (ClonePattern is CircularPattern circular)
            {
                circular.Axis = axis.ToLDD();
                circular.Origin = transform.ExtractTranslation().ToLDD();

            }
            else if (ClonePattern is MirrorPattern mirror)
            {
                mirror.Normal = axis.ToLDD();
                mirror.Origin = transform.ExtractTranslation().ToLDD();
            }
            else if (ClonePattern is LinearPattern linear)
            {
                linear.Direction = axis.ToLDD();
            }

            SetTransform(GetElementTransform(), false);
        }

        public override void ApplyTransform(Matrix4 transform)
        {
            base.ApplyTransform(transform);
        }

        protected override void OnTransformChanged()
        {
            base.OnTransformChanged();
            RecalculateBoundingBox();
        }

        private void Elements_CollectionChanged(object sender, CollectionChangedEventArgs e)
        {
            ElementModelsDirty = true;
            
        }

        public void SetElementModels(IEnumerable<PartElementModel> models)
        {
            ElementModels = models.ToList();
            ElementModelsDirty = false;
            RecalculateBoundingBox();
        }

        public override bool RayIntersects(Ray ray, out float distance)
        {
            distance = float.NaN;
            return false;
        }

        public override void RenderModel(Camera camera, MeshRenderMode mode = MeshRenderMode.Solid)
        {
            if (ClonePattern?.NumberOfInstances == 0 || ElementModels == null || ElementModels.Count == 0)
                return;

            
            if (ClonePattern is RepetitionPattern repetitionPattern)
                RenderRepetitionPattern(repetitionPattern, camera, mode);
            else if (ClonePattern is MirrorPattern mirrorPattern)
                RenderMirrorPattern(mirrorPattern, camera, mode);

            RenderCloneGizmo(ClonePattern, camera, mode);

            //if (IsSelected && !BoundingBox.IsEmpty && !IsEditingTransform)
            //{
            //    var selectionBox = BoundingBox;
            //    selectionBox.Size += new Vector3(0.1f);
            //    RenderHelper.DrawBoundingBox(Transform,
            //        selectionBox,
            //        new Vector4(0f, 1f, 1f, 1f), 1.5f);
            //}
        }

        private void RenderCloneGizmo(ClonePattern pattern, Camera camera, MeshRenderMode mode = MeshRenderMode.Solid)
        {
            GL.DepthMask(false);

            if (pattern is LinearPattern linearPattern)
            {
                float lineLength = (linearPattern.Repetitions) * linearPattern.Offset;
                //var dir = linearPattern.Direction.ToGL();
                var offsetTrans = ElementModels.FirstOrDefault()?.Transform ?? Matrix4.Identity;
                offsetTrans = Matrix4.CreateTranslation(offsetTrans.ExtractTranslation());
                RenderHelper.DrawLine(Transform * offsetTrans, new Vector4(1f, 1f, 0f, 1f), Vector3.Zero, Vector3.UnitZ * lineLength, 2f);
            }
            else if (pattern is CircularPattern circularPattern)
            {
                RenderHelper.DrawLine(Transform, new Vector4(1f, 1f, 0f, 1f), Vector3.UnitZ * -5f, Vector3.UnitZ * 5f, 2f);
            }
            else if (pattern is MirrorPattern mirrorPattern)
            {
                var rectBounds = new Vector4(-0.5f, -0.5f, 0.5f, 0.5f);
                if (!BoundingBox.IsEmpty)
                    rectBounds = new Vector4(BoundingBox.Left, BoundingBox.Top * -1f, BoundingBox.Right, BoundingBox.Bottom * -1f);

                Vector3 rectColor = IsSelected ? new Vector3(0.5f, 0.5f, 0.7f) : new Vector3(0.6f);

                

                RenderHelper.FillRectangle(Transform, rectBounds, Vector3.UnitZ, new Vector4(rectColor, 0.5f));
                RenderHelper.DrawRectangle(Transform, rectBounds, Vector3.UnitZ, new Vector4(rectColor, 1f));
                //RenderHelper.FillRectangle(Transform, new Vector2(2, 2), new Vector4(0.5f));
                //RenderHelper.DrawLine(Transform, new Vector4(1f, 1f, 0f, 1f), Vector3.UnitZ * -5f, Vector3.UnitZ * 5f, 2f);
            }

            GL.DepthMask(true);
        }

        private void RenderRepetitionPattern(RepetitionPattern pattern, Camera camera, MeshRenderMode mode = MeshRenderMode.Solid)
        {
            RepetitionCenters.Clear();

            for (int i = 0; i < ClonePattern.NumberOfInstances; i++)
                RepetitionCenters.Add(Vector3.Zero);

            int visibleElements = 0;

            foreach (var elementRef in pattern.Elements)
            {
                var elemModel = ElementModels.FirstOrDefault(x => x.Element.ID == elementRef.ElementID);
                if (elemModel == null)
                    continue;

                visibleElements++;

                var baseTransform = ItemTransform.FromMatrix(elemModel.Transform.ToLDD());
                bool isSelected = elemModel.IsSelected;
                var originalMaterial = elemModel.ModelMaterial;

                for (int i = 0; i <= pattern.Repetitions; i++)
                {
                    if (i == 0 && elemModel.Visible)
                        continue;

                    if (pattern.SkippedInstances.Contains(i))
                        continue;

                    RenderRepetitionPatternInstance(pattern, i, baseTransform, elemModel, camera, mode);
                }

                elemModel.IsSelected = isSelected;
                elemModel.ModelMaterial = originalMaterial;
            }

            if (visibleElements > 0)
            {
                for (int i = 0; i < pattern.Repetitions; i++)
                {
                    if (RepetitionCenters[i] != Vector3.Zero)
                        RepetitionCenters[i] /= (float)visibleElements;
                }

            }
            else
                RepetitionCenters.Clear();
        }

        private void RenderRepetitionPatternInstance(RepetitionPattern pattern, int instance, ItemTransform baseTransform, PartElementModel elemModel, Camera camera, MeshRenderMode mode = MeshRenderMode.Solid)
        {
            var trans = pattern.GetInstanceTransform(Transform.ToMatrix4d().ToLDD(), baseTransform, instance).ToMatrix().ToGL();

            if (instance > 0)
                RepetitionCenters[instance - 1] += trans.ExtractTranslation();

            bool renderTransparent = !elemModel.Visible;
            var modelMat = GetInstanceMaterial(elemModel.ModelMaterial, instance, renderTransparent);

            if (renderTransparent)
                GL.DepthMask(false);

            elemModel.ModelMaterial = modelMat;
            elemModel.IsSelected = IsSelected;
            elemModel.SetTemporaryTransform(trans);
            elemModel.RenderModel(camera, mode);
            elemModel.SetTemporaryTransform(null);

            if (renderTransparent)
                GL.DepthMask(true);
        }

        

        private void RenderMirrorPattern(MirrorPattern pattern, Camera camera, MeshRenderMode mode = MeshRenderMode.Solid)
        {
            var patternTransform = Transform.ToMatrix4d().ToLDD();
            if (IsEditingTransform)
                patternTransform = pattern.QuantizeTransform(patternTransform);

            foreach (var elementRef in pattern.Elements)
            {
                var elemModel = ElementModels.FirstOrDefault(x => x.Element.ID == elementRef.ElementID);
                if (elemModel == null)
                    continue;

                var baseTransform = ItemTransform.FromMatrix(elemModel.Transform.ToLDD());
                bool isSelected = elemModel.IsSelected;
                var originalMaterial = elemModel.ModelMaterial;

                for (int i = 0; i <= pattern.NumberOfInstances; i++)
                {
                    if (i == 0 && elemModel.Visible)
                        continue;
                    
                    var trans = pattern.GetInstanceTransform(patternTransform, baseTransform, i).ToMatrix().ToGL();

                    bool renderTransparent = !elemModel.Visible;
                    var modelMat = GetInstanceMaterial(elemModel.ModelMaterial, i, renderTransparent);

                    if (renderTransparent)
                        GL.DepthMask(false);

                    elemModel.ModelMaterial = modelMat;
                    elemModel.IsSelected = IsSelected;
                    elemModel.SetTemporaryTransform(trans);
                    elemModel.RenderModel(camera, mode);
                    elemModel.SetTemporaryTransform(null);

                    if (renderTransparent)
                        GL.DepthMask(true);
                }
                

                elemModel.IsSelected = isSelected;
                elemModel.ModelMaterial = originalMaterial;
            }
        }

        private MaterialInfo GetInstanceMaterial(MaterialInfo baseMaterial, int instance, bool renderTransparent)
        {
            var newMaterial = baseMaterial;
            newMaterial.Diffuse = Vector4.ComponentMin((newMaterial.Diffuse * 1.15f) + new Vector4(0.08f), Vector4.One);
            if (renderTransparent)
                newMaterial.Diffuse = new Vector4(newMaterial.Diffuse.Xyz, 0.6f);
            return newMaterial;
        }

        public override void RenderUI(Camera camera)
        {
            base.RenderUI(camera);

            if (!IsSelected)
                return;

            if (RepetitionCenters.Count > 0 && IsSelected)
            {
                for (int i = 0; i < RepetitionCenters.Count; i++)
                {
                    var screenPos = camera.WorldPointToScreen(RepetitionCenters[i]);
                    screenPos.X = (float)Math.Round(screenPos.X);
                    screenPos.Y = (float)Math.Round(screenPos.Y);
                    var boneBounds = new Vector4(screenPos.X - 20, screenPos.Y - 20, 40, 40);

                    UIRenderHelper.DrawShadowText((i + 1).ToString(),
                        UIRenderHelper.NormalFont, Color.White, boneBounds,
                        StringAlignment.Center, StringAlignment.Center);
                }
            }

            //foreach (var elementRef in ClonePattern.Elements)
            //{
            //    var elemModel = ElementModels.FirstOrDefault(x => x.Element.ID == elementRef.ElementID);
            //    if (elemModel != null && elemModel.Visible)
            //    {
            //        var originalTrans = elemModel.Transform;
            //        var baseTransform = ItemTransform.FromMatrix(elemModel.Transform.ToLDD());

            //        for (int i = 1; i <= ClonePattern.Repetitions; i++)
            //        {
            //            if (ClonePattern.SkippedInstances.Contains(i))
            //                continue;

            //            var trans = ClonePattern.ApplyTransform(baseTransform, i).ToMatrix().ToGL();
            //            var transCenter = trans.ExtractTranslation();
            //            var screenPos = camera.WorldPointToScreen(transCenter);
            //            var boneBounds = new Vector4(screenPos.X - 20, screenPos.Y - 20, 40, 40);

            //            UIRenderHelper.DrawShadowText(i.ToString(),
            //                UIRenderHelper.NormalFont, Color.White, boneBounds,
            //                StringAlignment.Center, StringAlignment.Center);
            //        }
            //    }
            //}
        }
    }
}
