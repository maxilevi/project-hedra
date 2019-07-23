from Core import translate
from Hedra.Mission import MissionBuilder, ItemCollect
from Hedra.Mission.Blocks import CollectMission, TalkMission, CraftMission
from Hedra.Items import ItemPool, ItemTier, ItemType, EquipmentType
from Hedra.Crafting import CraftingStation, CraftingInventory
from System import Array, ArgumentOutOfRangeException, Math, Object

QUEST_NAME = 'CollectAndCraft'
IS_QUEST = True
STATION_ATTRIB_NAME = 'CraftingStation'

def setup_timeline(position, giver, owner, rng):
    builder = MissionBuilder()
    has_crafting, items = add_collect_mission(builder, giver, owner, rng)
    
    if has_crafting:
        add_craft_mission(builder, owner, giver, items)
        
    return builder

def add_collect_mission(builder, giver, owner, rng):
    items = random_items(owner, rng)
    has_crafting = can_craft(items)
    
    collect = CollectMission()
    collect.Items = items

    keyword, arguments = get_collect_thoughts(items)
    talk = TalkMission(keyword, arguments)
    talk.Humanoid = giver
    
    builder.Next(collect)
    builder.Next(talk)
    
    return has_crafting, items

def get_collect_thoughts(items):
    params = ', '.join(map(lambda i: i.ToString(), items)).ToUpperInvariant()
    return 'quest_collect_dialog', params

def add_craft_mission(builder, owner, giver, previous_items):
    recipe, station, item_collect = random_crafts(previous_items)

    keyword, arguments = get_craft_thoughts(station, item_collect)
    talk = TalkMission(keyword, arguments)
    def give_recipe():
        if owner.Crafting.HasRecipe(recipe.Name):
            return
        talk.AddDialogLine(translate("quest_craft_take_recipe"))
        owner.AddOrDropItem(recipe)
        owner.MessageDispatcher.ShowPlaque(translate("quest_learn_recipe_plaque"), 3)

    talk.Humanoid = giver
    talk.OnTalk += give_recipe

    craft = CraftMission()
    craft.Station = station
    craft.Items = [item_collect]

    builder.Next(talk)
    builder.Next(craft)

def get_craft_thoughts(station, item_collect):
    name = item_collect.ToString().ToUpperInvariant()
    station_name = translate(station.ToString().ToLowerInvariant()).ToUpperInvariant()
    params = Array[Object]([name, station_name])
    
    if station != CraftingStation.None:
        return 'quest_craft_dialog', params
    return 'quest_craft_anywhere_dialog', params
    
    
def random_items(owner, rng):
    possible_items = ItemPool.Matching(lambda x: x.Tier == ItemTier.Misc and x.Name != ItemType.Gold.ToString())
    recipes = ItemPool.Matching(lambda t: t.EquipmentType == EquipmentType.Recipe.ToString())
    owner_recipes = owner.Crafting.RecipeOutputs.Select(lambda x: x.Name)
    possible_items = list(filter(lambda x: all(lambda r: CraftingInventory.GetOutputFromRecipe(r).Name != x.Name or x.Name in owner_recipes, recipes), possible_items))
                         
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
    return rng.Next(lower_bound, upper_bound)

def recipe_from_item(name, recipes, rng):
    possible_recipes = list(filter(lambda r: any(lambda i: i.Name == name, CraftingInventory.GetIngredients(r)), recipes))
    if possible_recipes:
        return possible_recipes[rng.Next(0, possible_recipes.Length)].Name
    return None

def can_craft(items):
    return all(lambda x: x.Recipe, items)

def random_crafts(items):
    item = items[0]
    recipe = ItemPool.Grab(item.Recipe)
    station = recipe.GetAttribute[CraftingStation](STATION_ATTRIB_NAME)
    output = CraftingInventory.GetOutputFromRecipe(recipe)
    ingredient_template = next(lambda i: i.Name == item.Name, CraftingInventory.GetIngredients(recipe))
    item_collect = ItemCollect()
    item_collect.Name = output.Name
    item_collect.Amount = Math.Max(1, int(item.Amount / ingredient_template.Amount))
    return recipe, station, item_collect

def can_give():
    return True

def get_reward():
    