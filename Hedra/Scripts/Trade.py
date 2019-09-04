from Hedra.Items import ItemPool, ItemTier

AMOUNT_ATTRIBUTE = 'Amount'
STONE_ARROW = 'StoneArrow'
GLASS_FLASK = 'GlassFlask'
WOODEN_BOWL = 'WoodenBowl'
BERRY = 'Berry'
BOAT = 'Boat'
PUMPKIN_PIE_RECIPE = 'PumpkinPieRecipe'
COOKED_MEAT_RECIPE = 'CookedMeatRecipe'
HEALTH_POTION_RECIPE = 'HealthPotionRecipe'
CORN_SOUP_RECIPE = 'CornSoupRecipe'
FISHING_ROD = 'FishingRod'
BAIT = 'Bait'
COMPANION_EQUIPMENT_TYPE = 'Pet'

def build_innkeeper_inventory(item_dict, inventory_size, rng):
    items = []
    add_items(items, item_dict)
    
def build_boat_merchant_inventory(item_dict, inventory_size, rng):
    items = [
        (inventory_size - 1, ItemPool.Grab(BOAT)),
        (1, ItemPool.Grab(FISHING_ROD)),
        (0, get_infinity_item(BAIT))
    ]
    add_items(items, item_dict)
    
def build_merchant_inventory(item_dict, inventory_size, rng):

    recipes = [
        ItemPool.Grab(PUMPKIN_PIE_RECIPE),
        ItemPool.Grab(COOKED_MEAT_RECIPE),
        ItemPool.Grab(HEALTH_POTION_RECIPE),
        ItemPool.Grab(CORN_SOUP_RECIPE),
    ]
    items = [
        (inventory_size - 1, get_infinity_item(BERRY)),
        (inventory_size - 2, get_infinity_item(GLASS_FLASK)),
        (inventory_size - 3, get_infinity_item(WOODEN_BOWL)),
        (inventory_size - 4, get_infinity_item(STONE_ARROW) if rng.Next(0, 2) == 1 else None),
        (inventory_size - 5, recipes[rng.Next(0, len(recipes))]),
    ]
    fishing_items = [
        (4, ItemPool.Grab(FISHING_ROD)),
        (5, get_infinity_item(BAIT))
    ]
    if rng.Next(0, 2) == 1:
        items += fishing_items
    add_items(items, item_dict)

    
def build_travelling_merchant_inventory(item_dict, inventory_size, rng):
    build_merchant_inventory(item_dict, inventory_size, rng)
    special_items = [
        (0, ItemPool.Grab(ItemTier.Common, COMPANION_EQUIPMENT_TYPE)),
        (1, ItemPool.Grab(ItemTier.Common, COMPANION_EQUIPMENT_TYPE)),
        (2, ItemPool.Grab(ItemTier.Common, COMPANION_EQUIPMENT_TYPE)),
        (3, ItemPool.Grab(BOAT))
    ]
    add_items(special_items, item_dict)
    
    
def build_clothier_inventory(item_dict, inventory_size, rng):
    items = []
    add_items(items, item_dict)

def build_mason_inventory(item_dict, inventory_size, rng):
    items = []
    add_items(items, item_dict)

def add_items(items, dict):
    for index, item in items:
        if item: dict.Add(index, item)

def get_infinity_item(item_name):
    item = ItemPool.Grab(item_name)
    item.SetAttribute(AMOUNT_ATTRIBUTE, int.MaxValue)
    return item


assert ItemPool.Exists(STONE_ARROW)
assert ItemPool.Exists(GLASS_FLASK)
assert ItemPool.Exists(WOODEN_BOWL)
assert ItemPool.Exists(BERRY)
assert ItemPool.Exists(BOAT)
assert ItemPool.Exists(PUMPKIN_PIE_RECIPE)
assert ItemPool.Exists(COOKED_MEAT_RECIPE)
assert ItemPool.Exists(HEALTH_POTION_RECIPE)
assert ItemPool.Exists(CORN_SOUP_RECIPE)
assert ItemPool.Exists(FISHING_ROD)
assert ItemPool.Exists(BAIT)