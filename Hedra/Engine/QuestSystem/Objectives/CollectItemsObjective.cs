/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 02/01/2017
 * Time: 06:04 a.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using Hedra.Engine.Generation;
using Hedra.Engine.Management;
using Hedra.Engine.Player;
using System.Collections;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.UI;
using OpenTK;

namespace Hedra.Engine.QuestSystem.Objectives
{
	/// <inheritdoc />
	/// <summary>
	/// Description of CollectItemsObjective.
	/// </summary>
	public class CollectItemsObjective : Objective
	{
		private readonly int _boarTusksAmount;
		private readonly int _turtleShellAmount;
		private readonly int _spiderEyeAmount;
		private readonly int _ratTailAmount;
		private bool _tusksComplete;
	    private bool _shellsComplete;
	    private bool _eyesComplete;
	    private bool _tailComplete;
	    private readonly EntityMesh _previewMesh;
		
		public CollectItemsObjective()
		{
			var rng = new Random(World.Seed + 12312);

			//_turtleShellAmount = rng.Next(0, 4);
			//_turtleShellAmount = _turtleShellAmount == 0 ? 1 : 0;
			
			_boarTusksAmount = rng.Next(0, 3);
			_spiderEyeAmount = rng.Next(0, 4);
			_ratTailAmount = rng.Next(0, 4);
			
			_tusksComplete = _boarTusksAmount == 0;
			_eyesComplete = _spiderEyeAmount == 0;
		    _tailComplete = _ratTailAmount == 0;
			_shellsComplete = _turtleShellAmount == 0;
			
			int typeAmount = (_turtleShellAmount > 0 ? 1 : 0) + (_ratTailAmount > 0 ? 1 : 0) 
				+ (_spiderEyeAmount > 0 ? 1 : 0) + (_boarTusksAmount > 0 ? 1 : 0);
			
			var count = 0;
			var scale = 4f;
			var previewData = new VertexData();

			if(_spiderEyeAmount > 0){
				VertexData data = AssetManager.PlyLoader("Assets/Items/SpiderEyes.ply",Vector3.One * scale);
				this.AddOffset(data, count, typeAmount, scale);
				previewData += data;
				count++;
			}

			if(_boarTusksAmount > 0){
				VertexData data = AssetManager.PlyLoader("Assets/Items/BoarTusk.ply",Vector3.One * scale);
				this.AddOffset(data, count, typeAmount, scale);
				previewData += data;
				count++;
			}

			if(_turtleShellAmount > 0){
				VertexData data = AssetManager.PlyLoader("Assets/Items/TurtleShell.ply",Vector3.One * scale * .7f);
				this.AddOffset(data, count, typeAmount, scale);
				previewData += data;
				count++;
			}

			if(_ratTailAmount > 0){
				VertexData data = AssetManager.PlyLoader("Assets/Items/RatTail.ply",Vector3.One * scale);
				this.AddOffset(data, count, typeAmount, scale);
				previewData += data;
				count++;
			}
			_previewMesh = EntityMesh.FromVertexData(previewData);
		}
		
		public void AddOffset(VertexData Data, int Count, int TypeAmount, float Size){
			if(Count == 0 && (TypeAmount == 2 || TypeAmount == 3) )
				Data.Transform( (-Vector3.UnitX * .5f + Vector3.UnitZ * .0f) * Size);
			
			else if(Count == 1 && (TypeAmount == 2 || TypeAmount == 3) )
				Data.Transform( (Vector3.UnitX * .5f + Vector3.UnitZ * .0f) * Size);
			
			else if (Count == 2 && TypeAmount == 3)
				Data.Transform( (-Vector3.UnitZ * .25f + Vector3.UnitY * .0f) * Size);
			
			else if (Count == 2 && TypeAmount == 4)
				Data.Transform( (-Vector3.UnitZ * .25f - Vector3.UnitX * .5f + Vector3.UnitY * .0f) * Size);
			
			else if (Count == 3 && TypeAmount == 4)
				Data.Transform( (-Vector3.UnitZ * .25f + Vector3.UnitX * .5f + Vector3.UnitY * .0f) * Size);
		}
		
		public override bool ShouldDisplay => false;

	    public override void Setup(Chunk UnderChunk){
			CoroutineManager.StartCoroutine(Update);
		}
		
		public override void Recreate()
		{
  			base.SetQuestParams();
			base.RunCoroutine();
			CoroutineManager.StartCoroutine(Update);
		}
		
		public override void SetOutObjectives()
		{
			this.AvailableOuts.Add(new VillageObjective());
		}
		
		public IEnumerator Update(){
			while(!Disposed){
				LocalPlayer player = Scenes.SceneManager.Game.Player;
				/*for(int i = 0; i < player.Inventory.Items.Length; i++){
					if(player.Inventory.Items[i] != null && player.Inventory.Items[i].Type == ItemType.Stackable){
						
						if(player.Inventory.Items[i].Info.MaterialType == Material.BoarTusk){
							if(player.Inventory.Items[i].Info.Damage >= _boarTusksAmount) _tusksComplete = true;
						}
						
						if(player.Inventory.Items[i].Info.MaterialType == Material.SpiderEye){
							if(player.Inventory.Items[i].Info.Damage >= _spiderEyeAmount) _eyesComplete = true;
						}
						
						if(player.Inventory.Items[i].Info.MaterialType == Material.RatTail){
							if(player.Inventory.Items[i].Info.Damage >= _ratTailAmount) _tailComplete = true;
						}
						
						if(player.Inventory.Items[i].Info.MaterialType == Material.TurtleShell){
							if(player.Inventory.Items[i].Info.Damage >= _turtleShellAmount) _shellsComplete = true;
						}
					}
				}*/
				if(_tusksComplete && _shellsComplete && _eyesComplete && _tailComplete)
					this.NextObjective();
				yield return null;
			}
		}
		
		public override void Dispose()
		{
			this.Disposed = true;
		}
		
		public override string Description {
			get {
				string desc = "Collect the following items: " + Environment.NewLine;
				if(_turtleShellAmount != 0)
					desc += "● "+_turtleShellAmount+" Turtle shell"+ ((_turtleShellAmount > 1) ? "s" : "") + Environment.NewLine;
				if(_spiderEyeAmount != 0)
					desc += "● "+_spiderEyeAmount+" Spider eye"+ ((_spiderEyeAmount > 1) ? "s" : "") + Environment.NewLine;
				if(_boarTusksAmount != 0)
					desc += "● "+_boarTusksAmount+" Boar tusk"+ ((_boarTusksAmount > 1) ? "s" : "") + Environment.NewLine;
				if(_ratTailAmount != 0)
					desc += "● "+_ratTailAmount+" Rat tail"+ ((_ratTailAmount > 1) ? "s" : "") + Environment.NewLine;
				return desc;
			}
		}
		
		public override uint QuestLogIcon {
			get {
				Scenes.SceneManager.Game.Player.UI.DrawPreview(_previewMesh, UserInterface.QuestFbo);
				return UserInterface.QuestFbo.TextureID[0];
			}
		}
	}
}
