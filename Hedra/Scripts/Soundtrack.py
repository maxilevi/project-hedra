from Core import *
from Hedra.Sound import SoundtrackManager

INTERPOLATION_TIME = 0.75
WAIT_TIME = 3.0

VILLAGE_AMBIENT = 0
MAIN_THEME = 1
RAIN = 2
GRAVEYARD_CHAMPION = 3
HOSTAGE_SITUATION = 4
ON_THE_LAM = 5

ACTION_SONGS = [
    "Sounds/Soundtrack/VillageAmbient.ogg",
    "Sounds/Soundtrack/MainTheme.ogg",
    "Sounds/Soundtrack/Rain.ogg",
    "Sounds/Soundtrack/GraveyardChampion.ogg",
    "Sounds/Soundtrack/HostageSituation.ogg",
    "Sounds/Soundtrack/OnTheLam.ogg"
]

FOREST = "Sounds/Soundtrack/ForestAmbient.ogg"
BACKGROUND_SONGS = shuffle([#[FOREST] +
    "Sounds/Soundtrack/CardinalCity.ogg",
    "Sounds/Soundtrack/ThroughTheGrasslands.ogg",
    "Sounds/Soundtrack/BreathOfDay.ogg",
    "Sounds/Soundtrack/TheVillage.ogg",
    "Sounds/Soundtrack/AdventurersMinuet.ogg",
    "Sounds/Soundtrack/Obelisk.ogg"
])

def set_pause_volume(vol):
    SoundtrackManager.PauseVolume = vol

def get_pause_volume():
    return SoundtrackManager.PauseVolume
    
def set_transition_volume(vol):
    SoundtrackManager.SongTransitionVolume = vol

def get_transition_volume():
    return SoundtrackManager.SongTransitionVolume

def resume_ambient(current_ambient):
    play_track(BACKGROUND_SONGS[current_ambient], False)
    return current_ambient

def resume_action(new):
    play_track(ACTION_SONGS[new], False)
    return new

def do_play_track(name):
    run_parallel(lambda: SoundtrackManager.PlayTrack(name))

def play_track(name, is_repeating):
    if is_repeating:
        do_play_track(name)
    else:
        quarter = WAIT_TIME * .25
        interpolate_volume(0.0, get_vol=get_transition_volume, set_vol=set_transition_volume, time=quarter)
        def after_wait():
            do_play_track(name)
            interpolate_volume(1.0, get_vol=get_transition_volume, set_vol=set_transition_volume, time=quarter)
        
        total_wait_time = quarter
        if name != ACTION_SONGS[MAIN_THEME]:
            total_wait_time += WAIT_TIME
        after_seconds(total_wait_time, after_wait, scale_time=False)

def on_song_end(is_playing_ambient, current_action_track, current_ambient_track):
    if not is_playing_ambient:
        play_track(ACTION_SONGS[current_action_track], True)
    else:
        resume_ambient(wrap(current_ambient_track + 1, BACKGROUND_SONGS))

def interpolate_volume(target, get_vol, set_vol, time = INTERPOLATION_TIME):
    vars = {'vol': get_vol()}
    def change_vol(t):
        vars['vol'] = lerp(vars['vol'], target, t)
        set_vol(vars['vol'])
    do_for_seconds(time, change_vol, scale_time=False)
        
def on_pause():
    interpolate_volume(0.0, get_vol=get_pause_volume, set_vol=set_pause_volume)
    
def on_resume():
    interpolate_volume(1.0, get_vol=get_pause_volume, set_vol=set_pause_volume)

def soundtrack_setup():
    vars = {'state': None}
    def update():
        new = is_paused() and (not is_start_menu() or is_loading())
        if vars['state'] == new:
            return
        if new:
            on_pause()
        else:
            on_resume()
        vars['state'] = new
    when_game_ready(lambda: do_every(0.25, update, scale_time=False))