from Core import translate
from Hedra.Mission import MissionBuilder, ItemCollect, QuestReward, QuestTier
from Hedra.Mission.Blocks import CollectMission, TalkMission, CraftMission
from Hedra.Items import ItemPool, ItemTier, ItemType, EquipmentType
from Hedra.Crafting import CraftingStation, CraftingInventory
from System import Array, ArgumentOutOfRangeException, Math, Object

IS_QUEST = True
QUEST_NAME = 'CollectAndCraft'
QUEST_TIER = QuestTier.Easy
STATION_ATTRIB_NAME = 'CraftingStation'
MAX_COLLECT_ITEM_TYPES = 1

def setup_timeline(position, giver, owner, rng):
    builder = MissionBuilder()
    has_crafting, items = add_collect_mission(builder, giver, owner, rng)
    
    if has_crafting:
        add_craft_mission(builder, owner, giver, items)
    
    reward = build_reward(items, rng)
    builder.SetReward(reward)
    return builder

def add_collect_mission(builder, giver, owner, rng):
    items = select_items(owner, rng)
    has_crafting = can_craft(items)
    
    collect = CollectMission()
    collect.Items = items

    builder.Next(collect)
    if not has_crafting:
        builder.MissionEnd += lambda: collect.ConsumeItems()
    return has_crafting, items

def add_craft_mission(builder, owner, giver, previous_items):
    recipe, station, item_collect = random_crafts(previous_items)

    craft = CraftMission()
    craft.Station = station
    craft.Items = Array[ItemCollect]([item_collect])

    talk = TalkMission(craft.DefaultOpeningDialog)
    def give_recipe():
        if owner.Crafting.HasRecipe(recipe.Name):
            return
        talk.AddDialogLine(translate("quest_craft_take_recipe"))
        owner.AddOrDropItem(recipe)
        owner.MessageDispatcher.ShowPlaque(translate("quest_learn_recipe_plaque"), 3)

    talk.Humanoid = giver
    talk.OnTalk += lambda _: give_recipe()

    builder.Next(talk)
    builder.Next(craft)
    builder.MissionEnd += lambda: craft.ConsumeItems()
    
def select_items(owner, rng):
    templates = random_items(owner, rng)
    items = []
    for i in xrange(0, MAX_COLLECT_ITEM_TYPES):
        template = templates[rng.Next(0, templates.Length)]
        if template not in items:
            items.append(template)
    return Array[ItemCollect](items)

def random_items(owner, rng):
    possible_items = ItemPool.Matching(lambda x: x.Tier == ItemTier.Misc and x.Name != ItemType.Gold.ToString())
    recipes = ItemPool.Matching(lambda t: t.EquipmentType == EquipmentType.Recipe.ToString())
    owner_recipes = map(lambda x: x.Name, owner.Crafting.RecipeOutputs)
    possible_items = list(filter(lambda x: all([CraftingInventory.GetOutputFromRecipe(r).Name != x.Name or x.Name in owner_recipes for r in recipes]), possible_items))
                         
    def to_item_collect(item):
        collect = ItemCollect()
        collect.Name = item.Name
        collect.Amount = amount_from_item(item, rng)
        collect.Recipe = recipe_from_item(item.Name, recipes, rng)
        return collect
                         
    return Array[ItemCollect](map(to_item_collect, possible_items))

def amount_from_item(item, rng):
    if not item.HasAttribute('Price'):
        raise ArgumentOutOfRangeException("Item '{0}' does not have a price attribute.".format(item.DisplayName))
    lower_bound = int(Math.Round(18.0 / item.GetAttribute[int]('Price')))
    upper_bound = int(Math.Round(42.0 / item.GetAttribute[int]('Price')))
    return max(1, rng.Next(lower_bound, upper_bound))

def recipe_from_item(name, recipes, rng):
    possible_recipes = list(filter(lambda r: any([i.Name == name for i in CraftingInventory.GetIngredients(r)]), recipes))
    if possible_recipes:
        return possible_recipes[rng.Next(0, len(possible_recipes))].Name
    return None

def can_craft(items):
    return all([x.Recipe for x in items])

def random_crafts(items):
    item = items[0]
    recipe = ItemPool.Grab(item.Recipe)
    station = recipe.GetAttribute[CraftingStation](STATION_ATTRIB_NAME)
    output = CraftingInventory.GetOutputFromRecipe(recipe)
    ingredient_template = [i for i in CraftingInventory.GetIngredients(recipe) if i.Name == item.Name][0]
    item_collect = ItemCollect()
    item_collect.Name = output.Name
    item_collect.Amount = Math.Max(1, int(item.Amount / ingredient_template.Amount))
    return recipe, station, item_collect

def can_give(position):
    return True

def build_reward(items, rng):

    def get_multiplier():
        return sum([ItemPool.Grab(x.Name).GetAttribute[int]('Price') * x.Amount for x in items]) / 25.0

    def get_random_item():
        n = rng.NextDouble()
        if n < 0.3:
            possibilities = ItemPool.Matching(lambda x: x.EquipmentType == EquipmentType.Recipe.ToString() and int(x.Tier) == int(ItemTier.Uncommon))
        elif n < 0.8:
            possibilities = ItemPool.Matching(lambda x: x.EquipmentType == EquipmentType.Recipe.ToString() and int(x.Tier) == int(ItemTier.Common))
        else:
            possibilities = ItemPool.Matching(lambda x: x.Tier == ItemTier.Misc)
        return possibilities[rng.Next(0, len(possibilities))]
    
    n = rng.NextDouble()
    reward = QuestReward()
    if n < 0.3:
        reward.Experience = int(rng.Next(3, 9) * get_multiplier())
    elif n < 0.7:
        reward.Gold = int(rng.Next(11, 25) * get_multiplier())
    elif 0.75 < n < 0.95:
        reward.Item = get_random_item()
    return reward