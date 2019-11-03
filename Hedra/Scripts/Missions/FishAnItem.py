from Hedra import World
from Hedra.Mission import MissionBuilder, QuestTier, QuestHint, QuestReward, DialogObject
from Hedra.Mission.Blocks import FishItemMission
from Hedra.Items import ItemPool, Trader
from System import Object, Array

IS_QUEST = True
QUEST_NAME = 'FishAnItem'
QUEST_TIER = QuestTier.Easy
QUEST_HINT = QuestHint.Fishing
WATER_SEARCH_RANGE = 64

def setup_timeline(position, giver, owner, rng):
    builder = MissionBuilder()

    possible_items = get_possible_items()
    _, fish_zone = World.NearestWaterBlock(position, WATER_SEARCH_RANGE)
    fish = FishItemMission()
    fish.Item = possible_items[rng.Next(0, len(possible_items))]
    fish.Zone = fish_zone
    fish.Radius = WATER_SEARCH_RANGE
    builder.Next(fish)

    reward = build_reward(fish.Item, rng)
    builder.SetReward(reward)
    return builder

def build_reward(item, rng):
    n = rng.NextDouble()
    reward = QuestReward()
    
    def get_multiplier():
        return min(1.0, Trader.Price(item) / 25.0)
    
    if n < 0.3:
        reward.Experience = int(rng.Next(3, 9) * get_multiplier())
    elif n < 0.7:
        reward.Gold = int(rng.Next(11, 25) * get_multiplier())
    elif 0.75 < n < 0.95:
        reward.Item = item
        reward.CustomDialog = DialogObject()
        reward.CustomDialog.Keyword = 'quest_fish_item_item_reward'
        reward.CustomDialog.Arguments = Array[Object]([item.DisplayName.ToUpperInvariant()])
    return reward

def get_possible_items():
    return ItemPool.Matching(lambda x: x.IsWeapon or x.IsArmor)

def can_give(position):
    return World.NearestWaterBlock(position, WATER_SEARCH_RANGE)[0] < WATER_SEARCH_RANGE