from Hedra import World
from Hedra.Mission import MissionBuilder, QuestTier

IS_QUEST = True
QUEST_NAME = 'CatchAPet'
QUEST_TIER = QuestTier.Easy
MAX_DISTANCE_SQUARED = 64 ** 2
VALID_PETS = [
    'Pug'
]

def setup_timeline(position, giver, owner, rng):
    builder = MissionBuilder()
    return builder


def can_give(position):
    return len(filter(lambda x: x.Type.lower() in VALID_PETS and (x.Position.Xz - position.Xz).LengthSquared < MAX_DISTANCE_SQUARED, World.Entities)) > 0