import TextDisplay
import clr
from Core import translate
from System import Array, Single
from System.Numerics import Vector3
from Hedra.Core import Timer, Time
from Hedra import World, Utils
from Hedra.Engine.EntitySystem import DropComponent
from Hedra.Components import HealthBarComponent, HealthBarType, RideComponent, DamageComponent, CompanionStatsComponent
from Hedra.AISystem import MinionAIComponent, BasicAIComponent
from Hedra.Engine.ItemSystem.Templates import ItemTemplate, ItemModelTemplate, AttributeTemplate
from Hedra.Items import ItemTier, ItemPool
from Hedra.Rendering import VertexData
from Hedra.Numerics import VectorExtensions

clr.ImportExtensions(VectorExtensions)
COMPANION_RESPAWN_TIME = 48 # Seconds
COMPANION_EQUIPMENT_TYPE = 'Pet'
CAGE_MODEL_SCALE = 0.75
CAGE_MODEL_PATH = 'Assets/Items/Misc/CompanionCage.ply'
GROWTH_ATTRIB_NAME = 'Growth'
IS_GROWN_ATTRIB_NAME = 'IsGrown'
CAN_RIDE_ATTRIB_NAME = 'CanRide'
MAX_SCALE_ATTRIB_NAME = 'MaxScale'
MODEL_ATTRIB_NAME = 'CompanionModel'
XP_ATTRIB_NAME = 'PetXp'
NAME_ATTRIB_NAME = 'PetName'
DEAD_TIMER_ATTRIB_NAME = 'DeadTimer'
HEALTH_ATTRIB_NAME = 'PetHealth'
MOB_TYPE_ATTRIB_NAME = 'Type'
RIDE_HEIGHT_ATTRIB = 'RideHeight'
PRICE_ATTRIB_NAME = 'Price'
BASE_GROWTH_SCALE = 0.5
GROWTH_TIME = 8.0 * 60.0 # 8 Minutes
GROWTH_SPEED = 1.0 / GROWTH_TIME
BASE_PRICE = 35
COMPANION_TYPES = [
    ('Pug', ItemTier.Uncommon, True),
    ('Cow', ItemTier.Uncommon, False),
    ('Bee', ItemTier.Rare, True),
    ('Wasp', ItemTier.Rare, True),
    ('Ooze', ItemTier.Rare, False),
    ('Pig', ItemTier.Uncommon, False),
    ('Sheep', ItemTier.Uncommon, False),
    ('Wolf', ItemTier.Uncommon, False),
    ('Horse', ItemTier.Uncommon, True),
    ('Deer', ItemTier.Rare, True),
    ('Crow', ItemTier.Rare, True),
    ('Alpaca', ItemTier.Unique, True),
    ('Raccoon', ItemTier.Rare, True),
    ('Crab', ItemTier.Unique, True),
    ('Turtle', ItemTier.Rare, True),
    ('Squirrel', ItemTier.Unique, True),
    ('MeleeBeetle', ItemTier.Unique, True),
    ('RangedBeetle', ItemTier.Unique, True),
    ('Bison', ItemTier.Unique, True)
]

def init(user, state):
    state['user'] = user
    state['pet'] = None
    state['pet_item'] = None
    state['pet_previous_enabled'] = True
    state['pet_previous_alpha'] = 1
    state['was_riding'] = False
    state['was_dead'] = False


def update(state):
    pet = state['pet']
    user = state['user']
    pet_item = user.Inventory.Pet

    if pet:
        if not state['was_dead'] and pet.IsDead:
            get_dead_timer(pet_item).Reset()
        state['was_dead'] = pet.IsDead
        handle_model(user, pet, state)

    if pet_item and (not pet or pet.IsDead) and get_dead_timer(pet_item).Ready:
        spawn_pet(state, pet_item)
        state['pet'].Health = state['pet'].MaxHealth
        pet_item.SetAttribute(HEALTH_ATTRIB_NAME, state['pet'].MaxHealth)

    if pet_item != state['pet_item'] and ((pet_item and get_dead_timer(pet_item).Ready) or (not pet_item)):
        spawn_pet(state, pet_item)
    
    if pet_item:
        if not pet or pet.IsDead:
            get_dead_timer(pet_item).Tick()
        update_growth(pet_item, pet)

