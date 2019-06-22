import Core
from OpenTK import Vector3, Vector4
from Hedra.WeaponSystem import FishingRod
from Hedra.WorldObjects import Projectile, LandType
from Hedra import World
from System import Array
from System import Single
from Hedra.Engine import Time
import math

def on_land(human, state, land_type):
    if land_type == LandType.Water:
        state['on_water'] = True
    else:
        disable_fishing(human, state)

def create_hook(human, hook_model, state):
    hook = Projectile(human, human.Model.LeftWeaponPosition, hook_model)
    hook.Mesh.Scale = Vector3.One * Single(0.5)
    hook.Lifetime = 4
    hook.Propulsion = human.LookingDirection * 2
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
    state['fishing_hook'].Dispose()

def setup_fishing(human, state, hook_model):
    human.IsFishing = True
    state['fishing_position'] = human.Position
    state['fishing_hook'] = create_hook(human, hook_model, state)
    state['line_curvature'] = -1
    state['on_water'] = False
    state['has_fish'] = False

def start_fishing(human, state, hook_model):

    if human.IsFishing:
        disable_fishing(human, state)

    setup_fishing(human, state, hook_model)
        
    def should_stop_fishing():
        return not isinstance(human.LeftWeapon, FishingRod) \
               or (human.Position - state['fishing_position']).LengthFast > 24 \
               or human.IsMoving

    Core.when(should_stop_fishing, lambda: disable_fishing(human, state))

def rod_tip(rod):
    return rod.MainMesh.TransformPoint((rod.MainWeaponSize.Y+4) * Vector3.UnitY)

def curve(p0, p1, p2, t):
    return Single((1.0 - t) ** 2) * p0 + Single(2.0 * (1.0 - t) * t) * p1 + Single(t ** 2.0) * p2

def smooth_curve(start, end, curvature):
    middle = (start + end) * Single(0.5) - Vector3.UnitY * 6 * Single(curvature)
    verts = []
    vertex_count = 7
    for i in xrange(0, vertex_count):
        t = i / float(vertex_count - 1)
        verts.append(curve(start, middle, end, Single(t)))
    return verts

def update_rod(human, rod, rod_line, state):
    rod_line.Enabled = human.IsFishing
    if not human.IsFishing:
        return   
    
    hook = state['fishing_hook']
    state['line_curvature'] = Core.lerp(state['line_curvature'], Core.lerp(1.0, -1.0, min(hook.Delta.LengthFast, 1.0)), Time.DeltaTime * 3.0)
    line_vertices = smooth_curve(rod_tip(rod), hook.Mesh.TransformPoint(Vector3.Zero), state['line_curvature'])
    if state['on_water']:
        hook.Mesh.LocalPosition = Vector3.UnitY * Single(1.0 * math.cos(Time.AccumulatedFrameTime) - 1.5)
        hook.Mesh.LocalRotation = Vector3.Zero

    
    rod.InAttackStance = True
    rod_line.Update(
        Array[Vector3](line_vertices),
        Array[Vector4]([Vector4.One] * len(line_vertices))
    )
    rod_line.Width = 2.0

def retrieve_fish(human, state):
    return state['has_fish']

    