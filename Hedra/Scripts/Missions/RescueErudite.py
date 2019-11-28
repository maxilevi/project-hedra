import MissionCore
from Core import translate
from Hedra.Mission import MissionBuilder, QuestTier, QuestReward, DialogObject
from Hedra.Mission.Blocks import FindStructureMission, CompleteStructureMission
from Hedra.Engine.StructureSystem.Overworld import BanditCampDesign
from System import Array, Object

IS_QUEST = True
QUEST_NAME = 'RescueErudite'
QUEST_TIER = QuestTier.Medium
MAX_DISTANCE = 2048

def setup_timeline(position, giver, owner, rng):
    builder = MissionBuilder()

    bandit_camp = MissionCore.find_structure(position, BanditCampDesign, MAX_DISTANCE)

    find = FindStructureMission()
    find.Design = BanditCampDesign()
    find.Position = bandit_camp.Position
    find.SetDescription(translate('quest_rescue_erudite_description', giver.Name))
    find.OverrideOpeningDialog(create_dialog(find.Design.DisplayName))
    builder.Next(find)
    
    complete = CompleteStructureMission()
    complete.StructureObject = bandit_camp.WorldObject
    complete.StructureDesign = bandit_camp.Design
    builder.Next(complete)
    
    builder.SetReward(build_reward(rng))

    return builder

def create_dialog(name):
    dialog = DialogObject()
    dialog.Keyword = 'quest_rescue_erudite_dialog'
    dialog.Arguments = Array[Object]([])
    return dialog

def build_reward(rng):
    n = rng.NextDouble()
    reward = QuestReward()
    return reward

def can_give(position):
    return len(MissionCore.nearby_structs_designs(position, BanditCampDesign, MAX_DISTANCE)) > 0