def get_dead_timer(pet_item):
    return pet_item.GetAttribute[Timer](DEAD_TIMER_ATTRIB_NAME)

def get_pet_growth(pet_item):
    return pet_item.GetAttribute[float](GROWTH_ATTRIB_NAME)

def update_growth(pet_item, pet):

    current_growth = get_pet_growth(pet_item)
    if current_growth < 1.0 and pet:
        max_scale = unserialize_pet_max_scale(pet_item)
        if abs(current_growth - 1.0) > 0.005:
            new_growth = Time.DeltaTime * GROWTH_SPEED
            pet_item.SetAttribute(GROWTH_ATTRIB_NAME, current_growth + new_growth)
            
            base_scale = max_scale * Single(BASE_GROWTH_SCALE)
            growth_scale = max_scale * Single((1.0 - BASE_GROWTH_SCALE) * current_growth)
            pet.Model.Scale = base_scale + growth_scale
        else:
            set_max_growth(pet_item)
            pet.Model.IsMountable = True
            pet.Model.Scale = max_scale

def set_max_growth(pet_item):
    pet_item.SetAttribute(GROWTH_ATTRIB_NAME, 1.0)
   
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
            pet.SearchComponent[MinionAIComponent]().Enabled = False

        pet.Model.Alpha = user.Model.Alpha
        pet.Model.Enabled = user.Model.Enabled
    elif not user.IsRiding:
        if state['was_riding']:
            pet.Model.Alpha = state['pet_previous_alpha']
            pet.Model.Enabled = state['pet_previous_enabled']
            pet.SearchComponent[MinionAIComponent]().Enabled = True
    state['was_riding'] = user.IsRiding

def get_ride_height(pet_item):
    return pet_item.GetAttribute[float](RIDE_HEIGHT_ATTRIB)


def spawn_pet(state, pet_item):
    pet = state['pet']
    if pet:
        pet.Dispose()
        state['pet'] = None
    
    if pet_item:
        user = state['user']
        type = pet_item.GetAttribute[str](MOB_TYPE_ATTRIB_NAME)
        pet = World.SpawnMob(type, user.Position + Vector3.UnitX * 12, Utils.Rng)
        pet.Name = pet_item.GetAttribute[str](NAME_ATTRIB_NAME)
        serialize_pet_max_scale(pet_item, pet.Model.Scale)
        pet.SearchComponent[DamageComponent]().Ignore(lambda entity: entity == user)
        pet.RemoveComponent(pet.SearchComponent[HealthBarComponent]())
        pet.AddComponent(HealthBarComponent(pet, pet.Name, HealthBarType.Friendly))
        pet.RemoveComponent(pet.SearchComponent[BasicAIComponent]())
        pet.AddComponent(MinionAIComponent(pet, user))
        pet.AddComponent(CompanionStatsComponent(pet, pet_item))
        pet.RemoveComponent(pet.SearchComponent[DropComponent]())
        pet.Removable = False
        pet.IsFriendly = True
        if pet_item.GetAttribute[bool](CAN_RIDE_ATTRIB_NAME):
            pet.AddComponent(RideComponent(pet, get_ride_height(pet_item)))
            pet.Model.IsMountable = get_pet_growth(pet_item) >= 1.0
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
    mob_name = translate((mob_template.DisplayName or mob_template.Name).lower())
    model_template = ItemModelTemplate()
    model_template.Path = CAGE_MODEL_PATH
    model_template.Scale = CAGE_MODEL_SCALE
    
    template = ItemTemplate()
    template.Name = 'Companion' + type
    template.DisplayName = translate('generic_companion_item_name', mob_name)
    template.Description = translate('generic_companion_item_desc', mob_name)
    template.Tier = tier
    template.EquipmentType = COMPANION_EQUIPMENT_TYPE
    template.Attributes = create_companion_attributes(type, can_ride, mob_template, mob_name)
    template.Model = model_template
    return template

