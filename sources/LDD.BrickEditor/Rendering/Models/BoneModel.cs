﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LDD.BrickEditor.ProjectHandling;
using LDD.Modding;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace LDD.BrickEditor.Rendering
{
    public class BoneModel : PartElementModel
    {
        public PartBone Bone => Element as PartBone;

        private BBox BoneBounding { get; set; }

        private Matrix4 SphereTransform;
        private Matrix4 ConeTransform;
        private Matrix4 ContourTransform;

        public float BoneLength { get; set; }
        public bool IsLengthDirty { get; set; }

        public BoneModel(PartBone element) : base(element)
        {
            UpdateBoundingBox();
            SetTransformFromElement();
            BoundingBox = BBox.FromCenterSize(Vector3.Zero, Vector3.One);
            BoneLength = 0.4f;
            UpdateModelTransforms();
            CalculateBoneLength();
        }

        protected override bool? VisibleOverride()
        {
            var extender = Element?.GetExtension<BoneElementExtension>();
            if (extender != null)
                return extender.IsVisible && extender.Manager.ShowBones;
            return base.VisibleOverride();
        }

        private void UpdateBoundingBox()
        {
            if (Bone.Bounding != null && !Bone.Bounding.IsEmpty)
            {
                BoneBounding = BBox.FromCenterSize(
                    (Vector3)Bone.Bounding.Center.ToGL(),
                    (Vector3)Bone.Bounding.Size.ToGL());
            }
            else
            {
                BoneBounding = BBox.Empty;
            }
        }

        protected override void OnTransformChanged()
        {
            base.OnTransformChanged();
            IsLengthDirty = true;
        }

        public void UpdateModelTransforms()
        {
            SphereTransform = Matrix4.CreateScale(0.2f);
            float coneScale = (BoneLength - 0.05f);
            ConeTransform = Matrix4.CreateScale(0.4f, coneScale, 0.4f) *
                Matrix4.CreateTranslation(0, 0.05f + (coneScale /2f), 0) *
                Matrix4.CreateRotationZ((float)Math.PI * -0.5f);
            ContourTransform = Matrix4.CreateScale(0.4f, 1f, 0.4f) *
                Matrix4.CreateTranslation(0, 0.05f, 0) *
                Matrix4.CreateRotationZ((float)Math.PI * -0.5f);
        }

        public void CalculateBoneLength()
        {
            var newLength = 0.4f;
            var target = Bone.GetLinkedBone();

            if (target != null)
                newLength = (float)(target.Transform.Position - Bone.Transform.Position).Length;

            if (BoneLength != newLength)
            {
                BoneLength = newLength;
                UpdateModelTransforms();
            }

            BoundingBox = BBox.FromCenterSize(
                new Vector3((newLength / 2f) - 0.1f, 0, 0), 
                new Vector3(newLength + 0.2f, 0.4f, 0.4f)
                );
            IsLengthDirty = false;
        }

        protected override void OnElementPropertyChanged(PropertyValueChangedEventArgs e)
        {
            base.OnElementPropertyChanged(e);
            if (e.PropertyName == nameof(PartBone.Bounding))
                UpdateBoundingBox();
            else if (e.PropertyName == nameof(PartBone.TargetBoneID))
                IsLengthDirty = true;
        }

        public override bool RayIntersects(Ray ray, out float distance)
        {
            distance = float.MaxValue;
            if (ModelManager.SphereModel.RayIntersects(ray, SphereTransform * Transform, out float result))
                distance = result;
            if (ModelManager.ConeModel.RayIntersects(ray, ConeTransform * Transform, out result))
                distance = Math.Min(distance, result);
            return distance != float.MaxValue;

            //return RayIntersectsGizmo(ray, 1f, 0.1f, out distance);
            //distance = float.NaN;
            //return false;
            //var bsphere = new BSphere(Origin, 0.5f);
            //return Ray.IntersectsSphere(ray, bsphere, out distance);
            //return RayIntersectsBoundingBox(ray, out distance);
        }

        public override void RenderModel(Camera camera, MeshRenderMode mode = MeshRenderMode.Solid)
        {
            var modelColor = new Vector4(1f, 0.6f, 0.1f, 0.5f);
            var wireColor = IsSelected ? RenderHelper.SelectionOutlineColor : RenderHelper.WireframeColor;
            var wireThickness = IsSelected ? 4f : 2f;
            GL.Disable(EnableCap.CullFace);
            GL.Disable(EnableCap.DepthTest);
            ModelManager.SphereModel.DrawOutlined(SphereTransform * Transform, modelColor, wireColor, wireThickness);
            ModelManager.ConeModel.DrawOutlined(ConeTransform * Transform, modelColor, wireColor, wireThickness);
            ModelManager.CircleModel.DrawColored(ContourTransform * Transform, wireColor);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);

            if (!BoneBounding.IsEmpty && IsSelected)
            {
                RenderHelper.DrawBoundingBox(Transform, BoneBounding,
                        new Vector4(0f, 1f, 1f, 1f), IsSelected ? 1.5f : 1f);
            }
        }

        public override void RenderUI(Camera camera)
        {
            if (ModelExtension?.Manager.ShowBones != true)
                return;

            var rootPos = Transform.ExtractTranslation();
            var tipPos = rootPos + Vector3.TransformVector(new Vector3(BoneLength, 0, 0), Transform);
            var boneCenter = (rootPos + tipPos) / 2f;
            var screenPos = camera.WorldPointToScreen(boneCenter);
            var boneBounds = new Vector4(screenPos.X - 30, screenPos.Y - 20, 60, 40);

            UIRenderHelper.DrawShadowText(Element.Name,
                UIRenderHelper.NormalFont, Color.White, boneBounds,
                StringAlignment.Center, StringAlignment.Center);
        }
    }
}
