using System;

namespace Hedra.Engine.QuestSystem.Designs
{
    public abstract class PassthroughDesign : QuestDesign
    {
        protected override QuestReward BuildReward(QuestObject Quest, Random Rng)
        {
            return Quest.Parameters.Get<QuestReward>("QuestReward");
        }

        protected override QuestParameters BuildParameters(QuestObject Previous, QuestParameters Parameters, Random Rng)
        {
            Parameters.Set("QuestDescendants", Previous.Parameters.Get<QuestDesign[]>("QuestDescendants"));
            Parameters.Set("QuestReward", Previous.Parameters.Get<QuestReward>("QuestReward"));
            return base.BuildParameters(Previous, Parameters, Rng);
        }

        protected override QuestDesign[] GetDescendants(QuestObject Quest)
        {
            return Quest.Parameters.Get<QuestDesign[]>("QuestDescendants");
        }
    }
}