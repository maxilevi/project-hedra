using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Hedra.Core;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.EnvironmentSystem;
using Hedra.Engine.Events;
using Hedra.Engine.Game;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.IO;
using Hedra.Engine.Management;
using Hedra.Engine.Native;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Core;
using Hedra.Engine.Rendering.Frustum;
using Hedra.Engine.Rendering.UI;
using Hedra.EntitySystem;
using Hedra.Game;
using Hedra.Rendering;
using Hedra.WeaponSystem;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;

namespace Hedra.Engine.Loader
{
    public class DebugInfoProvider : IDisposable
    {
        private readonly Panel _debugPanel;
        private readonly GUIText _debugText;
        private readonly BackgroundTexture _staticPool;
        private readonly BackgroundTexture _waterPool;
        private readonly BackgroundTexture _instancePool;
        private readonly BackgroundTexture _depthTexture;
        private readonly VBO<Vector3> _frustumPoints;
        private readonly VAO<Vector3> _frustumVAO;
        private float _passedTime;
        private bool _depthMode;
        private bool _extraDebugView;
        private bool _fpsOnTitle;
        private string _originalTitle;
        private int _voxelCount;
        private int _chunkCount = 1;

        public DebugInfoProvider()
        {
            _debugPanel = new Panel();
            _depthTexture = new BackgroundTexture(0, Vector2.Zero, Vector2.One);
            _depthTexture.TextureElement.Flipped = true;
            _debugText = new GUIText(string.Empty, new Vector2(.65f,-.5f), Color.Black, FontCache.GetNormal(12));
            _staticPool = new BackgroundTexture(0, new Vector2(0f, 0.90f), new Vector2(1024f / GameSettings.Width, 48f / GameSettings.Height));
            _waterPool = new BackgroundTexture(0, new Vector2(0f, 0.80f), new Vector2(1024f / GameSettings.Width, 16f / GameSettings.Height));
            _instancePool = new BackgroundTexture(0, new Vector2(0f, 0.75f), new Vector2(1024f / GameSettings.Width, 16f / GameSettings.Height));
            _debugPanel.AddElement(_staticPool);
            _debugPanel.AddElement(_waterPool);
            _debugPanel.AddElement(_instancePool);
            _debugPanel.AddElement(_debugText);
            _debugPanel.Disable();
            _originalTitle = Program.GameWindow.Title;
            var points = Culling.Frustum.Corners;
            _frustumPoints = new VBO<Vector3>(points, points.Length, VertexAttribPointerType.Float);
            _frustumVAO = new VAO<Vector3>(_frustumPoints);
            Log.WriteLine("Created debug elements.");
            
#if DEBUG
            if (OSManager.RunningPlatform == Platform.Windows)
            {
                GameLoader.EnableGLDebug();
            }
            
            EventDispatcher.RegisterKeyDown(this, delegate(object Sender, KeyEventArgs Args)
            {
                if (Args.Key == Key.F7 && GameSettings.DebugView)
                {
                    _depthMode = !_depthMode;
                }

                if (Args.Key == Key.F8 && GameSettings.DebugView)
                {
                    _extraDebugView = !_extraDebugView;
                }

                if (Args.Key == Key.F12 && GameSettings.DebugView)
                {
                    _fpsOnTitle = !_fpsOnTitle;
                }
            });
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
                var chunkBound = Chunk.Width / Chunk.BlockSize;
                var defaultVoxelCount = chunkBound * Chunk.Height * chunkBound;
                var lineBreak = $"{Environment.NewLine}{Environment.NewLine}";
                var text = $"X = {(int)player.BlockPosition.X} Y = {(int)(player.BlockPosition.Y)} Z={(int)player.BlockPosition.Z} Routines={RoutineManager.Count} Watchers={player.Loader.WatcherCount}";
                text += 
                    $"{lineBreak}Chunks={World.Chunks.Count} ChunkX={underChunk?.OffsetX ?? 0} ChunkZ={underChunk?.OffsetZ ?? 0}";
                text +=
                    $"{lineBreak}Textures ={TextureRegistry.Count} Fonts={FontCache.Count} Texts={TextCache.Count} VAO={VAO.Alive} VBO={VBO.Alive} FBO={FBO.Alive}";
                text += 
                    $"{lineBreak}AvgBuildTime={World.AverageBuildTime} MS AvgGenTime={World.AverageGenerationTime} MS Lights={ShaderManager.UsedLights}/{ShaderManager.MaxLights} Pitch={player.View.Pitch:0.00}";
                text += 
                    $"{lineBreak}MQueue = {World.MeshQueueCount} GQueue ={World.ChunkQueueCount} Time={(int)(SkyManager.DayTime/1000)}:{((int) ( ( SkyManager.DayTime/1000f - (int)(SkyManager.DayTime/1000) ) * 60)):00} H={World.Entities.Count(M => M.IsHumanoid)} Items={World.WorldObjects.Length} M&H={World.Entities.Count}";
                text += 
                    $"{lineBreak}Watchers={World.StructureHandler.Watchers.Length} Structs={World.StructureHandler.Structures.Length}->{World.StructureHandler.Structures.Sum(S => S.Children.Length)} Plateaus={World.WorldBuilding.Plateaux.Length} Groundworks={World.WorldBuilding.Groundworks.Length}";
                text +=
                    $"{lineBreak}Updates={UpdateManager.UpdateCount} Seed={World.Seed} FPS={Time.Framerate} MS={Time.Frametime}";
                text +=
                    $"{lineBreak}SkippedBinds={Renderer.TextureHandler.Skipped} SkippedUses={Renderer.ShaderHandler.Skipped} CulledObjects = {DrawManager.CulledObjectsCount}/{DrawManager.CullableObjectsCount}  Cache={CacheManager.CachedColors.Count}|{CacheManager.CachedExtradata.Count} Pitch={player.View.TargetPitch}";

                Renderer.TextureHandler.ResetStats();
                Renderer.ShaderHandler.ResetStats();
                _debugText.Text = text;
                _passedTime += Time.IndependentDeltaTime;
                if (_passedTime > 5.0f)
                {
                    _passedTime = 0;
                    _staticPool.TextureElement.TextureId = Graphics2D.LoadTexture(new BitmapObject
                    {
                        Bitmap = WorldRenderer.StaticBuffer.Visualize(),
                        Path = "Debug:GeometryPool"
                    });
                    _waterPool.TextureElement.TextureId = Graphics2D.LoadTexture(new BitmapObject
                    {
                        Bitmap = WorldRenderer.WaterBuffer.Visualize(),
                        Path = "Debug:WaterGeometryPool"
                    });
                    _instancePool.TextureElement.TextureId = Graphics2D.LoadTexture(new BitmapObject
                    {
                        Bitmap = WorldRenderer.InstanceBuffer.Visualize(),
                        Path = "Debug:InstanceGeometryPool"
                    });
                    var borderWidth = (chunkBound-1) * Chunk.Height * 8;
                    _voxelCount = (int) World.Chunks.Select(
                        C => (defaultVoxelCount - borderWidth) / C.Landscape.GeneratedLod + borderWidth).Sum();
                    _chunkCount = Math.Max(World.Chunks.Count, 1);
                }
            }
            else
            {
                _debugPanel.Disable();
            }

