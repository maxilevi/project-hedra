import MissionCore
from Core import translate
from System import Array, Object, Single
from System.Numerics import Vector3
from Hedra.Items import ItemPool, ItemTier
from Hedra.Mission import MissionBuilder, QuestTier, DialogObject, QuestReward, ItemCollect, QuestPriority
from Hedra.Mission.Blocks import FindEntityMission, DefeatEntityMission, CollectMission
from Hedra.AISystem import IBehaviourComponent
from Hedra.AISystem.Humanoid import EscapeAIComponent, CombatAIComponent
from Hedra.Engine.WorldBuilding import NPCCreator, BanditOptions, NameGenerator
from Hedra.Engine.EntitySystem import DropComponent

IS_QUEST = True
QUEST_NAME = 'BadHarvest'
QUEST_TIER = QuestTier.Easy

def setup_timeline(position, giver, owner, rng):
    builder = MissionBuilder()
    items = create_items(rng)
    builder.OpeningDialog = MissionCore.create_dialog('', [])

    reward = create_reward(items, rng)
    builder.SetReward(reward)
    return builder

def create_items(rng):
    count = rng.Next(1, 4)
    items = []
    options = ItemPool.Matching(lambda x: (x.IsWeapon or x.IsArmor) and x.Tier <= ItemTier.Rare)
    for i in range(count):
        items.append(
            ItemPool.Grab(options[rng.Next(0, len(options))].Name)
        )
    return items

def create_reward(items, rng):
    reward = QuestReward()
    reward.Gold = rng.Next(19, 52)
    reward.Item = items[rng.Next(0, len(items))] if rng.Next(0, 3) == 1 else None
    return reward

def can_give(position):
    return True