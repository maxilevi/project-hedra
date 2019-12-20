import VisualEffects
import clr
import MissionCore
import FarmCore
from System import Single
from System.Numerics import Vector4, Vector3
from Hedra import World
from Hedra.Mission import MissionBuilder, QuestTier, QuestReward, DialogObject
from Hedra.Mission.Blocks import CatchAnimalMission
from Hedra.Numerics import VectorExtensions

clr.ImportExtensions(VectorExtensions)
IS_QUEST = True
QUEST_NAME = 'FindCowsThatEscaped'
QUEST_TIER = QuestTier.Easy
MAX_SPAWN_DISTANCE = 768


def setup_timeline(position, giver, owner, rng):
    captured_vars = {'disposed': False}

    def on_dispose():
        captured_vars['disposed'] = True

    builder = MissionBuilder()
    builder.OpeningDialog = MissionCore.create_dialog('quest_find_cows_that_escaped_dialog')
    builder.MissionDispose += on_dispose
    # Add wheat weapon, make wheat attract cows. Complete when all cows within radius
    cows = create_cows(giver, rng)
    for cow in cows:
        catch = CatchAnimalMission()
        catch.Animal = cow
        catch.MissionBlockStart += lambda: VisualEffects.outline_while(cow, Vector4(1.0, 0.0, 0.0, 1.0), lambda: not captured_vars['disposed'])
        builder.Next(catch)

    reward = FarmCore.get_reward(rng)
    builder.SetReward(reward)
    return builder


def create_cows(giver, rng):
    cows = []
    count = rng.Next(2, 6)
    for i in range(count):
        position = giver.Position + Vector3(
            Single(rng.NextDouble() * MAX_SPAWN_DISTANCE * 2 - MAX_SPAWN_DISTANCE),
            Single(0.0),
            Single(rng.NextDouble() * MAX_SPAWN_DISTANCE * 2 - MAX_SPAWN_DISTANCE)
        )
        cow = World.SpawnMob('Cow', position, rng)
        cows.append(cow)
    return cows

def can_give(position):
    return FarmCore.can_give(position)