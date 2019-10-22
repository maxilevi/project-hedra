import clr
from Core import get_player
from Silk.NET.Input.Common import Key
from System.Numerics import Vector4, Vector2
from Hedra.Core import Timer
from Hedra.Rendering import Graphics2D
from System import Single
from Hedra.Numerics import VectorExtensions

clr.ImportExtensions(VectorExtensions)

FOCUS_COLOR = Vector4(0.08, 0.08, 0.08, 1)
DEFOCUS_COLOR = Vector4(0.152, 0.152, 0.152, 1)
CARET_SYMBOL = '|'

def init(state, ui_bar, ui_caret, ui_focus_button):
    ui_focus_button.Click += lambda _, args: on_click(args, state)
    state['in_focus'] = False
    state['ui_bar'] = ui_bar
    state['ui_caret'] = ui_caret
    state['ui_focus_button'] = ui_focus_button
    state['text'] = str()
    state['caret_index'] = 0
    state['caret_timer'] = Timer(0.5)
    state['caret_timer'].UseTimeScale = False
    state['caret_offset'] = Vector2()
    state['last_size'] = 0
    
def on_click(event_args, state):
    if is_enabled(state):
        focus(state)

def update_caret(state):

    caret = state['ui_caret']
    bar = state['ui_bar']
    focused = state['in_focus']
    
    if is_enabled(state):
        if not focused and caret.Enabled:
            caret.Disable()
        elif focused and not caret.Enabled:
            caret.Enable()
        state['ui_bar'].Text = state['text']
        
            
    if caret.Enabled:
        if state['caret_timer'].Tick():
            if caret.Text == CARET_SYMBOL:
                caret.Text = str()
            else:
                caret.Text = CARET_SYMBOL
        caret.Position = bar.Position + state['caret_offset']
    
    if focused:
        bar.BackgroundColor = FOCUS_COLOR
        get_player().CanInteract = False
    else:
        bar.BackgroundColor = DEFOCUS_COLOR

def has_space_left(state):
    return state['last_size'] < state['ui_bar'].Scale.X * .5

def is_enabled(state):
    return state['ui_bar'].Enabled

def on_key_down(event_args, state):
    cancel = False
    if is_enabled(state) and state['in_focus']:
        if event_args.Key == Key.Backspace or event_args.Key == Key.BackSlash:
            delete_character(state)
            cancel = True
        elif event_args.Key == Key.Left:
            move_caret(state, -1)
            cancel = True
        elif event_args.Key == Key.Right:
            move_caret(state, 1)
            cancel = True
        elif event_args.Key == Key.Escape:
            defocus(state)
        elif event_args.Key == Key.Enter:
            defocus(state)
    if cancel:
        event_args.Cancel()
    
def on_char_written(state, char):
    if is_enabled(state) and state['in_focus'] and has_space_left(state):
        add_character(state, char)


def delete_character(state):
    if not state['text']: return
    
    i = state['caret_index']
    if i > 0:
        state['text'] = str(state['text'][0:i-1]) + str(state['text'][i:])
        move_caret(state, -1)
        update_size(state)

def add_character(state, char):
    i = state['caret_index']
    state['text'] = str(state['text'][0:i]) + char + str(state['text'][i:])
    move_caret(state, 1)
    update_size(state)

def move_caret(state, dir):
    index = (state['caret_index'] + dir)
    
    if index < 0:
        index = 0
        
    if index > len(state['text']):
        index = len(state['text'])

    set_caret_position(state, index)

def set_caret_position(state, index):
    state['caret_offset'] = (Graphics2D.MeasureString(str(state['text'][0:index]), state['ui_bar'].TextFont).X * Single(2.0) - state['ui_bar'].Scale.X * Single(0.5)) * Vector2.UnitX
    state['caret_index'] = index

def update_size(state):
    state['last_size'] = Graphics2D.MeasureString(state['text'], state['ui_bar'].TextFont).X

def focus(state):
    state['in_focus'] = True
    set_caret_position(state, len(state['text']))
    state['previous_can_interact'] = get_player().CanInteract
    
    
def defocus(state):
    if in_focus(state):
        state['in_focus'] = False
        get_player().CanInteract = state['previous_can_interact']

def in_focus(state):
    return state['in_focus']

def set_text(text, state):
    state['text'] = text
    update_size(state)
    set_caret_position(state, len(text))
    