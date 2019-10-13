import Core
import math
import VisualEffects
import clr
from OpenToolkit.Mathematics import Vector3, Vector4, Matrix4
from System import Array, Single, ArgumentOutOfRangeException
from Hedra import World
from Hedra.Core import Time, Timer
from Hedra.WeaponSystem import FishingRod
from Hedra.WorldObjects import Projectile, LandType
from Hedra.Rendering import Colors, ObjectMesh
from Hedra.Rendering.Particles import ParticleShape
from Hedra.Items import ItemType, ItemPool, InventoryExtensions
from Hedra.EntitySystem import EntityExtensions

clr.ImportExtensions(EntityExtensions)
clr.ImportExtensions(InventoryExtensions)

FISHING_DISTANCE = 24
FISHING_CHANCE = 2600
FISHING_STRING_DETAIL = 7
FISHING_CATCH_TIME = 2.5
FISHING_ROD_COOLDOWN = 2
FISHING_HOOK_SCALE = 0.5
FISHING_HOOK_SPEED = 1
FISHING_HOOK_LIFETIME = 4
REWARD_CHANCE = .66
ROD_LINE_WIDTH = 2
PULL_SPEED = 1.0

BAIT_ITEM_NAME = 'Bait'
RAW_TROUT_NAME = 'RawTrout'
RAW_SALMON_NAME = 'RawSalmon'
RAW_FISH_NAME = 'RawFish'
HEALTH_POTION_NAME = 'HealthPotion'
STRENGTH_POTION_NAME = 'StrengthPotion'
SPEED_POTION_NAME = 'SpeedPotion'
DEXTERITY_POTION_NAME = 'StaminaPotion'

FISHING_REWARDS = [
    (STRENGTH_POTION_NAME, 0.025),
    (SPEED_POTION_NAME, 0.05),
    (DEXTERITY_POTION_NAME, 0.075),
    (HEALTH_POTION_NAME, 0.125),
    (RAW_SALMON_NAME, 0.20),
    (RAW_TROUT_NAME, 0.30),
    (RAW_FISH_NAME, 1.0)
]

def assert_constants():
    for name, val in FISHING_REWARDS + [(BAIT_ITEM_NAME, 0.0)]:
        assert ItemPool.Exists(name)

def get_fished_item(position):
    if Core.rand_float() < REWARD_CHANCE:    
        rewards = sorted(FISHING_REWARDS + get_fishing_zone_rewards(position), key=lambda x: x[1])
        roll = Core.rand_float()
        for fish_name, chance in rewards:
            if roll < chance:
                return ItemPool.Grab(fish_name)
    else:
        return None
    raise ArgumentOutOfRangeException('Failed to find a suitable fish')

def get_fishing_zone_rewards(position):
    zones = filter(lambda x: x.Affects(position), World.FishingZoneHandler.Zones)
    return map(lambda x: (x.FishingReward.Name, x.Chance), zones)

def get_bait(human):
    return human.Inventory.Search(lambda item: item.Name == BAIT_ITEM_NAME)

def has_bait(human):
    return get_bait(human) is not None

def show_no_bait_msg(human):
    human.MessageDispatcher.ShowNotification(Core.translate("fishing_no_bait"), Colors.ToColorStruct(Colors.Red), FISHING_ROD_COOLDOWN)

def show_used_bait_msg(human):
    human.ShowText(human.Position, Core.translate("fishing_used_bait"), Colors.ToColorStruct(Colors.Sienna), 14)

def consume_bait(human):
    human.Inventory.RemoveItem(get_bait(human), 1)
    show_used_bait_msg(human)

def check_can_fish(human):
    if not has_bait(human):
        show_no_bait_msg(human)
        return False
    human.Movement.CaptureMovement = False
    return True

def configure_rod(rod):
    rod.PrimaryAttackHasCooldown = True
    rod.PrimaryAttackCooldown = FISHING_ROD_COOLDOWN
    rod.SecondaryAttackEnabled = False

def on_land(human, state, land_type):
    if land_type == LandType.Water:
        state['on_water'] = True
    else:
        disable_fishing(human, state)

def disable_fishing(human, state):
    human.IsFishing = False
    human.IsSitting = False
    human.LeftWeapon.InAttackStance = False
    human.Model.Reset()
    VisualEffects.remove_shiver_effect(human)
    state['is_retrieving'] = False
    state['pull_back'] = False
    dispose(state)

def dispose(state):
    state['fishing_hook'].Dispose()
    if 'fish_model' in state:
        state['fish_model'].Dispose()

