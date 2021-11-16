using System;
using System.Collections.Generic;
using SixLabors.ImageSharp;
using SixLabors.Fonts;
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
using Hedra.Rendering.UI;
using Hedra.WeaponSystem;
using System.Numerics;
using Hedra.AISystem.Behaviours;
using Hedra.Engine.Bullet;
using Hedra.Engine.Core;
using Silk.NET.Input;


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
            _debugText = new GUIText(string.Empty, new Vector2(.65f, -.5f), Color.Black, FontCache.GetNormal(12));
            _staticPool = new BackgroundTexture(0, new Vector2(0f, 0.90f),
                new Vector2(1024f / GameSettings.Width, 48f / GameSettings.Height));
            _waterPool = new BackgroundTexture(0, new Vector2(0f, 0.80f),
                new Vector2(1024f / GameSettings.Width, 16f / GameSettings.Height));
            _instancePool = new BackgroundTexture(0, new Vector2(0f, 0.75f),
                new Vector2(1024f / GameSettings.Width, 16f / GameSettings.Height));
            _debugPanel.AddElement(_staticPool);
            _debugPanel.AddElement(_waterPool);
            _debugPanel.AddElement(_instancePool);
            _debugPanel.AddElement(_debugText);
            _debugPanel.Disable();
            _originalTitle = Program.GameWindow.Title;
            Log.WriteLine("Created debug elements.");

#if DEBUG
            if (OSManager.RunningPlatform == Platform.Windows) GameLoader.EnableGLDebug();

            EventDispatcher.RegisterKeyDown(this, delegate(object Sender, KeyEventArgs Args)
            {
                if (Args.Key == Key.F7 && GameSettings.DebugView) _depthMode = !_depthMode;

                if (Args.Key == Key.F8 && GameSettings.DebugView) _extraDebugView = !_extraDebugView;

                if (Args.Key == Key.F12 && GameSettings.DebugView) _fpsOnTitle = !_fpsOnTitle;
            });
