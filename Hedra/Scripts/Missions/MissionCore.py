import clr
from Hedra import World
from Hedra.Structures import MapBuilder
from System.Numerics import Vector2, Vector3
from Hedra.Numerics import VectorExtensions
from Hedra.Mission import QuestReward
from Hedra.Mission.Blocks import EndMission

clr.ImportExtensions(VectorExtensions)

CHUNK_WIDTH = 128
DEFAULT_MAX_STRUCTURE_SEARCH_DISTANCE = 4096

def nearby_struct_objects(position, type, max_distance=DEFAULT_MAX_STRUCTURE_SEARCH_DISTANCE):
    def do_find(struct_object):
        return isinstance(struct_object.Design, type) and (struct_object.Position - position).Xz().LengthSquared() < max_distance * max_distance
    return World.StructureHandler.Find(do_find)

def nearby_structs_designs(position, type, max_distance=DEFAULT_MAX_STRUCTURE_SEARCH_DISTANCE):
    radius = int(max_distance / CHUNK_WIDTH)
    structs = []
    chunk_space = World.ToChunkSpace(position)
    for x in xrange(-radius, radius):
        for z in xrange(-radius, radius):
            final_position = Vector3(chunk_space.X + x * CHUNK_WIDTH, 0, chunk_space.Y + z * CHUNK_WIDTH)
            region = World.BiomePool.GetRegion(final_position)
            sample = MapBuilder.Sample(final_position, region)
            if isinstance(sample, type):
                structs.append(sample)
    return structs

def find_structure(position, type, max_distance=DEFAULT_MAX_STRUCTURE_SEARCH_DISTANCE):
    return nearby_struct_objects(position, type, max_distance)[0]

def is_within_distance(entity_position, structure_position, max_distance=DEFAULT_MAX_STRUCTURE_SEARCH_DISTANCE):
    return (entity_position - structure_position).Xz().LengthSquared() < max_distance * max_distance

def build_generic_reward(rng):
    n = rng.NextDouble()
    reward = QuestReward()
    if n < 0.4:
        reward.Experience = int(rng.Next(2, 8))
    elif n < 0.9:
        reward.Gold = int(rng.Next(15, 35))
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