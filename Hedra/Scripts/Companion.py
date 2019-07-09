import TextDisplay
from Core import translate
from System import Array, Single
from OpenTK import Vector3
from Hedra.Core import Timer, Time
from Hedra import World, Utils
from Hedra.Components import HealthBarComponent, HealthBarType, RideComponent, DamageComponent
from Hedra.AISystem import MountAIComponent, BasicAIComponent
from Hedra.Engine.ItemSystem.Templates import ItemTemplate, ItemModelTemplate, AttributeTemplate
from Hedra.Items import ItemTier, ItemPool

COMPANION_RESPAWN_TIME = 24
COMPANION_EQUIPMENT_TYPE = 'Pet'
CAGE_MODEL_SCALE = 0.1
CAGE_MODEL_PATH = 'Assets/Items/Misc/CompanionCage.ply'
GROWTH_ATTRIB_NAME = 'Growth'
IS_GROWN_ATTRIB_NAME = 'IsGrown'
CAN_RIDE_ATTRIB_NAME = 'CanRide'
MAX_SCALE_ATTRIB_NAME = 'MaxScale'
XP_ATTRIB_NAME = 'PetXp'
MOB_TYPE_ATTRIB_NAME = 'Type'
BASE_GROWTH_SCALE = 0.5
GROWTH_TIME = .1 * 60.0 # 8 Minutes
GROWTH_SPEED = 1.0 / GROWTH_TIME
COMPANION_TYPES = [
    ('Pug', ItemTier.Common, True),
    ('Bee', ItemTier.Common, True),
    ('Wasp', ItemTier.Common, True),
    ('Ooze', ItemTier.Common, False),
    ('Pig', ItemTier.Common, False),
    ('Sheep', ItemTier.Common, True),
    ('Wolf', ItemTier.Common, True),
    ('Horse', ItemTier.Common, True)
]
DEFAULT_RIDE_INFO = 0.5
RIDE_INFO = {
    'Pug': 0.4,
    'Bee': 0.630
}

def init(user, state):
    state['user'] = user
    state['dead_timer'] = Timer(COMPANION_RESPAWN_TIME)
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
        
    if pet_item:
        update_growth(pet_item, pet)


def update_growth(pet_item, pet):

    current_growth = pet_item.GetAttribute[float](GROWTH_ATTRIB_NAME)
    if not pet_item.HasAttribute(IS_GROWN_ATTRIB_NAME) and pet:
        max_scale = unserialize_pet_max_scale(pet_item)
        if abs(current_growth - 1.0) > 0.005:
            new_growth = Time.DeltaTime * GROWTH_SPEED
            pet_item.SetAttribute(GROWTH_ATTRIB_NAME, current_growth + new_growth)
            
            base_scale = max_scale * Single(BASE_GROWTH_SCALE)
            growth_scale = max_scale * Single((1.0 - BASE_GROWTH_SCALE) * current_growth)
            pet.Model.Scale = base_scale + growth_scale
        else:
            pet_item.SetAttribute(GROWTH_ATTRIB_NAME, 1.0)
            pet_item.SetAttribute(IS_GROWN_ATTRIB_NAME, True, Hidden=True)
            pet.Model.IsMountable = True
            pet.Model.Scale = max_scale
   
def unserialize_pet_max_scale(pet_item):
    scale_array = pet_item.GetAttribute[Array[Single]](MAX_SCALE_ATTRIB_NAME)
    return Vector3(scale_array[0], scale_array[1], scale_array[2])

def serialize_pet_max_scale(pet_item, max_scale):
    pet_item.SetAttribute(
        MAX_SCALE_ATTRIB_NAME,
        Array[Single]([
            max_scale.X,
            max_scale.Y,
            max_scale.Z
        ]),
        Hidden=True
    )

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
        type = pet_item.GetAttribute[str](MOB_TYPE_ATTRIB_NAME)
        pet = World.SpawnMob(type, user.Position + Vector3.UnitX * 12, Utils.Rng)
        pet.Health = pet.MaxHealth
        pet.Level = 1
        serialize_pet_max_scale(pet_item, pet.Model.Scale)
        pet.SearchComponent[DamageComponent]().Ignore(lambda entity: entity == user)
        pet.RemoveComponent(pet.SearchComponent[HealthBarComponent]())
        pet.AddComponent(HealthBarComponent(pet, translate(pet.Name.ToLowerInvariant()), HealthBarType.Friendly))
        pet.RemoveComponent(pet.SearchComponent[BasicAIComponent]())
        pet.AddComponent(MountAIComponent(pet, user))
        pet.SearchComponent[MountAIComponent]().Enabled = True
        pet.Removable = False
        pet.IsFriendly = True
        if pet_item.GetAttribute[bool](CAN_RIDE_ATTRIB_NAME):
            pet.AddComponent(RideComponent(pet, RIDE_INFO[type] if type in RIDE_INFO else DEFAULT_RIDE_INFO))
            pet.Model.IsMountable = pet_item.HasAttribute(IS_GROWN_ATTRIB_NAME)
        state['pet'] = pet
    state['pet_item'] = pet_item
  
# Companion items are dynamically generated from the list above


def create_companion_templates():
    items = []
    for type, tier, can_ride in COMPANION_TYPES:
        items += [create_companion_template(type, tier, can_ride)]
    return Array[ItemTemplate](items)
   
     
def create_companion_template(type, tier, can_ride):
    mob_template = World.MobFactory.GetFactory(type)
    model_template = ItemModelTemplate()
    model_template.Path = CAGE_MODEL_PATH#mob_template.Model.Path
    model_template.Scale = CAGE_MODEL_SCALE#mob_template.Model.Scale * .05
    
    template = ItemTemplate()
    template.Name = 'Companion' + type
    template.DisplayName = translate('generic_companion_item_name', translate(type.lower()))
    template.Description = translate('generic_companion_item_desc', translate(type.lower()))
    template.Tier = tier
    template.EquipmentType = COMPANION_EQUIPMENT_TYPE
    template.Attributes = create_companion_attributes(type, can_ride)
    template.Model = model_template
    return template


def create_companion_attributes(type, can_ride):
    can_ride = can_ride
    pet_attribute = AttributeTemplate()
    pet_attribute.Name = MOB_TYPE_ATTRIB_NAME
    pet_attribute.Value = type
    pet_attribute.Hidden = True
    
    ride_attribute = AttributeTemplate()
    ride_attribute.Name = CAN_RIDE_ATTRIB_NAME
    ride_attribute.Value = can_ride
    ride_attribute.Hidden = True

    growth_attribute = AttributeTemplate()
    growth_attribute.Value = 0.0
    growth_attribute.Name = GROWTH_ATTRIB_NAME
    growth_attribute.Display = TextDisplay.PERCENTAGE_DISPLAY
    growth_attribute.Persist = True

    xp_attribute = AttributeTemplate()
    xp_attribute.Value = 0.0
    xp_attribute.Name = XP_ATTRIB_NAME
    xp_attribute.Hidden = True
    xp_attribute.Persist = True
    
    return Array[AttributeTemplate]([
        pet_attribute,
        ride_attribute,
        growth_attribute,
        xp_attribute
    ])
    
        
for name, _, _ in COMPANION_TYPES:
    assert World.MobFactory.ContainsFactory(name)

templates = create_companion_templates()
for template in templates:
    if not ItemPool.Exists(template.Name):
        ItemPool.Load(template)