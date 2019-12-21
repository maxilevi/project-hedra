import MissionCore
import FarmCore
import VisualEffects
from System import Single
from System.Numerics import Vector3, Vector4
from Core import translate, load_translation
from Hedra import World
from Hedra.AISystem import IBasicAIComponent
from Hedra.Components import TalkComponent, DamageComponent
from Hedra.Mission import MissionBuilder, QuestTier, DialogObject, QuestReward
from Hedra.Mission.Blocks import FindStructureMission, TalkMission, DefeatEntityMission
from Hedra.Engine.StructureSystem.Overworld import CottageWithFarmDesign
from Hedra.AISystem.Humanoid import FollowAIComponent


IS_QUEST = True
QUEST_NAME = 'PossessedCows'
QUEST_TIER = QuestTier.Medium
MAX_SPAWN_DISTANCE = 256

def setup_timeline(position, giver, owner, rng):
    builder = MissionBuilder()

    # Don't allow 2 quests of the same type.
    if MissionCore.contains_quest(owner, QUEST_NAME):
        return None

    farm_structure = MissionCore.find_structure(position, CottageWithFarmDesign)
    farm = farm_structure.WorldObject

    builder.MissionStart += lambda: on_mission_start(giver, owner)
    builder.FailWhen = lambda: giver.IsDead or not MissionCore.is_within_distance(giver.Position, farm_structure.Position)
    builder.MissionDispose += lambda: MissionCore.remove_component_if_exists(giver, FollowAIComponent)

    farm.EnsureEmpty()
    cows = spawn_possessed_cows(farm.Position, rng)

    find = FindStructureMission()
    find.Design = farm_structure.Design
    find.Position = farm_structure.Position
    find.SetDescription(translate('quest_possessed_cows_description', giver.Name))
    find.OverrideOpeningDialog(MissionCore.create_dialog('quest_possessed_cows_dialog'))
    find.MissionBlockEnd += lambda: on_farm_arrived(giver, owner, cows, rng)
    builder.Next(find)
    
    for cow in cows:
        defeat = DefeatEntityMission(cow)
        defeat.SetDescription('quest_defeat_possessed_cows_description', len(cows))
        builder.Next(defeat)

    builder.SetReward(FarmCore.get_reward(rng))
    return builder

def spawn_possessed_cows(farm_position, rng):
    cows = []
    count = rng.Next(2, 6)
    for i in range(count):
        position = farm_position.Position + Vector3(
            Single(rng.NextDouble() * MAX_SPAWN_DISTANCE * 2 - MAX_SPAWN_DISTANCE),
            Single(0.0),
            Single(rng.NextDouble() * MAX_SPAWN_DISTANCE * 2 - MAX_SPAWN_DISTANCE)
        )
        cow = World.SpawnMob('Cow', position, rng)
        cow.Name = translate('possessed_cow')
        MissionCore.remove_component_if_exists(cow, IBasicAIComponent)
        VisualEffects.set_outline(cow, Vector4(1.0, 0.0, 0.0, 1.0), True)
        # Make cow hostile
        cows.append(cow)
    return cows

def on_farm_arrived(giver, owner, enemies, rng):
    MissionCore.remove_component_if_exists(giver, TalkComponent)
    
    talk = TalkComponent(giver)
    giver.AddComponent(talk)
    
    talk.AddDialogLine(load_translation('quest_steal_from_witch_kill'))
    talk.AutoRemove = True
    talk.TalkToPlayer()

    for enemy in enemies:
        enemy.SearchComponent[CombatAIComponent]().SetTarget(owner if rng.Next(0, 2) == 1 else giver)

def make_follow(giver, target):
    MissionCore.remove_component_if_exists(giver, IBasicAIComponent)
    giver.AddComponent(FollowAIComponent(giver, target))

def on_mission_start(giver, target):
    make_follow(giver, target)
    giver.SearchComponent[DamageComponent]().Immune = False
    giver.SearchComponent[DamageComponent]().Ignore(lambda x: x == target)


def can_give(position):
    return len(MissionCore.nearby_structs_designs(position, CottageWithFarmDesign)) > 0