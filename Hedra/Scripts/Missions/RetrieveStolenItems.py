from System import Array, Object, Single
import MissionCore
from System.Numerics import Vector3
from Hedra.Items import ItemPool, ItemTier
from Hedra.Mission import MissionBuilder, QuestTier, DialogObject, QuestReward, ItemCollect
from Hedra.Mission.Blocks import FindEntityMission, DefeatEntityMission, CollectMission
from Hedra.AISystem import IBehaviourComponent
from Hedra.AISystem.Humanoid import EscapeAIComponent, CombatAIComponent
from Hedra.Engine.WorldBuilding import NPCCreator, BanditOptions, NameGenerator
from Hedra.Engine.EntitySystem import DropComponent

IS_QUEST = True
QUEST_NAME = 'RetrieveStolenItems'
QUEST_TIER = QuestTier.Medium
MAX_SPAWN_DISTANCE = 768

def setup_timeline(position, giver, owner, rng):
    builder = MissionBuilder()
    
    items = create_items(rng)
    
    builder.OpeningDialog = create_opening_dialog(items)
    
    criminals = create_criminals(owner, giver, items, rng)
    for criminal in criminals:
        find = FindEntityMission(criminals[0])
        find.MissionBlockEnd += lambda: on_found(owner, criminal, rng)
        builder.Next(find)
        
        defeat = DefeatEntityMission(criminal)
        builder.Next(defeat)
    
    collect = CollectMission()
    collect.Items = Array[ItemCollect]([MissionCore.to_item_collect(x) for x in items])
    collect.MissionBlockEnd += lambda: collect.ConsumeItems()
    builder.Next(collect)
    
    builder.SetReward(create_reward(items, rng))
    return builder

def create_criminals(owner, giver, items, rng):
    criminals = []
    for item in items:
        position = giver.Position + Vector3(
            Single(rng.NextDouble() * MAX_SPAWN_DISTANCE * 2 - MAX_SPAWN_DISTANCE),
            Single(0.0),
            Single(rng.NextDouble() * MAX_SPAWN_DISTANCE * 2 - MAX_SPAWN_DISTANCE)
        )
        bandit = NPCCreator.SpawnBandit(position, max(1, owner.Level - rng.Next(0, 5)), BanditOptions.Default)
        bandit.Name = NameGenerator.PickMaleName(rng)
        bandit.RemoveComponent(bandit.SearchComponent[DropComponent]())
        drop = DropComponent(bandit)
        drop.RandomDrop = False
        drop.ItemDrop = item
        drop.DropChance = 100
        bandit.AddComponent(drop)
        criminals.append(bandit)
    return criminals

def on_found(owner, npc, rng):
    if rng.Next(0, 3) == 1:
        ai = npc.SearchComponent[IBehaviourComponent]()
        if ai: npc.RemoveComponent(ai)
        npc.AddComponent(EscapeAIComponent(npc, owner))
    else:
        npc.SearchComponent[CombatAIComponent]().SetTarget(owner)

def create_opening_dialog(items):
    dialog = DialogObject()
    dialog.Keyword = 'quest_retrieve_stolen_items_dialog'
    dialog.Arguments = Array[Object]([])
    return dialog
    
def create_items(rng):
    count = rng.Next(1, 4)
    items = []
    options = ItemPool.Matching(lambda x: (x.IsWeapon or x.IsArmor) and x.Tier <= ItemTier.Rare)
    for i in range(count):
        items.append(
            ItemPool.Grab(options[rng.Next(0, len(options))].Name)
        )
    return items

def create_reward(items, rng):
    reward = QuestReward()
    reward.Gold = rng.Next(7, 27)
    reward.Item = items[rng.Next(0, len(items))] if rng.Next(0, 3) == 1 else None
    return reward

def can_give(position):
    return True