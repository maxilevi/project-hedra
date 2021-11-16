import clr
import System
import Items
from Hedra import World
from Hedra.Engine.StructureSystem import StructureDesign
from Hedra.Numerics import VectorExtensions
from Hedra.Mission import QuestReward, ItemCollect, DialogObject
from Hedra.Mission.Blocks import EndMission
from Hedra.AISystem.Humanoid import FollowAIComponent
from Hedra.AISystem import IBasicAIComponent

clr.ImportExtensions(VectorExtensions)

CHUNK_WIDTH = 128
DEFAULT_MAX_STRUCTURE_SEARCH_DISTANCE = 4096


def nearby_struct_objects(position, type, max_distance=DEFAULT_MAX_STRUCTURE_SEARCH_DISTANCE):
    def do_find(struct_object):
        return isinstance(struct_object.Design, type) and (
                    struct_object.Position - position).Xz().LengthSquared() < max_distance ** 2

    return World.StructureHandler.Find(do_find)


def nearby_structs_positions_designs(position, type, max_distance=DEFAULT_MAX_STRUCTURE_SEARCH_DISTANCE):
    return World.StructureHandler.NearbyStructuresPositionDesigns(position, type, max_distance)


def nearby_structs_designs(position, type, max_distance=DEFAULT_MAX_STRUCTURE_SEARCH_DISTANCE):
    return [pair.One for pair in nearby_structs_positions_designs(position, type, max_distance)]


def nearby_structs_positions(position, type, max_distance=DEFAULT_MAX_STRUCTURE_SEARCH_DISTANCE):
    return [pair.Two for pair in nearby_structs_positions_designs(position, type, max_distance)]


def find_structure(position, type, max_distance=DEFAULT_MAX_STRUCTURE_SEARCH_DISTANCE):
    nearby = nearby_struct_objects(position, type, max_distance)
    objects = None
    # If nearby structure have not been created yet we force them
    if not nearby:
        positions = nearby_structs_positions(position, type, max_distance)
        if not positions:
            raise System.ArgumentOutOfRangeException('Tried to fetch a structure but it has no nearby designs')
        search_for = System.Array[StructureDesign]([type()])
        for position in positions:
            World.StructureHandler.CheckStructures(position.Xz())  # , search_for)
            objects = nearby_struct_objects(position, type, max_distance)
            if objects: break
    return nearby[0] if nearby else objects[0]


def find_and_bind_structure(builder, position, type, max_distance=DEFAULT_MAX_STRUCTURE_SEARCH_DISTANCE):
    structure = find_structure(position, type, max_distance)
    if structure:
        structure.ActiveQuests += 1

        def on_disposed():
            structure.ActiveQuests -= 1

        builder.MissionDispose += on_disposed
    return structure


def is_inside_structure(position, structure_type):
    structures = nearby_struct_objects(position, structure_type)
    return any(
        [(position - object.Position).Xz().LengthSquared() < object.Design.PlateauRadius ** 2 for object in structures])


def is_within_distance(entity_position, structure_position, max_distance=DEFAULT_MAX_STRUCTURE_SEARCH_DISTANCE):
    return (entity_position - structure_position).Xz().LengthSquared() < max_distance ** 2


def build_generic_reward(rng):
    n = rng.NextDouble()
    reward = QuestReward()
    if n < 0.45:
        reward.Experience = int(rng.Next(5, 12))
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


def create_dialog(keyword, params=[]):
    dialog = DialogObject()
    dialog.Keyword = keyword
    dialog.Arguments = System.Array[System.Object](params)
    return dialog


def make_follow(giver, target):
    remove_component_if_exists(giver, IBasicAIComponent)
    follow = FollowAIComponent(giver, target)
    giver.AddComponent(follow)
    giver.AddBonusSpeedWhile(0.5,
                             System.Func[System.Boolean](lambda: giver.SearchComponent[FollowAIComponent]() == follow))
