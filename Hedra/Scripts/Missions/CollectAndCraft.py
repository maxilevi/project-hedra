from Hedra.Mission import MissionBuilder
from Hedra.Mission.Blocks import CollectMission, TalkMission, CraftMission

IS_QUEST = True

def setup_timeline(rng):
    builder = MissionBuilder()
    has_crafting = True
    
    add_collect_mission(builder, has_crafting)
    
    if has_crafting:
        add_craft_mission(builder)
        
    return builder.Mission

def add_collect_mission(builder, has_crafting):
    collect = CollectMission()
    builder.Next(collect)

    talk = TalkMission()
    builder.Next(talk)

def add_craft_mission(builder):
    craft = CraftMission()
    builder.Next(craft)

    talk = TalkMission()
    builder.Next(talk)

def place():
    pass

def can_give():
    return True