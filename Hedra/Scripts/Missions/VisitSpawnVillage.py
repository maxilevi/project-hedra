from Core import translate
from Hedra import World
from Hedra.Mission import MissionBuilder, QuestTier, DialogObject
from Hedra.Mission.Blocks import FindStructureMission
from Hedra.Structures import SpawnVillageDesign
from System import Array, Object

IS_QUEST = True
QUEST_NAME = 'VisitSpawnVillage'
QUEST_TIER = QuestTier.Easy

def setup_timeline(position, giver, owner, rng):
    builder = MissionBuilder()
    
    find = FindStructureMission()
    find.Design = SpawnVillageDesign()
    find.Position = World.SpawnVillagePoint
    find.OverrideOpeningDialog(create_dialog(find.Design.DisplayName))
    
    builder.Next(find)
    return builder

def create_dialog(name):
    dialog = DialogObject()
    dialog.Keyword = 'quest_spawn_dialog'
    dialog.Arguments = Array[Object]([])
    dialog.AddAfterLine(translate('quest_spawn_find_structure', Array[Object]([name])))
    return dialog

def can_give(position):
    return False