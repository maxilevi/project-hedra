using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Hedra.Engine.Events;
using Hedra.Engine.Generation;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering;
using Hedra.Engine.Sound;
using Hedra.Engine.Testing;
using OpenTK;
using OpenTK.Input;

namespace Hedra.Engine.Player
{
    public class PlayerMovement : MovementManager
    {
        private readonly LocalPlayer _player;
        private readonly Dictionary<Key, Action> _registeredKeys;
        private float _characterRotation;
        private float _yaw;
        private float _targetYaw;
        private Vector3 _angles;
        private Vector3 _targetAngles;
        private float _vehicleCooldown;

        public PlayerMovement(LocalPlayer Player) : base(Player)
        {
            _player = Player;
            _registeredKeys = new Dictionary<Key, Action>();

            EventDispatcher.RegisterMouseDown(this, this.OnMouseButtonDown);
            EventDispatcher.RegisterKeyDown(this, this.OnKeyDown);
            this.RegisterListeners();
        }

        public void OnMouseButtonDown(object Sender, MouseButtonEventArgs EventArgs)
        {
            if (!this.CaptureMovement || GameSettings.Paused || Human.IsKnocked || Human.IsDead
                || !Human.CanInteract || Human.IsRiding || Human.IsEating) return;

            if (EventArgs.Button == MouseButton.Middle) _player.Roll();
        }

        protected override void DoUpdate()
        {
            _vehicleCooldown -= Time.IndependantDeltaTime;
            if (!CaptureMovement || _player.IsKnocked || _player.IsDead || !Human.CanInteract || GameSettings.Paused)
                return;


            if ((GameManager.Keyboard[Key.W] || GameManager.Keyboard[Key.A] || GameManager.Keyboard[Key.S] || GameManager.Keyboard[Key.D]) && !_player.IsCasting)
            {
                Human.Model.Rotation = new Vector3(0, Human.Model.Rotation.Y, Human.Model.Rotation.Z);

            }

            if (!_player.IsGliding)
            {
                _characterRotation = Human.FacingDirection;
                if (GameManager.Keyboard[Key.D]) _characterRotation += -90f;
                if (GameManager.Keyboard[Key.A])_characterRotation += 90f;
                if (GameManager.Keyboard[Key.S]) _characterRotation += 180f;
                if (GameManager.Keyboard[Key.W]) _characterRotation += 0f;
                if (GameManager.Keyboard[Key.W] && GameManager.Keyboard[Key.D]) _characterRotation += 45f;
                if (GameManager.Keyboard[Key.W] && GameManager.Keyboard[Key.A]) _characterRotation += -45f;
                if (GameManager.Keyboard[Key.S] && GameManager.Keyboard[Key.D]) _characterRotation += 135f;
                if (GameManager.Keyboard[Key.S] && GameManager.Keyboard[Key.A]) _characterRotation += -135f;

                var keysPresses = 0f;
                var wPressed = GameManager.Keyboard[Key.W];
                var sPressed = GameManager.Keyboard[Key.S];
                var dPressed = GameManager.Keyboard[Key.D];
                var aPressed = GameManager.Keyboard[Key.A];
                if (dPressed && aPressed)
                {
                    dPressed = false;
                    aPressed = false;
                }
                if (wPressed && sPressed)
                {
                    wPressed = false;
                    sPressed = false;
                }
                keysPresses += wPressed ? 1f : 0f;
                keysPresses += sPressed ? 1f : 0f;
                keysPresses += dPressed ? 1f : 0f;
                keysPresses += aPressed ? 1f : 0f;
                keysPresses = 1f / (!wPressed && !sPressed && !aPressed && !dPressed ? 1f : keysPresses);
                if (keysPresses < 1f) keysPresses *= 1.5f;

                _targetAngles.Z = 5f * (_player.View.StackedYaw - _yaw);
                _targetAngles = Mathf.Clamp(_targetAngles, -10f, 10f);
                _angles = Mathf.Lerp(_angles, _targetAngles * (GameManager.Keyboard[Key.W] ? 1.0F : 0.0F), (float)Time.DeltaTime * 8f);
                _yaw = Mathf.Lerp(_yaw, _player.View.StackedYaw, (float)Time.DeltaTime * 2f);
                if (GameManager.Keyboard[Key.W])
                {
                    _targetYaw = _player.View.TargetYaw;
                    this.ProcessMovement(_characterRotation, this.MoveFormula(_player.View.Forward) * keysPresses);                   
                    this.Orientate();
                }
                _player.Model.TransformationMatrix = 
                    Matrix4.CreateRotationY(-_player.Model.Rotation.Y * Mathf.Radian) *
                    Matrix4.CreateRotationZ(_angles.Z * Mathf.Radian * (_player.IsUnderwater ? 0.0f : 1.0f)) *
                    Matrix4.CreateRotationY(_player.Model.Rotation.Y * Mathf.Radian);
                if (GameManager.Keyboard[Key.S])
                {
                    this.ProcessMovement(_characterRotation, this.MoveFormula(_player.View.Backward) * keysPresses);
                }

                if (GameManager.Keyboard[Key.A])
                {
                    this.ProcessMovement(_characterRotation, this.MoveFormula(_player.View.Left) * keysPresses);
                }

                if (GameManager.Keyboard[Key.D])
                {
                    this.ProcessMovement(_characterRotation, this.MoveFormula(_player.View.Right) * keysPresses);
                }
                
		        if(GameManager.Keyboard[Key.ControlLeft] && _player.Physics.InFrontOfWall)
		        {
		            if(_player.Stamina > 5)
		            {
		                if (!_player.IsClimbing)
		                {
		                    _player.AddBonusSpeedWhile(-_player.Speed + _player.Speed * .15f, () => _player.IsClimbing);
		                }
		                _player.IsClimbing = true;
                    }
		            else
		            {
		            	if(_player.IsClimbing)
			                _player.IsClimbing = false;
		            }
		        }
		        else
		        {
		            if(_player.IsClimbing)
		                _player.IsClimbing = false;
		        }
            }


            if (!_player.IsUnderwater) return;
            this.ClampSwimming(_player);
            if (GameManager.Keyboard[Key.Space]) this.MoveInWater(true);
            if (GameManager.Keyboard[Key.ShiftLeft]) this.MoveInWater(false);
        }
        
