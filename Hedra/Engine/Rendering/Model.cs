/*
 * Created by SharpDevelop.
 * User: Maxi Levi
 * Date: 02/05/2016
 * Time: 01:24 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Collections.Generic;
using System.Reflection;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.ItemSystem.WeaponSystem;
using Hedra.Engine.Management;
using Hedra.Engine.Player;
using OpenTK;

namespace Hedra.Engine.Rendering
{
    /// <summary>
    ///     A class for managing a group of EntityMeshes. Now works as a base class for the HumanModel & QuadrupedModel
    /// </summary>
    [Obsolete]
    public abstract class Model : IDisposable
    {
        private float _alpha = 1;
        private bool _enabled = true;
        private bool _fog;
        private bool _outline;
        private Vector4 _baseTint;
        private Vector3 _localPosition;
        private Vector3 _localRotation;
        private Vector3 _position;
        private Vector3 _rotation;
        private Vector3 _rotationPoint;
        private Vector3 _size;
        private Vector4 _tint;

        public bool Disposed;
        public float Height = 0;

        public ObjectMesh[] Meshes;

        public virtual float Alpha
        {
            get { return _alpha; }
            set
            {
                this.GatherMeshes();
                for (var i = 0; i < Meshes.Length; i++)
                    if (Meshes[i] != null)
                        Meshes[i].Alpha = value;
                _alpha = value;
            }
        }

        public virtual Vector4 BaseTint
        {
            get { return _baseTint; }
            set
            {
                this.GatherMeshes();
                for (var i = 0; i < Meshes.Length; i++)
                    if (Meshes[i] != null)
                        Meshes[i].BaseTint = value;
                _baseTint = value;
            }
        }

        public virtual bool Enabled
        {
            get { return _enabled; }
            set
            {
                this.GatherMeshes();
                for (var i = 0; i < Meshes.Length; i++)
                    if (Meshes[i] != null)
                        Meshes[i].Enabled = value;
                _enabled = value;
            }
        }

        public virtual bool ApplyFog
        {
            get { return _fog; }
            set
            {
                this.GatherMeshes();
                for (var i = 0; i < Meshes.Length; i++)
                    if (Meshes[i] != null)
                        Meshes[i].ApplyFog = value;
                _fog = value;
            }
        }

        public virtual Vector3 LocalPosition
        {
            get { return _localPosition; }
            set
            {
                this.GatherMeshes();
                for (var i = 0; i < Meshes.Length; i++)
                    if (Meshes[i] != null)
                        Meshes[i].LocalPosition = value;
                _localPosition = value;
            }
        }

        public virtual Vector3 LocalRotation
        {
            get { return _localRotation; }
            set
            {
                this.GatherMeshes();
                for (var i = 0; i < Meshes.Length; i++)
                    if (Meshes[i] != null)
                        Meshes[i].LocalRotation = value;
                _localRotation = value;
            }
        }

        public virtual bool Pause { get; set; }

        public virtual Vector3 Position
        {
            get { return _position; }
            set
            {
                this.GatherMeshes();

                for (var i = 0; i < Meshes.Length; i++)
                    if (Meshes[i] != null)
                    {
                        Meshes[i].Position = Meshes[i].Position - _position + value;
                        Meshes[i].RotationPoint = Meshes[i].Position - value;
                    }
                _position = value;
            }
        }

        public virtual Vector3 Rotation
        {
            get { return _rotation; }
            set
            {
                value = Mathf.FixNaN(value);

                this.GatherMeshes();
                for (var i = 0; i < Meshes.Length; i++)
                    if (Meshes[i] != null)
                        Meshes[i].Rotation = value;
                _rotation = value;
            }
        }

        public virtual Vector3 RotationPoint
        {
            get { return _rotationPoint; }
            set
            {
                this.GatherMeshes();
                for (var i = 0; i < Meshes.Length; i++)
                    if (Meshes[i] != null)
                        Meshes[i].RotationPoint = value;
                _rotationPoint = value;
            }
        }

        public virtual Vector3 Scale { get; set; }

        [Obsolete]
        public Vector3 Size
        {
            get { return _size; }
            set
            {
                this.GatherMeshes();
                for (var i = 0; i < Meshes.Length; i++)
                    if (Meshes[i] != null)
                        Meshes[i].Size = value;
                _size = value;
            }
        }

        public virtual Vector3 TargetRotation { get; set; }

        public virtual Vector4 Tint
        {
            get { return _tint; }
            set
            {
                this.GatherMeshes();
                for (var i = 0; i < Meshes.Length; i++)
                    if (Meshes[i] != null)
                        Meshes[i].Tint = value;
                _tint = value;
            }
        }

        public abstract void Run();
        public abstract void Idle();

        public virtual void Attack(Entity Target, float Damage)
        {
        }

        public virtual void Update()
        {
        }

        public void GatherMeshes(bool Force = false)
        {
            const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            if (Meshes != null && !Force) return;

            var meshList = new List<ObjectMesh>();

            foreach (FieldInfo field in this.GetType().GetFields(flags))
            {
                if (field.FieldType == typeof(ObjectMesh))
                    meshList.Add(field.GetValue(this) as ObjectMesh);
                if (field.FieldType == typeof(Weapon))
                    meshList.Add((field.GetValue(this) as Weapon)?.MainMesh);
            }

            Meshes = meshList.ToArray();
        }

        public virtual void Death()
        {
            this.GatherMeshes();
            this.RemoveModel();
        }


        public void RemoveModel()
        {
            for (var i = 0; i < Meshes.Length; i++)
                if (Meshes[i] != null)
                    DrawManager.Remove(Meshes[i]);
        }

        public virtual void Dispose()
        {
            this.GatherMeshes(true);
            for (var i = 0; i < Meshes.Length; i++)
                if (Meshes[i] != null)
                    Meshes[i].Dispose();
            Disposed = true;
            this.Update();
        }
    }
}