import Items
from Core import translate
from Hedra.Crafting import CraftingInventory
from Hedra.Engine.ItemSystem import ItemLoader
from Hedra.Items import ItemPool, Trader
from Hedra.Mission import MissionBuilder, QuestTier, QuestReward, ItemCollect
from Hedra.Mission.Blocks import CollectMission, TalkMission
from System import Array

import MissionCore

IS_QUEST = True
QUEST_NAME = 'CraftAPotion'
QUEST_TIER = QuestTier.Easy


def build_ingredient_list():
    ingredients = []

    def parse_ingredient(ingredient):
        price = Trader.SingleItemPrice(ingredient)
        return ingredient.Name, 1, 4

    food_recipe_ingredients = set()
    for r in ItemPool.Matching(lambda t: t.IsRecipe):
        output = CraftingInventory.GetOutputFromRecipe(r)
        if output.IsFood:
            for k in [x.Name for x in CraftingInventory.GetIngredients(r)]:
                food_recipe_ingredients.add(k)

    templates = ItemLoader.Templater.Templates
    for x in templates:
        is_food = any(j.Name == 'IsFood' and j.Value for j in x.Attributes)
        is_food_ingredient = x.Name in food_recipe_ingredients

        if is_food or is_food_ingredient:
            ingredients.append(x)

    return [parse_ingredient(ItemPool.Grab(x.Name)) for x in ingredients]


POSSIBLE_INGREDIENTS = build_ingredient_list()


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

        def on_talk():
            t = talk
            i = item_collect
            t.OnTalk += lambda _: give_recipe_if_necessary(owner, i, t)

        on_talk()
        builder.Next(talk)

        collect = CollectMission()
        collect.Items = Array[ItemCollect]([item_collect])
        collect.MissionBlockEnd += collect.ConsumeItems
        builder.Next(collect)

    builder.SetReward(build_reward(rng))
    return builder


def give_recipe_if_necessary(owner, item, talk):
    recipe = CraftingInventory.GetRecipeThatCrafts(item.Name)

    if not recipe or owner.Crafting.HasRecipe(recipe.Name):
        return

    talk.AddDialogLine(translate("quest_craft_take_recipe"))
    owner.AddOrDropItem(recipe)
    owner.MessageDispatcher.ShowPlaque(translate("quest_learn_recipe_plaque"), 3)


def select_ingredients(rng):
    ingredients = []
    count = rng.Next(3, 6)
    for _ in range(count):
        ingredients.append(POSSIBLE_INGREDIENTS[rng.Next(0, len(POSSIBLE_INGREDIENTS))])
    return ingredients


def build_reward(rng):
    reward = QuestReward()
    val = rng.NextDouble()
    if val <= 0.5:
        reward.CustomDialog = MissionCore.create_dialog('quest_craft_a_potion_failure', [])
    else:
        potion, lower_bound, upper_bound = POTIONS_TYPES[rng.Next(0, len(POTIONS_TYPES))]
        reward.Item = ItemPool.Grab(potion.Name)
        reward.Item.SetAttribute(Items.AMOUNT_ATTRIBUTE, rng.Next(lower_bound, upper_bound))
        reward.CustomDialog = MissionCore.create_dialog('quest_craft_a_potion_success',
                                                        [MissionCore.to_item_collect(reward.Item).ToString()])
    return reward


# This quest can only be given by villagers that spawn in campfires with cauldrons
def can_give(position):
    return False
