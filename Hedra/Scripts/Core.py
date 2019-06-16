from Hedra.Engine.Management import TaskScheduler
from Hedra.Core import Mathf
from Hedra.Engine import Time
from OpenTK import Vector4, Vector3, Vector2

def do_for_seconds(duration, func, on_finished=None):
    vars = {'time': 0}
    def do():
        func(vars['time'])
        vars['time'] += Time.DeltaTime
        if vars['time'] >= duration and on_finished:
            on_finished()
    TaskScheduler.While(lambda: vars['time'] < duration, do)

def after_seconds(seconds, f):
    def nothing(t):
        pass
    do_for_seconds(seconds, nothing, on_finished=f)

def lerp(a, b, t):
    return Mathf.Lerp(a, b, t)