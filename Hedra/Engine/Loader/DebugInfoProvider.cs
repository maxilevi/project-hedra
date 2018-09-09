using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.EnvironmentSystem;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.ItemSystem.WeaponSystem;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.UI;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Hedra.Engine.Loader
{
    public class DebugInfoProvider
    {
        private readonly Panel _debugPanel;
        private readonly GUIText _debugText;
	    private readonly Texture _geomPoolMemory;
	    private float _passedTime;

        public DebugInfoProvider()
        {
            _debugPanel = new Panel();
			
            _debugText = new GUIText(string.Empty, new Vector2(.7f,-.7f), Color.Black, FontCache.Get(AssetManager.NormalFamily,12));
	        _geomPoolMemory = new Texture(0, new Vector2(0f, 0.95f), new Vector2(1024f / GameSettings.Width, 16f / GameSettings.Height));
	        _debugPanel.AddElement(_debugText);
	        _debugPanel.AddElement(_geomPoolMemory);
            _debugPanel.Disable();
            Log.WriteLine("Created debug elements.");
	        
#if DEBUG
	        if (OSManager.RunningPlatform == Platform.Windows)
	        {
		        //GameLoader.EnableGLDebug();
	        }
#endif	 
        }

        public void Update()
        {
            var player = GameManager.Player;
			var chunkSpace = World.ToChunkSpace(player.Position);
			if(GameSettings.DebugView)
			{
				_debugPanel.Enable();
				var underChunk = World.GetChunkByOffset(chunkSpace);
				var text = $"X = {(int)player.BlockPosition.X} Y = {(int)(player.BlockPosition.Y)} Z={(int)player.BlockPosition.Z}";
				text += 
					$"\nChunks={World.Chunks.Count} ChunkX={underChunk?.OffsetX ?? 0} ChunkZ={underChunk?.OffsetZ ?? 0}";
				text += 
					$"\nLights={ShaderManager.UsedLights}/{ShaderManager.MaxLights}Pitch={player.View.Pitch}";
				text += 
					$"\nMesh Queue = {World.MeshQueueCount} Cache={CacheManager.CachedColors.Count} | {CacheManager.CachedExtradata.Count} Time={(int)(SkyManager.DayTime/1000)}:{((int) ( ( SkyManager.DayTime/1000f - (int)(SkyManager.DayTime/1000) ) * 60)):00}";
				text += 
					$"\nGeneration Queue ={World.ChunkQueueCount} Mobs={World.Entities.Count} Yaw={player.View.TargetYaw}";
				text += 
					$"\nTextures ={Graphics2D.Textures.Count} Seed={World.Seed} FPS={Utils.LastFrameRate} MS={Utils.FrameProccesingTime}";
				text +=
					$"\nCulledObjects = {DrawManager.CulledObjectsCount}/{DrawManager.CullableObjectsCount} Pitch={player.View.TargetPitch} Physics.Calls={Physics.Threading.Count}";

				_debugText.Text = text;
			    _passedTime += Time.IndependantDeltaTime;
			    if (_passedTime > 5.0f)
			    {
			        _passedTime = 0;
			        Graphics2D.Textures.Remove(_geomPoolMemory.TextureElement.TextureId);
                    Renderer.DeleteTexture(_geomPoolMemory.TextureElement.TextureId);
			        _geomPoolMemory.TextureElement.TextureId = Graphics2D.LoadTexture(WorldRenderer.StaticBuffer.Indices.Draw());
			    }
			}
            else
            {
				_debugPanel.Disable();
			}
        }

	    public void Draw()
	    {
		    if (GameSettings.DebugView)
		    {
		        var player = GameManager.Player;
		        var underChunk = World.GetChunkAt(player.Position);

		        if (underChunk != null)
		        {
		            for (int x = 0; x < Chunk.Width / Chunk.BlockSize; x++)
		            {
		                for (int z = 0; z < Chunk.Width / Chunk.BlockSize; z++)
		                {
		                    Vector3 BasePosition = new Vector3(x * Chunk.BlockSize + underChunk.OffsetX,
		                        Physics.HeightAtPosition(x * Chunk.BlockSize + underChunk.OffsetX,
		                            z * Chunk.BlockSize + underChunk.OffsetZ), z * Chunk.BlockSize + underChunk.OffsetZ);
		                    Vector3 Normal = Physics.NormalAtPosition(BasePosition);

		                    Renderer.Begin(PrimitiveType.Lines);
		                    Renderer.Color3(Colors.Yellow);
		                    Renderer.Vertex3(BasePosition);
		                    Renderer.Color3(Colors.Yellow);
		                    Renderer.Vertex3(BasePosition + Normal * 2);
		                    Renderer.End();
		                }
		            }
		        }
			    var Collisions = new List<ICollidable>();
				var Collisions2 = new List<ICollidable>();

				//Chunk UnderChunk = World.GetChunkAt(Player.BlockPosition);
				Chunk UnderChunkR = World.GetChunkAt(player.Position + new Vector3(Chunk.Width, 0, 0));
				Chunk UnderChunkL = World.GetChunkAt(player.Position - new Vector3(Chunk.Width, 0, 0));
				Chunk UnderChunkF = World.GetChunkAt(player.Position + new Vector3(0, 0, Chunk.Width));
				Chunk UnderChunkB = World.GetChunkAt(player.Position - new Vector3(0, 0, Chunk.Width));

				Collisions.AddRange(World.GlobalColliders);
				if (player.NearCollisions != null)
					Collisions.AddRange(player.NearCollisions);
				if (underChunk != null)
					Collisions.AddRange(underChunk.CollisionShapes);
				if (UnderChunkL != null)
					Collisions2.AddRange(UnderChunkL.CollisionShapes);
				if (UnderChunkR != null)
					Collisions2.AddRange(UnderChunkR.CollisionShapes);
				if (UnderChunkF != null)
					Collisions2.AddRange(UnderChunkF.CollisionShapes);
				if (UnderChunkB != null)
					Collisions2.AddRange(UnderChunkB.CollisionShapes);

				for (int i = 0; i < Collisions.Count; i++)
				{
					if (!(Collisions[i] is CollisionShape shape)) return;

					var pshape = player.Model.BroadphaseCollider;

					float radiiSum = shape.BroadphaseRadius + pshape.BroadphaseRadius;

					BasicGeometry.DrawShape(shape,
						(pshape.BroadphaseCenter - shape.BroadphaseCenter).LengthSquared < radiiSum * radiiSum
							? Colors.White
							: Colors.Red);
				}

				for (int i = 0; i < Collisions2.Count; i++)
				{
					if( Collisions2[i] is CollisionShape shape){
						BasicGeometry.DrawShape(shape, Colors.Yellow);
					}
				}
		        if (false)
		        {

		            Renderer.Begin(PrimitiveType.Lines);
		            Renderer.Color3(Colors.Blue);
		            Renderer.Vertex3(player.Position + Vector3.UnitZ * 2f);
		            Renderer.Color3(Colors.Blue);
		            Renderer.Vertex3(player.Position + Vector3.UnitZ * 4f);
		            Renderer.End();

		            Renderer.Begin(PrimitiveType.Lines);
		            Renderer.Color3(Colors.BlueViolet);
		            Renderer.Vertex3(player.Position - Vector3.UnitZ * 2f);
		            Renderer.Color3(Colors.BlueViolet);
		            Renderer.Vertex3(player.Position - Vector3.UnitZ * 4f);
		            Renderer.End();

		            Renderer.Begin(PrimitiveType.Lines);
		            Renderer.Color3(Colors.Red);
		            Renderer.Vertex3(player.Position + Vector3.UnitX * 2f);
		            Renderer.Color3(Colors.Red);
		            Renderer.Vertex3(player.Position + Vector3.UnitX * 4f);
		            Renderer.End();

		            Renderer.Begin(PrimitiveType.Lines);
		            Renderer.Color3(Colors.OrangeRed);
		            Renderer.Vertex3(player.Position - Vector3.UnitX * 2f);
		            Renderer.Color3(Colors.OrangeRed);
		            Renderer.Vertex3(player.Position - Vector3.UnitX * 4f);
		            Renderer.End();

		            Renderer.Begin(PrimitiveType.Lines);
		            Renderer.Color3(Colors.Yellow);
		            Renderer.Vertex3(player.Position + player.Orientation * 2f);
		            Renderer.Color3(Colors.Yellow);
		            Renderer.Vertex3(player.Position + player.Orientation * 4f);
		            Renderer.End();

		            World.Entities.ToList().ForEach(delegate(IEntity E)
		            {
		                if (E != null)
		                {
		                    Renderer.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
		                    //BasicGeometry.DrawBox(E.Model.BroadphaseBox.Min, E.Model.BroadphaseBox.Max - E.Model.BroadphaseBox.Min);
		                    Renderer.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
		                }
		            });
		            Renderer.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
		            BasicGeometry.DrawBox(GameManager.Player.Model.BaseBroadphaseBox.Min,
		                GameManager.Player.Model.BaseBroadphaseBox.Max - GameManager.Player.Model.BaseBroadphaseBox.Min);
		            Renderer.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);

		            if (GameManager.Player.Model.LeftWeapon != null &&
		                GameManager.Player.Model.LeftWeapon is MeleeWeapon melee)
		            {
		                Renderer.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
		                BasicGeometry.DrawShapes(melee.Shapes, Colors.White);
		                Renderer.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
		            }
		            World.Entities.ToList().ForEach(delegate(IEntity E)
		            {
		                if (E == null || !E.InUpdateRange) return;
		                var colliders = E.Model.Colliders;
		                for (var i = 0; i < colliders.Length; i++)
		                {
		                    Renderer.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
		                    //BasicGeometry.DrawShape(colliders[i], Color.GreenYellow);
		                    Renderer.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
		                }
		                Renderer.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
		                BasicGeometry.DrawShape(E.Model.BroadphaseCollider, Colors.GreenYellow);
		                Renderer.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
		            });

		            //var Player = Game.LPlayer;
		            /*var Collisions = new List<ICollidable>();
		            var Collisions2 = new List<ICollidable>();

		            //Chunk UnderChunk = World.GetChunkAt(Player.BlockPosition);
		            Chunk UnderChunkR = World.GetChunkAt(Player.Position + new Vector3(Chunk.Width, 0, 0));
		            Chunk UnderChunkL = World.GetChunkAt(Player.Position - new Vector3(Chunk.Width, 0, 0));
		            Chunk UnderChunkF = World.GetChunkAt(Player.Position + new Vector3(0, 0, Chunk.Width));
		            Chunk UnderChunkB = World.GetChunkAt(Player.Position - new Vector3(0, 0, Chunk.Width));

		            Collisions.AddRange(World.GlobalColliders);
		            if (Player.NearCollisions != null)
		                Collisions.AddRange(Player.NearCollisions);
		            if (UnderChunk != null)
		                Collisions.AddRange(UnderChunk.CollisionShapes);
		            if (UnderChunkL != null)
		                Collisions2.AddRange(UnderChunkL.CollisionShapes);
		            if (UnderChunkR != null)
		                Collisions2.AddRange(UnderChunkR.CollisionShapes);
		            if (UnderChunkF != null)
		                Collisions2.AddRange(UnderChunkF.CollisionShapes);
		            if (UnderChunkB != null)
		                Collisions2.AddRange(UnderChunkB.CollisionShapes);

		            for (int i = 0; i < Collisions.Count; i++)
		            {
		                if (!(Collisions[i] is CollisionShape shape)) return;

		                var pshape = Player.Model.BroadphaseCollider;

		                float radiiSum = shape.BroadphaseRadius + pshape.BroadphaseRadius;

		                BasicGeometry.DrawShape(shape,
		                    (pshape.BroadphaseCenter - shape.BroadphaseCenter).LengthSquared < radiiSum * radiiSum
		                        ? Colors.White
		                        : Colors.Red);
		            }

		            for (int i = 0; i < Collisions2.Count; i++)
		            {
		                /*var shape = Collisions2[i] as CollisionShape;
                        if( shape != null){
                            BasicGeometry.DrawShape(shape, Color.Yellow);
                        }
		            }*/
		        }
		    }
	    }
    }
}