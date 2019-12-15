import MissionCore
import Items
from System import Array, Object
from Hedra.Mission.Blocks import CollectMission, TalkMission
from Hedra.Mission import MissionBuilder, QuestTier, QuestReward, ItemCollect
from Hedra.Items import ItemPool, Trader

IS_QUEST = True
QUEST_NAME = 'CraftAPotion'
QUEST_TIER = QuestTier.Easy

def parse_ingredient(ingredient):
    price = Trader.SingleItemPrice(ingredient)
    return ingredient.Name, 1, 3

POSSIBLE_INGREDIENTS = [
    parse_ingredient(x) for x in ItemPool.Matching(lambda x: any(j.Name == 'IsFood' and j.Value for j in x.Attributes))
]

def parse_potion(potion):
    hp_price = Trader.SingleItemPrice(ItemPool.Grab(Items.HEALTH_POTION))
    price = Trader.SingleItemPrice(potion)
    lower = ((hp_price * 2) / price)
    upper = ((hp_price * 8) / price)
    return potion, lower, upper

POTIONS_TYPES = [parse_potion(x) for x in ItemPool.Matching(lambda x: x.EquipmentType == 'Potion')]

def setup_timeline(position, giver, owner, rng):
    builder = MissionBuilder()
    builder.OpeningDialog = MissionCore.create_dialog('quest_craft_a_potion_dialog', [])

    ingredients = select_ingredients(rng)
    for name, lower, upper in ingredients:

        item_collect = ItemCollect()
        item_collect.Name = name
        item_collect.Amount = rng.Next(lower, upper)
        
        talk = TalkMission(MissionCore.create_dialog('quest_craft_a_potion_collect', [item_collect.ToString()]))
        talk.Humanoid = giver
        builder.Next(talk)
        
        collect = CollectMission()
        collect.Items = Array[ItemCollect]([item_collect])
        collect.MissionBlockEnd += collect.ConsumeItems
        builder.Next(collect)

    builder.SetReward(build_reward(rng))
    return builder

def select_ingredients(rng):
    ingredients = []
    count = rng.Next(2, 6)
    for _ in range(count):
        ingredients.append(POSSIBLE_INGREDIENTS[rng.Next(0, len(POSSIBLE_INGREDIENTS))])
    return ingredients

def build_reward(rng):
    reward = QuestReward()
    val = rng.NextDouble()
    if val < 0.25:
        reward.CustomDialog = MissionCore.create_dialog('quest_craft_a_potion_failure', [])
    elif val <= 0.65:
        reward.Gold = rng.Next(17, 41)
    else:
        potion, lower_bound, upper_bound = POTIONS_TYPES[rng.Next(0, len(POTIONS_TYPES))]
        reward.Item = ItemPool.Grab(potion.Name)
        reward.Item.SetAttribute(Items.AMOUNT_ATTRIBUTE, rng.Next(lower_bound, upper_bound))
        reward.CustomDialog = MissionCore.create_dialog('quest_craft_a_potion_success', [MissionCore.to_item_collect(reward.Item).ToString()])
    return reward

# This quest can only be given by villagers that spawn in campfires with cauldrons
def can_give(position):
    return False