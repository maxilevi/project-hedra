from Hedra.Mission import MissionBuilder, QuestTier

IS_QUEST = True
QUEST_NAME = 'CraftAPotion'
QUEST_TIER = QuestTier.Easy

def setup_timeline(position, giver, owner, rng):
    builder = MissionBuilder()
    return builder


def can_give(position):
    return False