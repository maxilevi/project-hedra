from Hedra import World
from Hedra.Structures import MapBuilder
from OpenTK import Vector2, Vector3

CHUNK_WIDTH = 128

def nearby_struct_objects(position, type, max_distance):
    def do_find(struct_object):
        return isinstance(struct_object.Design, type) and (struct_object.Position - position).Xz.LengthSquared < max_distance * max_distance
    return World.StructureHandler.Find(do_find)

def nearby_structs_designs(position, type, max_distance):
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

def find_structure(position, type, max_distance):
    return nearby_struct_objects(position, type, max_distance)[0]