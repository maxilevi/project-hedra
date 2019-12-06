# This is a simple quest to make the player learn more about the travelling merchant

import MissionCore
from Hedra.Mission.Blocks import FindStructureMission, CompleteStructureMission
from Hedra.Mission import MissionBuilder, QuestTier, QuestHint, QuestReward, DialogObject, ItemCollect
from Hedra.Engine.StructureSystem.Overworld import TravellingMerchantDesign
from System import Object, Array

IS_QUEST = True
QUEST_NAME = 'BuyFromTravellingMerchant'
QUEST_TIER = QuestTier.Easy

def setup_timeline(position, giver, owner, rng):
    builder = MissionBuilder()

    items_to_collect = random_items()
    structure = MissionCore.find_structure(position, TravellingMerchantDesign)
    structure.WorldObject.ItemsToBuy = items_to_collect

    find = FindStructureMission()
    find.Design = structure.Design
    find.Position = structure.Position
    find.OverrideOpeningDialog(create_dialog(find.Design.DisplayName))
    builder.Next(find) 

    complete = CompleteStructureMission()
    complete.Design = structure.Design
    complete.WorldObject = structure.WorldObject
    builder.Next(complete)
    
    builder.SetReward(MissionCore.build_generic_reward(rng))
    builder.MissionEnd += lambda: items_to_collect.Consume()
    return builder

def random_items():
    collect = ItemCollect()
    return collect

def create_dialog(name):
    dialog = DialogObject()
    dialog.Keyword = 'quest_rescue_erudite_dialog'
    dialog.Arguments = Array[Object]([])
    return dialog

def can_give(position):
    return len(MissionCore.nearby_structs_designs(position, TravellingMerchantDesign)) > 0