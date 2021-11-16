using Hedra.Engine.EntitySystem;
using Hedra.Engine.Events;
using Hedra.Engine.Localization;
using Hedra.Engine.Player;
using Hedra.EntitySystem;
using Hedra.Localization;
using Hedra.Sound;
using SixLabors.ImageSharp;

namespace Hedra.Components
{
    public class ResetSkillPointsDialogComponent : Component<IHumanoid>
    {
        private readonly TalkComponent _talkComponent;
        private bool _accepted;
        private bool _asked;

        public ResetSkillPointsDialogComponent(IHumanoid Parent) : base(Parent)
        {
            _talkComponent = new TalkComponent(Parent);
            _talkComponent.OnTalkingStarted += BeforeTalking;
            _talkComponent.OnTalkingEnded += AfterTalking;
            Parent.AddComponent(_talkComponent);
            EventDispatcher.RegisterKeyDown(this, (_, EventArgs) =>
            {
                if (EventArgs.Key == Controls.Interact && CanAccept())
                {
                    OnAccept(LocalPlayer.Instance);
                    EventArgs.Cancel();
                }
            }, EventPriority.High);
        }

        private void BeforeTalking(IEntity Talkee)
        {
            _asked = false;
            if (!_accepted)
            {
                if (!(Talkee is LocalPlayer talkee)) return;
                var neededGold = CalculatePrice(talkee);
                if (neededGold != 0)
                {
                    if (talkee.Gold >= neededGold)
                        _talkComponent.AddDialogLine(Translation.Create("reset_skill_points_dialog", neededGold));
                    else
                        _talkComponent.AddDialogLine(Translation.Create("cannot_reset_skill_points_dialog",
                            neededGold));
                }
                else
                {
                    _talkComponent.AddDialogLine(Translation.Create("no_skill_points_reset_dialog"));
                }
            }
            else
            {
                _talkComponent.AddDialogLine(Translation.Create("skill_points_reset_dialog"));
            }
        }

        private void AfterTalking(IEntity Talkee)
        {
            _talkComponent.ClearDialogLines();
            if (!(Talkee is LocalPlayer talkee)) return;
            var neededGold = CalculatePrice(talkee);
            if (neededGold != 0 && talkee.Gold >= neededGold)
            {
                _asked = true;
                talkee.MessageDispatcher.ShowMessage(Translations.Get("accept_skill_change", Controls.Interact), 3,
                    Color.Gold);
            }
        }

        private void OnAccept(IPlayer Humanoid)
        {
            Humanoid.Gold -= CalculatePrice(Humanoid);
            Humanoid.AbilityTree.Reset();
            SoundPlayer.PlayUISound(SoundType.TransactionSound);
            _accepted = true;
            _asked = false;

            _talkComponent.ClearDialogLines();
            _talkComponent.AutoRemove = true;
            _talkComponent.TalkToPlayer();
        }

        private bool CanAccept()
        {
            var user = LocalPlayer.Instance;
            var neededGold = CalculatePrice(user);
            return user.Gold >= neededGold && Parent.IsNear(user, TalkComponent.TalkRadius) && _asked;
        }

        private int CalculatePrice(IPlayer Player)
        {
            return (int)(Player.AbilityTree.UsedPoints * 47.5f) + (Player.AbilityTree.HasSpecialization ? 200 : 0);
        }

        public override void Update()
        {
        }

        public override void Dispose()
        {
            base.Dispose();
            EventDispatcher.UnregisterKeyDown(this);
        }
    }
}