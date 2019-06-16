from Hedra.Engine.Management import TaskScheduler
from Hedra.Core import Mathf
from Hedra.Engine import Time
from Hedra import Utils
from Hedra.Engine.Game import GameManager, GameSettings

def do_for_seconds(duration, func, on_finished=None, scale_time=True):
    vars = {'time': 0}
    def do():
        func(vars['time'])
        vars['time'] += Time.DeltaTime if scale_time else Time.IndependentDeltaTime
        if vars['time'] >= duration and on_finished:
            on_finished()
    TaskScheduler.While(lambda: vars['time'] < duration, do)

def after_seconds(seconds, f, scale_time=True):
    def nothing(t):
        pass
    do_for_seconds(seconds, nothing, on_finished=f, scale_time=scale_time)

def do_every(interval, func, should_cancel=None, scale_time=True):
    def do():
        func()
        if not should_cancel or not should_cancel():
            after_seconds(interval, do, scale_time=scale_time)
    after_seconds(interval, do, scale_time=scale_time)

def when(condition, func):
    TaskScheduler.When(condition, func)
    
def when_game_ready(func):
    when(lambda: GameManager.PlayerExists, func)

def run_parallel(func):
    TaskScheduler.Parallel(func)

def lerp(a, b, t):
    return Mathf.Lerp(a, b, t)

def rand(start, end):
    return Utils.Rng.Next(start, end)

def shuffle(songs):
    for i in reversed(xrange(0, len(songs))):
        j = rand(0, i)
        songs[i], songs[j] = songs[j], songs[i]
    return songs

def wrap(index, arr):
    return index % len(arr)

def is_loading():
    return GameManager.IsLoading

def is_paused():
    return GameSettings.Paused
    
def is_start_menu():
    return GameManager.InStartMenu