using Hedra.Engine.Player;
using Hedra.Engine.Player.QuestSystem.Views;
using Hedra.EntitySystem;
using System.Numerics;

namespace Hedra.Mission.Blocks
{
    public delegate void OnMissionBlockEnd();

    public delegate void OnMissionBlockStart();
    
    public abstract class MissionBlock
    {
        public event OnMissionBlockEnd MissionBlockEnd;
        public event OnMissionBlockStart MissionBlockStart;
        
        private DialogObject _newDialog;
        public IPlayer Owner { get; set; }
        public IHumanoid Giver { get; set; }
        public abstract bool IsCompleted { get; }
        public abstract void Setup();
        public abstract QuestView BuildView();
        public abstract bool HasLocation { get; }
        public virtual Vector3 Location { get; }
        public abstract string ShortDescription { get; }
        public abstract string Description { get; }
        public abstract DialogObject DefaultOpeningDialog { get; }
        public void OverrideOpeningDialog(DialogObject Dialog)
        {
            _newDialog = Dialog;
        }

        public DialogObject OpeningDialog => _newDialog ?? DefaultOpeningDialog;

        public void Start()
        {
            MissionBlockStart?.Invoke();
        }
        
        public virtual void Cleanup()
        {
            MissionBlockEnd?.Invoke();
        }
        
        public virtual void Dispose()
        {
            
        }
    }
}