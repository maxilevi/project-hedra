/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 16/06/2017
 * Time: 02:19 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using Hedra.Core;
using Hedra.Engine.Game;
using OpenTK;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Rendering.Animation;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Localization;
using Hedra.Engine.Management;
using Hedra.Engine.Player;
using Hedra.EntitySystem;
using Hedra.Items;
using Hedra.Localization;

namespace Hedra.Engine.WorldBuilding
{
    /// <summary>
    /// Description of Chest.
    /// </summary>
    public sealed class Chest : InteractableStructure
    {
        private static readonly CollisionShape DefaultShape;

        public override string Message => Translations.Get("interact_chest");
        public override int InteractDistance => 16;
        protected override bool DisposeAfterUse => false;
        protected override bool CanInteract => IsClosed && (Condition?.Invoke() ?? true);
        private string ChestModelPath => Season.IsChristmas ? "Assets/Chr/ChristmasChestIdle.dae" : "Assets/Chr/ChestIdle.dae"; 

        public Item ItemSpecification { get; set; }
        public Func<bool> Condition { get; set; }
        public event OnItemCollect OnPickup;
        private readonly AnimatedModel _model;
        private readonly Animation _idleAnimation;
        private readonly Animation _openAnimation;
        private Chunk _underChunk;

        static Chest()
        {
            DefaultShape = AssetManager.LoadCollisionShapes("Assets/Env/Chest.ply", 1, Vector3.One)[0];
        }

        public Chest(Vector3 Position, Item ItemSpecification) : base(Position)
        {
            this._model = AnimationModelLoader.LoadEntity(ChestModelPath);
            this._idleAnimation = AnimationLoader.LoadAnimation("Assets/Chr/ChestIdle.dae");
            this._openAnimation = AnimationLoader.LoadAnimation("Assets/Chr/ChestOpen.dae");        
            this._openAnimation.Loop = false;
            this._openAnimation.Speed = 1.5f;
            this._openAnimation.OnAnimationEnd += delegate
            { 
                var worldItem = World.DropItem(ItemSpecification, this.Position);
                worldItem.Position = new Vector3(worldItem.Position.X, worldItem.Position.Y + .75f * this.Scale.Y, worldItem.Position.Z);
                worldItem.OnPickup += delegate(IPlayer Player)
                {
                    OnPickup?.Invoke(Player);
                };
            };
            
            this._model.PlayAnimation(_idleAnimation);
            this._model.Scale = Vector3.One * 3.5f;
            this._model.ApplyFog = true;
            this.ItemSpecification = ItemSpecification;
        }

        public override void Update(float DeltaTime)
        {
            if (_model != null) _model.Position = Position;
            base.Update(DeltaTime);
        }

        protected override void DoUpdate(float DeltaTime)
        {
            base.DoUpdate(DeltaTime);
            if (_model != null)
            {
                _model.Update();
                HandleColliders();
            }
        }

        protected override void OnSelected(IHumanoid Humanoid)
        {
            base.OnSelected(Humanoid);
            _model.Tint = new Vector4(2.5f, 2.5f, 2.5f, 1);
        }

        protected override void OnDeselected(IHumanoid Humanoid)
        {
            base.OnDeselected(Humanoid);
            _model.Tint = new Vector4(1, 1, 1, 1);
        }

        private void HandleColliders()
        {
            var underChunk = World.GetChunkAt(this.Position);
            if (underChunk != null && underChunk.BuildedWithStructures && _underChunk != underChunk)
            {
                _underChunk = underChunk;
                Position = new Vector3(Position.X, Physics.HeightAtPosition(Position) + .5f, Position.Z);

                var shape = DefaultShape.Clone() as CollisionShape;
                shape.Transform(Matrix4.CreateScale(this.Scale));
                shape.Transform(-Vector3.UnitX * 1.5f);
                shape.Transform(Matrix4.CreateRotationY(this.Rotation.Y * Mathf.Radian) *
                                Matrix4.CreateRotationX(this.Rotation.X * Mathf.Radian) *
                                Matrix4.CreateRotationZ(this.Rotation.Z * Mathf.Radian));

                shape.Transform(Position);
                _underChunk.AddCollisionShape(shape);
            }
        }

        protected override void Interact(IHumanoid Humanoid)
        {
            _model.PlayAnimation(_openAnimation);
        }

        public bool IsClosed => _model.AnimationPlaying == _idleAnimation;
        
        public Vector3 Scale
        {
            get => _model.Scale;
            set => _model.Scale = value;
        }
        
        public Vector3 Rotation
        {
            get => _model.LocalRotation;
            set => _model.LocalRotation = value;
        }

        public override void Dispose()
        {
            base.Dispose();
            this._model.Dispose();
        }
    }
}
