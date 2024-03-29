using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using Hedra.Engine.BiomeSystem;
using Hedra.Engine.EnvironmentSystem;
using Hedra.Engine.StructureSystem;
using Hedra.Game;

namespace Hedra.Engine.Player
{
    public delegate void OnRealmChanged(WorldType Type);

    public class RealmHandler : ISerializableHandler
    {
        public const int Overworld = 0;
        public const int GhostTown = 1;
        public const int ShroomDimension = 2;
        private readonly List<Realm> _activeRealms;
        private int _currentRealm;

        public RealmHandler()
        {
            _activeRealms = new List<Realm>();
        }

        public void Dump(BinaryWriter Writer)
        {
            var realms = _activeRealms.ToArray();
            Writer.Write(realms.Length);
            for (var i = 0; i < realms.Length; ++i) realms[i].Dump(Writer);
            Writer.Write(_currentRealm);
        }

        public void Load(BinaryReader Reader)
        {
            _activeRealms.Clear();
            var length = Reader.ReadInt32();
            for (var i = 0; i < length; ++i) _activeRealms.Add(Realm.Load(Reader));
            Go(_activeRealms[Reader.ReadInt32()]);
        }

        public event OnRealmChanged RealmChanged;

        public void Update()
        {
            if (_activeRealms.Count == 0) return;
            _activeRealms[_currentRealm].Update();
        }

        public void Create(int Seed, WorldType Type = WorldType.Overworld)
        {
            var realm = _activeRealms.FirstOrDefault(R => R.Seed == Seed && R.Type == Type);
            if (realm == null) DoCreate(Seed, Type);
        }

        private void Go(Realm Realm)
        {
            _currentRealm = _activeRealms.IndexOf(Realm);
            Realm.Go();
        }

        public void GoTo(int Index)
        {
            var currentRealm = _currentRealm;
            Go(_activeRealms[Index]);
            if (Index != currentRealm)
                RealmChanged?.Invoke(_activeRealms[_currentRealm].Type);
        }

        public bool HasRealm(int Index)
        {
            return Index > 0 && Index < _activeRealms.Count;
        }

        private Realm DoCreate(int Seed, WorldType Type)
        {
            var realm = new Realm
            {
                Type = Type,
                Seed = Seed
            };
            _activeRealms.Add(realm);
            return realm;
        }

        public void Reset()
        {
            var realms = _activeRealms.ToArray();
            for (var i = 0; i < realms.Length; ++i) realms[i].Dispose();
            _activeRealms.Clear();
        }

        private class Realm
        {
            private WorldHandler _handler;
            public int Seed { get; set; }
            public WorldType Type { get; set; }
            public float Daytime { get; set; } = 12000;
            public Vector3 Position { get; set; } = World.SpawnPoint;
            public Vector3 MarkedDirection { get; set; }

            public void Go()
            {
                World.Recreate(Seed, Type);
                SkyManager.DayTime = Daytime;
                SkyManager.LoadTime = true;
                GameManager.Player.Position = World.FindPlaceablePosition(GameManager.Player, Position);
                GameManager.Player.Minimap.UnMark();
                GameManager.Player.Questing.Trigger();
                if (MarkedDirection != Vector3.Zero)
                    GameManager.Player.Minimap.Mark(MarkedDirection);
                _handler?.Dispose();
                _handler = WorldHandler.Create(Type);
            }

            public void Update()
            {
                Position = GameManager.Player.Position;
                MarkedDirection = GameManager.Player.Minimap.MarkedDirection;
                _handler.Update();
            }

            public void Dump(BinaryWriter Writer)
            {
                Writer.Write(Seed);
                Writer.Write((int)Type);
                Writer.Write(SkyManager.DayTime);
                Writer.Write(SanitizePosition(Position));
                Writer.Write(MarkedDirection);
            }

            public static Realm Load(BinaryReader Reader)
            {
                return new Realm
                {
                    Seed = Reader.ReadInt32(),
                    Type = (WorldType)Reader.ReadInt32(),
                    Daytime = Reader.ReadSingle(),
                    Position = Reader.ReadVector3(),
                    MarkedDirection = Reader.ReadVector3()
                };
            }

            private Vector3 SanitizePosition(Vector3 Location)
            {
                var structures = StructureHandler.GetNearStructures(Location);
                for (var i = 0; i < structures.Length; ++i)
                {
                    if (structures[i].Design.CanSpawnInside) continue;
                    Location = World.FindSpawningPoint(Location);
                    Location = World.FindPlaceablePosition(GameManager.Player, Location);
                    break;
                }

                return Location;
            }

            public void Dispose()
            {
                _handler?.Dispose();
            }
        }
    }
}