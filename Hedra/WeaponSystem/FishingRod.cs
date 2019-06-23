using System.Collections.Generic;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering;
using Hedra.Engine.Scripting;
using Hedra.EntitySystem;
using Hedra.Rendering;
using OpenTK;

namespace Hedra.WeaponSystem
{
    public class FishingRod : Tool
    {
        private readonly Line3D _line;
        private static readonly VertexData _hook;
        private readonly Dictionary<string, object> _state;

        static FishingRod()
        {
            _hook = AssetManager.PLYLoader("Assets/Env/Objects/FishHook.ply", Vector3.One);
        }
        
        public FishingRod(VertexData Contents) : base(Contents)
        {
            _line = new Line3D();
            _state = new Dictionary<string, object>();
            Interpreter.GetFunction("Fishing.py", "configure_rod")(this);
        }
        public override uint PrimaryAttackIcon => WeaponIcons.FishingRodPrimaryAttack;
        public override uint SecondaryAttackIcon => WeaponIcons.FishingRodSecondaryAttack;
        protected override float PrimarySpeed => 1.0f;

        protected override string[] PrimaryAnimationsNames => new[]
        {
            "Assets/Chr/WarriorFish.dae",
        };

        protected override float SecondarySpeed => 1.0f;

        public override void Update(IHumanoid Human)
        {
            base.Update(Human);
            Interpreter.GetFunction("Fishing.py", "update_rod")(Human, this, _line, _state);
        }

        protected override string[] SecondaryAnimationsNames => new[]
        {
            "Assets/Chr/WarriorFish.dae"
        };

        protected override void OnPrimaryAttackEvent(AttackEventType Type, AttackOptions Options)
        {
            base.OnPrimaryAttackEvent(Type, Options);
            if(Type == AttackEventType.Mid)
                Interpreter.GetFunction("Fishing.py", "start_fishing")(Owner, _state, _hook);
        }

        protected override void OnSecondaryAttackEvent(AttackEventType Type, AttackOptions Options)
        {
            base.OnSecondaryAttackEvent(Type, Options);
        }

        public override void Attack2(IHumanoid Human, AttackOptions Options)
        {
            if(Interpreter.GetFunction("Fishing.py", "retrieve_fish")(Owner, _state))
                base.Attack2(Human, Options);
        }
        
        public override void Attack1(IHumanoid Human, AttackOptions Options)
        {
            if (Human.IsFishing)
                Interpreter.GetFunction("Fishing.py", "disable_fishing")(Owner, _state);
            base.Attack1(Human, Options);
        }

        public override void Dispose()
        {
            base.Dispose();
            _line.Dispose();
        }
    }
}