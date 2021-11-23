from Hedra.Sound import SoundtrackManager

from Core import *

INTERPOLATION_TIME = 2.0
MENU_INTERPOLATION_TIME = 0.75
VILLAGE_AMBIENT = 0
MAIN_THEME = 1
RAIN = 2
GRAVEYARD_CHAMPION = 3
HOSTAGE_SITUATION = 4
ON_THE_LAM = 5
FACING_THE_BEAST = 6
SKELETON_SKIRMISH = 7

ACTION_SONGS = [
    "Sounds/Soundtrack/VillageAmbient.ogg",
    "Sounds/Soundtrack/MainTheme.ogg",
    "Sounds/Soundtrack/Rain.ogg",
    "Sounds/Soundtrack/GraveyardChampion.ogg",
    "Sounds/Soundtrack/HostageSituation.ogg",
    "Sounds/Soundtrack/OnTheLam.ogg",
    "Sounds/Soundtrack/FacingTheBeast.ogg",
    "Sounds/Soundtrack/SkeletonSkirmish.ogg"
]

FOREST = "Sounds/Soundtrack/ForestAmbient.ogg"

BACKGROUND_FAST_SONGS = shuffle([
    "Sounds/Soundtrack/ThroughTheGrasslands.ogg",
    "Sounds/Soundtrack/AdventurersMinuet.ogg",
    "Sounds/Soundtrack/TheVillage.ogg",
    "Sounds/Soundtrack/DawnInTheCity.ogg",
])

BACKGROUND_SLOW_SONGS = shuffle([
    "Sounds/Soundtrack/Obelisk.ogg",
    "Sounds/Soundtrack/CardinalCity.ogg",
    "Sounds/Soundtrack/BreathOfDay.ogg",
    "Sounds/Soundtrack/DeepPlaces.ogg",
])

BACKGROUND_SONGS = [
    BACKGROUND_FAST_SONGS.pop(),
    BACKGROUND_SLOW_SONGS.pop(),
    BACKGROUND_FAST_SONGS.pop(),
    BACKGROUND_SLOW_SONGS.pop(),
    BACKGROUND_FAST_SONGS.pop(),
    BACKGROUND_SLOW_SONGS.pop(),
    BACKGROUND_FAST_SONGS.pop()
]


def get_current_track_name(is_playing_ambient, current_action_track, current_ambient_track):
    return BACKGROUND_SONGS[current_ambient_track] if is_playing_ambient else ACTION_SONGS[current_action_track]


def set_pause_volume(vol):
    SoundtrackManager.PauseVolume = vol


def get_pause_volume():
    return SoundtrackManager.PauseVolume


def set_transition_volume(vol):
    SoundtrackManager.SongTransitionVolume = vol


def get_transition_volume():
    return SoundtrackManager.SongTransitionVolume


def resume_ambient(current_ambient):
    print('Resuming ambient ' + str(current_ambient) + ' ' + BACKGROUND_SONGS[current_ambient])
    play_track(BACKGROUND_SONGS[current_ambient], False)
    return current_ambient


def resume_action(new):
    print('Resuming action ' + str(new) + ' ' + ACTION_SONGS[new])
    play_track(ACTION_SONGS[new], False)
    return new


def do_play_track(name, when=None):
    print('Playing track ' + name)

    def do():
        SoundtrackManager.PlayTrack(name)
        if when: when()

    run_parallel(do)


def play_track(name, is_repeating):
    if is_repeating:
        do_play_track(name)
    else:
        interpolate_volume(0.0, get_vol=get_transition_volume, set_vol=set_transition_volume)

        def after_wait():
            def turn_up_volume():
                interpolate_volume(1.0, get_vol=get_transition_volume, set_vol=set_transition_volume)

            do_play_track(name, when=turn_up_volume)

        after_seconds(INTERPOLATION_TIME, after_wait, scale_time=False)


def on_song_end(is_playing_ambient, current_action_track, current_ambient_track):
    print('Song ended!')
    if not is_playing_ambient:
        play_track(ACTION_SONGS[current_action_track], True)
        return current_action_track

    return resume_ambient(wrap(current_ambient_track + 1, BACKGROUND_SONGS))


def interpolate_volume(target, get_vol, set_vol, time=INTERPOLATION_TIME, should_stop=None):
    vars = {'start': get_vol(), 'stop': False}

    def change_vol(t):
        if vars['stop']: return
        if should_stop and should_stop():
            vars['stop'] = True
        new_vol = lerp(vars['start'], target, t / time)
        set_vol(new_vol)

    do_for_seconds(time, change_vol, scale_time=False)


def on_pause(should_stop):
    interpolate_volume(0.0, get_vol=get_pause_volume, set_vol=set_pause_volume, time=MENU_INTERPOLATION_TIME,
                       should_stop=should_stop)


def on_resume(should_stop):
    interpolate_volume(1.0, get_vol=get_pause_volume, set_vol=set_pause_volume, time=MENU_INTERPOLATION_TIME * 8,
                       should_stop=should_stop)


def soundtrack_setup():
    vars = {'is_paused': None}

    def update():
        new = is_paused() and (not is_start_menu() or is_loading())
        if vars['is_paused'] == new:
            return
        if new:
            on_pause(lambda: not vars['is_paused'])
        else:
            on_resume(lambda: vars['is_paused'])
        vars['is_paused'] = new

    when_game_ready(lambda: do_every(0.25, update, scale_time=False))
