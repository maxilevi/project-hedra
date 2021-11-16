from Core import translate, load_translation
from Hedra.AISystem import IBasicAIComponent
from Hedra.AISystem.Humanoid import FollowAIComponent
from Hedra.Components import TalkComponent, DamageComponent
from Hedra.Engine.CacheSystem import CacheItem
from Hedra.Engine.EnvironmentSystem import SkyManager
from Hedra.Engine.StructureSystem.Overworld import CottageWithVegetablePlotDesign
from Hedra.Engine.WorldBuilding import NPCCreator, BanditOptions
from Hedra.EntitySystem import IEntity
from Hedra.Mission import MissionBuilder, QuestTier
from Hedra.Mission.Blocks import FindStructureMission, TalkMission, DefeatEntityMission, WaitForTimeMission
from System import Array

import FarmCore
import MissionCore

IS_QUEST = True
QUEST_NAME = 'OccupiedHouse'
QUEST_TIER = QuestTier.Medium


def setup_timeline(position, giver, owner, rng):
    builder = MissionBuilder()

    # Don't allow 2 quests of the same type.
    if MissionCore.contains_quest(owner, QUEST_NAME):
        return None

    cottage_structure = MissionCore.find_and_bind_structure(builder, position, CottageWithVegetablePlotDesign)
    cottage = cottage_structure.WorldObject

    bandits = spawn_bandits(builder, cottage, owner, rng)
    cottage.Setup(cottage_structure)

    is_day = not SkyManager.IsNight
    wait = WaitForTimeMission(0, 100.0)

    find = FindStructureMission()
    find.MissionBlockStart += lambda: cottage.MakeEmpty()
    find.DefaultIcon = CacheItem.WindmillIcon
    find.Design = cottage_structure.Design
    find.Position = cottage_structure.Position
    find.DistanceMultiplier = 3.25
    find.SetDescription(translate('quest_occupied_house_description', giver.Name))
    find.OverrideOpeningDialog(MissionCore.create_dialog('quest_occupied_house_dialog'))
    find.MissionBlockEnd += lambda: on_arrived(giver, owner, is_day)

    defeat = DefeatEntityMission(Array[IEntity](bandits))
    defeat.MissionBlockEnd += lambda: MissionCore.make_follow(giver, owner)

    talk = TalkMission(MissionCore.create_dialog('quest_occupied_house_end_wait'))
    talk.Humanoid = giver

    builder.Next(find)
    if is_day:
        builder.Next(wait)
        builder.Next(talk)
    builder.Next(defeat)

    builder.SetReward(FarmCore.get_reward(rng))
    builder.MissionStart += lambda: on_mission_start(giver, owner)
    builder.FailWhen = lambda: fail_when(giver, cottage_structure)
    builder.MissionDispose += lambda: on_dispose(giver, is_day, wait)
    return builder


def on_dispose(giver, is_day, wait):
    MissionCore.remove_component_if_exists(giver, FollowAIComponent)
    if is_day:
        wait.Pop()


def fail_when(giver, cottage_structure):
    return giver.IsDead or not MissionCore.is_within_distance(giver.Position, cottage_structure.Position)


def spawn_bandits(builder, cottage, owner, rng):
    bandits = []
    count = len(cottage.BanditPositions)
    for i in range(count):
        position = cottage.BanditPositions[i]
        bandit = NPCCreator.SpawnBandit(position, owner.Level + rng.Next(1, 6), BanditOptions.Quest(builder))
        bandit.SearchComponent[DamageComponent]().Ignore(lambda x: x in bandits)
        bandits.append(bandit)
    return bandits


def on_arrived(giver, owner, is_day):
    MissionCore.remove_component_if_exists(giver, TalkComponent)
    MissionCore.remove_component_if_exists(giver, IBasicAIComponent)

    talk = TalkComponent(giver)
    giver.AddComponent(talk)

    if is_day:
        talk.AddDialogLine(load_translation('quest_occupied_house_arrive'))
        owner.IsSitting = True
        giver.IsSitting = True
    else:
        talk.AddDialogLine(load_translation('quest_occupied_house_end_wait_0'))

    talk.AutoRemove = True
    talk.TalkToPlayer()


def on_mission_start(giver, target):
    MissionCore.make_follow(giver, target)
    giver.SearchComponent[DamageComponent]().Immune = False
    giver.SearchComponent[DamageComponent]().Ignore(lambda x: x == target)


def can_give(position):
    return len(MissionCore.nearby_structs_designs(position, CottageWithVegetablePlotDesign)) > 0 \
           and not MissionCore.is_inside_structure(position, CottageWithVegetablePlotDesign)