def setup_fishing(human, state, hook_model):
    human.IsFishing = True
    human.LeftWeapon.SecondaryAttackEnabled = True
    human.Movement.CaptureMovement = True
    state['fish'] = None
    state['fishing_position'] = human.Position
    state['fishing_hook'] = create_hook(human, hook_model, state)
    state['line_curvature'] = -1
    state['on_water'] = False
    state['has_fish'] = False
    state['is_retrieving'] = False
    state['pull_back'] = False
    state['fish_timer'] = Timer(FISHING_CATCH_TIME)

def start_fishing(human, state, hook_model):

    setup_fishing(human, state, hook_model)
        
    def should_stop_fishing():
        return (not isinstance(human.LeftWeapon, FishingRod) \
               or (human.Position - state['fishing_position']).LengthFast > FISHING_DISTANCE \
               or human.IsMoving) and not state['is_retrieving']

    Core.when(should_stop_fishing, lambda: disable_fishing(human, state))

def get_water_color(position):
    under_chunk = World.GetChunkAt(position)
    if under_chunk:
        return under_chunk.Biome.Colors.WaterColor
    return Colors.DeepSkyBlue

def get_rod_rotation(has_fish):
    rot = Vector3(110, -45, 0)
    if has_fish:
        rot = Vector3(110, -25, -35 * math.cos(Time.AccumulatedFrameTime))
    return rot

def calculate_hook_offset(has_fish):
    movement_factor = 1.0
    default_offset = Vector3.UnitY * Single(-1.5)
    if has_fish:
        movement_factor = 0.0
        default_offset = Vector3.UnitY * Single(-2.0)
    offset_x = Vector3.UnitX * Single(0.15 * math.cos(Time.AccumulatedFrameTime * 35) * (1.0 - movement_factor))
    offset_y = Vector3.UnitY * Single(1.0 * math.cos(Time.AccumulatedFrameTime) * movement_factor)
    offset_z = Vector3.UnitZ * Single(0.15 * math.sin(Time.AccumulatedFrameTime * 35) * (1.0 - movement_factor))
    return offset_x + offset_y + offset_z + default_offset

def calculate_line(rod, hook, state):
    interpolation_speed = 3.0
    target_curvature = Core.lerp(1.0, -1.0, min(hook.Delta.LengthFast, 1.0))
    if state['has_fish']:
        target_curvature = 0.0
        interpolation_speed = 5.0
    if state['pull_back']:
        target_curvature = -1.0
        interpolation_speed = 5.0
    state['line_curvature'] = Core.lerp(state['line_curvature'], target_curvature, Time.DeltaTime * interpolation_speed)
    return smooth_curve(rod_tip(rod), hook.Mesh.TransformPoint(Vector3.Zero), state['line_curvature'])

def check_for_fish(human, state):

    timer = state['fish_timer']
    if not state['has_fish'] and not Core.is_paused() and Core.rand(0, FISHING_CHANCE) == 1:
        VisualEffects.add_shiver_effect(human, 0.5)
        state['has_fish'] = True
        consume_bait(human)
        timer.AlertTime = FISHING_CATCH_TIME
        timer.Reset()

    if state['has_fish'] and timer.Tick():
        VisualEffects.remove_shiver_effect(human)
        state['has_fish'] = False

def end_pull(human, state):
    disable_fishing(human, state)
    if state['fish']:
        human.AddOrDropItem(state['fish'])
        human.ShowText(human.Position, "+ 1 " + state['fish'].DisplayName.upper(), Colors.ToColorStruct(Colors.Gray), 14)

def rod_position(rod, offset):
    return rod.MainMesh.TransformPoint(offset)

def rod_tip(rod, offset=Vector3.Zero):
    return rod.MainMesh.TransformPoint((rod.MainWeaponSize.Y+4) * Vector3.UnitY + offset)

def curve(p0, p1, p2, t):
    return Single((1.0 - t) ** 2) * p0 + Single(2.0 * (1.0 - t) * t) * p1 + Single(t ** 2.0) * p2

def smooth_curve(start, end, curvature, orientation=Vector3.UnitY):
    middle = (start + end) * Single(0.5) - orientation * 6 * Single(curvature)
    vertices = []
    vertex_count = FISHING_STRING_DETAIL
    for i in xrange(0, vertex_count):
        t = i / float(vertex_count - 1)
        vertices.append(curve(start, middle, end, Single(t)))
    return vertices

