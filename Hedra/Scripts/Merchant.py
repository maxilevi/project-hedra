from Hedra.Items import ItemPool

AMOUNT_ATTRIBUTE = 'Amount'
STONE_ARROW = 'StoneArrow'
GLASS_FLASK = 'GlassFlask'
WOODEN_BOWL = 'WoodenBowl'
BERRY = 'Berry'
HORSE_MOUNT = 'HorseMount'
BOAT = 'Boat'
PUMPKIN_PIE_RECIPE = 'PumpkinPieRecipe'
COOKED_MEAT_RECIPE = 'CookedMeatRecipe'
HEALTH_POTION_RECIPE = 'HealthPotionRecipe'
CORN_SOUP_RECIPE = 'CornSoupRecipe'

def build_inventory(item_dict, is_travelling_merchant, inventory_size, rng):
    items = get_base_items(inventory_size, rng)
    if is_travelling_merchant:
        items += get_special_items()
    for index, item in items:
        item_dict.Add(index, item)

def get_base_items(inventory_size, rng):
    recipes = get_base_recipes()
    return [
        (inventory_size - 1, get_infinity_item(BERRY)),
        (inventory_size - 2, get_infinity_item(GLASS_FLASK)),
        (inventory_size - 3, get_infinity_item(WOODEN_BOWL)),
        (inventory_size - 4, get_infinity_item(STONE_ARROW) if rng.Next(0, 2) == 1 else None),
        (inventory_size - 5, None), #recipes[rng.Next(0, len(recipes))]
    ]

def get_base_recipes():
    return [
        ItemPool.Grab(PUMPKIN_PIE_RECIPE),
        ItemPool.Grab(COOKED_MEAT_RECIPE),
        ItemPool.Grab(HEALTH_POTION_RECIPE),
        ItemPool.Grab(CORN_SOUP_RECIPE),
    ]

def get_special_items():
    return [
        (0, ItemPool.Grab(HORSE_MOUNT)),
        (2, ItemPool.Grab(BOAT))
    ]

def get_infinity_item(item_name):
    item = ItemPool.Grab(item_name)
    item.SetAttribute(AMOUNT_ATTRIBUTE, int.MaxValue)
    return item

assert ItemPool.Exists(STONE_ARROW)
assert ItemPool.Exists(GLASS_FLASK)
assert ItemPool.Exists(WOODEN_BOWL)
assert ItemPool.Exists(BERRY)
assert ItemPool.Exists(HORSE_MOUNT)
assert ItemPool.Exists(BOAT)
assert ItemPool.Exists(PUMPKIN_PIE_RECIPE)
assert ItemPool.Exists(COOKED_MEAT_RECIPE)
assert ItemPool.Exists(HEALTH_POTION_RECIPE)
assert ItemPool.Exists(CORN_SOUP_RECIPE)