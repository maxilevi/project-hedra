from Hedra import Utils
from Hedra.Core import TaskScheduler, Time
from Hedra.Engine.Localization import Translation
from Hedra.Game import GameManager, GameSettings
from Hedra.Localization import Translations
from Hedra.Numerics import Mathf


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


def rand_float():
    return float(Utils.Rng.NextDouble())


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


def translate(msg, *args):
    return Translations.Get(msg, *args)


def load_translation(msg, *args):
    return Translation.Create(msg, *args)


def get_player():
    return GameManager.Player
