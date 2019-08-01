import Companion
from Hedra import World
from Hedra.Mission import MissionBuilder, QuestTier, QuestReward, DialogObject
from Hedra.Mission.Blocks import CatchAnimalMission
from Hedra.Items import ItemPool
from System import Object, Array

IS_QUEST = True
QUEST_NAME = 'CatchAPet'
QUEST_TIER = QuestTier.Easy
MAX_DISTANCE_SQUARED = 256 ** 2
MIN_DISTANCE_SQUARED = 48 ** 2
VALID_PETS = [
    'Pug'
]

def get_pet_reward(name):
    item = ItemPool.Grab(name)
    Companion.set_max_growth(item)
    return item

POSSIBLE_REWARDS = {
    'Pug': get_pet_reward('CompanionPug'),
}

def setup_timeline(position, giver, owner, rng):
    builder = MissionBuilder()
    
    animals = get_nearby_animals(position)
    catch = CatchAnimalMission()
    catch.Animal = animals[rng.Next(0, len(animals))]
    builder.Next(catch)
    
    reward = build_reward(catch.Animal, rng)
    builder.SetReward(reward)
    return builder

def build_reward(animal, rng):
    n = rng.NextDouble()
    reward = QuestReward()
    if n < 0.3:
        reward.Experience = rng.Next(5, 12)
    elif n < 0.7:
        reward.Gold = rng.Next(7, 17)
    elif 0.85 < n < 0.95:
        reward.Item = POSSIBLE_REWARDS[animal.Type]
        reward.CustomDialog = DialogObject()
        reward.CustomDialog.Keyword = 'quest_catch_animal_item'
        reward.CustomDialog.Arguments = Array[Object]([animal.Name.ToUpperInvariant()])
    return reward

def get_nearby_animals(position):
    return filter(lambda x: x.Type in VALID_PETS and MIN_DISTANCE_SQUARED < (x.Position.Xz - position.Xz).LengthSquared < MAX_DISTANCE_SQUARED, World.Entities)

def can_give(position):
    return len(get_nearby_animals(position)) > 0