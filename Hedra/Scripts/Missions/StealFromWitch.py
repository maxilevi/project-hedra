import MissionCore
import clr
from Core import translate, load_translation
import System
from Hedra.AISystem import BasicAIComponent, IBasicAIComponent
from Hedra.Components import TalkComponent, DamageComponent
from Hedra.Mission import MissionBuilder, QuestTier, DialogObject, QuestReward
from Hedra.Mission.Blocks import FindStructureMission, CompleteStructureMission, TalkMission, WaitForMission
from Hedra.Engine.StructureSystem.Overworld import WitchHutDesign
from Hedra.EntitySystem import EntityExtensions
from Hedra.AISystem.Humanoid import EscapeAIComponent, FollowAIComponent, CombatAIComponent, CommandBasedAIComponent
from Hedra.Items import ItemPool
from System import Array, Object

clr.ImportExtensions(EntityExtensions)

IS_QUEST = True
QUEST_NAME = 'StealFromWitch'
QUEST_TIER = QuestTier.Medium
OUTCOME_HUT_EMPTY = 0
OUTCOME_FIGHT = 1
OUTCOME_RUNAWAY = 2
OUTCOME_SHARE = 3
OUTCOME_SCAM = 4
OUTCOME_EMPTY = 5
STEAL_DURATION = 16
POSSIBLE_REWARDS = [
    ('Bone', 7),
    ('DarkEssence', 2),
    ('EnchantedBranch', 1),
    ('GlassFlask', 16),
    ('HealthPotion', 6),
    ('ManaPotion', 6),
    ('SpeedPotion', 2),
    ('StrengthPotion', 2),
    ('StaminaPotion', 2),
]

def setup_timeline(position, giver, owner, rng):
    builder = MissionBuilder()
    
    # Don't allow 2 quests of the same type.
    if MissionCore.contains_quest(owner, QUEST_NAME): 
        return None

    witch_hut_structure = MissionCore.find_and_bind_structure(builder, position, WitchHutDesign)
    hut = witch_hut_structure.WorldObject
    hut_outcome = select_hut_outcome(rng.Next(0, 10))
    steal_outcome = select_steal_outcome(rng.Next(0, 10))

    builder.MissionStart += lambda: on_mission_start(giver, owner)
    builder.FailWhen = lambda: giver.IsDead or not MissionCore.is_within_distance(giver.Position, witch_hut_structure.Position)
    builder.MissionDispose += lambda: MissionCore.remove_component_if_exists(giver, FollowAIComponent)

    find = FindStructureMission()
    find.Design = witch_hut_structure.Design
    find.Position = witch_hut_structure.Position
    find.SetDescription(translate('quest_steal_from_witch_description', giver.Name))
    find.OverrideOpeningDialog(create_dialog(find.Design.DisplayName))
    find.MissionBlockEnd += lambda: on_hut_arrived(giver, witch_hut_structure.WorldObject, hut_outcome, owner, builder)
    builder.Next(find)
    
    if hut_outcome is OUTCOME_HUT_EMPTY:
        hut.EnsureHutIsEmpty()
    
    if hut_outcome is OUTCOME_FIGHT or hut_outcome is OUTCOME_RUNAWAY:
        hut.EnsureWitchesSpawned()
        
    if hut_outcome is OUTCOME_FIGHT:
        complete = CompleteStructureMission()
        complete.MissionBlockStart += lambda: MissionCore.remove_component_if_exists(giver, BasicAIComponent)
        complete.StructureObject = hut
        complete.StructureDesign = witch_hut_structure.Design
        builder.Next(complete)
        
    if hut_outcome is OUTCOME_FIGHT or hut_outcome is OUTCOME_HUT_EMPTY:
        steal = WaitForMission(giver, STEAL_DURATION)
        steal.MissionBlockStart += lambda: start_steal_animation(giver, hut)
        builder.Next(steal)
    
    reward = QuestReward()
    reward.CustomDialog = select_dialog_from_steal_outcome(steal_outcome)
    reward.RewardGiven += lambda: add_outcome_effects(steal_outcome, owner, giver)
    if steal_outcome is OUTCOME_SHARE:
        item_name, max_amount = POSSIBLE_REWARDS[rng.Next(0, len(POSSIBLE_REWARDS))]
        item = ItemPool.Grab(item_name)
        item.SetAttribute('Amount', rng.Next(1, max_amount+1))
        reward.Item = item
        reward.CustomDialog.Arguments = Array[Object]([MissionCore.make_item_string(item)])
    
    builder.SetReward(reward)
    return builder