        private void RegisterKey(Key Key, Action Action)
        {
            _registeredKeys.Add(Key, Action);
        }

        private void RegisterListeners()
        {
            this.RegisterKey(Key.Q, delegate
            {
                if (_player.CanInteract) _player.EatFood();
            });

            this.RegisterKey(Key.G, delegate
            {
                if (!GameManager.InStartMenu && !GameManager.InMenu && !Human.IsKnocked
                    && Human.CanInteract && _vehicleCooldown < 0)
                {
                    var vehicleItem = _player.Inventory.Vehicle;
                    if (vehicleItem == null && !GameSettings.Paused)
                    {
                        _player.MessageDispatcher.ShowNotification("YOU NEED A VEHICLE TO DO THAT", Color.Red, 3f, true);
                    }
                    else if (vehicleItem != null)
                    {
                        _vehicleCooldown = .25f;
                        IVehicle vehicle = null;
                        switch (vehicleItem.Name)
                        {
                            // FIXME: Use a more OOP style
                            case "Boat":
                                vehicle = _player.Boat;
                                break;
                            case "Glider":
                                vehicle =_player.Glider;
                                break;
                            default:
                                throw new ArgumentOutOfRangeException($"Failed to find a vehicle from '{vehicleItem.Name}'.");
                        }
                        if (vehicle.Enabled) vehicle.Disable();
                        else vehicle.Enable();
                    }
                }
            });

            this.RegisterKey(Key.R, delegate
            {
                if (!GameSettings.Paused && _player.IsDead)
                    _player.Respawn();
            });

            this.RegisterKey(Key.Space, delegate
            {
                if (!_player.IsUnderwater) this.Jump();
            });

            this.RegisterKey(Key.F, delegate
            {
                if (!GameSettings.Paused && _player.CanInteract)
                {
                    _player.HandLamp.Enabled = !_player.HandLamp.Enabled;
                    SoundManager.PlaySound(SoundType.NotificationSound, _player.Position, false, 1f, .5f);
                }
            });

            this.RegisterKey(Key.F3, delegate
            {
                GameSettings.DebugView = !GameSettings.DebugView && GameSettings.DebugMode;             
            });
        }

