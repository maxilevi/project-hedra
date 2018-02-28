/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 15/08/2017
 * Time: 08:51 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using Hedra.Engine.Management;
using OpenTK;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.UI;
using Hedra.Engine.Item;
using System.Drawing;

namespace Hedra.Engine.Player
{
	/// <summary>
	/// Description of TradeSystem.
	/// </summary>
	public class TradeSystem
	{
		public const int MaxItems = 24;
		public LocalPlayer Player;
		public Panel TradePanel;
		public InventoryItem[] MerchantItems = new InventoryItem[MaxItems];
		private Vector2 TargetResolution2 = new Vector2(1366,768), TargetResolution = new Vector2(800,600), TargetResolution3 = new Vector2(1024,578);
		private Button[] MerchantButtons = new Button[MaxItems], InventoryButtons = new Button[16];
		private EntityMesh[] ModelsL;
		private Texture Tooltip;
		private GUIText TooltipText, TooltipPrice, CoinCount, TooltipHint;
		private bool TooltipEnabled = false, HoverSell;
		private InventoryItem HoverItem;
	    private Dictionary<InventoryItem, int> ItemsReference;
		
		public TradeSystem(LocalPlayer Player){
			this.Player = Player;
			this.TradePanel = new Panel();
			Texture TradeHUD = new Texture(Graphics2D.LoadFromAssets("Assets/UI/Trade.png"), Vector2.Zero, Mathf.ScaleGUI(TargetResolution2, Vector2.One * .8f));
			//MerchantButtons
			Vector2 Increase = Vector2.Zero;
			#region Merchant buttons
			for(int i = 0; i < MerchantButtons.Length; i++){
				MerchantButtons[i] = new Button(Mathf.ScaleGUI(TargetResolution, new Vector2(-0.525f,0.415f) + Increase), Mathf.ScaleGUI(TargetResolution, new Vector2(0.0515f,0.07f)) * 0.95f, GUIRenderer.TransparentTexture);
				MerchantButtons[i].PlaySound = false;
				Increase += new Vector2(0.0535f,0) * 2 + new Vector2(0.00825f, 0);
				//To lazy to use % just test all the cases
				if(i == 3 || i == 7 || i == 11 || i == 15 || i==19){
					Increase -= new Vector2(0, 0.066f) * 2 + new Vector2(0, 0.0125f) * 1.65f;
					Increase.X = 0;
				}
				int k = i;
				MerchantButtons[i].HoverEnter += delegate {
					if(MerchantItems[k] != null){
						TooltipEnabled = true;
						HoverItem = MerchantItems[k];
						HoverSell = true;
					}
				};
				MerchantButtons[i].HoverExit += delegate { 
					TooltipEnabled = false;
					HoverItem = null;
				};
				
				MerchantButtons[i].Click += delegate { 
					if( Player.Inventory.Gold >= PriceFormula(MerchantItems[k], true)){
						this.Player.Inventory.Items[Inventory.CoinHolder].Info = ItemInfo.Gold(Player.Inventory.Gold - PriceFormula(MerchantItems[k], true));
						this.Player.Inventory.AddItem(MerchantItems[k]);
						//this.SetItem(k, null);
						this.RefreshInventory();
						this.UpdateMerchandise();
						Sound.SoundManager.PlaySoundInPlayersLocation(Sound.SoundType.TransactionSound, 1f, 1f);
					}else{
						Sound.SoundManager.PlaySoundInPlayersLocation(Sound.SoundType.NotificationSound, 1f, 1f);
					}
				};
				this.TradePanel.AddElement(MerchantButtons[i]);
			}
			#endregion
			
			Increase = Vector2.Zero;
			#region InventoryButtons
			for(int i = 0; i < InventoryButtons.Length; i++){
				InventoryButtons[i] = new Button(Mathf.ScaleGUI(TargetResolution, new Vector2(0.24f,0.3f) + Increase), Mathf.ScaleGUI(TargetResolution, new Vector2(0.0515f,0.07f)) * 0.95f, GUIRenderer.TransparentTexture);
				InventoryButtons[i].PlaySound = false;
				Increase += new Vector2(0.0535f,0) * 2 + new Vector2(0.00825f, 0);
				//To lazy to use % just test all the cases
				if(i == 3 || i == 7 || i == 11 || i == 15 || i==19){
					if(i == 15)
						Increase -= new Vector2(0, 0.05f) * 1f;	
					Increase -= new Vector2(0, 0.066f) * 2 + new Vector2(0, 0.0125f) * 1.65f;
					Increase.X = 0;
				}
				int k = i;
				InventoryButtons[i].HoverEnter += delegate {
					if(InventoryButtons[k] != null){
						TooltipEnabled = true;
						HoverItem = Player.Inventory.Items[k];
						HoverSell = false;
					}
				};
				InventoryButtons[i].HoverExit += delegate { 
					TooltipEnabled = false;
					HoverItem = null;
				};
				InventoryButtons[i].Click += delegate {
                    if(this.Player.Inventory.Items[k] == null) return;

					this.Player.Inventory.AddItem( new InventoryItem(ItemType.Coin, ItemInfo.Gold(PriceFormula(this.Player.Inventory.Items[k], false))) );
					this.AddItem(this.Player.Inventory.Items[k]);
					this.Player.Inventory.SetItem(null, k);
					this.RefreshInventory();
					this.UpdateMerchandise();
					Sound.SoundManager.PlaySoundInPlayersLocation(Sound.SoundType.TransactionSound, 1f, 1f);
					
					
				};
				this.TradePanel.AddElement(InventoryButtons[i]);
			}
			#endregion
			
			Tooltip = new Texture(Graphics2D.LoadFromAssets("Assets/UI/Tooltip.png"), Vector2.Zero, Mathf.ScaleGUI(TargetResolution, new Vector2(0.225f, 0.2f)) );
			TooltipText = new GUIText("", Vector2.Zero, Color.White, FontCache.Get(AssetManager.Fonts.Families[0],13, FontStyle.Bold));
			TooltipPrice = new GUIText("", Vector2.Zero, Color.White, FontCache.Get(AssetManager.Fonts.Families[0],12, FontStyle.Bold));
			CoinCount = new GUIText("", Mathf.ScaleGUI(TargetResolution3, new Vector2(0.330f,-0.325f)), Color.White, FontCache.Get(AssetManager.Fonts.Families[0],13, FontStyle.Bold));
			TooltipHint = new GUIText("CLICK TO BUY", Vector2.Zero, Color.White, FontCache.Get(AssetManager.Fonts.Families[0],10, FontStyle.Bold));
			
			
			this.TradePanel.AddElement(CoinCount);
			this.TradePanel.AddElement(TooltipText);
			this.TradePanel.AddElement(TradeHUD);
			this.TradePanel.AddElement(Tooltip);
			this.TradePanel.Disable();
			this.TradePanel.OnPanelStateChange += OnInventoryOpened;
		}
		
