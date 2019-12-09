import MissionCore
from Core import translate
from Hedra.Mission import MissionBuilder, QuestTier, DialogObject
from Hedra.Mission.Blocks import FindStructureMission, CompleteStructureMission
from Hedra.Engine.StructureSystem.Overworld import BanditCampDesign
from System import Array, Object

IS_QUEST = True
QUEST_NAME = 'RescueErudite'
QUEST_TIER = QuestTier.Medium
MAX_DISTANCE = 4096

def setup_timeline(position, giver, owner, rng):
    builder = MissionBuilder()
        
    bandit_camp = MissionCore.find_structure(position, BanditCampDesign)

    find = FindStructureMission()
    find.Design = bandit_camp.Design
    find.Position = bandit_camp.Position
    find.SetDescription(translate('quest_rescue_erudite_description', giver.Name))
    find.OverrideOpeningDialog(create_dialog(find.Design.DisplayName))
    builder.Next(find)
    
    complete = CompleteStructureMission()
    complete.StructureObject = bandit_camp.WorldObject
    complete.StructureDesign = bandit_camp.Design
    builder.Next(complete)
    
    builder.SetReward(MissionCore.build_generic_reward(rng))
    builder.FailWhen = lambda : bandit_camp.WorldObject.Rescuee.IsDead
    return builder

def create_dialog(name):
    dialog = DialogObject()
    dialog.Keyword = 'quest_rescue_erudite_dialog'
    dialog.Arguments = Array[Object]([])
    return dialog

def can_give(position):
    return len(MissionCore.nearby_structs_designs(position, BanditCampDesign)) > 0