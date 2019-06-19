from Core import *
from OpenTK import Vector4

def outline(human, color, seconds):
    quarter = seconds * .25
    def set(state, color = None):
        human.Model.Outline = state
        if color:
            human.Model.OutlineColor = color
            
    start_color = Vector4()
    vars = {'current': start_color}
    
    def in_transition(t):
        vars['current'] = lerp(vars['current'], color, t)
        set(True, vars['current'])
    
    def out_transition(t):
        vars['current'] = lerp(vars['current'], start_color, t)
        set(True, vars['current'])

    do_for_seconds(quarter, in_transition)
    def finalize():
        do_for_seconds(quarter, out_transition, on_finished=lambda: set(False))

    after_seconds(seconds, finalize)
    



 
def test():
    outline(player, Vector4(1,0,0,1), 4.0)