        public void OnKeyDown(object Sender, KeyEventArgs EventArgs)
        {
            if (_registeredKeys.ContainsKey(EventArgs.Key)) _registeredKeys[EventArgs.Key]();

            var pushedText = false;
            if (EventArgs.Key == Key.Enter && _player.Chat.Focused)
            {
                _player.Chat.PushText();
                pushedText = true;
            }

            if (EventArgs.Key == Key.Enter && !GameSettings.Paused && !_player.IsDead && Human.CanInteract && !pushedText)
            {
                _player.Chat.Focus();
            }

            if (EventArgs.Key == Key.Escape && !_player.UI.GamePanel.Enabled && !_player.UI.Hide)
                SoundManager.PlayUISound(SoundType.ButtonClick);

            if (EventArgs.Key == Key.Escape && _player.Chat.Focused)
            {
                _player.Chat.LoseFocus();
            }

            //Kinda specific?
            if (EventArgs.Key == Key.Escape && _player.UI.OptionsMenu.DonateBtcButton.Enabled)
            {
                _player.UI.OptionsMenu.DonateBtcButton.ForceClick();
            }
            if (EventArgs.Key == Key.F2)
            {
                if (!Directory.Exists(AssetManager.AppData + "/Screenshots/")) Directory.CreateDirectory(AssetManager.AppData + "/Screenshots/");
                _player.MessageDispatcher.ShowNotification("Saved screenshot as " + Recorder.SaveScreenshot(AssetManager.AppData + "/Screenshots/"), System.Drawing.Color.White, 3f, false);
            }
            if (EventArgs.Key == Key.F4) _player.UI.ShowHelp = !_player.UI.ShowHelp;

            if (EventArgs.Key == Key.F1)
            {
                _player.UI.Hide = !_player.UI.Hide;
            }

            if (GameSettings.DebugView && EventArgs.Key == Key.F5)
            {
                World.ReloadModules();

                _player.Chat.AddLine("Modules reloaded.");
            }
            if (GameSettings.DebugView && EventArgs.Key == Key.F6)
            {
                World.Discard();
                lock (World.Chunks)
                {
                    for (int i = 0; i < World.Chunks.Count; i++)
                    {
                        World.RemoveChunk(World.Chunks[i]);
                    }
                }
                _player.Chat.AddLine("Chunks discarded.");
            }
            if (EventArgs.Key == Key.F12)
            {
                WorldRenderer.StaticBuffer.Vertices.DrawAndSave();
                WorldRenderer.StaticBuffer.Indices.DrawAndSave();
                WorldRenderer.WaterBuffer.Colors.DrawAndSave();
            }

#if DEBUG
            if (EventArgs.Key == Key.F10)
            {
                ShaderManager.ReloadShaders();
            }
            if (EventArgs.Key == Key.F9 && _player.CanInteract)
            {
                Recorder.Active = !Recorder.Active;
            }
            if (EventArgs.Key == Key.H && _player.CanInteract) GameSettings.LockFrustum = !GameSettings.LockFrustum;

            if (EventArgs.Key == Key.Number7 && _player.CanInteract)
            {
                EnvironmentSystem.SkyManager.Sky.Enabled = !EnvironmentSystem.SkyManager.Sky.Enabled;
            }

            if (EventArgs.Key == Key.J)
            {
                World.AddChunkToQueue(World.GetChunkByOffset(World.ToChunkSpace(_player.Position)), true);
            }

            if (EventArgs.Key == Key.L && _player.CanInteract) GameSettings.Wireframe = !GameSettings.Wireframe;


            if (EventArgs.Key == Key.Keypad0 && _player.CanInteract)
            {
                _player.Physics.TargetPosition += Vector3.UnitY * 25f;
            }
            if (EventArgs.Key == Key.Insert && _player.CanInteract)
            {
                _player.AbilityTree.Reset();
            }
            if (EventArgs.Key == Key.Keypad2 && _player.CanInteract)
            {
                _player.Health = _player.MaxHealth;
            }
            if (EventArgs.Key == Key.F11)
                Tester.Run(AssetManager.AppPath + "/unitTests.txt");
#endif
        }
    }
}
