import Core
import math
import VisualEffects
from OpenTK import Vector3, Vector4
from Hedra.Core import Time, Timer
from Hedra.WeaponSystem import FishingRod
from Hedra.WorldObjects import Projectile, LandType
from Hedra.Engine.Rendering import Colors
from Hedra.Rendering.Particles import ParticleShape
from Hedra import World
from System import Array
from System import Single

FISHING_DISTANCE = 24
FISHING_CHANCE = 300
FISHING_STRING_DETAIL = 7
FISHING_CATCH_TIME = 1.5
FISHING_ROD_COOLDOWN = 2
FISHING_HOOK_SCALE = 0.5
FISHING_HOOK_SPEED = 2
FISHING_HOOK_LIFETIME = 4


def configure_rod(rod):
    rod.PrimaryAttackHasCooldown = True
    rod.PrimaryAttackCooldown = FISHING_ROD_COOLDOWN

def on_land(human, state, land_type):
    if land_type == LandType.Water:
        state['on_water'] = True
    else:
        disable_fishing(human, state)

def create_hook(human, hook_model, state):
    hook = Projectile(human, human.Model.LeftWeaponPosition, hook_model)
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

def disable_fishing(human, state):
    human.IsFishing = False
    human.IsSitting = False
    human.LeftWeapon.InAttackStance = False
    human.Model.Reset()
    VisualEffects.remove_shiver_effect(human)
    state['fishing_hook'].Dispose()

def setup_fishing(human, state, hook_model):
    human.IsFishing = True
    state['fishing_position'] = human.Position
    state['fishing_hook'] = create_hook(human, hook_model, state)
    state['line_curvature'] = -1
    state['on_water'] = False
    state['has_fish'] = False

def start_fishing(human, state, hook_model):

    setup_fishing(human, state, hook_model)
        
    def should_stop_fishing():
        return not isinstance(human.LeftWeapon, FishingRod) \
               or (human.Position - state['fishing_position']).LengthFast > FISHING_DISTANCE \
               or human.IsMoving

    Core.when(should_stop_fishing, lambda: disable_fishing(human, state))

def rod_tip(rod):
    return rod.MainMesh.TransformPoint((rod.MainWeaponSize.Y+4) * Vector3.UnitY)

def curve(p0, p1, p2, t):
    return Single((1.0 - t) ** 2) * p0 + Single(2.0 * (1.0 - t) * t) * p1 + Single(t ** 2.0) * p2

def smooth_curve(start, end, curvature):
    middle = (start + end) * Single(0.5) - Vector3.UnitY * 6 * Single(curvature)
    vertices = []
    vertex_count = FISHING_STRING_DETAIL
    for i in xrange(0, vertex_count):
        t = i / float(vertex_count - 1)
        vertices.append(curve(start, middle, end, Single(t)))
    return vertices

def get_water_color(position):
    under_chunk = World.GetChunkAt(position)
    if under_chunk:
        return under_chunk.Biome.Colors.WaterColor
    return Colors.DeepSkyBlue

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

    state['line_curvature'] = Core.lerp(state['line_curvature'], target_curvature, Time.DeltaTime * interpolation_speed)
    return smooth_curve(rod_tip(rod), hook.Mesh.TransformPoint(Vector3.Zero), state['line_curvature'])

def check_for_fish(human, state):
    if not state['has_fish'] and Core.rand(0, FISHING_CHANCE) == 1:
        VisualEffects.add_shiver_effect(human, 0.5)
        state['has_fish'] = True
        state['fish_timer'] = Timer(3.0ffa)
    
        
def update_rod(human, rod, rod_line, state):

    rod_line.Enabled = human.IsFishing
    
    if human.IsFishing:
        hook = state['fishing_hook']
        
        if state['on_water']:
            hook.Mesh.LocalPosition = calculate_hook_offset(state['has_fish'])
            hook.Mesh.LocalRotation = Vector3.Zero

        if state['has_fish']:
            has_fish_effect(hook.Position)
            
        line_vertices = calculate_line(rod, hook, state)
        rod.InAttackStance = True
        rod.SecondaryAttackEnabled = state['has_fish']
        rod.MainMesh.LocalRotation = get_rod_rotation(state['has_fish'])
        rod_line.Update(
            Array[Vector3](line_vertices),
            Array[Vector4]([Vector4.One] * len(line_vertices))
        )
        rod_line.Width = 2.0
        
        check_for_fish(human, state)

def retrieve_fish(human, state):
    return state['has_fish']

    