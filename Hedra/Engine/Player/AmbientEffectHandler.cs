using System;
using System.Windows.Forms;
using Hedra.Core;
using Hedra.Engine.Game;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Geometry;
using Hedra.Engine.Sound;
using Hedra.Game;
using Hedra.Sound;
using OpenTK;

namespace Hedra.Engine.Player
{
    public class AmbientEffectHandler : IAmbientEffectHandler, IDisposable
    {
        private const float MaxRange = 32;
        private readonly AreaSound _riverAreaSound;
        private readonly IPlayer _player;
        private Vector3 _lastPosition;
        private float _nearestWater;
        private bool _wasAnyNull;

        public AmbientEffectHandler(IPlayer Player)
        {
            _player = Player;
            _riverAreaSound = new AreaSound(SoundType.River, Vector3.Zero, 16f);
        }
        
        public void Update()
        {
            HandleRiverSounds();
            HandleSwimmingEffects();
        }

        private void HandleSwimmingEffects()
        {
            var underBlock0 = World.GetBlockAt(_player.View.CameraEyePosition * new Vector3(1, 1f / Chunk.BlockSize, 1) + Vector3.UnitY * (0 + IsoSurfaceCreator.WaterQuadOffset));
            var underBlock1 = World.GetBlockAt(_player.View.CameraEyePosition * new Vector3(1, 1f / Chunk.BlockSize, 1) + Vector3.UnitY * (1 + IsoSurfaceCreator.WaterQuadOffset));
            var underBlock2 = World.GetBlockAt(_player.View.CameraEyePosition * new Vector3(1, 1f / Chunk.BlockSize, 1) + Vector3.UnitY * (2 + IsoSurfaceCreator.WaterQuadOffset));
            var underBlock3 = World.GetBlockAt(_player.View.CameraEyePosition * new Vector3(1, 1f / Chunk.BlockSize, 1) + Vector3.UnitY * (3 + IsoSurfaceCreator.WaterQuadOffset));
            var lowestY = World.GetLowestY( (int) _player.View.CameraEyePosition.X, (int) _player.View.CameraEyePosition.Z);
            
            if(underBlock0.Type != BlockType.Water 
               && _player.View.CameraEyePosition.Y / Chunk.BlockSize >= lowestY + 2 &&  underBlock1.Type != BlockType.Water && underBlock2.Type != BlockType.Water && underBlock3.Type != BlockType.Water)
            {
                GameSettings.DistortEffect = false;
                GameSettings.UnderWaterEffect = false;
                WorldRenderer.ShowWaterBackfaces = false;
            }

            if(underBlock0.Type == BlockType.Water 
               || _player.View.CameraEyePosition.Y / Chunk.BlockSize <= lowestY + 2 && (underBlock1.Type == BlockType.Water || underBlock2.Type == BlockType.Water || underBlock3.Type == BlockType.Water))
            {
                GameSettings.UnderWaterEffect = true;
                GameSettings.DistortEffect = true;
                WorldRenderer.ShowWaterBackfaces = true;
            }
        }
        
        private void HandleRiverSounds()
        {
            _riverAreaSound.Update(_nearestWater < MaxRange);
            _riverAreaSound.Type = GameSettings.UnderWaterEffect ? SoundType.Underwater : SoundType.River;
            if ((_lastPosition - _player.Position).LengthSquared < 1f && !_wasAnyNull) return;
            _riverAreaSound.Position = _player.Position;
            _nearestWater = NearestWaterBlock();
            _riverAreaSound.Volume = (1-Math.Min(_nearestWater, MaxRange) / MaxRange)*.1f;
            _lastPosition = _player.Position;
        }

        private float NearestWaterBlock()
        {
            var nearest = Math.Pow(MaxRange+1, 2);
            _wasAnyNull = false;
            for (var x = -1; x < 2; x++)
            {
                for (var z = -1; z < 2; z++)
                {
                    var chunk = World.GetChunkAt(_player.Position + new Vector3(x, 0, z) * Chunk.Width);
                    if (chunk == null || !chunk.BuildedWithStructures || !chunk.HasWater)
                    {
                        _wasAnyNull = chunk == null || !chunk.BuildedWithStructures;
                        continue;
                    }
                    var dist = NearestWaterBlockOnChunk(chunk, _player.Position);
                    if (dist < nearest) nearest = dist;
                } 
            }
            return (float) Math.Sqrt(nearest);
        }
        
        private static float NearestWaterBlockOnChunk(Chunk UnderChunk, Vector3 Position)
        {
            var nearest = float.MaxValue;
            var positions = UnderChunk.GetWaterPositions();
            for (var i = 0; i < positions.Length; i++)
            {
                var realPosition = positions[i].ToVector3() * Chunk.BlockSize + UnderChunk.Position;
                var dist = (realPosition - Position).LengthSquared;
                if (dist < nearest) nearest = dist;
            }
            return nearest;
        }

        public void Dispose()
        {
            _riverAreaSound.Dispose();
        }
    }
}