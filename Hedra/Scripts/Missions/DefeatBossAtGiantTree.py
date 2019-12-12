import MissionCore
from Core import translate
from System import Array, Object, Single
from Hedra.Mission import MissionBuilder, QuestTier, DialogObject, QuestReward, ItemCollect
from Hedra.Mission.Blocks import FindStructureMission, CompleteStructureMission
from Hedra.Engine.StructureSystem.Overworld import GiantTreeDesign

IS_QUEST = True
QUEST_NAME = 'DefeatBossAtGiantTree'
QUEST_TIER = QuestTier.Medium

def setup_timeline(position, giver, owner, rng):
    builder = MissionBuilder()

    giant_tree = MissionCore.find_structure(position, GiantTreeDesign)

    find = FindStructureMission()
    find.Design = giant_tree.Design
    find.Position = giant_tree.Position
    find.SetDescription(translate('quest_find_boss_description', giver.Name))
    find.OverrideOpeningDialog(create_opening_dialog())
    builder.Next(find)

    complete = CompleteStructureMission()
    complete.StructureObject = giant_tree.WorldObject
    complete.StructureDesign = giant_tree.Design
    builder.Next(complete)

    builder.SetReward(MissionCore.build_generic_reward(rng))
    return builder

def create_opening_dialog():
    dialog = DialogObject()
    dialog.Keyword = 'quest_defeat_boss_dialog'
    dialog.Arguments = Array[Object]([])
    return dialog

def can_give(position):
    return len(MissionCore.nearby_structs_designs(position, GiantTreeDesign)) > 0