from Core import translate
from OpenTK import Vector3
from Hedra.Core import Timer
from Hedra import World, Utils
from Hedra.Components import HealthBarComponent, HealthBarType
from Hedra.AISystem import MountAIComponent, BasicAIComponent

PET_RESPAWN_TIME = 8

def init(user, state):
    state['user'] = user
    state['dead_timer'] = Timer(PET_RESPAWN_TIME)
    state['dead_timer_set'] = False
    state['pet'] = None
    state['pet_item'] = None
    state['pet_previous_enabled'] = True
    state['pet_previous_alpha'] = 1
    state['was_riding'] = False


def update(state):
    pet = state['pet']
    user = state['user']

    if pet:
        handle_model(user, pet, state)
        
    if pet and pet.IsDead and not state['dead_timer_set']:
        state['dead_timer'].Reset()
        state['dead_timer_set'] = True

    pet_item = user.Inventory.Pet
    if (pet_item != state['pet_item']) or (pet and pet.IsDead and state['dead_timer'].Tick()):
        spawn_pet(state, pet_item)
        state['dead_timer_set'] = False


def handle_model(user, pet, state):
    if user.IsRiding or not pet.Model.Enabled:
        if not state['was_riding'] and user.IsRiding:
            state['pet_previous_enabled'] = pet.Model.Enabled
            state['pet_previous_alpha'] = pet.Model.Alpha

        pet.Model.Alpha = user.Model.Alpha
        pet.Model.Enabled = user.Model.Enabled
    else:
        pet.Model.Alpha = state['pet_previous_alpha']
        pet.Model.Enabled = state['pet_previous_enabled']
    state['was_riding'] = user.IsRiding


def spawn_pet(state, pet_item):
    pet = state['pet']
    if pet:
        pet.Dispose()
        state['pet'] = None
    
    if pet_item:
        user = state['user']
        pet = World.SpawnMob(pet_item.GetAttribute[str]("MobType"), user.Position + Vector3.UnitX * 12, Utils.Rng)
        pet.Health = pet.MaxHealth
        pet.Level = 1
        pet.RemoveComponent(pet.SearchComponent[HealthBarComponent]())
        pet.AddComponent(HealthBarComponent(pet, translate(pet.Name.ToLowerInvariant()), HealthBarType.Friendly))
        pet.RemoveComponent(pet.SearchComponent[BasicAIComponent]())
        pet.AddComponent(MountAIComponent(pet, user))
        pet.Removable = False
        pet.IsFriendly = True
        pet.Model.IsMountable = True
        state['pet'] = pet
    state['pet_item'] = pet_item
        
def get_entity(state):
    return state['pet']