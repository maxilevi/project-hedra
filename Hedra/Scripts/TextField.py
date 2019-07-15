from OpenTK.Input import Key
from OpenTK import Vector4, Vector2
from Hedra.Core import Timer
from Hedra.Rendering import Graphics2D
from System import Single

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
    
def on_click(event_args, state):
    if is_enabled(state):
        state['in_focus'] = True

def update_caret(state):

    caret = state['ui_caret']
    bar = state['ui_bar']
    focused = state['in_focus']
    state['ui_bar'].Text = state['text']
    
    if is_enabled(state):
        if not focused and caret.Enabled:
            caret.Disable()
        elif focused and not caret.Enabled:
            caret.Enable()
        
            
    if caret.Enabled:
        if state['caret_timer'].Tick():
            if caret.Text == CARET_SYMBOL:
                caret.Text = str()
            else:
                caret.Text = CARET_SYMBOL
        caret.Position = bar.Position + state['caret_offset']
    
    if focused:
        bar.BackgroundColor = FOCUS_COLOR
    else:
        bar.BackgroundColor = DEFOCUS_COLOR

def on_key_press(event_args, state):
    if is_enabled(state) and state['in_focus']:
        add_character(state, event_args.KeyChar)

def is_enabled(state):
    return state['ui_bar'].Enabled

def on_key_down(event_args, state):
    cancel = False
    if is_enabled(state) and state['in_focus']:
        if event_args.Key == Key.BackSpace or event_args.Key == Key.BackSlash or event_args.Key == Key.NonUSBackSlash:
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
    

def delete_character(state):
    if len(state['text']) > 0:
        i = state['caret_index']
        state['text'] = str(state['text'][0:i-1]) + str(state['text'][i:])
        move_caret(state, -1)

def add_character(state, char):
    i = state['caret_index']
    state['text'] = str(state['text'][0:i]) + char + str(state['text'][i:])
    move_caret(state, 1)

def move_caret(state, dir):
    index = (state['caret_index'] + dir)
    
    if index < 0:
        index = 0
        
    if index > len(state['text']):
        index = len(state['text'])

    state['caret_offset'] = (Graphics2D.MeasureString(str(state['text'][0:index]), state['ui_bar'].TextFont).X * Single(1.0)) * Vector2.UnitX
    state['caret_index'] = index

def focus(state):
    state['in_focus'] = True
    state['caret_index'] = len(state['text'])
    
def defocus(state):
    state['in_focus'] = False

def in_focus(state):
    return state['in_focus']