		public void Update(){
			if(TooltipEnabled && this.TradePanel.Enabled){
				
				Tooltip.TextureElement.Position = Mathf.ToNormalizedDeviceCoordinates(new Vector2( System.Windows.Forms.Cursor.Position.X, Constants.HEIGHT-System.Windows.Forms.Cursor.Position.Y));
				Tooltip.TextureElement.Position -= Mathf.ScaleGUI(TargetResolution3, Tooltip.TextureElement.Scale.X * Vector2.UnitX * 1.25f);
				Tooltip.TextureElement.Position += Mathf.ScaleGUI(TargetResolution3, Tooltip.TextureElement.Scale.Y * Vector2.UnitY * 1.00f);
				//Tooltip.TextureElement.Position = Tooltip.TextureElement.Position;
				
				if(HoverItem != null){
					Tooltip.Enable();
					TooltipText.Enable();
					TooltipPrice.Enable();
					TooltipHint.Enable();
				
					TooltipPrice.FontColor = (Player.Inventory.Gold >= PriceFormula(HoverItem, true) || !HoverSell) ? Color.DarkGreen : Color.Red;
					
					TooltipText.Text = ((ItemType.Stackable == HoverItem.Type) ? HoverItem.Info.Damage+" " : "") +HoverItem.Name.Replace("Mount","");
					TooltipPrice.Text = PriceFormula(HoverItem, HoverSell)+" Gold";
					TooltipHint.Text = (!HoverSell) ? "CLICK TO SELL" : "CLICK TO BUY";
					
					TooltipText.UiText.Position = Tooltip.TextureElement.Position + Vector2.UnitY * .04f;
					TooltipPrice.UiText.Position = TooltipText.UiText.Position - TooltipText.UiText.Scale.Y * Vector2.UnitY * 2.5f;
					TooltipHint.UiText.Position = TooltipText.UiText.Position - TooltipText.UiText.Scale.Y * Vector2.UnitY * 4.5f;
				}
			}else{
				Tooltip.Disable();
				TooltipText.Disable();
				TooltipPrice.Disable();
				TooltipHint.Disable();
			}
			if(this.TradePanel.Enabled){
				CoinCount.Text = Player.Inventory.Gold+ " GOLD";
				
			}
			
		}
		
