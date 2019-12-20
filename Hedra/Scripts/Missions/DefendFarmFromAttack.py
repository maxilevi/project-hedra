import MissionCore
import FarmCore
import clr
from System import Array, Object, Single
from System.Numerics import Vector3
from Hedra.Mission import MissionBuilder, QuestTier, QuestHint, DialogObject, QuestReward, ItemCollect, QuestPriority
from Hedra.Mission.Blocks import DefendMission, DefeatEntityMission, CollectMission
from Hedra.AISystem.Humanoid import EscapeAIComponent, CombatAIComponent
from Hedra.AISystem import IBasicAIComponent
from Hedra.Engine.WorldBuilding import NPCCreator, BanditOptions, NameGenerator
from Hedra.Engine.EntitySystem import DropComponent
from Hedra.EntitySystem import IEntity
from Hedra.Numerics import VectorExtensions

clr.ImportExtensions(VectorExtensions)

IS_QUEST = True
QUEST_NAME = 'DefendFarmFromAttack'
QUEST_TIER = QuestTier.Medium
QUEST_HINT = QuestHint.Farm
MAX_SPAWN_DISTANCE = 256
MIN_SPAWN_DISTANCE = 64

def setup_timeline(position, giver, owner, rng):
    builder = MissionBuilder()
    builder.OpeningDialog = MissionCore.create_dialog('quest_defend_farm_from_attack_dialog')

    criminals = create_criminals(owner, giver, rng)
    
    defend = DefendMission(giver, criminals)
    defend.MissionBlockStart += lambda: setup_giver(giver)
    builder.Next(defend)

    reward = FarmCore.get_reward(rng)
    builder.SetReward(reward)
    return builder

def setup_giver(giver):
    MissionCore.remove_component_if_exists(giver, IBasicAIComponent)
    #giver.AddComponent()

def create_criminals(owner, giver, rng):
    criminals = []
    count = rng.Next(2, 6)
    for i in range(count):
        position = giver.Position
        while (position - giver.Position).LengthSquared() < MIN_SPAWN_DISTANCE ** 2:
            position = giver.Position + Vector3(
                Single(rng.NextDouble() * MAX_SPAWN_DISTANCE * 2 - MAX_SPAWN_DISTANCE),
                Single(0.0),
                Single(rng.NextDouble() * MAX_SPAWN_DISTANCE * 2 - MAX_SPAWN_DISTANCE)
            )
        bandit = NPCCreator.SpawnBandit(position, max(1, owner.Level - rng.Next(0, 5)), BanditOptions.Default)
        bandit.SearchComponent[CombatAIComponent]().SetTarget(giver)
        bandit.RemoveComponent(bandit.SearchComponent[DropComponent]())
        criminals.append(bandit)
    return Array[IEntity](criminals)

    

def can_give(position):
    return FarmCore.can_give(position)