#endif
        }

        public void Update()
        {
            var player = GameManager.Player;
            var chunkSpace = World.ToChunkSpace(player.Position);
            if (GameSettings.DebugView)
            {
                _debugPanel.Enable();
                var underChunk = World.GetChunkByOffset(chunkSpace);
                var chunkBound = Chunk.Width / Chunk.BlockSize;
                var chunkCount = World.Chunks.Count;
                var maxMemory = (int)(Chunk.BoundsZ * Chunk.BoundsX * Chunk.BoundsY * 2 * chunkCount / 1024f / 1024f);
                var blockMemory = World.Chunks.Sum(C => C.MemoryUsed) / 1024 / 1024;
                var compressedChunks = World.Chunks.Sum(C => C.IsCompressed ? 1 : 0);
                var uncompressedChunks = World.Chunks.Count - compressedChunks;
                var block = World.GetBlockAt(player.Position);
                var lineBreak = $"{Environment.NewLine}{Environment.NewLine}";
                var text =
                    $"X = {(int)player.Position.X} Y = {(int)player.Position.Y} Z={(int)player.Position.Z} Routines={RoutineManager.Count} Watchers={player.Loader.WatcherCount} Grounded={player.IsGrounded}";
                text +=
                    $"{lineBreak}DrawCalls={DrawManager.DrawCalls} VBOUpdates={VBO.VBOUpdatesInLastFrame} Chunks={chunkCount} ChunkX={underChunk?.OffsetX ?? 0} ChunkZ={underChunk?.OffsetZ ?? 0}";
                text +=
                    $"{lineBreak}Cache={CacheManager.UsedBytes / 1024 / 1024}MB DynCache={DynamicCache.UsedBytes / 1024 / 1024} MB WaterMap={ChunkTerrainMeshBuilder.WaterMappingsCount} WaterChunkMap={ChunkTerrainMeshBuilder.ChunkWaterMapCount} Bullet={BulletPhysics.UsedBytes / 1024 / 1024}MB";
                text +=
                    $"{lineBreak}Textures ={TextureRegistry.Count} Fonts={FontCache.Count} Texts={TextCache.Count} VAO={VAO.Alive} VBO={VBOCache.CachedVBOs}/{VBO.Alive} FBO={FBO.Alive} Lights={ShaderManager.UsedLights}/{ShaderManager.MaxLights}";
                text +=
                    $"{lineBreak}MESH={World.Builder.AverageBuildTime}MS BLOCK={World.Builder.AverageBlockTime}MS STRUCT= {World.Builder.AverageStructureTime}MS C/U={compressedChunks}/{uncompressedChunks} MEM={blockMemory}MB / {blockMemory / (float)maxMemory * 100}%";
                text +=
                    $"{lineBreak}QUEUES = {World.Builder.MeshQueueCount} / {World.Builder.BlockQueueCount} / {World.Builder.StructureQueueCount} THREADS = {World.Builder.MeshThreads} / {World.Builder.BlockThreads} / {World.Builder.StructureThreads} Time={(int)(SkyManager.DayTime / 1000)}:{(int)((SkyManager.DayTime / 1000f - (int)(SkyManager.DayTime / 1000)) * 60):00} H={World.Entities.Count(M => M.IsHumanoid)} Items={World.WorldObjects.Length} M&H={World.Entities.Count}";
                text +=
                    $"{lineBreak}Watchers={World.StructureHandler.Watchers.Length} Structs={World.StructureHandler.Structures.Length}->{World.StructureHandler.Structures.Sum(S => S.Children.Length)} Plateaus={World.WorldBuilding.Plateaux.Length} Groundworks={World.WorldBuilding.Groundworks.Length} BType={block.Type}";
                text +=
                    $"{lineBreak}AIStorage={TraverseStorage.Instance.StorageCount} Updates={UpdateManager.UpdateCount} Seed={World.Seed} FPS={Time.Framerate} MS={Time.Frametime} BDensity={block.Density} Pitch={player.View.Pitch:0.00}";
                text +=
                    $"{lineBreak}SkippedBinds={Renderer.TextureHandler.Skipped} SkippedUses={Renderer.ShaderHandler.Skipped} CulledObjects = {DrawManager.CulledObjectsCount}/{DrawManager.CullableObjectsCount}  Cache={CacheManager.CachedColors.Count}|{CacheManager.CachedExtradata.Count} Pitch={player.View.TargetPitch}";
                VBO.VBOUpdatesInLastFrame = 0;
                Renderer.TextureHandler.ResetStats();
                Renderer.ShaderHandler.ResetStats();
                _debugText.Text = text;
                _passedTime += Time.IndependentDeltaTime;
                if (_passedTime > 5.0f && false)
                {
                    _passedTime = 0;
                    _staticPool.TextureElement.TextureId = Graphics2D.LoadTexture(new BitmapObject
                    {
                        Bitmap = WorldRenderer.StaticBuffer.Visualize(),
                        Path = "Debug:GeometryPool"
                    }, false);
                    _waterPool.TextureElement.TextureId = Graphics2D.LoadTexture(new BitmapObject
                    {
                        Bitmap = WorldRenderer.WaterBuffer.Visualize(),
                        Path = "Debug:WaterGeometryPool"
                    }, false);
                    _instancePool.TextureElement.TextureId = Graphics2D.LoadTexture(new BitmapObject
                    {
                        Bitmap = WorldRenderer.InstanceBuffer.Visualize(),
                        Path = "Debug:InstanceGeometryPool"
                    }, false);
                    var borderWidth = (chunkBound - 1) * Chunk.Height * 8;
                    _voxelCount = (int)0;
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
                Program.GameWindow.Title = $"{_originalTitle} FPS={Time.Framerate} MS={Time.Frametime}";
            else if (Program.GameWindow.Title != _originalTitle) Program.GameWindow.Title = _originalTitle;
        }

        public void Draw()
        {
            var player = GameManager.Player;
            if (GameSettings.DebugView && _extraDebugView)
            {
                var underChunk = World.GetChunkAt(player.Position);

                if (underChunk != null)
                {
                    /*
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
            }
        }

        public void Dispose()
        {
            EventDispatcher.UnregisterKeyDown(this);
        }
    }
}