from Hedra.Mission import Quests

QUEST_ORDER = [
    Quests.TheBeginning
]


def get_next_quest(story_settings):
    return QUEST_ORDER[story_settings.CompletedSteps]


def has_finished_story(story_settings):
    return len(QUEST_ORDER) == story_settings.CompletedSteps
