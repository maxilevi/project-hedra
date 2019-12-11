import clr
from Hedra import World
from Hedra.Components import TradeComponent, TalkComponent
from Hedra.Mission import MissionBuilder, QuestTier, DialogObject
from Hedra.Mission.Blocks import TalkMission
from System import Array, Object, Single
from System.Numerics import Vector3
from Hedra.Numerics import VectorExtensions
from Hedra.Engine.WorldBuilding import NameGenerator, NPCCreator

clr.ImportExtensions(VectorExtensions)

IS_QUEST = True
QUEST_NAME = 'TheBeginning'
QUEST_TIER = QuestTier.Easy
IS_STORYLINE = True
MAX_DISTANCE = 768

def setup_timeline(position, giver, owner, rng):
    builder = MissionBuilder()

    builder.OpeningDialog = create_dialog()

    talk_to = []
    for i in range(rng.Next(3, 7)):
        npc_position = position + Vector3(
            Single(rng.NextDouble() * MAX_DISTANCE * 2 - MAX_DISTANCE),
            Single(0.0),
            Single(rng.NextDouble() * MAX_DISTANCE * 2 - MAX_DISTANCE)
        )
        entity = NPCCreator.SpawnVillager(npc_position, rng)
        trade = entity.SearchComponent[TradeComponent]()
        if trade: entity.RemoveComponent(trade)
        talk = entity.SearchComponent[TalkComponent]()
        if talk: entity.RemoveComponent(talk)
        talk_to.append(entity)
    
    for npc in talk_to:
        talk = TalkMission(create_talk_dialog(npc))
        talk.Humanoid = npc
        builder.Next(talk)
        
    builder.ReturnToComplete = False
    return builder

def create_talk_dialog(npc):
    dialog = DialogObject()
    dialog.Keyword = 'quest_the_beginning_npc_dialog'
    dialog.Arguments = Array[Object]([npc.Name])
    return dialog

def create_dialog():
    dialog = DialogObject()
    dialog.Keyword = 'quest_the_beginning_dialog'
    dialog.Arguments = Array[Object]([])
    return dialog

def can_give(position):
    return False
