/*
 * Author: Zaphyk
 * Date: 07/02/2016
 * Time: 02:12 a.m.
 *
 */
 #pragma warning disable 0618
using System;
using OpenTK;
using OpenTK.Input;
using Hedra.Engine.Generation;
using Hedra.Engine.Events;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.QuestSystem;
using System.Collections;
using Hedra.Engine.ClassSystem;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.ModuleSystem;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Rendering.Animation;
using Hedra.Engine.Sound;
using Hedra.Engine.UnitTesting;

namespace Hedra.Engine.Player
{
	 public class MovementManager : EventListener, IDisposable
	{

		private Humanoid Human;
		public bool Check = true;
		public bool MoveFeet, UnlockAnimations, OrientateWhileMoving = true;
		public float JumpDist = 8f;
		public float MoveCount = 1;
		public bool IsFloating = false;
		public Vector3 RollDirection = Vector3.Zero; 
		private bool _inJumpCoroutine;
		public float TargetSpeed = 1.75f;
	    private const float NormalSpeed = 2.25f;
	    private const float AttackingSpeed = 1.25f;

        public MovementManager(Humanoid RefPlayer){
			Human = RefPlayer;
		}
		
		public void Dispose(){
			EventDispatcher.Remove(this);
			Human = null;
		}
		
		public void MoveInWater(bool Up){
			//Behaviour when player is underwater
			if(Human.IsUnderwater){
				if(Human.IsRolling || Human.IsDead || !Human.CanInteract) return;
					Human.IsGrounded = false;
					Human.Physics.Velocity = Vector3.Zero;
					Human.Model.Rotation = new Vector3(0, Human.Model.Rotation.Y, Human.Model.Rotation.Z);
					if(Up)
						Human.Physics.TargetPosition += Vector3.UnitY * 12.5f * (float) Time.deltaTime;
					else
						Human.Physics.TargetPosition -= Vector3.UnitY * 12.5f * (float) Time.deltaTime;

                    Human.Physics.TargetPosition = new Vector3(
                        Human.Physics.TargetPosition.X,
                        Math.Max(Physics.HeightAtPosition(Human.Physics.TargetPosition)+2, Human.Physics.TargetPosition.Y),
                        Human.Physics.TargetPosition.Z);

					IsFloating = true;
			}
		}
		
		public void Jump(){		
			if(_inJumpCoroutine || Human.Knocked || Human.IsCasting || Human.IsRiding ||
                Human.IsRolling || Human.IsDead || !Human.IsGrounded || !Human.CanInteract ||
                Math.Abs(Human.Physics.TargetPosition.Y - Human.Position.Y) > 1.0f || !this.Check)
				return;

		    Human.IsSitting = false;
		    Human.IsGrounded = false;
		    SoundManager.PlaySound(SoundType.Jump, Human.Position, false, 1f, 1f);
		    CoroutineManager.StartCoroutine(JumpCoroutine);
        }
		
		public IEnumerator JumpCoroutine(){
			_inJumpCoroutine = true;
			float StartingY = Human.Physics.TargetPosition.Y;
			var quit = false;
			Human.Physics.GravityDirection = Vector3.Zero;

			while(Human.Model.Position.Y < StartingY+JumpDist && !quit){
				float prevTarget = Human.Physics.TargetPosition.Y;
			    var command = new MoveCommand()
			    {
			        Delta = Vector3.UnitY * 80,
			        Parent = Human
			    };
				Human.Physics.ProccessCommand( command );
			    if(Math.Abs(prevTarget - Human.Physics.TargetPosition.Y) < .01f)
                	quit = true;
				
				yield return null;
			}
            TaskManager.Delay(1000, delegate
            {
                _inJumpCoroutine = false;
            });
			Human.Physics.GravityDirection = -Vector3.UnitY;
		}
		
		public bool IsJumping => _inJumpCoroutine;

