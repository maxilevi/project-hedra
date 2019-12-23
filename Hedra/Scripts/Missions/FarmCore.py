import MissionCore
import Items
from System import Array, Object
from Hedra.Mission import QuestReward, DialogObject
from Hedra.Items import ItemPool
from Hedra.Engine.StructureSystem.Overworld import CottageWithFarmDesign

POSSIBLE_ITEM_REWARDS = [
    (ItemPool.Grab(Items.FARMER_HAT), 'quest_farmer_hat_reward_dialog'),
    (ItemPool.Grab(Items.FARMING_RAKE), 'quest_farming_rake_reward_dialog'),
    (ItemPool.Grab(Items.COMPANION_COW), 'quest_companion_cow_reward_dialog')
]

def get_reward(rng):
    n = rng.NextDouble()
    if n < 0.75:
        return MissionCore.build_generic_reward(rng)
    else:
        reward = QuestReward()
        item, dialog = POSSIBLE_ITEM_REWARDS[rng.Next(0, len(POSSIBLE_ITEM_REWARDS))]
        reward.Item = item
        reward.CustomDialog = DialogObject(dialog, Array[Object]([MissionCore.to_item_collect(item).ToString().ToUpperInvariant()]))
        return reward
    
def can_give(position):
    return MissionCore.is_inside_structure(position, CottageWithFarmDesign)