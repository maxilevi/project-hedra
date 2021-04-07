using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Hedra.Core;
using Hedra.Engine.Events;
using Hedra.Engine.Game;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.Localization;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Core;
using Hedra.Engine.Sound;
using Hedra.Engine.Windowing;
using Hedra.Game;
using Hedra.Localization;
using Hedra.Sound;
using Hedra.User;
using System.Numerics;
using Hedra.Numerics;
using Silk.NET.Input.Common;
using KeyEventArgs = Hedra.Engine.Events.KeyEventArgs;

namespace Hedra.Engine.Player
{
    public class PlayerMovement : MovementManager, IDisposable
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

        private void OnMouseButtonDown(object Sender, MouseButtonEventArgs EventArgs)
        {
            if (!this.CaptureMovement || GameSettings.Paused || Human.IsKnocked || Human.IsDead
                || !Human.CanInteract || Human.IsRiding || Human.IsEating) return;

            if (EventArgs.Button == MouseButton.Middle)
            {
                if(GameManager.Keyboard[Controls.Rightward] || GameManager.Keyboard[Controls.Leftward])
                    _player.Roll(RollType.Sideways);
                else
                    _player.Roll(RollType.Normal);
            }
        }

        protected override void DoUpdate()
        {
            _vehicleCooldown -= Time.IndependentDeltaTime;
            if (!CaptureMovement || _player.IsKnocked || _player.IsDead || !Human.CanInteract || GameSettings.Paused)
                return;


            if ((GameManager.Keyboard[Controls.Forward] || GameManager.Keyboard[Controls.Leftward] || GameManager.Keyboard[Controls.Backward] || GameManager.Keyboard[Controls.Rightward]) && !_player.IsCasting)
            {
                Human.Model.LocalRotation = new Vector3(0, Human.Model.LocalRotation.Y, Human.Model.LocalRotation.Z);

            }

            HandleSprinting();

            if (!_player.IsGliding)
            {
                _characterRotation = Human.FacingDirection;
                if (GameManager.Keyboard[Controls.Rightward]) _characterRotation += -90f;
                if (GameManager.Keyboard[Controls.Leftward]) _characterRotation += 90f;
                if (GameManager.Keyboard[Controls.Backward]) _characterRotation += 180f;
                if (GameManager.Keyboard[Controls.Forward]) _characterRotation += 0f;
                if (GameManager.Keyboard[Controls.Forward] && GameManager.Keyboard[Controls.Rightward]) _characterRotation += 45f;
                if (GameManager.Keyboard[Controls.Forward] && GameManager.Keyboard[Controls.Leftward]) _characterRotation += -45f;
                if (GameManager.Keyboard[Controls.Backward] && GameManager.Keyboard[Controls.Rightward]) _characterRotation += 135f;
                if (GameManager.Keyboard[Controls.Backward] && GameManager.Keyboard[Controls.Leftward]) _characterRotation += -135f;

                var keysPresses = 0f;
                var wPressed = GameManager.Keyboard[Controls.Forward];
                var sPressed = GameManager.Keyboard[Controls.Backward];
                var dPressed = GameManager.Keyboard[Controls.Rightward];
                var aPressed = GameManager.Keyboard[Controls.Leftward];
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

                var isRiding = _player.IsRiding;
                var limit = isRiding ? 25f : 17.5f;
                _targetAngles.Z = (limit / 2) * (_player.View.StackedYaw - _yaw);
                _targetAngles = Mathf.Clamp(_targetAngles, -limit, limit);
                _angles = Mathf.Lerp(_angles, _targetAngles * (GameManager.Keyboard[Controls.Forward] ? 1.0F : 0.0F), (float)Time.DeltaTime * 8f);
                _yaw = Mathf.Lerp(_yaw, _player.View.StackedYaw, (float)Time.DeltaTime * 2f);
                IsMovingForward = GameManager.Keyboard[Controls.Forward];
                IsMovingBackwards = GameManager.Keyboard[Controls.Backward];
                if (GameManager.Keyboard[Controls.Forward] || GameSettings.ContinousMove)
                {
                    if (GameSettings.ContinousMove)
                    {
                        Human.Physics.CollidesWithStructures = false;
                        Human.Physics.CollidesWithEntities = false;
                    }
                    _targetYaw = _player.View.TargetYaw;
                    this.ProcessMovement(_characterRotation, Human.Physics.MoveFormula(_player.View.Forward) * keysPresses);                   
                    this.Orientate();
                }
                _player.Model.TiltMatrix =
                    Matrix4x4.CreateRotationZ(_angles.Z * Mathf.Radian * (_player.IsUnderwater ? 0.0f : 1.0f));
                _player.Model.TransformationMatrix = Matrix4x4.Identity;
                if (GameManager.Keyboard[Controls.Backward])
                {
                    this.ProcessMovement(_characterRotation, Human.Physics.MoveFormula(_player.View.Backward) * keysPresses);
                    RollFacing = _characterRotation;
                }

                if (GameManager.Keyboard[Controls.Leftward])
                {
                    this.ProcessMovement(_characterRotation, Human.Physics.MoveFormula(_player.View.Left) * keysPresses);
                    RollDirection = Human.Physics.MoveFormula(_player.View.Left, false).Xz().ToVector3().NormalizedFast();
                    RollFacing = _characterRotation;
                }

                if (GameManager.Keyboard[Controls.Rightward])
                {
                    ProcessMovement(_characterRotation, Human.Physics.MoveFormula(_player.View.Right) * keysPresses);
                    RollDirection = Human.Physics.MoveFormula(_player.View.Right, false).Xz().ToVector3().NormalizedFast();
                    RollFacing = _characterRotation;
                }
                /*
                if(GameManager.Keyboard[Controls.Climb] && _player.Physics.InFrontOfWall)
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
                }*/
            }
        

            if (!_player.IsUnderwater) return;
            if (GameManager.Keyboard[Controls.Jump]) this.MoveInWater(true);
            if (GameManager.Keyboard[Controls.Descend]) this.MoveInWater(false);
        }

