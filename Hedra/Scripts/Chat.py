import Core
import TextDisplay
from OpenTK import Vector2
from OpenTK.Input import Key
from Hedra.Core import Time
from Hedra.User import WordFilter, CommandManager
from Hedra.Game import GameSettings
from Hedra.Input import Cursor
from Hedra.Sound import SoundPlayer, SoundType
from Hedra.Rendering import Colors
from Hedra.Rendering.UI import Panel, GUIText, TextField, FontCache

TEXT_BOX_POSITION = Vector2(-0.95, -.65)
COMMAND_LINE_POSITION = Vector2(-0.825, -0.725)
COMMAND_LINE_SIZE = Vector2(.225, .02)
MAX_HISTORY = 25
MAX_LINES = 10
MAX_FADE_TIME = 4.0
FADE_SPEED = 1.0

def init(user, state):
    panel = Panel()
    
    command_line = TextField(Vector2.Zero, COMMAND_LINE_SIZE, panel, CurvedBorders=False)
    text_box = GUIText(str(), Vector2.Zero, Colors.ToColorStruct(Colors.White), FontCache.GetNormal(10))
    
    panel.AddElement(command_line)
    panel.AddElement(text_box)
    
    state['user'] = user
    state['ui_panel'] = panel
    state['ui_command_line'] = command_line
    state['ui_text_box'] = text_box
    state['command_history'] = []
    state['command_history_index'] = 0
    state['text'] = []
    state['open'] = False
    state['fading'] = False

def update(state):
    command_line = state['ui_command_line']
    text_box = state['ui_text_box']

    text_box.Position = TEXT_BOX_POSITION + text_box.Scale.X * Vector2.UnitX + text_box.Scale.Y * Vector2.UnitY
    command_line.Position = COMMAND_LINE_POSITION
    if state['open'] and not text_box.Enabled:
        text_box.Enable()
        text_box.Opacity = 1.0
    elif not state['open'] and not state['fading'] and text_box.Enabled:
        fade(state)
        
    if state['fading']:
        update_fade(state)
        
    if not should_show(state['user']):
        disable(state)
    
def fade(state):
    state['fading'] = True
    state['fade_time'] = MAX_FADE_TIME
    
def update_fade(state):
    if state['open']:
        state['fading'] = False

    text_box = state['ui_text_box']        
    if state['fade_time'] <= 0.0:
        state['fading'] = False
        text_box.Disable()

    if state['fading']:
        text_box.Opacity = (state['fade_time'] / MAX_FADE_TIME) ** 0.5
        state['fade_time'] = state['fade_time'] - FADE_SPEED * Time.DeltaTime
    

def on_key_down(event_args, state):
    success = False
    if event_args.Key == Key.Enter:
        if not state['open'] and can_open(state['user']):
            open(state)
            success = True
        elif state['open']:
            push_text(state)
            success = True
  
    elif event_args.Key == Key.Escape and state['open']:
        close(state)
        success = True
    
    elif event_args.Key == Key.Up and state['open'] and state['command_history']:
        cycle_history(state)
        success = True
        
    if success:
        SoundPlayer.PlaySound(SoundType.NotificationSound, state['user'].Position)

def cycle_history(state):
    command_line = state['ui_command_line']
    command_line.Text = state['command_history'][state['command_history_index']]
    state['command_history_index'] = (state['command_history_index'] + 1) % len(state['command_history'])

def push_text(state):
    command_line = state['ui_command_line']
    command_text = command_line.Text
    if command_text:
        if '/' == command_text[0]:
            success, result = CommandManager.ProcessCommand(command_text, state['user'])
            if success:
                add_line(state, result)
        else:
            output_msg = state['user'].Name + ": " + WordFilter.Filter(command_text)
            add_line(state, output_msg)
        add_history(state, command_text)
    close(state)

def add_line(state, line):
    text = state['text']
    text.append(line)
    if len(text) > MAX_LINES:
        text = text[1:]
    text_box = state['ui_text_box']
    text_box.Text = TextDisplay.NEW_LINE.join(text)
    state['text'] = text

def add_history(state, line):
    history = state['command_history']
    history.insert(0, line)
    state['command_history'] = history[:MAX_HISTORY]

def clear(state):
    state['text'] = []

def open(state):
    Cursor.Show = True
    
    user = state['user']
    user.CanInteract = False
    user.View.CaptureMovement = False
    user.View.LockMouse = False
    user.UI.GamePanel.Cross.Disable()
    
    command_line = state['ui_command_line']
    command_line.Enable()
    command_line.Text = str()
    command_line.InFocus = True
    
    state['open'] = True
    state['command_history_index'] = 0

def close(state):
    Cursor.Show = False
    Cursor.Center()
    
    user = state['user']
    user.CanInteract = True
    user.View.CaptureMovement = True
    user.View.LockMouse = True
    user.UI.GamePanel.Cross.Enable()

    command_line = state['ui_command_line']
    command_line.Disable()
    command_line.InFocus = False
    state['open'] = False

def can_open(user):
    return should_show(user) and not user.InterfaceOpened and user.CanInteract

def should_show(user):
    return not Core.is_paused() and not Core.is_loading() and not Core.is_start_menu() and not user.IsDead and GameSettings.ShowChat
           

def enable(state):
    state['ui_panel'].Enable()
    
def disable(state):
    state['ui_panel'].Disable()
    if state['open']:
        close(state)