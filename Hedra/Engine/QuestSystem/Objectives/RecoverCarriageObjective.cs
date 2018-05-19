using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.UI;
using OpenTK;

namespace Hedra.Engine.QuestSystem.Objectives
{
    public class RecoverCarriageObjective : Objective
    {
        private Entity _carriage;
        private Humanoid[] _enemies;
        private Humanoid[] _villagers;

        public override bool ShouldDisplay => true;

        public override Vector3 IconPosition => _carriage.Position;

        public override string Description => "Bring back to the village the lost carriage.";

        public override uint QuestLogIcon
        {
            get
            {
                if (_carriage == null) return 0;

                //LocalPlayer.Instance.UI.DrawPreview(_carriage.Model, UserInterface.QuestFbo);
                return UserInterface.QuestFbo.TextureID[0];
            }
        }

        public override void Recreate()
        {
            base.Recreate();
            _carriage = World.QuestManager.SpawnCarriage(this.ObjectivePosition);

            _enemies = new Humanoid[3];
            for (int i = 0; i < _enemies.Length; i++)
            {
                _enemies[i] = World.QuestManager.SpawnBandit(this.ObjectivePosition 
                    + Vector3.TransformPosition(Vector3.UnitZ * 96f, Matrix4.CreateRotationY(360 / _enemies.Length * i * Mathf.Radian)), 
                    false);

                _enemies[i].RemoveComponent(_enemies[i].SearchComponent<WarriorAIComponent>());
                _enemies[i].RemoveComponent(_enemies[i].SearchComponent<ArcherAIComponent>());
            }

            _villagers = new Humanoid[2];
            _villagers[0] = World.QuestManager.SpawnVillager(this.ObjectivePosition + Vector3.UnitX * 20f - Vector3.UnitZ * 2, false);
            _villagers[1] = World.QuestManager.SpawnHumanoid(HumanType.Merchant, this.ObjectivePosition + Vector3.UnitX * 20f + Vector3.UnitZ * 2);

            _villagers[0].RemoveComponent(_villagers[0].SearchComponent<VillagerAIComponent>());
            _villagers[1].RemoveComponent(_villagers[1].SearchComponent<MerchantComponent>());

            _villagers[0].Physics.HasCollision = false;
            _villagers[1].Physics.HasCollision = false;

            _villagers[0].Rotation = new Vector3(0, 45+90, 0);
            _villagers[1].Rotation = new Vector3(0, 45, 0);

            World.QuestManager.AddVillagePosition(this.ObjectivePosition, 96f);

            CoroutineManager.StartCoroutine(Update);
        }

        private IEnumerator Update()
        {
            while (!Disposed)
            {
                if(!_carriage.InUpdateRange)
                    _carriage.Update();

                for (int i = 0; i < _villagers.Length; i++)
                {
                    _villagers[i].Model.TiedSitting();
                }

                yield return null;
            }
        }

         public override void SetOutObjectives()
        {
            this.AvailableOuts.Add( new VillageObjective() );
        }

        public override void Setup(Chunk UnderChunk)
        {
            Position = new Vector3(ObjectivePosition.X, Physics.HeightAtPosition(ObjectivePosition), ObjectivePosition.Z);

            /*CollidableStructure fort = new CollidableStructure(Position, null);
            //sWorld.StructureGenerator.GenerateWoodenFort(Position, UnderChunk, fort);
            World.AddChunkToQueue(UnderChunk, true);

            var rng = new Random(World.Seed + 1232342);
            int extraCampfires = _enemies.Length;
            var model = new VertexData();
            var shapes = new List<CollisionShape>();
            for (int i = 0; i < extraCampfires; i++)
            {
                var scaleMatrix = Matrix4.CreateScale(4);
                List<CollisionShape> cShapes = AssetManager.LoadCollisionShapes("Campfire0.ply", 7, Vector3.One);
                cShapes.RemoveAt(0);//The first one is the fire

                Matrix4 rot = Matrix4.CreateRotationY(360 / extraCampfires * i * Mathf.Radian);
                Matrix4 posMatrix = Matrix4.CreateTranslation(this.Position);
                float dist = 36f + rng.NextFloat() * 6f;

                for (int k = 0; k < cShapes.Count; k++)
                {
                    cShapes[k].Transform(scaleMatrix);
                    cShapes[k].Transform(Matrix4.CreateTranslation(Vector3.UnitZ * dist));
                    cShapes[k].Transform(rot);
                    cShapes[k].Transform(posMatrix);
                }
                VertexData campfire = AssetManager.PlyLoader("Assets/Env/Campfire1.ply", Vector3.One);
                campfire.Transform(scaleMatrix);
                campfire.Transform(Matrix4.CreateTranslation(Vector3.UnitZ * dist));
                campfire.Transform(rot);
                campfire.Transform(posMatrix);
                campfire.Color(AssetManager.ColorCode1, Utils.VariateColor(World.StructureGenerator.TentColor(rng), 15, rng));
                model += campfire;

                shapes.AddRange(cShapes.ToArray());
            }
            //UnderChunk.AddCollisionShape(shapes.ToArray());
            //UnderChunk.AddStaticElement(model);
            World.AddChunkToQueue(UnderChunk, true);*/
        }

        public override void Dispose()
        {

        }
    }
}