        private void HandleSprinting()
        {
            if (GameManager.Keyboard[Controls.Sprint] && _player.Stamina > 10 && _player.IsGrounded)
            {
                _player.IsSprinting = true;
            }
            else
            {
                if(_player.IsSprinting)
                    _player.IsSprinting = false;
            }
        }

        private void RegisterKey(Key Key, Action Action)
        {
            _registeredKeys.Add(Key, Action);
        }

        private void RegisterListeners()
        {
            this.RegisterKey(Controls.Eat, delegate
            {
                if (_player.CanInteract) _player.EatFood();
            });

            this.RegisterKey(Controls.SpecialItem, delegate
            {
                if (!GameManager.InStartMenu && !GameManager.InMenu && !Human.IsKnocked
                    && Human.CanInteract && _vehicleCooldown < 0)
                {
                    var vehicleItem = _player.Inventory.Vehicle;
                    if (vehicleItem == null && !GameSettings.Paused)
                    {
                        _player.MessageDispatcher.ShowNotification(Translations.Get("need_vehicle"), Color.Red, 3f, true);
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
            this.RegisterKey(Controls.Help, delegate
            {
                _player.UI.ShowHelp = !_player.UI.ShowHelp;             
            });
            
            this.RegisterKey(Key.F3, delegate
            {
                GameSettings.DebugView = !GameSettings.DebugView && GameSettings.DebugMode;             
            });
            this.RegisterKey(Key.F12, delegate
            {
                GameSettings.DebugPhysics = !GameSettings.DebugPhysics && GameSettings.DebugMode;             
            });
            this.RegisterKey(Key.F5, delegate
            {
                Chat.Log($"ObjectsInSimulation = '{Bullet.BulletPhysics.ObjectsInSimulation}'; RigidbodyCount = '{Bullet.BulletPhysics.RigidbodyCount}'");
            });
        }

        public void OnKeyDown(object Sender, KeyEventArgs EventArgs)
        {
            if (_registeredKeys.ContainsKey(EventArgs.Key)) _registeredKeys[EventArgs.Key]();

            if (Controls.Jump == EventArgs.Key)
            {
                if (!_player.IsUnderwater) this.Jump();
            }

            if (EventArgs.Key == Key.Escape && !_player.UI.GamePanel.Enabled && !_player.UI.Hide)
                SoundPlayer.PlayUISound(SoundType.ButtonClick);

            if (EventArgs.Key == Key.F2)
            {
                if (!Directory.Exists(AssetManager.AppPath + "/Screenshots/")) Directory.CreateDirectory(AssetManager.AppPath + "/Screenshots/");
                _player.MessageDispatcher.ShowNotification($"{Translations.Get("saved_screenshot", Recorder.SaveScreenshot($"{AssetManager.AppPath}/Screenshots/"))}", Color.White, 3f, false);
            }

            if (EventArgs.Key == Key.F6)
            {
                AutosaveManager.Save();
                _player.MessageDispatcher.ShowNotification($"{Translations.Get("game_saved_success")}", Color.White, 2f);
            }

            if (EventArgs.Key == Key.F1)
            {
                _player.UI.Hide = !_player.UI.Hide;
            }

            if (GameSettings.DebugView && EventArgs.Key == Key.F5)
            {
                World.ReloadModules();

                //_player.Chat.AddLine("Modules reloaded.");
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
            if (EventArgs.Key == Key.O && _player.CanInteract) GameSettings.LockFrustum = !GameSettings.LockFrustum;

            if (EventArgs.Key == Key.Number7 && _player.CanInteract)
            {
                EnvironmentSystem.SkyManager.Sky.Enabled = !EnvironmentSystem.SkyManager.Sky.Enabled;
            }

            if (EventArgs.Key == Key.J)
            {
                World.AddChunkToQueue(World.GetChunkByOffset(World.ToChunkSpace(_player.Position)), ChunkQueueType.Mesh);
            }

            if (EventArgs.Key == Key.K)
            {
                World.MarkChunkReady(World.GetChunkByOffset(World.ToChunkSpace(_player.Position)));
            }

            if (EventArgs.Key == Key.P)
            {
                //var c = World.GetChunkByOffset(World.ToChunkSpace(_player.Position));
            }

            if (EventArgs.Key == Key.L && _player.CanInteract) GameSettings.Wireframe = !GameSettings.Wireframe;


            if (EventArgs.Key == Key.Keypad0 && _player.CanInteract)
            {
                _player.Physics.Translate(Vector3.UnitY * 25f);
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
            {
                GameManager.LoadCharacter(DataManager.PlayerFiles[0]);
            }
#endif
        }

        public void Dispose()
        {
            EventDispatcher.UnregisterMouseDown(this);
            EventDispatcher.UnregisterKeyDown(this);
        }
    }
}