	    public override void OnMouseButtonDown(object sender, MouseButtonEventArgs e){
			if(!Check || GameSettings.Paused || Human.Knocked || Human.IsDead || !Human.CanInteract || Human.IsRiding || Human.IsEating)
				return;	
			if(e.Button == MouseButton.Middle && Human is LocalPlayer){
				(Human as LocalPlayer).Roll();
			}
		}
		
		public void OrientatePlayer(LocalPlayer Player){
			Player.Model.TargetRotation = new Vector3(Player.Model.Rotation.X, -Player.Model.FacingDirection, Player.Model.Rotation.Z);
			Player.Orientation = new Vector3( (float) Math.Cos(Player.View.Yaw) * 5f * 1.75f, 0, (float) Math.Sin(Player.View.Yaw) * 5f * 1.75f).Normalized();
		}
		
		public void Update(){
			if(Human is LocalPlayer)
				CheckInput(Human as LocalPlayer);

			
			if(MoveFeet){
            	if(OrientateWhileMoving)
            		Human.Model.TargetRotation = new Vector3(Human.Model.Rotation.X, -Human.Model.FacingDirection, Human.Model.Rotation.Z);
				
				//Move
				for(int i = 0; i < 8; i++){
					if(OrientateWhileMoving){
						LocalPlayer Player = Human as LocalPlayer;
						if(Player != null){
							//RollDirection = new Vector3(Player.Model.Rotation.X, -Player.Model.FacingDirection, Player.Model.Rotation.Z);
							Player.Orientation = new Vector3( (float) Math.Cos(Player.View.Yaw) * 5f * TargetSpeed, 0, (float) Math.Sin(Player.View.Yaw) * 5f * TargetSpeed).Normalized();
						}
						for(int k = 0; k < 10; k++)
							Human.Physics.Move(Human.Orientation * MoveCount * .1f );
					}else{//Reduce timestep so collision works fine
						for(int k = 0; k < 12; k++)
							Human.Physics.Move(Human.Orientation * MoveCount * .1f);
					}
				}
			}
			
			if(!Human.IsGrounded && !Human.IsDead && Human.CanInteract && !Human.IsRiding && Human.IsUnderwater && !GameSettings.Paused){
				if((Program.GameWindow.Keyboard[Key.W] || Program.GameWindow.Keyboard[Key.A] ||  Program.GameWindow.Keyboard[Key.S] || Program.GameWindow.Keyboard[Key.D])){
					Human.Model.Swim();
					Human.Model.TargetRotation = new Vector3(90,Human.Model.TargetRotation.Y,Human.Model.TargetRotation.Z);
				}else{
					Human.Model.IdleSwim();
					Human.Model.TargetRotation = new Vector3(0,Human.Model.TargetRotation.Y,Human.Model.TargetRotation.Z);
				}
			}
				
		}
		