		public void UpdateMerchandise(){

			ModelsL = new EntityMesh[24];
			for(int i = 0; i < MerchantButtons.Length; i++){
				MerchantButtons[i].Texture.IdPointer = null;
				MerchantButtons[i].Texture.TextureId = GUIRenderer.TransparentTexture;
			}
			for(int i = 0; i < MerchantItems.Length; i++){
				if(MerchantItems[i] != null){
					ModelsL[i] = MerchantItems[i].GetMesh(Vector3.One);
					ModelsL[i].UseFog = false;
				}
			}
			for(int i = 0; i < MerchantItems.Length; i++){
				if(MerchantItems[i] != null){
					int k = i;
					MerchantButtons[i].Texture.IdPointer = delegate{Player.Inventory.Draw(k, ModelsL); return UserInterface.InventoryFbo.TextureID[0];};
				}
			}
			
			for(int i = 0; i < ModelsL.Length; i++){
				if(ModelsL[i] != null){
					ModelsL[i].Position = Vector3.Zero;
					DrawManager.Remove(ModelsL[i]);
				}
			}
		}
		
		public void RefreshInventory(){
			EntityMesh[] ModelsL = new EntityMesh[16];
			for(int i = 0; i < InventoryButtons.Length; i++){
				InventoryButtons[i].Texture.IdPointer = null;
				InventoryButtons[i].Texture.TextureId = GUIRenderer.TransparentTexture;
			}
			for(int i = 0; i < InventoryButtons.Length; i++){
				if(Player.Inventory.Items[i] != null){
					ModelsL[i] = Player.Inventory.Items[i].GetMesh(Vector3.One);
					ModelsL[i].UseFog = false;
				}
			}
			for(int i = 0; i < InventoryButtons.Length; i++){
				if(Player.Inventory.Items[i] != null){
					int k = i;
					InventoryButtons[i].Texture.IdPointer = delegate(){Player.Inventory.Draw(k, ModelsL); return UserInterface.InventoryFbo.TextureID[0];};
				}
			}
			
			for(int i = 0; i < ModelsL.Length; i++){
				if(ModelsL[i] != null){
					ModelsL[i].Position = Vector3.Zero;
					DrawManager.Remove(ModelsL[i]);
				}
			}
		}
		
		public void AddItem(InventoryItem Item)
		{
		    int i = 0;
            for (i = 0; i < MerchantItems.Length; i++){
				if(MerchantItems[i] == null){
					MerchantItems[i] = Item;
					break;
				}
			}
		    ItemsReference.Add(Item, i);

        }
		
		public void SetItem(int Index, InventoryItem Item){
			MerchantItems[Index] = Item;
			this.UpdateMerchandise();
		}
		
		public void SetMerchantItems(Dictionary<InventoryItem, int> Items)
		{
		    this.ItemsReference = Items;
            this.MerchantItems = new InventoryItem[MaxItems];
			foreach(KeyValuePair<InventoryItem, int> Pair in Items){
				this.SetItem(Pair.Value, Pair.Key);
			}
		}
		
