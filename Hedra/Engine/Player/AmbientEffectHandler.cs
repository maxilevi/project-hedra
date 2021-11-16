using System;
using System.Collections.Generic;
using System.Linq;

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
using System.Numerics;

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
            var underBlock0 = World.GetBlockAt(_player.View.CameraEyePosition);

            if(underBlock0.Type == BlockType.Water || underBlock0.Type == BlockType.Seafloor)
            {
                GameSettings.UnderWaterEffect = true;
                GameSettings.DistortEffect = true;
                WorldRenderer.ShowWaterBackfaces = true;
            }
            else
            {
                GameSettings.DistortEffect = false;
                GameSettings.UnderWaterEffect = false;
                WorldRenderer.ShowWaterBackfaces = false;
            }
        }
        
        private void HandleRiverSounds()
        {
            _riverAreaSound.Update(_nearestWater < MaxRange);
            _riverAreaSound.Type = GameSettings.UnderWaterEffect ? SoundType.Underwater : SoundType.River;
            const float errorMargin = (Chunk.BlockSize * 2) * (Chunk.BlockSize * 2);
            if ((_lastPosition - _player.Position).LengthSquared() < errorMargin && !_wasAnyNull) return;
            _riverAreaSound.Position = _player.Position;
            _nearestWater = NearestWaterBlock();
            _riverAreaSound.Volume = (1-Math.Min(_nearestWater, MaxRange) / MaxRange) * .75f;
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
                    var dist = World.NearestWaterBlockOnChunk(chunk, _player.Position, out _);
                    if (dist < nearest) nearest = dist;
                } 
            }
            return (float) Math.Sqrt(nearest);
        }

        public void Dispose()
        {
            _riverAreaSound.Dispose();
        }
    }
}