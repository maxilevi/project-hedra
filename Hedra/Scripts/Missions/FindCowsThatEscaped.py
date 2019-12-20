import clr
import MissionCore
import FarmCore
import Items
from System import Single, Array, Func, Boolean
from System.Numerics import Vector4, Vector3
from Hedra import World
from Hedra.Mission import MissionBuilder, QuestTier, QuestReward, DialogObject
from Hedra.Mission.Blocks import ReuniteEntitiesMission
from Hedra.Numerics import VectorExtensions
from Hedra.Components import DamageComponent
from Hedra.EntitySystem import IEntity
from Hedra.Items import ItemPool

clr.ImportExtensions(VectorExtensions)
IS_QUEST = True
QUEST_NAME = 'FindCowsThatEscaped'
QUEST_TIER = QuestTier.Easy
MAX_SPAWN_DISTANCE = 512


def setup_timeline(position, giver, owner, rng):

    builder = MissionBuilder()
    builder.OpeningDialog = MissionCore.create_dialog('quest_find_cows_that_escaped_dialog')

    cows = create_cows(giver, rng)
    builder.FailWhen = lambda: any([cow.IsDead for cow in cows])
        
    reunite = ReuniteEntitiesMission(giver.Position, Array[IEntity](cows))
    reunite.MissionBlockStart += lambda: owner.Inventory.AddItem(ItemPool.Grab(Items.ANIMAL_FOOD))
        
    builder.Next(reunite)

    reward = FarmCore.get_reward(rng)
    reward.RewardGiven += lambda: remove_animal_food(owner)
    builder.SetReward(reward)
    return builder

def remove_animal_food(owner):
    item = owner.Inventory.Search(lambda x: x.Name == Items.ANIMAL_FOOD)
    if item:
        owner.Inventory.RemoveItem(item)

def create_cows(giver, rng):
    cows = []
    count = rng.Next(2, 4)
    for i in range(count):
        position = giver.Position + Vector3(
            Single(rng.NextDouble() * MAX_SPAWN_DISTANCE * 2 - MAX_SPAWN_DISTANCE),
            Single(0.0),
            Single(rng.NextDouble() * MAX_SPAWN_DISTANCE * 2 - MAX_SPAWN_DISTANCE)
        )
        cow = World.SpawnMob('Cow', position, rng)
        cow.AddBonusSpeedWhile(0.75, Func[Boolean](lambda: True))
        cows.append(cow)
    return cows

def can_give(position):
    return FarmCore.can_give(position)