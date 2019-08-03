import MissionCore
from Core import translate
from Hedra import World
from Hedra.Mission import MissionBuilder, QuestTier, QuestReward, DialogObject
from Hedra.Mission.Blocks import FindStructureMission, CompleteStructureMission
from Hedra.Structures import GhostTownPortalDesign, SpawnGhostTownPortalDesign, GhostTownBossDesign
from System import Array, Object

IS_QUEST = True
QUEST_NAME = 'FindAndKillLich'
QUEST_TIER = QuestTier.Medium
MAX_DISTANCE = 2048

def setup_timeline(position, giver, owner, rng):
    builder = MissionBuilder()

    find_and_use_portal(position, builder, rng)
    find_and_defeat_lich(builder)
    find_and_use_wayback_portal(position, builder, rng)

    return builder

def find_and_defeat_lich(builder):
    find = FindStructureMission()
    find.Design = GhostTownBossDesign()
    find.Position = GhostTownBossDesign.Position
    find.OverrideOpeningDialog(create_dialog(find.Design.DisplayName))
    builder.Next(find)
    
    back_portal = MissionCore.find_structure(SpawnGhostTownPortalDesign.Position, SpawnGhostTownPortalDesign, 1024)
    complete = CompleteStructureMission()
    complete.StructureObject = back_portal.WorldObject
    complete.StructureDesign = back_portal.Design
    builder.Next(complete)

def find_and_use_wayback_portal(position, builder, rng):
    find = FindStructureMission()
    find.Design = SpawnGhostTownPortalDesign()
    find.Position = SpawnGhostTownPortalDesign.Position
    find.OverrideOpeningDialog(create_dialog(find.Design.DisplayName))
    builder.Next(find)

    back_portal = MissionCore.find_structure(SpawnGhostTownPortalDesign.Position, SpawnGhostTownPortalDesign, 1024)
    complete = CompleteStructureMission()
    complete.StructureObject = back_portal.WorldObject
    complete.StructureDesign = back_portal.Design
    builder.Next(complete)

def find_and_use_portal(position, builder, rng):
    portals = MissionCore.nearby_struct_objects(position, GhostTownPortalDesign, MAX_DISTANCE)
    portal = portals[rng.Next(0, len(portals))]

    find = FindStructureMission()
    find.Design = GhostTownPortalDesign()
    find.Position = portal.Position
    find.OverrideOpeningDialog(create_dialog(find.Design.DisplayName))
    builder.Next(find)

    complete = CompleteStructureMission()
    complete.StructureObject = portal.WorldObject
    complete.StructureDesign = portal.Design
    builder.Next(complete)

def create_dialog(name):
    dialog = DialogObject()
    dialog.Keyword = 'quest_kill_lich_dialog'
    dialog.Arguments = Array[Object]([])
    dialog.AddAfterLine(translate('quest_generic_find_structure', Array[Object]([name])))
    return dialog

def build_reward(rng):
    n = rng.NextDouble()
    reward = QuestReward()
    return reward

def can_give(position):
    return len(MissionCore.nearby_structs_designs(position, GhostTownPortalDesign, MAX_DISTANCE)) > 0