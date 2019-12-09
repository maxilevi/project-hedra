import clr
from Core import *
from System.Numerics import Vector4
from Hedra.Numerics import VectorExtensions

clr.ImportExtensions(VectorExtensions)

def set_outline(human, color, state):
    human.Model.Outline = state
    if color:
        human.Model.OutlineColor = color

def outline(human, color, seconds):
    quarter = seconds * .25

    start_color = Vector4()
    vars = {'current': start_color}

    def in_transition(t):
        vars['current'] = lerp(vars['current'], color, t)
        set_outline(human, vars['current'], True)

    def out_transition(t):
        vars['current'] = lerp(vars['current'], start_color, t)
        set_outline(human, vars['current'], True)

    do_for_seconds(quarter, in_transition)
    def finalize():
        do_for_seconds(quarter, out_transition, on_finished=lambda: set_outline(human, Vector4(), False))

    after_seconds(seconds, finalize)

def outline_while(human, color, condition):
    set_outline(human, color, True)
    when(lambda: not condition(), lambda: set_outline(human, color, False))
    
def add_shiver_effect(human, intensity):
    human.LeftWeapon.Charging = True
    human.LeftWeapon.ChargingIntensity = intensity

def remove_shiver_effect(human):
    human.LeftWeapon.Charging = False
    human.LeftWeapon.ChargingIntensity = 0