		public int PriceFormula(InventoryItem Item, bool Buy)
		{

			float Price = 0f;
			float PriceMultiplier = !Buy ? 1.0f : 1.75f;
			
			if(Item.Type == ItemType.Food || Item.Type == ItemType.Stackable)
				return (int) Math.Floor(1 * Item.Info.Damage * PriceMultiplier);
			
			if(Item.Type == ItemType.Hammer)
				Price += 8f * PriceMultiplier;
			
			else if(Item.Type == ItemType.Knife)
				Price += 2f * PriceMultiplier;
			
			else if(Item.Type == ItemType.Sword)
				Price += 4f * PriceMultiplier;
			
			else if(Item.Type == ItemType.Claw)
				Price += 6f * PriceMultiplier;
			
			else if(Item.Type == ItemType.Katar)
				Price += 4f * PriceMultiplier;
			
			else if(Item.Type == ItemType.Bow)
				Price += 2f * PriceMultiplier;
			
			else if(Item.Type == ItemType.Axe)
				Price += 6f * PriceMultiplier;
			
			else if(Item.Type == ItemType.DoubleBlades)
				Price += 4f * PriceMultiplier;
			
			else if(Item.Type == ItemType.Glider)
				Price += 180f * PriceMultiplier;
			
			else if(Item.Type == ItemType.Mount)
				Price += 80f * PriceMultiplier;
			
			else if(Item.Type == ItemType.Ring)
				Price += 6f * PriceMultiplier;
			
			else if(Item.Type == ItemType.Stackable)
				Price += 1f * PriceMultiplier * Item.Info.Damage;
			
			if(Item.Type == ItemType.Hammer || Item.Type == ItemType.Ring || Item.Type == ItemType.Axe || Item.Type == ItemType.Bow ||
			   Item.Type == ItemType.Katar || Item.Type == ItemType.Claw || Item.Type == ItemType.Sword || Item.Type == ItemType.Knife || Item.Type == ItemType.DoubleBlades){
				
				Price += Item.Info.Damage * .25f * PriceMultiplier;
				Price += Item.Info.CritMultiplier * 4.0f * PriceMultiplier;
				
				if(Item.Info.MaterialType == Material.Fluorite)
					Price += 8f * PriceMultiplier;
				else if(Item.Info.MaterialType == Material.Arsenic)
					Price += 8f * PriceMultiplier;
				else if(Item.Info.MaterialType == Material.Quartz)
					Price += 4f * PriceMultiplier;
				else if(Item.Info.MaterialType == Material.Calcite)
					Price += 6f * PriceMultiplier;
				else if(Item.Info.MaterialType == Material.Copper)
					Price += 2f * PriceMultiplier;
				else if(Item.Info.MaterialType == Material.Sapphire)
					Price += 6f * PriceMultiplier;
				else if(Item.Info.MaterialType == Material.Andesine)
					Price += 8f * PriceMultiplier;
				else if(Item.Info.MaterialType == Material.Beryl)
					Price += 6f * PriceMultiplier;
				else if(Item.Info.MaterialType == Material.Amber)
					Price += 8f * PriceMultiplier;
				else if(Item.Info.MaterialType == Material.Gold)
					Price += 10f * PriceMultiplier;
				else if(Item.Info.MaterialType == Material.Iron)
					Price += 8f * PriceMultiplier;
				else
					Log.WriteLine("Material not found" + Item.Info.MaterialType);
				
			}else if(Item.Type == ItemType.Mount){
				
				if(Item.Info.MaterialType == Material.WolfMount)
					Price += 40f * PriceMultiplier;
				else if(Item.Info.MaterialType == Material.HorseMount)
					Price += 20f * PriceMultiplier;
				
			}
			if(Price == 0){
				Log.WriteLine("Item "+Item.Type+" not found on market");	
				return 1;
			}
			return (int) Math.Floor(Price);
		}
		
		public void OnInventoryOpened(object sender, PanelState State){
			if(State == PanelState.Enabled){
				UpdateManager.CursorShown = true;
				Player.CanInteract = false;
				Player.View.LockMouse = false;
				Player.Movement.Check = false;
				Player.View.Check = false;
				this.RefreshInventory();
			}else{
				if(Constants.CHARACTER_CHOOSED)
					UpdateManager.CursorShown = false;
				Player.View.LockMouse = true;
				Player.Movement.Check = true;
				Player.CanInteract = true;
				Player.View.Check = true;
			}
		}
		
		
		private bool _show = false;
		public bool Show{
			get{ return _show; }
			set{
				if(Player.Inventory.Show || Scenes.SceneManager.Game.IsLoading || _show == value)
					return;
				
				if(value)
					TradePanel.Enable();
				else
					TradePanel.Disable();
				
				_show = value;
				Sound.SoundManager.PlaySoundInPlayersLocation(Sound.SoundType.OnOff, 1.0f, 0.6f);
			}
		}	
	}
}
