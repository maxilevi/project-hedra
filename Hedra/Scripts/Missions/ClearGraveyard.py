import MissionCore
from Hedra.Mission import MissionBuilder, QuestTier, DialogObject
from Hedra.Mission.Blocks import FindStructureMission, CompleteStructureMission
from Hedra.Engine.StructureSystem.Overworld import GraveyardDesign
from System import Array, Object

IS_QUEST = True
QUEST_NAME = 'ClearGraveyard'
QUEST_TIER = QuestTier.Medium

def setup_timeline(position, giver, owner, rng):
    builder = MissionBuilder()

    graveyard = MissionCore.find_and_bind_structure(builder, position, GraveyardDesign)

    find = FindStructureMission()
    find.Design = graveyard.Design
    find.Position = graveyard.Position
    find.OverrideOpeningDialog(create_dialog(find.Design.DisplayName))
    builder.Next(find)

    complete = CompleteStructureMission()
    complete.StructureObject = graveyard.WorldObject
    complete.StructureDesign = graveyard.Design
    builder.Next(complete)

    builder.SetReward(MissionCore.build_generic_reward(rng))
    return builder

def create_dialog(name):
    dialog = DialogObject()
    dialog.Keyword = 'quest_clear_graveyard_dialog'
    dialog.Arguments = Array[Object]([])
    return dialog

def can_give(position):
    return len(MissionCore.nearby_structs_designs(position, GraveyardDesign)) > 0