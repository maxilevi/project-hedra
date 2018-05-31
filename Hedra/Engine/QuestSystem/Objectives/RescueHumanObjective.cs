/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 13/12/2016
 * Time: 04:08 a.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections;
using System.Collections.Generic;
using Hedra.Engine.Management;
using OpenTK;
using Hedra.Engine.Generation;
using Hedra.Engine.Rendering;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering.UI;
using Hedra.Engine.Rendering.Particles;
using Hedra.Engine.StructureSystem;


namespace Hedra.Engine.QuestSystem.Objectives
{
	/// <summary>
	/// Description of RescueHumanObjective.
	/// </summary>
	public class RescueHumanObjective : Objective
	{
		private List<Humanoid> _enemies;
		private bool _oldManSaved;
		private Humanoid _oldMan;
		private string _oldManName;
		
		public RescueHumanObjective(){
			//Temporary oldman model for the icon

			var rng = new Random(World.Seed + 4213);
			_oldMan = new Humanoid();
			_oldMan.Model = new HumanoidModel(_oldMan);
			//OldMan.Model.HairColor = new Vector4(.9f, .9f, .9f, 1f);
			//OldMan.Model.EyeColor = HumanModel.EyesColors[new Random(World.Seed + 1234214).Next(0, HumanModel.EyesColors.Length)];
			//OldMan.Model.UpdateModel();
			_oldMan.Position = this.Position + Vector3.UnitY * 7f + Vector3.UnitZ * 3.0f;
			_oldMan.Physics.UsePhysics = false;
			_oldMan.AddonHealth = 100f;
			_oldMan.Health = _oldMan.MaxHealth / 3 + rng.NextFloat() * 48f - 24f;
			_oldMan.MobType = MobType.Human;
			_oldMan.Rotation = new Vector3(-90,0,0);
			_oldMan.Physics.HasCollision = false;
			_oldMan.Physics.CanCollide = false;
			_oldMan.BlockPosition = this.Position + Vector3.UnitY * 7f + Vector3.UnitZ * 3.0f;
			_oldMan.Model.Position = _oldMan.BlockPosition;

			
			//OldMan.Model.Tied();
			
			_oldMan.AddComponent(new FollowAIComponent(_oldMan));
			
			//Diablo easter egg
			
			World.AddEntity(_oldMan);
			
			CoroutineManager.StartCoroutine(Update);
			
			string pName = GameManager.Player.Name;
			if(pName == "Aidan" || pName == "Tyrael"){
				_oldManName = "Deckard Cain";
			}else{
				_oldManName = NameGenerator.PickMaleName(rng);
			}

		    _oldMan.AddComponent(new HealthBarComponent(_oldMan, _oldManName));
		    _oldMan.SearchComponent<DamageComponent>().Immune = true;
        }
		
		public override void SetOutObjectives(){
			AvailableOuts.Add(new VillageObjective(_oldMan));
		}
		
		public override bool ShouldDisplay => true;
		
		public override void Setup(Chunk UnderChunk)
		{
			CoroutineManager.StartCoroutine(FireParticles);
			
			var rng = new Random(World.Seed + 532214);
			Position = new Vector3(ObjectivePosition.X, Physics.HeightAtPosition(ObjectivePosition), ObjectivePosition.Z);
			
			VertexData roasterModel = AssetManager.PlyLoader("Assets/Env/Roaster.ply", Vector3.One);
			roasterModel.Transform( Matrix4.CreateScale(.7f) );//Reduce a bit its size only
			VertexData centerModel = AssetManager.PlyLoader("Assets/Env/Campfire2.ply", Vector3.One);
			
			Matrix4 transMatrix = Matrix4.CreateScale(3 + rng.NextFloat() * 1.5f);		
			Matrix4 scaleMatrix = transMatrix;	
			transMatrix *= Matrix4.CreateTranslation( Position );
			
			roasterModel.Transform(transMatrix);
			centerModel.Transform(transMatrix);
			
			Model = roasterModel + centerModel;
			
			UnderChunk.AddStaticElement(Model);
			
			Shapes = new List<CollisionShape>();
			List<CollisionShape> campfireShapes = AssetManager.LoadCollisionShapes("Campfire0.ply", 7, Vector3.One);
			List<CollisionShape> roasterShapes = AssetManager.LoadCollisionShapes("Roaster0.ply", 5, Vector3.One);
			Shapes.Add(campfireShapes[0]);
			Shapes.AddRange(roasterShapes.ToArray());
			for(var i = 0; i < Shapes.Count; i++){
				if(i != 0)
					Shapes[i].Transform( Matrix4.CreateScale(.7f) );
				Shapes[i].Transform(transMatrix);
			}
			
			int extraCampfires = 3 + rng.Next(0, 3);
			for(int i = 0; i < extraCampfires; i++){
				List<CollisionShape> cShapes = AssetManager.LoadCollisionShapes("Campfire0.ply", 7, Vector3.One);
				cShapes.RemoveAt(0);//The first one is the fire
				
				Matrix4 rot = Matrix4.CreateRotationY( 360 / extraCampfires * i * Mathf.Radian);
				Matrix4 posMatrix = Matrix4.CreateTranslation(Position);
				float dist = 6 + rng.NextFloat() * 4f;
				
				for(int k = 0; k < cShapes.Count; k++){
					cShapes[k].Transform(scaleMatrix);
					cShapes[k].Transform( Matrix4.CreateTranslation(Vector3.UnitX * dist) );
					cShapes[k].Transform(rot);
					cShapes[k].Transform( posMatrix );
				}
				VertexData campfire = AssetManager.PlyLoader("Assets/Env/Campfire1.ply", Vector3.One);
				campfire.Transform(scaleMatrix);
				campfire.Transform( Matrix4.CreateTranslation(Vector3.UnitX * dist) );
				campfire.Transform(rot);
				campfire.Transform( posMatrix );
				campfire.Color( AssetManager.ColorCode1, Utils.VariateColor(CampfireDesign.TentColor(rng), 15, rng) );
				
				Shapes.AddRange(cShapes.ToArray());
				Model += campfire;
			}
			
			UnderChunk.AddCollisionShape(Shapes.ToArray());
			
			_enemies = new List<Humanoid>();
			for(int i = 0; i < extraCampfires; i++){
				_enemies.Add( World.QuestManager.SpawnBandit(Position + new Vector3(rng.NextFloat() * 48 - 24f, 0, rng.NextFloat() * 48 - 24f), false, false) );
			}
			
		}
		
