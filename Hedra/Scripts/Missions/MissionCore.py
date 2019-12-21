import clr
import System
import Items
from Hedra import World
from Hedra.Structures import MapBuilder
from System.Numerics import Vector2, Vector3
from Hedra.Numerics import VectorExtensions
from Hedra.Mission import QuestReward, ItemCollect, DialogObject
from Hedra.Mission.Blocks import EndMission

clr.ImportExtensions(VectorExtensions)

CHUNK_WIDTH = 128
DEFAULT_MAX_STRUCTURE_SEARCH_DISTANCE = 4096

def nearby_struct_objects(position, type, max_distance=DEFAULT_MAX_STRUCTURE_SEARCH_DISTANCE):
    def do_find(struct_object):
        return isinstance(struct_object.Design, type) and (struct_object.Position - position).Xz().LengthSquared() < max_distance * max_distance
    return World.StructureHandler.Find(do_find)

def nearby_structs_positions_designs(position, type, max_distance=DEFAULT_MAX_STRUCTURE_SEARCH_DISTANCE):
    radius = int(max_distance / CHUNK_WIDTH)
    structs = []
    chunk_space = World.ToChunkSpace(position)
    for x in xrange(-radius, radius):
        for z in xrange(-radius, radius):
            final_position = Vector3(chunk_space.X + x * CHUNK_WIDTH, 0, chunk_space.Y + z * CHUNK_WIDTH)
            region = World.BiomePool.GetRegion(final_position)
            sample = MapBuilder.Sample(final_position, region)
            if isinstance(sample, type):
                structs.append((sample, final_position))
    return structs

def nearby_structs_designs(position, type, max_distance=DEFAULT_MAX_STRUCTURE_SEARCH_DISTANCE):
    return [design for design, _ in nearby_structs_positions_designs(position, type, max_distance)]

def nearby_structs_positions(position, type, max_distance=DEFAULT_MAX_STRUCTURE_SEARCH_DISTANCE):
    return [final_position for _, final_position in nearby_structs_positions_designs(position, type, max_distance)]

def find_structure(position, type, max_distance=DEFAULT_MAX_STRUCTURE_SEARCH_DISTANCE):
    nearby = nearby_struct_objects(position, type, max_distance)
    objects = None
    # If nearby structure have not been created yet we force them
    if not nearby:
        positions = nearby_structs_positions(position, type, max_distance)
        if not positions:
            raise System.ArgumentOutOfRangeException('Tried to fetch a structure but it has no nearby designs')
        for position in positions:
            World.StructureHandler.CheckStructures(position.Xz())
            objects = nearby_struct_objects(position, type, max_distance)
            if objects: break
    return nearby[0] if nearby else objects[0]

def is_inside_structure(position, structure_type):
    structures = nearby_struct_objects(position, structure_type)
    return any([(position - object.Position).Xz().LengthSquared() < object.Design.PlateauRadius ** 2 for object in structures])

def is_within_distance(entity_position, structure_position, max_distance=DEFAULT_MAX_STRUCTURE_SEARCH_DISTANCE):
    return (entity_position - structure_position).Xz().LengthSquared() < max_distance * max_distance

def build_generic_reward(rng):
    n = rng.NextDouble()
    reward = QuestReward()
    if n < 0.45:
        reward.Experience = int(rng.Next(3, 12))
    elif n < 0.9:
        reward.Gold = int(rng.Next(15, 45))
    return reward

def remove_component_if_exists(parent, type):
    component = parent.SearchComponent[type]()
    if component:
        parent.RemoveComponent(component)
        
def contains_quest(owner, name):
    for quest in owner.Questing.ActiveQuests:
        if quest.QuestType == name:
            return True
    return False

def make_item_string(item):
    return EndMission.MakeItemString(item)

def to_item_collect(item):
    collect = ItemCollect()
    collect.Name = item.Name
    collect.Amount = item.GetAttribute[int](Items.AMOUNT_ATTRIBUTE) if item.HasAttribute(Items.AMOUNT_ATTRIBUTE) else 1
    return collect

def create_dialog(keyword, params = []):
    dialog = DialogObject()
    dialog.Keyword = keyword
    dialog.Arguments = System.Array[System.Object](params)
    return dialog