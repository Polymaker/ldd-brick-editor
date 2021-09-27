using LDD.Core.Primitives.Connectors;
using LDD.Modding;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LDD.BrickEditor.Rendering.Models
{
    public class StudRefModel : ModelBase
    {
        private StudReference _Stud;
        public StudReference Stud
        {
            get => _Stud;
            set => SetStud(value);
        }

        private Vector4 StudBounds { get; set; }

        public StudRefModel()
        {
            IsUserTransformable = false;
        }

        public override bool RayIntersects(Ray ray, out float distance)
        {
            if (Stud != null)
            {
                var conn = Stud.Connection;
                var connTrans = GetBaseTransform(conn);

                var localRay = Ray.Transform(ray, connTrans.Inverted());

                if (Ray.IntersectsPlane(localRay, new Plane(Vector3.Zero, Vector3.UnitY, 0f), out distance))
                {
                    var hitPos = localRay.Origin + localRay.Direction * distance;
                    if (hitPos.X < StudBounds.X || hitPos.X > StudBounds.X + StudBounds.Z
                        || hitPos.Z < StudBounds.Y || hitPos.Z > StudBounds.Y + StudBounds.W)
                    {
                        return false;
                    }
                    return true;
                }
            }

            distance = 0;
            return false;
        }

        private void SetStud(StudReference studReference)
        {
            if (_Stud != null)
                _Stud.PropertyValueChanged -= StudReference_PropertyValueChanged;
            _Stud = studReference;

            if (studReference != null)
            {
                StudBounds = GetStudCellBounds(studReference);
                studReference.PropertyValueChanged += StudReference_PropertyValueChanged;

                var studsSize = new Vector3(StudBounds.X, 0.2f, StudBounds.Y);
                BoundingBox = BBox.FromCenterSize(studsSize * new Vector3(0.5f, 0, 0.5f), studsSize);
            }
            else
            {
                StudBounds = Vector4.Zero;
            }
        }

        private void StudReference_PropertyValueChanged(object sender, System.ComponentModel.PropertyValueChangedEventArgs e)
        {
            if (e.PropertyName == nameof(StudReference.PositionX) || e.PropertyName == nameof(StudReference.PositionY))
                StudBounds = GetStudCellBounds(Stud);
        }

        public override void RenderModel(Camera camera, MeshRenderMode mode = MeshRenderMode.Solid)
        {
            if (Stud != null)
            {
                var conn = Stud.Connection;

                var connTrans = GetBaseTransform(conn);

                Matrix4 offsetTrans = Matrix4.CreateTranslation(StudBounds.X, 0, StudBounds.Y);

                var finalTrans = offsetTrans * connTrans;

                RenderHelper.FillRectangle(finalTrans, new Vector2(StudBounds.Z, StudBounds.W), new Vector4(0f, 0f, 1f, 0.4f), 3f);
                RenderHelper.DrawRectangle(finalTrans,  new Vector2(StudBounds.Z, StudBounds.W),  new Vector4(0.1f, 0.1f, 1,1), 3f);
            }
            base.RenderModel(camera, mode);
        }

        private Vector4 GetStudCellBounds(StudReference stud)
        {
            var studConn = stud.Connector;

            float cellWidth = (stud.PositionX % 2 == 1) ? 0.48f : 0.32f;
            float cellHeight = (stud.PositionY % 2 == 1) ? 0.48f : 0.32f;

            float offsetX = stud.PositionX * 0.4f;
            float offsetY = stud.PositionY * 0.4f;

            if (Stud.PositionX > 0)
                offsetX -= cellWidth / 2f;

            if (Stud.PositionY > 0)
                offsetY -= cellHeight / 2f;

            if (stud.PositionX == 0 || stud.PositionX == studConn.ArrayWidth - 1)
                cellWidth /= 2f;
            if (stud.PositionY == 0 || stud.PositionY == studConn.ArrayHeight - 1)
                cellHeight /= 2f;

            return new Vector4(offsetX, offsetY, cellWidth, cellHeight);
        }

        private Matrix4 GetBaseTransform(PartConnection connection)
        {
            if (connection is IPhysicalElement physicalElement)
            {
                var baseTransform = physicalElement.Transform.ToMatrixD().ToGL();

                if (connection.Parent is IPhysicalElement parentElem)
                {
                    var parentTransform = parentElem.Transform.ToMatrixD().ToGL();
                    baseTransform = baseTransform * parentTransform;
                }

                return baseTransform.ToMatrix4();
            }
            return Matrix4.Identity;
        }

        protected override void OnSelectedChanged()
        {
            base.OnSelectedChanged();
            if (!IsSelected && Visible)
                Visible = false;
        }
    }
}