		private IEnumerator Update(){
			while(!Disposed && !_oldManSaved){
				yield return null;

			    if (_oldManSaved) continue;

			    _oldMan.BlockPosition = this.ObjectivePosition.Xz.ToVector3() + Vector3.UnitY * 7f + Vector3.UnitZ * 3.0f 
			                            + Vector3.UnitY * Physics.HeightAtPosition(ObjectivePosition);

			    _oldMan.Model.Position = _oldMan.BlockPosition;
			    _oldMan.Model.Tied();
			    _oldMan.Name = _oldManName;

			    if (!_oldMan.InUpdateRange)
			        _oldMan.Update();

			    LocalPlayer player = GameManager.Player;
			    if (!AreEnemiesDead || !((player.Position - Position).LengthSquared < 16 * 16)) continue;

			    LocalPlayer.Instance.MessageDispatcher.ShowMessage("PRESS [E] TO UNTIE", .25f);

			    //if (Events.EventDispatcher.LastKeyDown != OpenTK.Input.Key.E) continue;

			    _oldManSaved = true;
			    _oldMan.Physics.UsePhysics = true;
			    _oldMan.Physics.HasCollision = true;
			    _oldMan.Physics.CanCollide = true;
			    _oldMan.Rotation = Vector3.Zero;
			    _oldMan.Position += Vector3.UnitX * 2.5f;
			    _oldMan.SearchComponent<FollowAIComponent>().ToFollow = player;
			    //OldMan.AddComponent(new TalkComponent(OldMan));
			    base.NextObjective();
			    yield break;
			}
		}
		
		private bool AreEnemiesDead{
			get{
				if(_enemies == null) return false;
					
				for(int i = 0; i < _enemies.Count; i++){
					if(_enemies[i] != null && !_enemies[i].IsDead){
						return false;
					}
				}
				return true;
			}
		}
		
		private IEnumerator FireParticles(){
			while(!Disposed){
				yield return null; // Wait 2 frames
				yield return null;
				World.Particles.Color = Particle3D.FireColor;
				World.Particles.VariateUniformly = false;
				World.Particles.Position = this.Position + Vector3.UnitY * 2f;
				World.Particles.Scale = Vector3.One * .5f;
				World.Particles.ScaleErrorMargin = new Vector3(.35f,.35f,.35f);
				World.Particles.Direction = Vector3.UnitY * .05f;
				World.Particles.ParticleLifetime = 2f;
				World.Particles.GravityEffect = 0.0f;
				World.Particles.PositionErrorMargin = new Vector3(1.5f, 1.5f, 1.5f);
					
				World.Particles.Emit();
			}
		}
		public override void Dispose()
		{
			Disposed = true;
		}
		
		public override string Description => "Rescue "+_oldManName+" the scholar from the kidnappers";

	    public override uint QuestLogIcon {
			get {
				HumanoidModel model = _oldMan.Model;
				
				Vector3 prevRot = model.Model.Rotation; 
				model.Model.Scale *= 2f;
				model.Model.Rotation = Vector3.Zero;

				//GameManager.Player.UI.DrawPreview(_oldMan.Model, UserInterface.QuestFbo);

				model.Model.Scale /= 2f;
				model.Model.Rotation = prevRot;
				return UserInterface.QuestFbo.TextureID[0];
			}
		}
	}
}