def start_steal_animation(giver, hut):
    MissionCore.remove_component_if_exists(giver, IBasicAIComponent)
    giver.AddComponent(CommandBasedAIComponent(giver))
    giver.SearchComponent[CommandBasedAIComponent]().WalkTo(hut.Witch0Position)

def add_outcome_effects(outcome, owner, giver):
    if outcome is OUTCOME_SCAM:
        make_run_away(giver, owner)

def on_hut_arrived(giver, hut, outcome, owner, builder):
    MissionCore.remove_component_if_exists(giver, TalkComponent)
    talk = TalkComponent(giver)

    if outcome is OUTCOME_FIGHT:
        dialog_line = 'quest_steal_from_witch_kill'
    elif outcome is OUTCOME_HUT_EMPTY: 
        dialog_line = 'quest_steal_from_witch_empty'
    elif outcome is OUTCOME_RUNAWAY:
        make_run_away(giver, owner)
        dialog_line = 'quest_steal_from_witch_escape'
        owner.Questing.Abandon(builder.Mission)
    else:
        raise System.ArgumentException('Invalid hut outcome')

    talk.AddDialogLine(load_translation(dialog_line))
    talk.AutoRemove = True
    giver.AddComponent(talk)
    talk.TalkToPlayer()
    
    # This makes the enemies focus the player
    for enemy in hut.Enemies:
        enemy.SearchComponent[CombatAIComponent]().SetTarget(owner)

def select_dialog_from_steal_outcome(outcome):
    if outcome is OUTCOME_SHARE:
        dialog_line = 'quest_steal_from_witch_hut_reward_success'
    elif outcome is OUTCOME_SCAM:
        dialog_line = 'quest_steal_from_witch_hut_reward_escape'
    elif outcome is OUTCOME_EMPTY:
        dialog_line = 'quest_steal_from_witch_hut_reward_failure'
    else:
        raise System.ArgumentException('Invalid steal outcome')
    dialog = DialogObject()
    dialog.Keyword = dialog_line
    dialog.Arguments = Array[Object]([])
    return dialog

def make_run_away(giver, target):
    MissionCore.remove_component_if_exists(giver, IBasicAIComponent)
    MissionCore.remove_component_if_exists(giver, FollowAIComponent)
    giver.AddComponent(EscapeAIComponent(giver, target))

def make_follow(giver, target):
    MissionCore.remove_component_if_exists(giver, IBasicAIComponent)
    giver.AddComponent(FollowAIComponent(giver, target))

def on_mission_start(giver, target):
    make_follow(giver, target)
    giver.SearchComponent[DamageComponent]().Immune = False
    giver.SearchComponent[DamageComponent]().Ignore(lambda x: x == target)

def select_hut_outcome(n):
    if n < 7: # 70%
        return OUTCOME_FIGHT
    elif n < 9: # 20%
        return OUTCOME_HUT_EMPTY
    return OUTCOME_RUNAWAY

def select_steal_outcome(n):
    if n < 5: # 50%
        return OUTCOME_SHARE
    elif n < 8:
        return OUTCOME_SCAM
    else:
        return OUTCOME_EMPTY

def create_dialog(name):
    dialog = DialogObject()
    dialog.Keyword = 'quest_steal_from_witch_dialog'
    dialog.Arguments = Array[Object]([])
    return dialog

def can_give(position):
    return len(MissionCore.nearby_structs_designs(position, WitchHutDesign)) > 0