def calculate_ride_height(mob_template):
    scale = mob_template.Model.Scale
    model = VertexData.Load(mob_template.Model.Path, Vector3.One * scale)
    # Find the highest vertex that is nearest the center point
    array = [-v.Y for v in model.Vertices if v.Xz().LengthSquared() < 0.5 * scale * 0.5 * scale]
    sorted_array = sorted(array)
    model.Dispose()
    # Use the average of the K smallest vertices as the point
    k = 4
    avg = Single(0.0)
    for i in range(k):
        avg += -sorted_array[i]
    avg = avg / k / mob_template.Model.Scale
    return avg


def create_companion_attributes(type, can_ride, mob_template, mob_name):
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
    
    price_attribute = AttributeTemplate()
    price_attribute.Value = BASE_PRICE * mob_template.Level
    price_attribute.Hidden = True
    price_attribute.Name = PRICE_ATTRIB_NAME
    
    model_attribute = AttributeTemplate()
    model_attribute.Value = VertexData.Load(mob_template.Model.Path, Vector3.One * Single(mob_template.Model.Scale * .1))
    model_attribute.Hidden = True
    model_attribute.Name = MODEL_ATTRIB_NAME
    
    health_attribute = AttributeTemplate()
    health_attribute.Value = mob_template.MaxHealth
    health_attribute.Persist = True
    health_attribute.Hidden = True
    health_attribute.Name = HEALTH_ATTRIB_NAME

    dead_timer = Timer(COMPANION_RESPAWN_TIME)
    dead_timer.AutoReset = False
    dead_timer.MarkReady()
    
    dead_timer_attribute = AttributeTemplate()
    dead_timer_attribute.Value = dead_timer
    dead_timer_attribute.Hidden = True
    dead_timer_attribute.Name = DEAD_TIMER_ATTRIB_NAME
    
    name_attribute = AttributeTemplate()
    name_attribute.Value = mob_name
    name_attribute.Hidden = True
    name_attribute.Persist = True
    name_attribute.Name = NAME_ATTRIB_NAME

    ride_height_attribute = AttributeTemplate()
    ride_height_attribute.Value = calculate_ride_height(mob_template)
    ride_height_attribute.Hidden = True
    ride_height_attribute.Name = RIDE_HEIGHT_ATTRIB
    
    return Array[AttributeTemplate]([
        pet_attribute,
        ride_attribute,
        growth_attribute,
        xp_attribute,
        price_attribute,
        model_attribute,
        health_attribute,
        dead_timer_attribute,
        name_attribute,
        ride_height_attribute
    ])

def update_ui(pet_item, pet_entity, top_left, top_right, bottom_left, bottom_right, level, name):
    if not pet_item: return
    
    name.Text = pet_item.GetAttribute[str](NAME_ATTRIB_NAME)
    
    if pet_entity and not pet_entity.IsDead:
        top_left.Text = '{0} {1}'.format(
            int(pet_entity.Health),
            translate('health_points')
        )
        top_right.Text = '{0}/{1} {2}'.format(
            int(pet_entity.SearchComponent[CompanionStatsComponent]().XP),
            int(pet_entity.SearchComponent[CompanionStatsComponent]().MaxXP),
            translate('experience_points')
        )
        level.Text = '{0} {1}'.format(
            translate('level').upper(),
            pet_entity.SearchComponent[CompanionStatsComponent]().Level
        )
        bottom_left.Text = '{0} {1}'.format(
            '{:.2f}'.format(pet_entity.AttackDamage),
            translate('attack_damage_label')
        )
        bottom_right.Text = '{0} {1}'.format(
            '{:.2f}'.format(pet_entity.Speed * RideComponent.SpeedMultiplier),
            translate('speed_label')
        )
    else:
        top_left.Text = str()
        top_right.Text = str()
        level.Text = 'RESPAWN IN {0}s'.format(
            '{:.2f}'.format(get_dead_timer(pet_item).AlertTime - get_dead_timer(pet_item).AlertTime * get_dead_timer(pet_item).Progress)
        )
        bottom_left.Text = str()
        bottom_right.Text = str()

for name, _, _ in COMPANION_TYPES:
    assert World.MobFactory.ContainsFactory(name)

templates = create_companion_templates()
for template in templates:
    if not ItemPool.Exists(template.Name):
        ItemPool.Load(template)