def update_rod(human, rod, rod_line, state):

    is_retrieving = ('is_retrieving' in state and state['is_retrieving'])
    is_active = human.IsFishing or is_retrieving
    rod_line.Enabled = True
    
    if rod_line.Enabled:
        if is_active:
            line_vertices = on_rod_active(human, rod, state)
        else:
            line_vertices = on_rod_idle(rod)
    
        rod_line.Update(
            Array[Vector3](line_vertices),
            Array[Vector4]([Vector4.One] * len(line_vertices))
        )
        rod_line.Width = ROD_LINE_WIDTH

def on_rod_idle(rod):
    rot_mat = Matrix4.CreateFromQuaternion(rod.MainMesh.TransformationMatrix.ExtractRotation())
    rod_tip_offset = Vector3.UnitZ * Single(0.125) + Vector3.UnitX * Single(-0.125)
    rod_mid_offset = Single(-0.25) * Vector3.UnitX + Vector3.UnitY * 2
    curvature = -0.05
    offset = Vector3.TransformPosition(Vector3.UnitX if rod.InAttackStance else Vector3.UnitZ, rot_mat)
    return smooth_curve(rod_tip(rod, rod_tip_offset), rod_position(rod, rod_mid_offset), curvature, offset)

def on_rod_active(human, rod, state):
    hook = state['fishing_hook']
    human.LeftWeapon.InAttackStance = True

    if state['on_water']:
        hook.Mesh.LocalPosition = Core.lerp(hook.Mesh.LocalPosition, calculate_hook_offset(state['has_fish']), Time.DeltaTime * 2.0)
        hook.Mesh.LocalRotation = Vector3.Zero

    if state['has_fish']:
        has_fish_effect(hook.Position)
        
    check_for_fish(human, state)
    if state['pull_back']:
        update_pull(human, state, hook)
    return calculate_line(rod, hook, state)

def update_pull(human, state, hook):
    update_pull_hook(human, state, hook)
    update_fish_model(hook, state)
    if state['pull_time'] >= 1.0:
        end_pull(human, state)

def update_pull_hook(human, state, hook):
    state['pull_time'] += Time.DeltaTime * PULL_SPEED
    start, end = state['pull_position'], human.Model.LeftWeaponPosition
    middle = (start + end) * Single(0.5) + Vector3.UnitY * 36
    hook.Position = curve(start, middle, end, state['pull_time'])

def update_fish_model(hook, state):
    if 'fish_model' in state:
        hook.Mesh.Enabled = False
        fish_model = state['fish_model']
        fish_model.Position = hook.Mesh.TransformPoint(Vector3.Zero)

def start_retrieval(human, state):
    human.IsFishing = False
    human.IsSitting = False
    human.LeftWeapon.InAttackStance = False
    state['is_retrieving'] = True

def retrieve_fish(human, state):
    position = state['fishing_hook'].Position
    state['pull_time'] = 0
    state['pull_back'] = True
    state['pull_position'] = position
    if state['has_fish']:
        state['fish'] = get_fished_item(position)
        if state['fish']:
            state['fish_model'] = ObjectMesh.FromVertexData(state['fish'].Model.Clone().Scale(Vector3.One * 2))

def has_fish_effect(position):
    World.Particles.VariateUniformly = True
    World.Particles.Color = Vector4(get_water_color(position).Xyz, .5)
    World.Particles.Position = position - Vector3.UnitY * 2
    World.Particles.Scale = Vector3.One * Single(.5)
    World.Particles.ScaleErrorMargin = Vector3(.25, .25, .25)
    World.Particles.Direction = Vector3.UnitY * Single(.25)
    World.Particles.ParticleLifetime = 1
    World.Particles.GravityEffect = .05
    World.Particles.PositionErrorMargin = Vector3(5, 1, 5)
    World.Particles.Shape = ParticleShape.Sphere
    World.Particles.Emit()

def create_hook(human, hook_model, state):
    hook = Projectile(human, human.Model.LeftWeaponPosition + Vector3.UnitY * Single(2.5), hook_model)
    hook.Mesh.Scale = Vector3.One * Single(FISHING_HOOK_SCALE)
    hook.Lifetime = FISHING_HOOK_LIFETIME
    hook.Propulsion = human.LookingDirection * FISHING_HOOK_SPEED
    hook.LandEventHandler += lambda _, land_type: on_land(human, state, land_type)
    hook.PlaySound = False
    hook.ShowParticlesOnDestroy = False
    hook.CollideWithWater = True
    hook.ManuallyDispose = True
    World.AddWorldObject(hook)
    return hook

assert_constants()