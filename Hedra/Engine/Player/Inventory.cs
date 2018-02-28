/*
 * Created by SharpDevelop.
 * User: Maxi Levi
 * Date: 27/04/2016
 * Time: 08:00 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using OpenTK;
using OpenTK.Input;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.UI;
using Hedra.Engine.EntitySystem;
using System.Drawing;
using System.Drawing.Text;
using OpenTK.Graphics.OpenGL;
using Hedra.Engine.Events;
using System.Collections.Generic;
using Hedra.Engine.Item;
using System.IO;
using Hedra.Engine.QuestSystem;
using System.Linq;
using Hedra.Engine.Generation;


namespace Hedra.Engine.Player
{
	/// <summary>
	/// Description of Inventory.
	/// </summary>
	public class Inventory : EventListener
	{
		public InventoryItem[] Items {get; private set;}
		public Button[] ItemsButtons {get; private set;}
		public GUIText[] ItemsText {get; private set;}
		public ItemType[][] EquipmentTypes = new ItemType[8][];
		public Panel InventoryPanel;
		public Panel ItemInfoPanel;
		private PrivateFontCollection Fonts = new PrivateFontCollection();
		private LocalPlayer Player;
		private UIElement[] ItemStats = new UIElement[6];
		private Vector2 TargetResolution = new Vector2(800,600);
		private Vector2 TargetResolution2 = new Vector2(1024,578);
		public const int WeaponHolder = 16;
		public const int FoodHolder = 23;
		public const int RingHolder = 20;
		public const int GliderHolder = 22;
		public const int MountHolder = 21;
		public const int CoinHolder = 19;
		public const int InventorySpaces = 16;
		public const int WeaponEquipmentType = 0;
		private GUIText ItemTitle;
		private GUIText ItemDmg;
		private GUIText ItemEffect;
		private GUIText ItemSpeed;
		private Texture ItemImage, PlayerTexture, Background;
		public GUIText Name, Level;
		private Vector2 ItemStatsOffset = new Vector2(-0.055f, 0.1f);
		
		public Inventory(LocalPlayer Player)
		{
			//this.ItemsFBO = new FBO(Constants.WIDTH, Constants.HEIGHT);
			//this.MultisampleItemsFBO = new FBO(Constants.WIDTH, Constants.HEIGHT, true, 4);
			this.Player = Player;
			this.Items = new InventoryItem[24];
			this.ItemsButtons = new Button[24];
			this.ItemsText = new GUIText[24];
			this.InventoryPanel = new Panel();
			this.ItemInfoPanel = new Panel();
			byte[] sans = AssetManager.ReadBinary("Assets/Sans.ttf", AssetManager.DataFile3);
			this.Fonts.AddMemoryFont(Utils.IntPtrFromByteArray(sans), sans.Length);

			EquipmentTypes[0] = new []{ ItemType.MaxItems };
			EquipmentTypes[1] = new ItemType[]{ };
			EquipmentTypes[2] = new ItemType[]{ };
			EquipmentTypes[3] = new []{ ItemType.Coin };
			EquipmentTypes[4] = new []{ ItemType.Ring };
			EquipmentTypes[5] = new []{ ItemType.Mount };
			EquipmentTypes[6] = new []{ ItemType.Glider };
			EquipmentTypes[7] = new []{ ItemType.Food };
			
			Background = new Texture(Graphics2D.LoadFromAssets("Assets/UI/Inventory.png"), Vector2.Zero, Mathf.ScaleGUI( TargetResolution2, Vector2.One));
			Name = new GUIText("", Mathf.ScaleGUI( TargetResolution2, new Vector2(-0.135f,0.6f)), Color.White, FontCache.Get(Fonts.Families[0], 20));
			Level = new GUIText("", Mathf.ScaleGUI( TargetResolution2, new Vector2(-0.135f,0.525f)), Color.White, FontCache.Get(UserInterface.Fonts.Families[0], 12));
			PlayerTexture = new Texture(UserInterface.PlayerFbo.TextureID[0], Mathf.ScaleGUI( TargetResolution2, new Vector2(-.14f, .05f)), Mathf.ScaleGUI( TargetResolution2, new Vector2(0.1f, 0.133f) * 3.4f));
			PlayerTexture.TextureElement.Flipped = true;
            //PlayerTexture.TextureElement.FXAA = true;
			
			//ItemStats Panel
			ItemStats[0] = new Texture(Graphics2D.LoadFromAssets("Assets/UI/ItemInfo.png"), Mathf.ScaleGUI(TargetResolution2, Vector2.Zero), Mathf.ScaleGUI(TargetResolution2, Vector2.One));
			
			ItemTitle = new GUIText("", Mathf.ScaleGUI(TargetResolution2, new Vector2(-0.625f,-0.2605f) + ItemStatsOffset), Color.White, FontCache.Get(Fonts.Families[0], 13));
			ItemImage = new Texture(GUIRenderer.TransparentTexture, Mathf.ScaleGUI(TargetResolution2, new Vector2(-0.775f,0f) + ItemStatsOffset) ,Mathf.ScaleGUI(TargetResolution, new Vector2(0.0515f,0.07f)) * 3.3f);
			ItemDmg = new GUIText("DAMAGE: ", Mathf.ScaleGUI(TargetResolution2, new Vector2(-0.575f,0.25f) + ItemStatsOffset), Color.White, FontCache.Get(Fonts.Families[0], 11));
			ItemEffect = new GUIText("EFFECT: ", Mathf.ScaleGUI(TargetResolution2, new Vector2(-0.575f,-0.05f) + ItemStatsOffset), Color.White, FontCache.Get(Fonts.Families[0], 11));
			ItemSpeed = new GUIText("ATTACK SPEED: ", Mathf.ScaleGUI(TargetResolution2, new Vector2(-0.575f,0.10f) + ItemStatsOffset), Color.White, FontCache.Get(Fonts.Families[0], 11));
            ItemImage.TextureElement.Fxaa = true;


            this.ItemInfoPanel.AddElement(ItemImage);
			this.ItemInfoPanel.AddElement(ItemTitle);
			this.ItemInfoPanel.AddElement(ItemDmg);
			this.ItemInfoPanel.AddElement(ItemEffect);
			this.ItemInfoPanel.AddElement(ItemSpeed);
			
			this.InventoryPanel.AddElement(Background);
			this.InventoryPanel.AddElement(Name);
			this.InventoryPanel.AddElement(Level);
			this.InventoryPanel.AddElement(PlayerTexture);

			for(int i = 0; i < ItemStats.Length; i++){
				if(ItemStats[i] != null)
					this.ItemInfoPanel.AddElement(ItemStats[i]);
			}
		
			this.InventoryPanel.Disable();
			this.ItemInfoPanel.Disable();
			this.InventoryPanel.OnPanelStateChange += this.OnInventoryOpened;
			
			Vector2 Increase = Vector2.Zero;
			for(int i = 0; i < Items.Length; i++){
				if(i < InventorySpaces){
					#region InventoryButtons
					ItemsButtons[i] = new Button(Mathf.ScaleGUI(TargetResolution, new Vector2(0.142f,0.4f) + Increase), Mathf.ScaleGUI(TargetResolution, new Vector2(0.0515f,0.07f)) * 0.95f, GUIRenderer.TransparentTexture);
					ItemsButtons[i].PlaySound = false;
                    //ItemsButtons[i].Texture.FXAA = true;
                    Increase += new Vector2(0.0515f,0) * 2 + new Vector2(0.00825f, 0);
					//To lazy to use % just test all the cases
					if(i == 3 || i == 7 || i == 11 || i == 15){
						Increase -= new Vector2(0, 0.066f) * 2 + new Vector2(0, 0.0125f);
						Increase.X = 0;
					}
					ItemsText[i] = new GUIText("1", ItemsButtons[i].Position + new Vector2(ItemsButtons[i].Scale.X * .65f,-ItemsButtons[i].Scale.Y * .75f ),
			                                                   Color.White, FontCache.Get(Fonts.Families[0], 8));
					int I = i;
					ItemsButtons[i].Click += delegate(object sender, MouseButtonEventArgs e) {
						if(I == CoinHolder) return;
						if(e.Button == MouseButton.Right){
							this.Use(I);
						}else if(e.Button == MouseButton.Left){
							if(FollowingButton == ItemsButtons[I])
								return;
							
							if(FollowingButton == null && Items[I] != null){
								PrevCoords = ItemsButtons[I].Position;
								FollowingButton = ItemsButtons[I];
							}
							if(FollowingButton != null){
								
								if(Items[I] == null){
									//Restore the index of the item this button contains
									int j;
									for(j = 0; j <ItemsButtons.Length; j++){
										if(FollowingButton == ItemsButtons[j])
											break;
									}
									this.SetItem(Items[j], I);
									this.SetItem(null, j);
									FollowingButton = null;
									ItemsButtons[j].Position = PrevCoords;
								}else{
									int j;
									for(j = 0; j <ItemsButtons.Length; j++){
										if(FollowingButton == ItemsButtons[j])
											break;
									}
									this.FollowingButton.Position = ItemsButtons[j].Position;
									this.FollowingButton = ItemsButtons[j];
									InventoryItem I2 = Items[I];
									this.SetItem(Items[j], I);
									this.SetItem(I2, j);
								}
							}
						}
					};
					ItemsButtons[i].HoverEnter += delegate { 
						if(I == CoinHolder) return;
						if(Items[I] != null){
							this.SetItemStats(I);
						}
					};
					ItemsButtons[i].HoverExit += delegate {
						if(I == CoinHolder) return;						
						ItemTitle.Text = "";
						ItemImage.TextureElement.IdPointer = null;
						this.ItemInfoPanel.Disable();
					};
					this.InventoryPanel.AddElement(ItemsButtons[i]);
					this.InventoryPanel.AddElement(ItemsText[i]);
					#endregion
				}else{
					#region EquipmentButtons
					if(i == InventorySpaces)
						Increase = Vector2.Zero;
					ItemsButtons[i] = new Button(Mathf.ScaleGUI(TargetResolution, new Vector2(-0.34f,-0.33f) + Increase), Mathf.ScaleGUI(TargetResolution, new Vector2(0.0475f,0.062f)), GUIRenderer.TransparentTexture);
				    ItemsButtons[i].PlaySound = false;
                    //ItemsButtons[i].Texture.FXAA = true;
                    ItemsText[i] = new GUIText("1", ItemsButtons[i].Position + new Vector2(ItemsButtons[i].Scale.X * .65f,-ItemsButtons[i].Scale.Y * .75f ),
			                                                   Color.White, FontCache.Get(Fonts.Families[0], 8));
					
					Increase += new Vector2(0.0475f,0) * 2 + new Vector2(0.008f, 0);
					//To lazy to use % just test all the cases
					if(i == InventorySpaces+3){
						Increase -= new Vector2(0, 0.062f) * 2 + new Vector2(0, 0.01f);
						Increase.X = 0;
					}
					
					int I = i;
					ItemsButtons[i].Click += delegate(object sender, MouseButtonEventArgs e) {
						if(I == CoinHolder) return;
						if(e.Button == MouseButton.Right){
							this.Use(I);
						}else if(e.Button == MouseButton.Left){
							if(FollowingButton == null && Items[I] != null){
								PrevCoords = ItemsButtons[I].Position;
								FollowingButton = ItemsButtons[I];
								Sound.SoundManager.PlaySoundInPlayersLocation(Sound.SoundType.OnOff, 0.75f, 0.05f);
							}
							if(FollowingButton != null){
								if(Items[I] == null){
									//Restore the index of the item this button contains
									int j;
									for(j = 0; j <ItemsButtons.Length; j++){
										if(FollowingButton == ItemsButtons[j])
											break;
									}
									bool IsType = false;
									for(int l = 0; l < EquipmentTypes[I - InventorySpaces].Length; l++){
										if(Items[j] != null && EquipmentTypes[I - InventorySpaces][l] == Items[j].Type){
											IsType = true;
											break;
										}
									}
									if(Items[j] == null && !IsType){
										Sound.SoundManager.PlaySoundInPlayersLocation(Sound.SoundType.OnOff, -0.25f, 0.05f);
										return;
									}
									if(this.SetItem(Items[j], I))
										this.SetItem(null, j);
									else
										this.SetItem(Items[j], j);
									FollowingButton = null;
									ItemsButtons[j].Position = PrevCoords;
									Sound.SoundManager.PlaySoundInPlayersLocation(Sound.SoundType.OnOff, 0.75f, 0.05f);
								}else{
									int j;
									for(j = 0; j <ItemsButtons.Length; j++){
										if(FollowingButton == ItemsButtons[j])
											break;
									}
									bool IsType = false;
									for(int l = 0; l < EquipmentTypes[I - InventorySpaces].Length; l++){
										if(Items[j] != null && EquipmentTypes[I - InventorySpaces][l] == Items[j].Type){
											IsType = true;
											break;
										}
									}
									if(Items[j] != null && !IsType){
										Sound.SoundManager.PlaySoundInPlayersLocation(Sound.SoundType.OnOff, -0.25f, 0.05f);
										return;
									}
									InventoryItem OldItem = Items[I];
									if(this.SetItem(Items[j], I))
										this.SetItem(OldItem, j);
									else
										this.SetItem(Items[j], j);
									Sound.SoundManager.PlaySoundInPlayersLocation(Sound.SoundType.OnOff, 0.75f, 0.05f);	
								}
							}
						}
					};
					ItemsButtons[i].Texture.IsEnabled = true;
					ItemsButtons[i].HoverEnter += delegate {
						if(I == CoinHolder) return;						
						if(Items[I] != null){
							this.SetItemStats(I);
						}
					};
					ItemsButtons[i].HoverExit += delegate {
						if(I == CoinHolder) return;						
						ItemTitle.Text = "";
						ItemImage.TextureElement.IdPointer = null;
						this.ItemInfoPanel.Disable();
					};
					this.InventoryPanel.AddElement(ItemsButtons[i]);
					this.InventoryPanel.AddElement(ItemsText[i]);
				}
				#endregion
			}
		}
		
		//When right clicking
		public void Use(int I){
			if(Items[I] != null){
				int NewIndex = 0;
				if(I < InventorySpaces){
					if(EquipmentTypes[WeaponEquipmentType].Contains(Items[I].Type) )NewIndex = Inventory.WeaponHolder;
					if(Items[I].Type == ItemType.Ring)NewIndex = RingHolder;
					if(Items[I].Type == ItemType.Glider)NewIndex = GliderHolder;
					if(Items[I].Type == ItemType.Mount)NewIndex = MountHolder;
					if(Items[I].Type == ItemType.Food)NewIndex = FoodHolder;
					if(Items[I].Type == ItemType.Coin)NewIndex = CoinHolder;
				}else{
					NewIndex = -1;
				}
				
				if(Items[I].Type == ItemType.Mount)
					Player.IsRiding = false;
				
				if(NewIndex != -1){
					if(Items[I].Type == ItemType.Mount && Networking.NetworkManager.IsConnected)
						Player.MessageDispatcher.ShowNotification("MOUNTS ARE NOT SUPPORTED YET", Color.DarkRed, 3f);
					
					
					InventoryItem PrevItem = Items[NewIndex];
					this.SetItem(Items[I], NewIndex);
					this.SetItem(PrevItem, I);	
				}else{

					if(Items[I].Type == ItemType.Food){
						for(int i = 0; i < Items.Length; i++){
							if(Items[i] == null){
								this.SetItem(Items[I], i);
								break;
							}
						}
					}else{
						this.AddItem(Items[I]);
					}
					this.SetItem(null, I);
				}
			}
			this.UpdateInventory();
			this.ItemInfoPanel.Disable();
		}
		
		public void SetItemStats(int I){
			
			MaterialInfo MInfo;
			ItemSpeed.FontColor = Color.White;
			ItemEffect.FontColor = Color.White;
			ItemDmg.FontColor = Color.White;
			ItemImage.Position = Mathf.ScaleGUI(TargetResolution2, new Vector2(-0.775f,0.1f) + ItemStatsOffset);//Reset position.
			ItemEffect.Position = Mathf.ScaleGUI(TargetResolution2, new Vector2(-0.575f,-0.05f) + ItemStatsOffset);
			if(Items[I].Type == ItemType.Glider){
				
				ItemTitle.Text = Items[I].Name.ToUpperInvariant();
				ItemImage.TextureElement.IdPointer = ItemsButtons[I].Texture.IdPointer;
				ItemImage.Position = Mathf.ScaleGUI(TargetResolution2, new Vector2(-0.625f,0.15f) + ItemStatsOffset);
				ItemDmg.Text = "";
				ItemEffect.Text = "";
				ItemSpeed.Text = "";
				
				ItemInfoPanel.Enable();
				return;
			} else if(Items[I].Type == ItemType.Food){
				
				ItemTitle.Text = Items[I].Name.ToUpperInvariant();
				ItemImage.TextureElement.IdPointer = ItemsButtons[I].Texture.IdPointer;
				ItemImage.Position = Mathf.ScaleGUI(TargetResolution2, new Vector2(-0.75f,0.1f) + ItemStatsOffset);//Reset position.
				ItemDmg.Text = "SATURATION: "+Item.ItemPool.MaterialInfo(Items[I].Info.MaterialType).AttackPower;
				ItemSpeed.Text = "AMOUNT: "+( (int)Items[I].Info.Damage );
				ItemEffect.Text = "";
				
				ItemInfoPanel.Enable();
				return;
			}else if(Items[I].Type == ItemType.Stackable){
				
				ItemTitle.Text = Items[I].Name.ToUpperInvariant();
                ItemImage.TextureElement.IdPointer = ItemsButtons[I].Texture.IdPointer;
				ItemImage.Position = Mathf.ScaleGUI(TargetResolution2, new Vector2(-0.65f,0.15f) + ItemStatsOffset);
				ItemDmg.Text = "";
				ItemEffect.Position = Mathf.ScaleGUI(TargetResolution2, new Vector2(-0.65f,-0.15f) + ItemStatsOffset);
				ItemEffect.Text = "AMOUNT: "+( (int)Items[I].Info.Damage );
				ItemSpeed.Text = "";
				
				ItemInfoPanel.Enable();
				ItemEffect.Position = Mathf.ScaleGUI(TargetResolution2, new Vector2(-0.65f,-0.15f) + ItemStatsOffset);
				return;
			} else if(Items[I].Type == ItemType.Ring){
				
				ItemImage.Position = Mathf.ScaleGUI(TargetResolution2, new Vector2(-0.775f,0.1f) + ItemStatsOffset);//Reset position.
				ItemTitle.Text = Items[I].Name.ToUpperInvariant();
				ItemImage.TextureElement.IdPointer = ItemsButtons[I].Texture.IdPointer;
				MInfo = Item.ItemPool.MaterialInfo(Items[I].Info.MaterialType);
				ItemDmg.FontColor = (MInfo.MovementSpeed < 0) ? Color.Red : Color.ForestGreen;
				ItemDmg.Text = "SPEED: "+((MInfo.MovementSpeed < 0) ? "" : "+")+(int)(MInfo.MovementSpeed*100)+"%";
				ItemEffect.Text ="";
				ItemSpeed.Text = "EFFECT: "+MInfo.Effect.ToString().ToUpperInvariant();
				ItemInfoPanel.Enable();
				return;
			} else if(Items[I].Type == ItemType.Mount){
				
				ItemTitle.Text = Items[I].Name.ToUpperInvariant();
                ItemImage.TextureElement.IdPointer = ItemsButtons[I].Texture.IdPointer;
				ItemImage.Position = Mathf.ScaleGUI(TargetResolution2, new Vector2(-0.625f,0.15f) + ItemStatsOffset);
				ItemDmg.Text = "";
				ItemEffect.Position = Mathf.ScaleGUI(TargetResolution2, new Vector2(-0.625f,-0.15f) + ItemStatsOffset);
				ItemEffect.Text = "RIGHT-CLICK TO EQUIP";
				ItemSpeed.Text = "";
				
				ItemInfoPanel.Enable();
				ItemEffect.Position = Mathf.ScaleGUI(TargetResolution2, new Vector2(-0.625f,-0.15f) + ItemStatsOffset);
				ItemInfoPanel.Enable();
				return;
			} else if(Items[I].Type == ItemType.Coin){
				
				ItemTitle.Text = "";
				ItemImage.TextureElement.IdPointer = delegate{ return 0; };
				ItemImage.Position = Mathf.ScaleGUI(TargetResolution2, new Vector2(-0.625f,0.15f) + ItemStatsOffset);
				ItemDmg.Text = "";
				ItemEffect.Position = Mathf.ScaleGUI(TargetResolution2, new Vector2(-0.625f,-0.15f) + ItemStatsOffset);
				ItemEffect.Text = "";
				ItemSpeed.Text = "";
				
				ItemInfoPanel.Enable();
				ItemEffect.Position = Mathf.ScaleGUI(TargetResolution2, new Vector2(-0.625f,-0.15f) + ItemStatsOffset);
				ItemInfoPanel.Enable();
				return;
			}
			
			MInfo = Item.ItemPool.MaterialInfo(Items[I].Info.MaterialType);
			
			ItemTitle.Text = ""+Items[I].Name.ToUpperInvariant();
			ItemImage.TextureElement.IdPointer = ItemsButtons[I].Texture.IdPointer;
			ItemDmg.Text = "DAMAGE: "+( (int)Items[I].Info.Damage + MInfo.AttackPower ).ToString();
			ItemEffect.FontColor = (Items[I].Info.CritMultiplier < 0) ? Color.Red : Color.ForestGreen;
			ItemEffect.Text = "CRITICAL HIT: " + ((Items[I].Info.CritMultiplier < 0) ? "" : "+" )+ (int) (Items[I].Info.CritMultiplier*100*.5f)+"%";
			
			ItemSpeed.FontColor = (MInfo.AttackSpeed-100 < 0) ? Color.Red : Color.ForestGreen;
			ItemSpeed.Text = "ATTACK SPEED: "+ ((MInfo.AttackSpeed-100 > 0) ? "+" : "")+( (int) MInfo.AttackSpeed-100).ToString().ToUpperInvariant()+ "%";
			//MInfo.
			ItemInfoPanel.Enable();
		}
		
		public ItemInfo GetWeaponInfo()
		{
		    return Items[28]?.Info ?? new ItemInfo(Material.None, 8);
		}
		
		private EntityMesh[] Models;
		private float ItemRotation = 0;
		public void Draw(int ID, EntityMesh[] Models){
			if(Models[ID] == null)
				return;
			
			Models[ID].AnimationRotation = new Vector3(0,ItemRotation,0);
			ItemRotation += (25 * (float) Time.deltaTime) / ItemCount;

			int PrevFBO = GraphicsLayer.FboBound;
			int PrevShader = GraphicsLayer.ShaderBound;
			UserInterface.InventoryFbo.Bind();
			
	       	Matrix4 ProjectionMatrix = Matrix4.CreatePerspectiveFieldOfView(50 * Mathf.Radian, 1.33f, 1, Constants.VIEW_DISTANCE);
			GL.MatrixMode(MatrixMode.Projection);
			GL.LoadMatrix(ref ProjectionMatrix);
			
			Matrix4 LookAt = Matrix4.LookAt(Vector3.UnitZ * 3.25f, Vector3.UnitY * .75f, Vector3.UnitY);
			GL.MatrixMode(MatrixMode.Modelview);
			GL.LoadMatrix(ref LookAt);

			GL.Clear(ClearBufferMask.ColorBufferBit);
			GL.Enable(EnableCap.Blend);
			GL.Enable(EnableCap.DepthTest);

			Models[ID].Draw();
			
			/*GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, ItemsFBO.BufferID);
			GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, MultisampleItemsFBO.BufferID);
			GL.BlitFramebuffer(0,Constants.HEIGHT,Constants.WIDTH,0, 0,Constants.HEIGHT,Constants.WIDTH,0
			                   , ClearBufferMask.ColorBufferBit, BlitFramebufferFilter.Nearest);
			
			GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, 0);
			GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, 0);*/
			
			GL.BindFramebuffer(FramebufferTarget.Framebuffer, PrevFBO);
			GraphicsLayer.FboBound = PrevFBO;
			GL.UseProgram(PrevShader);
			GraphicsLayer.ShaderBound = PrevShader;
			GL.Enable(EnableCap.Blend);
			GL.Disable(EnableCap.DepthTest);

		}
		
		public int ItemCount{
			get{
				var k = 0;
				for(int i = 0; i < Items.Length; i++){
					if(Items[i] != null) k++;
				}
				return k;
			}
		}
		
		private Button FollowingButton;
		private Vector2 PrevCoords;
		public override void OnMouseMove(object sender, MouseMoveEventArgs e){
			if(FollowingButton != null){
				Vector2 Coords = Mathf.ToNormalizedDeviceCoordinates(e.Mouse.X, Constants.HEIGHT-e.Mouse.Y);
				FollowingButton.Position = Coords;
			}
		}
		
		public override void OnMouseButtonDown(object sender, MouseButtonEventArgs e)
		{
			if(!Show)return;
			
			Vector2 Scale = Mathf.ScaleGUI(TargetResolution, new Vector2(0.5f, 0.5f));
			Vector2 Position = Vector2.Zero;
			Vector2 Coords = Mathf.ToNormalizedDeviceCoordinates(e.Mouse.X, e.Mouse.Y);
			
			if( !(Position.Y + Scale.Y > -Coords.Y && Position.Y - Scale.Y < -Coords.Y 
			    && Position.X + Scale.X > Coords.X && Position.X - Scale.X < Coords.X) ){
				
				if(FollowingButton != null){
					int j;
					for(j = 0; j <ItemsButtons.Length; j++){
						if(FollowingButton == ItemsButtons[j])
							break;
					}
					InventoryItem Item = Items[j];
					this.SetItem(null, j);
					ItemsButtons[j].Position = PrevCoords;
					
					if(Item != null){
						if(Networking.NetworkManager.IsConnected && !Networking.NetworkManager.IsHost)
							Networking.NetworkManager.DropItem(Item, Player.Position + Vector3.UnitY * 2f + Player.Orientation * 3f);
						else	
							World.DropItem(Item, Player.Position + Vector3.UnitY * 2f + Player.Orientation * 3f);
					}
					ItemInfoPanel.Disable();
				}
			}
		}
		
		private InventoryItem PreviousRing;
		private EntityComponent LastEffect;
		public void UpdateInventory(){
			if(Show){
			
				for(int i = 0; i < Items.Length; i++){
					if(Items[i] != null && (Items[i].Type == ItemType.Food || Items[i].Type == ItemType.Stackable || Items[i].Type == ItemType.Coin) ){
						if(InventoryPanel.Enabled)
							ItemsText[i].Enable();
						ItemsText[i].Text = ( (int) Items[i].Info.Damage).ToString();
					}else{
						ItemsText[i].Disable();
						ItemsText[i].Text = "";
					}
				}
			}
			//Set the ring effects
			if(Items[RingHolder] != PreviousRing){
				//If it wasnt null removed previous stats
				if(PreviousRing != null){
					
					if(LastEffect != null){
						this.Player.RemoveComponent(LastEffect);
						LastEffect.Dispose();
					}
					this.Player.Speed -= ItemPool.MaterialInfo(PreviousRing.Info.MaterialType).MovementSpeed;
				}
				//If it isnt null add current stats
				//Add Slow effect 
				//Add speed effect 
				//Add bleed effect 
				//Add freeze effect
				PreviousRing = Items[RingHolder];
				if(PreviousRing != null){
					MaterialInfo MInfo = ItemPool.MaterialInfo(PreviousRing.Info.MaterialType);
					
					this.Player.Speed += MInfo.MovementSpeed;
					if(MInfo.Effect == EffectType.Fire)
						LastEffect = new FireComponent(this.Player);
					
					else if(MInfo.Effect == EffectType.Poison)
						LastEffect = new PoisonousComponent(this.Player);
					
					else if(MInfo.Effect == EffectType.Slow)
						LastEffect = new SlowComponent(this.Player);
					
					else if(MInfo.Effect == EffectType.Bleed)
						LastEffect = new BleedComponent(this.Player);
					
					else if(MInfo.Effect == EffectType.Freeze)
						LastEffect = new FreezeComponent(this.Player);
					
					if(LastEffect != null)
						this.Player.AddComponent( LastEffect );
					
				}
			}
		}
		
		public bool AddItem(InventoryItem Item){
			if(ItemCount == Items.Length){
				Player.MessageDispatcher.ShowNotification("YOU CAN'T CARRY MORE ITEMS", Color.DarkRed, 3f, true);
			}
			if(Item.Type == ItemType.Food){
				for(int i = 0; i < Items.Length; i++){
					if(Items[i] != null){
						if(Items[i].Info.MaterialType == Item.Info.MaterialType){
							Items[i].Info = new ItemInfo(Items[i].Info.MaterialType, Items[i].Info.Damage+1);
							this.UpdateInventory();
							return true;
						}
					}
				}
				this.SetItem(Item, Inventory.FoodHolder);
				return true;	
			}
			
			if(Item.Type == ItemType.Coin){
				for(int i = 0; i < Items.Length; i++){
					if(Items[i] != null){
						if(Items[i].Type == Item.Type){
							Items[i].Info = new ItemInfo(Items[i].Info.MaterialType, Items[i].Info.Damage+Item.Info.Damage);
							this.UpdateInventory();
							return true;
						}
					}
				}
				this.SetItem(Item, Inventory.FoodHolder);
				return true;	
			}
			
			if(Item.Type == ItemType.Stackable){
				for(int i = 0; i < Items.Length; i++){
					if(Items[i] != null){
						if(Items[i].Info.MaterialType == Item.Info.MaterialType){
							Items[i].Info = new ItemInfo(Items[i].Info.MaterialType, Items[i].Info.Damage+1);
							this.UpdateInventory();
							return true;
						}
					}
				}
			}
			
			for(int i = 0; i < Items.Length; i++){
				if(Items[i] == null){
					this.SetItem(Item, i);
					return true;
				}
			}
			return false;
		}
		public bool SetItem(InventoryItem Item, int Index){
			return SetItem(Item, Index, false);
		}
		public bool SetItem(InventoryItem Item, int Index, bool BulkSet){
			#region Redirect old spaces
			if(Index == 28) 
				Index = WeaponHolder;
			
			if(Index == 27) 
				Index = FoodHolder;
			
			if(Index == 25) 
				Index = MountHolder;
			
			if(Index == 30) 
				Index = GliderHolder;
			
			if(Index == 29) 
				Index = RingHolder;
			
			if(Index >= Items.Length) 
				Index = Index-26+InventorySpaces;
			#endregion
			
			if(!BulkSet){
				if(Item != null && Item.Type != ItemType.Ring && Index == Inventory.RingHolder) return false;
				if(Item != null && !EquipmentTypes[WeaponEquipmentType].Contains(Item.Type) && Index == Inventory.WeaponHolder) return false;
				if(Item != null && Item.Type != ItemType.Food && Index == Inventory.FoodHolder) return false;
				if(Item != null && Item.Type != ItemType.Glider && Index == Inventory.GliderHolder) return false;
			}
			Items[Index] = Item;
			ItemsButtons[Index].Texture.TextureId = GUIRenderer.TransparentTexture;

			EntityMesh[] ModelsL = new EntityMesh[31];
			for(int i = 0; i < ItemsButtons.Length; i++){
				ItemsButtons[i].Texture.IdPointer = null;
				ItemsButtons[i].Texture.TextureId = GUIRenderer.TransparentTexture;
			}
			for(int i = 0; i < Items.Length; i++){
				if(Items[i] != null){
					ModelsL[i] = Items[i].GetMesh(Vector3.One);
					ModelsL[i].UseFog = false;
				}
			}
			for(int i = 0; i < Items.Length; i++){
				if(Items[i] != null){
					int k = i;
					ItemsButtons[i].Texture.IdPointer = delegate(){this.Draw(k, Models); return UserInterface.InventoryFbo.TextureID[0];};
				}
			}
			
			for(int i = 0; i < ModelsL.Length; i++){
				if(ModelsL[i] != null){
					ModelsL[i].Position = Vector3.Zero;
					DrawManager.Remove(ModelsL[i]);
				}
			}
			Models = ModelsL;
			if(MainWeapon != null){
				//Force cache update
				//Items[28].Info = Items[28].Info;
				Player.Model.SetWeapon( MainWeapon.Weapon);
			}else{
				Player.Model.SetWeapon( Weapon.Empty );
			}
			this.UpdateInventory();
			return true;
		}
		
		public int Gold{
			get{ 
				if(Items[CoinHolder] == null){
					Items[CoinHolder] = new InventoryItem(ItemType.Coin, ItemInfo.Gold(0));
					return 0;
				}
				return (int) Items[CoinHolder].Info.Damage;
			}
		}
		
		public InventoryItem MainWeapon{
			get{ return Items[WeaponHolder]; }
		}
		public InventoryItem Glider{
			get{ return Items[GliderHolder]; }
		}
		public InventoryItem Ring{
			get{ return Items[RingHolder]; }
		}
		
		
		
		public void SetItems(Dictionary<InventoryItem, int> ItemData){
			if(ItemData == null)
				return;
			
			for(int i = 0; i < this.Items.Length; i++){
				this.Items[i] = null;
			}
			
			foreach(KeyValuePair<InventoryItem, int> Entry in ItemData){
				this.SetItem(Entry.Key, Entry.Value, true);
			}
			
			if(Items[WeaponHolder] == null)
				Player.Model.SetWeapon( Weapon.Empty);
		}
		
		public void OnInventoryOpened(object sender, PanelState State){
			if(State == PanelState.Enabled){
				UpdateManager.CursorShown = true;
				Player.View.LockMouse = false;
				Player.Movement.Check = false;
				Player.View.Check = false;
			}else{
				if(Constants.CHARACTER_CHOOSED)
					UpdateManager.CursorShown = false;
				Player.View.LockMouse = true;
				Player.Movement.Check = true;
				Player.View.Check = true;
				ItemInfoPanel.Disable();
			}
		}
		
		public void Resize(){
			//this.ItemsFBO = this.ItemsFBO.Resize();
			//this.MultisampleItemsFBO = this.MultisampleItemsFBO.Resize();
		}
		
		private bool m_Show = false;
		public bool Show{
			get{ return m_Show; }
			set{
				if(Player.SkillSystem.Show || Scenes.SceneManager.Game.IsLoading || value == m_Show || Player.Trade.Show)
					return;				
				m_Show = value;
				if(m_Show){
					InventoryPanel.Enable();
					this.UpdateInventory();
					Sound.SoundManager.PlaySoundInPlayersLocation(Sound.SoundType.OnOff, 1, 0.25f);
				}else{
					this.ItemInfoPanel.Disable();
					InventoryPanel.Disable();
					this.UpdateInventory();
					Sound.SoundManager.PlaySoundInPlayersLocation(Sound.SoundType.OnOff, 1, 0.25f);
				}
			}
		}
	}
}
