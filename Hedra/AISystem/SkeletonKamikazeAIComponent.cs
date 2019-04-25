using Hedra.Engine;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Management;
using Hedra.Engine.Player;
using Hedra.EntitySystem;

namespace Hedra.AISystem
{
    public class SkeletonKamikazeAIComponent : HostileAIComponent
    {
        private readonly QuadrupedModel _model;
        private bool _isExploding;
        private bool _exploded;
        
        public SkeletonKamikazeAIComponent(Entity Parent) : base(Parent)
        {
            Parent.SearchComponent<DamageComponent>().PlayDeleteAnimation = false;
            _model = (QuadrupedModel) Parent.Model;
            Parent.BeforeDamaging += BeforeDamaging;
        }

        private void BeforeDamaging(IEntity Victim, float Damage)
        {
            if (_isExploding || _exploded) return;
            _isExploding = true;
            _model.DisposeAnimation();
            Firewave.Create(Parent, Parent.AttackDamage);
        }

        public override void Update()
        {
            base.Update();
            if (_isExploding)
            {
                _model.DisposeTime += Time.DeltaTime * 16f;
                if (_model.DisposeTime > 4)
                {
                    _isExploding = false;
                    _exploded = true;
                    Kill();
                }
            }
        }

        private void Kill()
        {
            Executer.ExecuteOnMainThread(() => Parent.Dispose());
        }
    }
}