		#region PLAYER - SPECIFIC
		private float DirectionY = 0;
		private void CheckInput(LocalPlayer Player){
			if(!Check|| Player.Knocked || Player.IsDead || !Human.CanInteract || GameSettings.Paused)
				return;

				
			if((Program.GameWindow.Keyboard[Key.W] || Program.GameWindow.Keyboard[Key.A] ||  Program.GameWindow.Keyboard[Key.S] || Program.GameWindow.Keyboard[Key.D]) && !Player.IsCasting)
         	{
            	Player.Model.Run();
            	Human.Model.Rotation = new Vector3(0,Human.Model.Rotation.Y,Human.Model.Rotation.Z);

            }else{
				if(!Player.IsSitting)
            		Player.Model.Idle();
            	
			}

		    if(!Player.IsGliding)
			{
				 DirectionY = -Player.Model.FacingDirection;
	            if(Program.GameWindow.Keyboard[Key.D])
	            	DirectionY += -90;
	            if(Program.GameWindow.Keyboard[Key.A])
	            	DirectionY += 90;
	            if(Program.GameWindow.Keyboard[Key.S])
	            	DirectionY += 180;
	            if(Program.GameWindow.Keyboard[Key.W])
	            	DirectionY += 0;
	            
	            if(Program.GameWindow.Keyboard[Key.W] && Program.GameWindow.Keyboard[Key.D])
	            	DirectionY += 45;
	            if(Program.GameWindow.Keyboard[Key.W] && Program.GameWindow.Keyboard[Key.A])
	            	DirectionY += -45;
	            
	            if(Program.GameWindow.Keyboard[Key.S] && Program.GameWindow.Keyboard[Key.D])
	            	DirectionY += 135;
	            if(Program.GameWindow.Keyboard[Key.S] && Program.GameWindow.Keyboard[Key.A])
	            	DirectionY += -135;
                	                 	
				float movementSpeed = (Player.IsUnderwater && !Player.IsGrounded ? 1.25f : 1.0f) * Player.Speed;
			    var keysPresses = 0f;
			    keysPresses += Program.GameWindow.Keyboard[Key.W] ? 1f : 0f;
			    keysPresses += Program.GameWindow.Keyboard[Key.S] ? 1f : 0f;
			    keysPresses += Program.GameWindow.Keyboard[Key.D] ? 1f : 0f;
			    keysPresses += Program.GameWindow.Keyboard[Key.A] ? 1f : 0f;
			    keysPresses = 1f / keysPresses;
			    if (keysPresses < 1f) keysPresses *= 1.5f;
			    float speed = Human.IsAttacking && Human.Class is ArcherDesign ? AttackingSpeed : NormalSpeed;

                if (Program.GameWindow.Keyboard[Key.W])
				{
					var moveSpace = new Vector3( 
                        (float) Math.Cos(Player.View.Yaw) * 5f * TargetSpeed * movementSpeed,
                        0,
                        (float) Math.Sin(Player.View.Yaw) * 5f * TargetSpeed * movementSpeed );
				    this.ProcessMovement(Player, speed * moveSpace * keysPresses);

                    Player.Model.TargetRotation = new Vector3(Player.Model.Rotation.X, DirectionY, Player.Model.Rotation.Z);
					Player.Orientation = new Vector3( moveSpace.X, 0, moveSpace.Z).NormalizedFast();
				}
					
		        if (Program.GameWindow.Keyboard[Key.S])
		        { 
		            var moveSpace = new Vector3( 
                        (float) -(Math.Cos(Player.View.Yaw) * 5f * TargetSpeed * movementSpeed),
                        0,
                        (float) -(Math.Sin(Player.View.Yaw) * 5f * TargetSpeed * movementSpeed) );
		            this.ProcessMovement(Player, speed * moveSpace * keysPresses);
                }
		            
		        if (Program.GameWindow.Keyboard[Key.A])
		        {
		            var moveSpace = new Vector3( 
                        (float) -(Math.Cos(Player.View.Yaw + Math.PI / 2) * 5f * TargetSpeed * movementSpeed),
                        0,
                        (float) -(Math.Sin(Player.View.Yaw + Math.PI / 2) * 5f * TargetSpeed * movementSpeed) );
		            this.ProcessMovement(Player, speed * moveSpace * keysPresses);
                }
		       
		        if (Program.GameWindow.Keyboard[Key.D])
		        {
		            var moveSpace = new Vector3( 
                        (float) Math.Cos(Player.View.Yaw + Math.PI / 2) * 5f * TargetSpeed * movementSpeed,
                        0,
                        (float) Math.Sin(Player.View.Yaw + Math.PI / 2) * 5f * TargetSpeed * movementSpeed);
                    this.ProcessMovement(Player, speed * moveSpace * keysPresses);
		        }	 

                #region Climb (Indev)
                /*
		        if(Program.GameWindow.Keyboard[Key.ControlLeft]){
		            if(Player.Stamina > 5){
		            	Player.IsClimbing = true;
						Player.Climb();
		            }else{
		            	if(Player.IsClimbing){
		            		Player.IsClimbing = false;
		            		Player.EndClimb();
		            	}
		            }
		        }else{
		            if(Player.IsClimbing){
		            	Player.IsClimbing = false;
		            	Player.EndClimb();
		            }
		        }*/
                #endregion
            }

			
			if(Player.IsUnderwater){
				if(Program.GameWindow.Keyboard[Key.Space]) this.MoveInWater(true);		
				if(Program.GameWindow.Keyboard[Key.ShiftLeft]) this.MoveInWater(false);	
			}
		}

