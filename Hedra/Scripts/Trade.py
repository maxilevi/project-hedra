from Hedra.Items import ItemPool, ItemTier

import Items

AMOUNT_ATTRIBUTE = 'Amount'
COMPANION_EQUIPMENT_TYPE = 'Pet'
NORMAL_RECIPES = [x.Name for x in ItemPool.Matching(
    lambda x: x.IsRecipe and (x.Tier == ItemTier.Uncommon or x.Tier == ItemTier.Common))]
RARE_RECIPES = [x.Name for x in ItemPool.Matching(lambda x: x.IsRecipe and x.Tier == ItemTier.Rare)]
ARMOR_POOL = [x.Name for x in
              ItemPool.Matching(lambda x: x.IsArmor and (x.Tier == ItemTier.Uncommon or x.Tier == ItemTier.Common))]


def build_innkeeper_inventory(item_dict, inventory_size, rng):
    items = []
    add_items(items, item_dict)


def build_boat_merchant_inventory(item_dict, inventory_size, rng):
    items = [
        (inventory_size - 1, ItemPool.Grab(Items.BOAT)),
        (1, ItemPool.Grab(Items.FISHING_ROD)),
        (0, get_infinity_item(Items.BAIT))
    ]
    add_items(items, item_dict)


def build_merchant_inventory(item_dict, inventory_size, rng):
    items = [
        (inventory_size - 1, get_infinity_item(Items.BERRY)),
        (inventory_size - 2, get_infinity_item(Items.GLASS_FLASK)),
        (inventory_size - 3, get_infinity_item(Items.WOODEN_BOWL)),
        (inventory_size - 4, get_infinity_item(Items.STONE_ARROW) if rng.Next(0, 2) == 1 else None),
        (inventory_size - 5, ItemPool.Grab(NORMAL_RECIPES[rng.Next(0, len(NORMAL_RECIPES))])),
        (inventory_size - 6, ItemPool.Grab(NORMAL_RECIPES[rng.Next(0, len(NORMAL_RECIPES))]))
    ]
    fishing_items = [
        (4, ItemPool.Grab(Items.FISHING_ROD)),
        (5, get_infinity_item(Items.BAIT))
    ]
    if rng.Next(0, 2) == 1:
        items += fishing_items
    add_items(items, item_dict)


def build_travelling_merchant_inventory(item_dict, inventory_size, rng):
    build_merchant_inventory(item_dict, inventory_size, rng)
    special_items = [
        (0, ItemPool.Grab(Items.HOLDING_BAG)),
        (1, ItemPool.Grab(ItemTier.Uncommon, COMPANION_EQUIPMENT_TYPE)),
        (3, ItemPool.Grab(Items.BOAT)),
        (7, get_infinity_item(Items.DEXTERITY_POTION)),
        (8, get_infinity_item(Items.SPEED_POTION)),
        (9, get_infinity_item(Items.STRENGTH_POTION)),
        (10, ItemPool.Grab(RARE_RECIPES[rng.Next(0, len(RARE_RECIPES))]) if rng.Next(0, 2) == 1 else None)
    ]
    add_items(special_items, item_dict)


def build_clothier_inventory(item_dict, inventory_size, rng):
    items = [
        (1, ItemPool.Grab(ARMOR_POOL[rng.Next(0, len(ARMOR_POOL))])),
        (2, ItemPool.Grab(ARMOR_POOL[rng.Next(0, len(ARMOR_POOL))])),
    ]
    add_items(items, item_dict)


def build_mason_inventory(item_dict, inventory_size, rng):
    items = [
        (1, ItemPool.Grab(ARMOR_POOL[rng.Next(0, len(ARMOR_POOL))])),
        (2, ItemPool.Grab(ARMOR_POOL[rng.Next(0, len(ARMOR_POOL))])),
    ]
    add_items(items, item_dict)


def add_items(items, dict):
    for index, item in items:
        if item: dict.Add(index, item)


def get_infinity_item(item_name):
    item = ItemPool.Grab(item_name)
    item.SetAttribute(AMOUNT_ATTRIBUTE, int.MaxValue)
    return item


assert ItemPool.Exists(Items.STONE_ARROW)
assert ItemPool.Exists(Items.GLASS_FLASK)
assert ItemPool.Exists(Items.WOODEN_BOWL)
assert ItemPool.Exists(Items.BERRY)
assert ItemPool.Exists(Items.BOAT)
assert ItemPool.Exists(Items.PUMPKIN_PIE_RECIPE)
assert ItemPool.Exists(Items.COOKED_MEAT_RECIPE)
assert ItemPool.Exists(Items.HEALTH_POTION_RECIPE)
assert ItemPool.Exists(Items.CORN_SOUP_RECIPE)
assert ItemPool.Exists(Items.FISHING_ROD)
assert ItemPool.Exists(Items.BAIT)
assert len(NORMAL_RECIPES) > 0
assert len(RARE_RECIPES) > 0
