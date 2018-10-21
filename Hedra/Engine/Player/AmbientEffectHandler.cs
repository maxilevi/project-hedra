using System;
using Hedra.Engine.Game;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.Sound;
using OpenTK;

namespace Hedra.Engine.Player
{
    public class AmbientEffectHandler : IAmbientEffectHandler, IDisposable
    {
        private const float MaxRange = 32;
        private readonly AreaSound _areaSound;
        private readonly IPlayer _player;
        private Vector3 _lastPosition;
        private float _nearestWater;
        private bool _wasAnyNull;

        public AmbientEffectHandler(IPlayer Player)
        {
            _player = Player;
            _areaSound = new AreaSound(SoundType.River, Vector3.Zero, 16f);
        }
        
        public void Update()
        {
            _areaSound.Update(_nearestWater < MaxRange);
            _areaSound.Type = GameSettings.UnderWaterEffect ? SoundType.Underwater : SoundType.River;
            if ((_lastPosition - _player.Position).LengthSquared < 0.25f && !_wasAnyNull) return;
            _areaSound.Position = _player.Position;
            _nearestWater = NearestWaterBlock();
            _areaSound.Volume = (1-Math.Min(_nearestWater, MaxRange) / MaxRange)*.1f;
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
                    var dist = NearestWaterBlockOnChunk(chunk);
                    if (dist < nearest) nearest = dist;
                } 
            }
            return (float) Math.Sqrt(nearest);
        }

        private float NearestWaterBlockOnChunk(Chunk UnderChunk)
        {
            var nearest = float.MaxValue;
            var positions = UnderChunk.GetWaterPositions();
            for (var i = 0; i < positions.Length; i++)
            {
                var realPosition = positions[i].ToVector3() * Chunk.BlockSize + UnderChunk.Position;
                var dist = (realPosition - _player.Position).LengthSquared;
                if (dist < nearest) nearest = dist;
            }
            return nearest;
        }

        public void Dispose()
        {
            _areaSound.Dispose();
        }
    }
}