	    public void ProcessMovement(Humanoid Human, Vector3 moveSpace)
	    {
            for (int i = 0; i < 10; i++)
	            Human.Physics.Move(moveSpace * .1f);

	        if (!Human.WasAttacking && !Human.IsAttacking)
	        {
	            Human.Model.TargetRotation = new Vector3(Human.Model.Rotation.X, DirectionY, Human.Model.Rotation.Z);
	            Human.Orientation = new Vector3(moveSpace.X, 0, moveSpace.Z).NormalizedFast();
	        }
	        RollDirection = new Vector3(Human.Model.Rotation.X, DirectionY, Human.Model.Rotation.Z);
	        Human.IsMoving = moveSpace.LengthSquared > 0;
        }
		
		public override void OnKeyDown(object sender, KeyboardKeyEventArgs e){
			var player = Human as LocalPlayer;
			if(player == null) return;
			
			if(e.Key == Key.Q && Human.CanInteract){
				player.EatFood();
			}

			if(e.Key == Key.G && Human.CanInteract && !GameManager.InMenu && !Human.Knocked && !GameManager.InStartMenu)
			{
			    if(player.Inventory.Glider == null && !GameSettings.Paused){
					player.MessageDispatcher.ShowNotification("YOU NEED A GLIDER TO DO THAT", System.Drawing.Color.DarkRed, 3f, true);
					return; // Player doesnt have a glider item.
				}

			    player.IsGliding = !player.IsGliding;
			}
			
			if(e.Key == Key.R && !GameSettings.Paused && (player.IsDead || World.QuestManager.Quest.IsLost || World.QuestManager.QuestCompleted) ){
				player.Respawn();
			}
			
			if (e.Key == Key.Space && !player.IsUnderwater)
			{
					Jump();
			}
			
			if(e.Key == Key.I && !GameSettings.Paused && Human.CanInteract && !player.IsDead && !player.Map.Show && !player.AbilityTree.Show)
                player.Inventory.Show = !player.Inventory.Show;
			
			if(e.Key == Key.X && !GameSettings.Paused && !player.IsDead && Human.CanInteract && !player.Map.Show && !player.Inventory.Show)
                player.AbilityTree.Show = !player.AbilityTree.Show;
			
			
			if(e.Key == Key.M && !GameSettings.Paused && !player.IsDead && Human.CanInteract && !player.Inventory.Show && !player.AbilityTree.Show)
				player.Map.Show = !player.Map.Show ;
			
			
			bool PushedText = false;
			if(e.Key == Key.Enter && player.Chat.Focused){
				player.Chat.PushText();
				PushedText = true;
			}
			
			if(e.Key == Key.Enter && !GameSettings.Paused && !player.IsDead && Human.CanInteract && !PushedText){
				player.Chat.Focus();
			}

		    if (e.Key == Key.Escape && !player.UI.GamePanel.Enabled && !player.UI.Hide)
                Sound.SoundManager.PlayUISound(Sound.SoundType.ButtonClick);

            if (e.Key == Key.Escape && player.Chat.Focused){
				player.Chat.LoseFocus();
			}
			
			//Kinda specific?
			if(e.Key == Key.Escape && player.UI.OptionsMenu.DonateBtcButton.Enabled){
				player.UI.OptionsMenu.DonateBtcButton.ForceClick();
			}
			
			if(e.Key == Key.T && !GameSettings.Paused && !player.IsDead && Human.CanInteract && !player.AbilityTree.Show){
				if(player.QuestLog.Show)
					player.QuestLog.Show = false;
				else
					player.QuestLog.Show = true;
			}
			
			if(e.Key == Key.Escape && !GameManager.InStartMenu){
				if(player.QuestLog.Show || player.Inventory.Show || player.AbilityTree.Show || player.Chat.Focused || player.Trade.Show){
					
					if(player.Chat.Focused)
						player.Chat.LoseFocus();
					if(player.QuestLog.Show)
						player.QuestLog.Show = false;
					if(player.Inventory.Show)
						player.Inventory.Show = false;
					if(player.AbilityTree.Show)
						player.AbilityTree.Show = false;
				    if (player.Trade.Show) player.Trade.Cancel();
                }
                else{
					
					if(!player.UI.Menu.Enabled)
						player.UI.ShowMenu();
					else
						player.UI.HideMenu();
				}
					
			}
			if(e.Key == Key.F7){
				//AnalyticsManager.BugReport();
			}
			if(e.Key == Key.F3){
			    GameSettings.Debug = !GameSettings.Debug;
			}
			if(e.Key == Key.F2){
                AssetManager.CreateDirectory(AssetManager.AppData + "/Screenshots/");
				player.MessageDispatcher.ShowNotification( "Saved screenshot as "+Recorder.SaveScreenshot(AssetManager.AppData + "/Screenshots/"), System.Drawing.Color.White, 3f, false );
			}
			if(e.Key == Key.F4) 
				player.UI.ShowHelp = !player.UI.ShowHelp;
			if(e.Key == Key.F1){
				if(player.UI.Hide)
					player.UI.Hide = false;
				else
					player.UI.Hide = true;
			}
			if(e.Key == Key.F && !GameSettings.Paused && player.CanInteract){
				if(player.HandLamp.Enabled)
					player.HandLamp.Enabled = false;
				else
					player.HandLamp.Enabled = true;
				
				Sound.SoundManager.PlaySound(Sound.SoundType.NotificationSound, player.Position, false, 1f, .5f);
			}
			
			
			if(GameSettings.Debug && e.Key == Key.F5){
				World.ReloadModules();

                player.Chat.AddLine("Modules reloaded.");
			}
			if(GameSettings.Debug && e.Key == Key.F6){
				World.MeshQueue.SafeDiscard();
				World.ChunkGenerationQueue.SafeDiscard();
				lock(World.Chunks){
					for(int i = 0; i < World.Chunks.Count; i++){
						World.RemoveChunk(World.Chunks[i]);
					}
				}
				player.Chat.AddLine("Chunks discarded.");
			}
		    if (e.Key == Key.F12)
		    {
		        WorldRenderer.StaticBuffer.Vertices.DrawAndSave();
		        WorldRenderer.StaticBuffer.Indices.DrawAndSave();
		        WorldRenderer.WaterBuffer.Colors.DrawAndSave();
		    }

#if DEBUG
			if(e.Key == Key.F9 && player.CanInteract){
				if(Recorder.Active)
					Recorder.Active = false;
				else
					Recorder.Active = true;
			}
		    if (e.Key == Key.H && player.CanInteract) GameSettings.LockFrustum = !GameSettings.LockFrustum;

            if (e.Key == Key.Number7 && player.CanInteract)
			{
			    Enviroment.SkyManager.Skydome.Enabled = !Enviroment.SkyManager.Skydome.Enabled;
			}
			
			if(e.Key == Key.J){
				World.AddChunkToQueue(World.GetChunkByOffset(World.ToChunkSpace(player.Position)), true);
			}
			
			if(e.Key == Key.L && player.CanInteract) GameSettings.Wireframe = !GameSettings.Wireframe;
			

			if(e.Key == Key.Keypad0 && player.CanInteract){
				player.Physics.TargetPosition += Vector3.UnitY * 25f;
			}
            if (e.Key == Key.Insert && player.CanInteract){
				player.AbilityTree.Reset();
			}
			if(e.Key == Key.Keypad7 && player.CanInteract){
				World.QuestManager.Recreate();
			}
			if(e.Key == Key.Keypad2 && player.CanInteract){
				player.Health = player.MaxHealth;
			}
			if(e.Key == Key.K && player.CanInteract){
				World.GetChunkAt(player.Position).Landscape.DefineBlocks();
			}
            if(e.Key == Key.F11)
                UnitTester.Run(AssetManager.AppPath + "/unitTests.txt");
#endif
        }
		
		#endregion
	}
}
