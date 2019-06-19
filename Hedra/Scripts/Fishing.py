import Core
from Hedra.Engine.Rendering import BasicGeometry
from OpenTK import Vector3, Vector4
from Hedra.WeaponSystem import FishingRod
from System import Array

def start_fishing(human):
    human.IsSitting = True
    human.IsFishing = True
        
    def should_stop_fishing():
        return not human.IsSitting or human.IsMoving or not isinstance(human.LeftWeapon, FishingRod)

    def disable():
        human.IsFishing = False
        human.IsSitting = False
        human.LeftWeapon.InAttackStance = False

    Core.when(should_stop_fishing, disable)
    
def update_rod(human, rod, rod_line):
    if human.IsFishing:
        human.IsSitting = not human.IsMoving
        rod.InAttackStance = True
        rod_line.Update(
            Array[Vector3]([human.Model.LeftWeaponPosition, human.Model.LeftWeaponPosition + Vector3.UnitY * 5]),
            Array[Vector4]([Vector4.One, Vector4.One])
        )
        rod_line.Width = 1
    rod_line.Enabled = human.IsFishing
    

def retrieve_fish(human):
    pass

def test():
    set_fishing(player)
    