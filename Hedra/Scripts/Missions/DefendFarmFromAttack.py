import MissionCore
import FarmCore
import clr
import Core
from System import Array, Object, Single
from System.Numerics import Vector3
from Hedra.Mission import MissionBuilder, QuestTier, QuestHint, DialogObject, QuestReward, ItemCollect, QuestPriority
from Hedra.Mission.Blocks import DefendMission, DefeatEntityMission, CollectMission
from Hedra.AISystem.Humanoid import EscapeAIComponent, CombatAIComponent
from Hedra.AISystem import IBasicAIComponent
from Hedra.Engine.WorldBuilding import NPCCreator, BanditOptions, NameGenerator
from Hedra.Engine.EntitySystem import DropComponent
from Hedra.EntitySystem import IEntity
from Hedra.Components import DamageComponent
from Hedra.Numerics import VectorExtensions

clr.ImportExtensions(VectorExtensions)

IS_QUEST = True
QUEST_NAME = 'DefendFarmFromAttack'
QUEST_TIER = QuestTier.Medium
QUEST_HINT = QuestHint.Farm
MAX_SPAWN_DISTANCE = 384
MIN_SPAWN_DISTANCE = 96

def setup_timeline(position, giver, owner, rng):
    builder = MissionBuilder()
    builder.OpeningDialog = MissionCore.create_dialog('quest_defend_farm_from_attack_dialog')

    criminals, ais = create_criminals(builder, owner, giver, rng)
    
    defend = DefendMission(giver, criminals)
    defend.MissionBlockStart += lambda: setup_giver_and_bandits(giver, criminals, ais)
    builder.Next(defend)

    reward = FarmCore.get_reward(rng)
    builder.SetReward(reward)
    return builder

def setup_giver_and_bandits(giver, criminals, ais):
    MissionCore.remove_component_if_exists(giver, IBasicAIComponent)
    giver.SearchComponent[DamageComponent]().Immune = False
    giver.BonusHealth = giver.MaxHealth * 4
    giver.Health = giver.MaxHealth
    indexes = sorted(list(range(len(criminals))), key=lambda x: -(criminals[x].Position - giver.Position).LengthSquared())
    for i in indexes:
        ai = ais[i]
        criminal = criminals[i]
        Core.after_seconds(5 * i + 1, lambda cmp=ai, cri=criminal: cri.AddComponent(cmp))

def create_criminals(builder, owner, giver, rng):
    criminals = []
    ais = []
    count = rng.Next(2, 6)
    for i in range(count):
        position = giver.Position
        while (position - giver.Position).LengthSquared() < MIN_SPAWN_DISTANCE ** 2:
            position = giver.Position + Vector3(
                Single(rng.NextDouble() * MAX_SPAWN_DISTANCE * 2 - MAX_SPAWN_DISTANCE),
                Single(0.0),
                Single(rng.NextDouble() * MAX_SPAWN_DISTANCE * 2 - MAX_SPAWN_DISTANCE)
            )
        bandit = NPCCreator.SpawnBandit(position, max(1, owner.Level - rng.Next(0, 5)), BanditOptions.Quest(builder))
        ai = bandit.SearchComponent[CombatAIComponent]()
        ai.SetGuardSpawnPoint(False)
        ai.SetCanExplore(False)
        ai.SetTarget(giver)
        ai.CanForgetTargets = False
        bandit.RemoveComponent(ai, False)
        bandit.RemoveComponent(bandit.SearchComponent[DropComponent]())
        bandit.SearchComponent[DamageComponent]().Ignore(lambda x: x in criminals)
        criminals.append(bandit)
        ais.append(ai)
    return Array[IEntity](criminals), ais

    

def can_give(position):
    return FarmCore.can_give(position)