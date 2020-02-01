# This is a simple quest to make the player learn more about the travelling merchant

import MissionCore
import Items
from Hedra.Mission.Blocks import FindStructureMission, CompleteStructureMission
from Hedra.Mission import MissionBuilder, QuestTier, QuestHint, QuestReward, DialogObject, ItemCollect
from Hedra.Engine.StructureSystem.Overworld import TravellingMerchantDesign
from System import Object, Array
from Hedra.Items import ItemPool, Trader

IS_QUEST = True
QUEST_NAME = 'BuyFromTravelling'
QUEST_TIER = QuestTier.Easy
POSSIBLE_ITEMS = [
    (Items.STRENGTH_POTION, (1, 7)),
    (Items.DEXTERITY_POTION, (1, 7)),
    (Items.SPEED_POTION, (1, 7))
]

def setup_timeline(position, giver, owner, rng):
    builder = MissionBuilder()

    item_to_collect = random_items(rng)
    structure = MissionCore.find_and_bind_structure(builder, position, TravellingMerchantDesign)
    structure.WorldObject.ItemsToBuy = item_to_collect

    find = FindStructureMission()
    find.Design = structure.Design
    find.Position = structure.Position
    find.OverrideOpeningDialog(create_dialog(find.Design.DisplayName, item_to_collect))
    builder.Next(find) 

    complete = CompleteStructureMission()
    complete.StructureDesign = structure.Design
    complete.StructureObject = structure.WorldObject
    complete.MissionBlockEnd += lambda: item_to_collect.Consume(owner)
    builder.Next(complete)
    
    reward = QuestReward()
    reward.Gold = rng.Next(9, 17)
    reward.Gold += Trader.SingleItemPrice(ItemPool.Grab(item_to_collect.Name)) * Trader.BuyMultiplier * item_to_collect.Amount
    
    builder.SetReward(reward)
    builder.MissionEnd += lambda: item_to_collect.Consume(owner)
    return builder


def random_items(rng):
    name, amount = POSSIBLE_ITEMS[rng.Next(0, len(POSSIBLE_ITEMS))]
    collect = ItemCollect()
    collect.Name = name
    collect.Amount = rng.Next(amount[0], amount[1])
    return collect

def create_dialog(name, collect):
    dialog = DialogObject()
    dialog.Keyword = 'quest_trade_travelling_merchant_dialog'
    dialog.Arguments = Array[Object]([collect.ToString(), name])
    return dialog

def can_give(position):
    return len(MissionCore.nearby_structs_designs(position, TravellingMerchantDesign)) > 0

for item, _ in POSSIBLE_ITEMS:
    assert ItemPool.Exists(item)