            if (_depthMode)
            {
                _depthTexture.TextureElement.TextureId = DrawManager.MainBuffer.Ssao.FirstPass.TextureId[0];
                _depthTexture.Enable();
            }
            else
            {
                _depthTexture.Disable();
            }
            if (_fpsOnTitle)
            {
                Program.GameWindow.Title = $"{_originalTitle} FPS={Time.Framerate} MS={Time.Frametime}";
            }
            else if(Program.GameWindow.Title != _originalTitle)
            {
                Program.GameWindow.Title = _originalTitle;
            }
        }

        public void Draw()
        {
            if (GameSettings.DebugView && _extraDebugView)
            {
                var player = GameManager.Player;
                var underChunk = World.GetChunkAt(player.Position);

                if (underChunk != null)
                {/*
                    for (var x = 0; x < Chunk.Width / Chunk.BlockSize; x++)
                    {
                        for (var z = 0; z < Chunk.Width / Chunk.BlockSize; z++)
                        {
                            var basePosition = new Vector3(x * Chunk.BlockSize + underChunk.OffsetX,
                                Physics.HeightAtPosition(x * Chunk.BlockSize + underChunk.OffsetX,
                                    z * Chunk.BlockSize + underChunk.OffsetZ), z * Chunk.BlockSize + underChunk.OffsetZ);
                            var normal = Physics.NormalAtPosition(basePosition);

                            BasicGeometry.DrawLine(basePosition, basePosition + normal * 2, Colors.Yellow);
                        }
                    }*/
                }
                var collisions = new List<ICollidable>();
                if (player.NearCollisions != null)
                    collisions.AddRange(player.NearCollisions);

                for (int i = 0; i < collisions.Count; i++)
                {
                    if (!(collisions[i] is CollisionShape shape)) return;

                    var pshape = player.Model.BroadphaseCollider;

                    float radiiSum = shape.BroadphaseRadius + pshape.BroadphaseRadius;

                    BasicGeometry.DrawShape(shape,
                        (pshape.BroadphaseCenter - shape.BroadphaseCenter).LengthSquared < radiiSum * radiiSum
                            ? Colors.White
                            : Colors.Red);
                }

                if (false)
                {
                    
                    BasicGeometry.DrawLine(player.Position + Vector3.UnitZ * 2f, player.Position + Vector3.UnitZ * 4f,
                        Colors.Blue);

                    BasicGeometry.DrawLine(player.Position - Vector3.UnitZ * 2f, player.Position - Vector3.UnitZ * 4f,
                        Colors.BlueViolet);

                    BasicGeometry.DrawLine(player.Position + Vector3.UnitX * 2f, player.Position + Vector3.UnitX * 4f,
                        Colors.Red);

                    BasicGeometry.DrawLine(player.Position - Vector3.UnitX * 2f, player.Position - Vector3.UnitX * 4f,
                        Colors.OrangeRed);

                    BasicGeometry.DrawLine(player.Position + player.Orientation * 2f, player.Position + player.Orientation * 4f,
                        Colors.Yellow);

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
                    //BasicGeometry.DrawBox(GameManager.Player.Model.BaseBroadphaseBox.Min, GameManager.Player.Model.BaseBroadphaseBox.Max - GameManager.Player.Model.BaseBroadphaseBox.Min);
                    Renderer.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);

                    if (GameManager.Player.LeftWeapon != null &&
                        GameManager.Player.LeftWeapon is MeleeWeapon melee)
                    {
                        Renderer.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
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

        public void Dispose()
        {
            EventDispatcher.UnregisterKeyDown(this);
        }
    }
}