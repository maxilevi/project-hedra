/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 04/09/2016
 * Time: 06:52 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.EntitySystem.BossSystem;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
using OpenTK;
using Hedra.Engine.Management;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.UI;
using Hedra.Engine.Scenes;

namespace Hedra.Engine.QuestSystem.Objectives
{
	/// <summary>
	/// Description of BossObjective.
	/// </summary>
	internal class BossObjective : Objective
	{
		public Entity Boss;
		private MobType _bossType;
		
		public override void SetOutObjectives()
		{
			this.AvailableOuts.Add(new VillageObjective());
		}
		
		public override void Recreate(){ 
            base.Recreate();

			/*int n = rng.Next(0, 1);
			if(n == 0){
				base.NoTreesRadius = 16;
				base.CenterHeight = 92;
				base.CenterMaxHeight = 0;
				base.CenterRadius = 384;
                lock(World.QuestManager.Plateaus)
				    World.QuestManager.AddPlateau(new Plateau(this.ObjectivePosition, base.CenterRadius+512, -240, 0));
			}*/
			
			Boss?.Dispose();
			//Boss = BossGenerator.Generate(rng, out _bossType);
			Boss.BlockPosition = World.QuestManager.ObjectivePosition;
			Boss.Model.Position = Boss.BlockPosition;

			CoroutineManager.StartCoroutine(this.Update);
		}
		
		public IEnumerator Update(){
			while(!Boss.IsDead){

			    yield return null;
			}
			this.NextObjective();
		}
		
		public override void Setup(Chunk UnderChunk){}
		
		public override bool ShouldDisplay{
			get{
				if(GameManager.Player == null) return false;
				
				return !(Boss != null && Boss.IsDead) || !(Boss != null && (GameManager.Player.Position - Boss.Position).LengthSquared < 24*24);
			}
		}
		
		public override void Dispose()
		{
		    Boss?.Dispose();
		    Disposed = true;
		}
		
		public override string Description => "Defeat "+Boss.Name+" located in the "+this.ObjectiveDirection.ToLowerInvariant()+" of the map.";

	    public override Vector3 IconPosition {
			get { 
				if(Boss == null)
					return base.IconPosition;

				return base.IconPosition + Vector3.UnitY * 96;
			}
		}
		
		public override uint QuestLogIcon {
			get {
				if(_bossType == MobType.Bee || _bossType == MobType.Rat){
					/*var model = Boss.Model as QuadrupedModel;
				    if (model == null) return UserInterface.QuestFbo.TextureID[0];

                    model.Model.Scale *= 2;
                    if(_bossType == MobType.Bee)
				        model.Model.Position -= Vector3.UnitY * 8f;
				    //GameManager.Player.UI.DrawPreview(Boss.Model, UserInterface.QuestFbo);
				    model.Model.Scale /= 2;*/
				}else{
				    /*if (!(Boss.Model is QuadrupedModel model)) return UserInterface.QuestFbo.TextureID[0];

				    Vector3 prevRot = model.Model.Rotation;
				    model.Model.Rotation = Vector3.Zero;
                    model.Model.Update();
				    //GameManager.Player.UI.DrawPreview(Boss.Model, UserInterface.QuestFbo);
				    model.Model.Rotation = prevRot;*/
				}
				return UserInterface.QuestFbo.TextureID[0];
			}
		}
	}
}
