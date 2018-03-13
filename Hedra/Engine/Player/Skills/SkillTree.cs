/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 05/08/2016
 * Time: 07:58 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
 using System;
 using System.Text;
 using Hedra.Engine.Rendering.UI;
 using Hedra.Engine.Rendering;
 using Hedra.Engine.Rendering.Effects;
 using System.Drawing;
 using Hedra.Engine.Management;
 using OpenTK;
 using System.Collections.Generic;
 using System.IO;
 using System.Runtime.Serialization.Formatters.Binary;
 using OpenTK.Graphics.OpenGL;

namespace Hedra.Engine.Player
{
	/// <summary>
	/// Description of SkillSystem.
	/// </summary>
	public class SkillTree : IRenderable
	{
		public Vector4 LinesColor = new Vector4(.6f,.6f,.6f,1f);
		public Vector4 ActiveColor = new Vector4(1,0,0,1);
		public Vector2 BagPosition, BagScale;
		
		private Vector2 TargetResolution = new Vector2(1366,768);
		private LocalPlayer Player;
		private Panel SkillsPanel, SkillsInfoPanel;
		private TreeSlot[][] Slots = new TreeSlot[3][];
		private SkillTreeType Type = SkillTreeType.FIRETREE;
		private GUIText PointsText;
		private Texture SkillInfoBck;
		private Vector2 SkillSize;
		private GUIText SkillInfoManaCost, SkillInfoCooldown, SkillInfoDesc, SkillInfoName;
		private Texture SkillInfoTexture;
		
		public SkillTree(LocalPlayer Player){
			this.Player = Player;
			this.SkillsPanel = new Panel();
			for(int i = 0; i < 3; i++){
				Slots[i] = new TreeSlot[4];
				for(int j = 0; j < 4; j++){
					Slots[i][j] = new TreeSlot();
				}
			}
			float Scale = .9f;
			float PositionX = -.65f;
			//Create Tabs
			{
				/*
				RenderableButton FireTab = new RenderableButton(new Vector2(PositionX - 0.085f, Mathf.ScaleGUI(TargetResolution, new Vector2(.2f, .55f) * Scale).Y + 0.05f ),
				                             Vector2.Zero, "FIRE", 0, Color.White, FontCache.Get(AssetManager.Fonts.Families[0], 12, FontStyle.Bold));
				
				RenderableButton WaterTab = new RenderableButton(new Vector2(FireTab.Position.X + FireTab.Scale.X*3f, Mathf.ScaleGUI(TargetResolution, new Vector2(.2f, .55f) * Scale).Y + 0.05f ),
				                             Vector2.Zero, "WATER", 0, Color.White, FontCache.Get(AssetManager.Fonts.Families[0], 12, FontStyle.Bold));
				
				RenderableButton AirTab = new RenderableButton(new Vector2(WaterTab.Position.X + WaterTab.Scale.X*2f, Mathf.ScaleGUI(TargetResolution, new Vector2(.2f, .55f) * Scale).Y + 0.05f ),
				                             Vector2.Zero, "AIR", 0, Color.White, FontCache.Get(AssetManager.Fonts.Families[0], 12, FontStyle.Bold));
				
				RenderableButton EarthTab = new RenderableButton(new Vector2(AirTab.Position.X + AirTab.Scale.X * 3f + 0.015f, Mathf.ScaleGUI(TargetResolution, new Vector2(.2f, .55f) * Scale).Y + 0.05f ),
				                             Vector2.Zero, "EARTH", 0, Color.White, FontCache.Get(AssetManager.Fonts.Families[0], 12, FontStyle.Bold));				
				
				this.SkillsPanel.AddElement(WaterTab);
				this.SkillsPanel.AddElement(FireTab);
				this.SkillsPanel.AddElement(EarthTab);
				this.SkillsPanel.AddElement(AirTab);
				
				DrawManager.UIRenderer.Add(FireTab, false);
				DrawManager.UIRenderer.Add(WaterTab, false);
				DrawManager.UIRenderer.Add(AirTab, false);
				DrawManager.UIRenderer.Add(EarthTab, false);
				
				FireTab.Click += delegate { this.SetTreeTemplate(SkillTreeType.FIRETREE); };
				WaterTab.Click += delegate { this.SetTreeTemplate(SkillTreeType.WATERTREE); };
				AirTab.Click += delegate { this.SetTreeTemplate(SkillTreeType.AIRTREE); };
				EarthTab.Click += delegate { this.SetTreeTemplate(SkillTreeType.EARTHTREE); };*/
			}
			
			//Create the skill tree
			{

				Texture Background = new Texture(Graphics2D.LoadFromAssets("Assets/UI/Skilltree.png"), Vector2.Zero,  Mathf.ScaleGUI(new Vector2(1366, 768), Vector2.One));
				
				Vector2 Offsets = Mathf.ScaleGUI(TargetResolution, new Vector2(0.05725f*1.65f,0.3f) * Scale);
				float SOffsetX = Mathf.ScaleGUI(TargetResolution, new Vector2(0.05725f * 1.2f,0) * Scale).X;
				float OffsetX = Offsets.X;
				float OffsetY = Offsets.Y;
				float SizeX = Mathf.ScaleGUI(new Vector2(1000,1000), new Vector2(.055f,-.055f) * Scale).X;
				float SizeY = Mathf.ScaleGUI(new Vector2(1000,1000), new Vector2(.055f,-.055f) * Scale).Y;
				float AddY = 0;
				uint TexId = Graphics2D.LoadTexture( new Bitmap( new MemoryStream(AssetManager.ReadBinary("HolderSkill.png", AssetManager.DataFile3))) );;
				int y = 0;
				for(int x = 0; x < 4; x++){
					if(x == 3){
						x = 0;
						y++;
						AddY = y * OffsetY + y * SizeY;
						if( y == 4)
							break;
					}
					SkillSize = Mathf.ScaleGUI(new Vector2(1000,1000), new Vector2(.055f,.055f) * Scale);
					RenderableButton Skill = new RenderableButton(new Vector2(SOffsetX + OffsetX * x + SizeX * x, -OffsetY - AddY)
					                                              + Mathf.ScaleGUI(TargetResolution, new Vector2(PositionX-.2f, .65f)*Scale), SkillSize, TexId);
				    RenderableText SkillText = new RenderableText("0",new Vector2(SOffsetX + OffsetX * x + SizeX * x, -OffsetY - AddY)
					                                              + Mathf.ScaleGUI(TargetResolution, new Vector2(PositionX-.2f, .6f) * Scale), Color.White, FontCache.Get(UserInterface.Fonts.Families[0], 12 * Scale, FontStyle.Regular));
					Texture Border = new Texture(Colors.FromHtml("#FF9F00"), new Vector2(SOffsetX + OffsetX * x + SizeX * x, -OffsetY - AddY)
					                           					  + Mathf.ScaleGUI(TargetResolution, new Vector2(PositionX-.2f, .65f) * Scale), SkillSize * 1.125f);
					int k = x, j = y;
					Skill.HoverEnter += delegate {
						this.SetSkillInfoUI(k,j);
						SkillsInfoPanel.Enable();
					};
					Skill.HoverExit += delegate { SkillsInfoPanel.Disable(); };
					
				    
				    Skill.Click += delegate 
				    {
				    	if(!this.Show) return;
				    	
				    	if(this.AvailablePoints > 0 && ( !Slots[k][j].Locked || Player.Level >= j*5) )
				    	{
				    		if(!IsPreviousSet(k,j)) goto FAIL;
				    		Slots[k][j].Level += 1;
							this.UpdatePoints();
							this.BuildRelationshipLines();
					        this.SetSkillInfoUI(k, j);
                            Player.Skills.SettedUp = true;
							return;
							
							FAIL:
				    			Player.MessageDispatcher.ShowNotification("YOU NEED TO UNLOCK THE PREVIOUS SKILL", Color.DarkRed, 3.5f);
							
				    	}
				    	else
				    	{
				    		if(Slots[k][j].Locked)
				    			Player.MessageDispatcher.ShowNotification("YOU NEED LEVEL "+j*5+" TO UNLOCK THIS SKILL", Color.DarkRed, 3.5f);
				    		else
				    			Sound.SoundManager.PlaySoundInPlayersLocation(Sound.SoundType.OnOff, 1.0f, 0.6f);
				    	}
				    	
				    };
				    

				    Skill.Enlarge = false;
				    Skill.Texture.Grayscale = true;
				    Skill.PlaySound = false;
				    Slots[x][y].Texture = Border;
				    Slots[x][y].Button = Skill;
				    Slots[x][y].LevelText = SkillText;
				    Skill.Clickable = true;
				    Skill.PlaySound = true;
				    this.SkillsPanel.AddElement(Border);
				    this.SkillsPanel.AddElement(Skill);
				    this.SkillsPanel.AddElement(SkillText);
				}
				GUIText Title = new GUIText("SKILL TREE",  Mathf.ScaleGUI(TargetResolution, new Vector2(PositionX, .5f)*Scale), Color.White, FontCache.Get(AssetManager.Fonts.Families[0], 16*Scale, FontStyle.Bold));
				PointsText = new GUIText("AVAILABLE POINTS: "+AvailablePoints, Mathf.ScaleGUI(TargetResolution, new Vector2(PositionX, -.5f)*Scale), Color.White, FontCache.Get(AssetManager.Fonts.Families[0], 16*Scale, FontStyle.Bold));
				
				this.SkillsPanel.AddElement(PointsText);
				this.SkillsPanel.AddElement(Title);
				this.SkillsPanel.AddElement(Background);
				this.SkillsPanel.Disable();
				this.SkillsPanel.OnPanelStateChange += new OnPanelStateChangeEventHandler(OnWindowOpened);
				
				DrawManager.UIRenderer.Add(this, DrawOrder.After);
				for(int i = 0; i < Slots.Length; i++){
					for(int j = 0; j < Slots[i].Length; j++){
						DrawManager.UIRenderer.Add(Slots[i][j].Button as RenderableButton, DrawOrder.After);
						DrawManager.UIRenderer.Add(Slots[i][j].LevelText, DrawOrder.After);
					}
				}
			}
			
			//Create Drag&Drop Menu
			{
				BagPosition = Mathf.ScaleGUI(TargetResolution, new Vector2(0f, 0f));
				BagScale = Mathf.ScaleGUI(TargetResolution, new Vector2(.25f, .35f));
				GUIText DragDrop = new GUIText("DRAG & DROP",Mathf.ScaleGUI(TargetResolution, new Vector2(0f, .3f)), Color.White, FontCache.Get(AssetManager.Fonts.Families[0], 16, FontStyle.Bold));
				this.UpdateBag();
				
				this.SkillsPanel.AddElement(DragDrop);
			}
			
			//Create info
			{
				this.SkillsInfoPanel = new Panel();
				
				Vector2 BckScale = Mathf.ScaleGUI(TargetResolution, new Vector2(0.125f, 0.4f));
				Vector2 BckPosition = Mathf.ScaleGUI(TargetResolution, new Vector2(.55f, .0f));
				SkillInfoBck = new Texture(Graphics2D.LoadFromAssets("Assets/UI/SkillInfo.png"), Vector2.Zero,  Mathf.ScaleGUI(new Vector2(1366, 768), Vector2.One));
				//SkillInfoBck.Scale = BckScale;
				SkillInfoName = new GUIText("SKILL NAME", new Vector2(BckPosition.X, BckPosition.Y + BckScale.Y - .075f), Color.White, FontCache.Get(AssetManager.Fonts.Families[0], 14, FontStyle.Bold) );
				SkillInfoManaCost = new GUIText("MANA COST", new Vector2(BckPosition.X, BckPosition.Y - BckScale.Y * .05f), Bar.Blue.ToColor(), FontCache.Get(AssetManager.Fonts.Families[0], 11, FontStyle.Bold));
				SkillInfoCooldown = new GUIText("CD COST", new Vector2(BckPosition.X, BckPosition.Y - BckScale.Y * .2f), Bar.Violet.ToColor(), FontCache.Get(AssetManager.Fonts.Families[0], 11, FontStyle.Bold));
				SkillInfoDesc = new GUIText("SKILL DESC", new Vector2(BckPosition.X, BckPosition.Y - BckScale.Y * .5f), Color.White, FontCache.Get(AssetManager.Fonts.Families[0], 10, FontStyle.Bold) );
				SkillInfoTexture = new Texture(0, new Vector2(BckPosition.X, BckPosition.Y + BckScale.Y * .4f), SkillSize * 1.25f );
				
				this.SkillsInfoPanel.AddElement(SkillInfoCooldown);
				this.SkillsInfoPanel.AddElement(SkillInfoManaCost);
				this.SkillsInfoPanel.AddElement(SkillInfoBck);
				this.SkillsInfoPanel.AddElement(SkillInfoName);
				this.SkillsInfoPanel.AddElement(SkillInfoDesc);
				this.SkillsInfoPanel.AddElement(SkillInfoTexture);
			}
		}
		
		private void SetSkillInfoUI(int k, int j){
			SkillInfoName.Text = Utils.AddSpacesToSentence(Slots[k][j].AbilityType.ToString().Replace("Hedra.Engine.Player.", ""), false);
			SkillInfoTexture.TextureElement.TextureId = Slots[k][j].Image;
			for(int i = 0; i < Player.Skills.Skills.Length; i++){
				if( Player.Skills.Skills[i].GetType() == Slots[k][j].AbilityType ){
					if(Player.Skills.Skills[i].ManaCost > 0){
						SkillInfoManaCost.Text = "Mana Cost: "+Player.Skills.Skills[i].ManaCost;
					}else{
						SkillInfoManaCost.Text = "";
					}
					if(Player.Skills.Skills[i].MaxCooldown > 0){
						SkillInfoCooldown.Text = "Cooldown: "+Player.Skills.Skills[i].MaxCooldown;
					}else{
						SkillInfoCooldown.Text = "";
					}
					SkillInfoDesc.Text = Player.Skills.Skills[i].Description;
					break;
				}
			}
			
			int CharacterLimit = (int) 20;
			
			StringBuilder Builder = new StringBuilder();
			string OriginalString = SkillInfoDesc.Text; 
			while(OriginalString.Length > CharacterLimit){
				int Nearest = Utils.FindNearestSeparator(OriginalString, CharacterLimit-1);
				Builder.AppendLine(OriginalString.Substring(0, Nearest));
				OriginalString = OriginalString.Substring(Nearest, OriginalString.Length - Nearest);
			}
			Builder.Append(OriginalString);
			if(Builder.Length == 0)
				SkillInfoDesc.Text = OriginalString;
			else
				SkillInfoDesc.Text = Builder.ToString();
		}
		   
		private LinesShader Shader = new LinesShader("Shaders/Lines.vert","Shaders/Lines.frag");
		public void Draw(){
			if(!Show || Lines == null)
				return;
			
			Shader.Bind();
			GL.LineWidth(4.5f);
			GL.Enable(EnableCap.LineSmooth);

		    Data.Bind();
			GL.EnableVertexAttribArray(0);
		    GL.EnableVertexAttribArray(1);

            GL.DrawArrays(PrimitiveType.Lines, 0, Lines.Count);

		    GL.DisableVertexAttribArray(0);
		    GL.DisableVertexAttribArray(1);

            Shader.UnBind();
		}
		
		private VBO<Vector2> Lines;
		private VBO<Vector4> LinesColors;
	    private VAO<Vector2, Vector4> Data;
		public void BuildRelationshipLines(){
			if(Lines == null){
				Lines = new VBO<Vector2>(new Vector2[]{}, 0, VertexAttribPointerType.Float);
				LinesColors = new VBO<Vector4>(new Vector4[]{}, 0, VertexAttribPointerType.Float);
			    Data = new VAO<Vector2, Vector4>(Lines, LinesColors);
            }
				
			List<Vector2> LinesData = new List<Vector2>();
			List<Vector4> ColorsData = new List<Vector4>();
			for(int x = 0; x < Slots.Length; x++){
				
				if(Slots[x][0].Enabled && Slots[x][1].Enabled){
					LinesData.AddRange( FromV2 (Slots[x][0].Button.Position, Slots[x][1].Button.Position ) );
				}else if (Slots[x][0].Enabled && Slots[x][2].Enabled){
					LinesData.AddRange( FromV2 (Slots[x][0].Button.Position, Slots[x][2].Button.Position ) );
				}else if(Slots[x][0].Enabled && Slots[x][3].Enabled){
					LinesData.AddRange( FromV2 (Slots[x][0].Button.Position, Slots[x][3].Button.Position ) );
				}
				
				if(Slots[x][1].Enabled && Slots[x][2].Enabled){
					LinesData.AddRange( FromV2 (Slots[x][1].Button.Position, Slots[x][2].Button.Position ) );
				}else if (Slots[x][1].Enabled && Slots[x][3].Enabled){
					LinesData.AddRange( FromV2 (Slots[x][1].Button.Position, Slots[x][3].Button.Position ) );
				}
				
				if(Slots[x][2].Enabled && Slots[x][3].Enabled){
					LinesData.AddRange( FromV2 (Slots[x][2].Button.Position, Slots[x][3].Button.Position ) );
				}
				
				//Colors
				if(Slots[x][0].Enabled && Slots[x][1].Enabled){
					ColorsData.AddRange( FromV4 (!Slots[x][0].Locked && Slots[x][0].Level > 0, !Slots[x][1].Locked && Slots[x][1].Level > 0) );
				}else if (Slots[x][0].Enabled && Slots[x][2].Enabled){
					ColorsData.AddRange( FromV4 (!Slots[x][0].Locked && Slots[x][0].Level > 0, !Slots[x][2].Locked && Slots[x][2].Level > 0) );
				}else if(Slots[x][0].Enabled && Slots[x][3].Enabled){
					ColorsData.AddRange( FromV4 (!Slots[x][0].Locked && Slots[x][0].Level > 0, !Slots[x][3].Locked && Slots[x][3].Level > 0 ) );
				}
				
				if(Slots[x][1].Enabled && Slots[x][2].Enabled){
					ColorsData.AddRange( FromV4 (!Slots[x][1].Locked && Slots[x][1].Level > 0, !Slots[x][2].Locked && Slots[x][2].Level > 0) );
				}else if (Slots[x][1].Enabled && Slots[x][3].Enabled){
					ColorsData.AddRange( FromV4 (!Slots[x][1].Locked && Slots[x][1].Level > 0, !Slots[x][3].Locked && Slots[x][3].Level > 0 ) );
				}
				
				if(Slots[x][2].Enabled && Slots[x][3].Enabled){
					ColorsData.AddRange( FromV4 (!Slots[x][2].Locked && Slots[x][2].Level > 0, !Slots[x][3].Locked && Slots[x][3].Level > 0 ) );
				}

			}
			Lines.Update(LinesData.ToArray(), LinesData.Count * Vector2.SizeInBytes);// Count * float * 2
			LinesColors.Update(ColorsData.ToArray(), ColorsData.Count * Vector4.SizeInBytes);

        }
		
		//HelperMethod

		private Vector2[] FromV2(Vector2 V1, Vector2 V2){
			return new Vector2[]{V1, V2};
		}
		private Vector4[] FromV4(bool V1, bool V2){
			return new Vector4[]{(!V1) ? this.LinesColor : this.ActiveColor, (!V2) ? this.LinesColor : this.ActiveColor};
		}
		
		private void SetBlueprint(TreeBlueprint Blueprint){
			for(int i = 0; i < Player.Skills.Skills.Length; i++){
				Player.Skills.Skills[i].Level = 0;
			}
			
			for(int i = 0; i < Blueprint.Slots.Length; i++){
				for(int j = 0; j < Blueprint.Slots[i].Length; j++){
					this.Slots[i][j].Image = Blueprint.Slots[i][j].Image;
					this.Slots[i][j].Enabled = Blueprint.Slots[i][j].Enabled;
					this.Slots[i][j].Locked = (j * 5 > Player.Level) ? true : false;
					this.Slots[i][j].AbilityType = Blueprint.Slots[i][j].AbilityType;
					Type Ability = this.Slots[i][j].AbilityType;
					
					if(Ability == null)continue;
					if(this.Slots[i][j].Level == 0)continue;
					
					for(int k = 0; k < Player.Skills.Skills.Length; k++){
						if( Player.Skills.Skills[k].GetType() == Ability ){
							Player.Skills.Skills[k].Level = this.Slots[i][j].Level;
						}
					}
				}
			}
			this.ActiveColor = Blueprint.ActiveColor;
		}
		
		public void SetTreeTemplate(SkillTreeType Type){
			this.SetBlueprint(this.ParseType(Type));
			BuildRelationshipLines();
			
			SkillsPanel.Enable();
			SkillsPanel.Disable();
			UpdateManager.CursorShown = true;
			Player.View.LockMouse = true;
			Player.Movement.Check = true;
			Player.View.Check = true;
			
		}
		
		public TreeBlueprint ParseType(SkillTreeType Type){
			switch(Type){
				case SkillTreeType.FIRETREE:
					return new FireTreeBlueprint();
				case SkillTreeType.WATERTREE:
					return new WaterTreeBlueprint();
				case SkillTreeType.ARCHERTREE:
					return new ArcherTreeBlueprint();
				case SkillTreeType.ROGUETREE:
					return new RogueTreeBlueprint();
				case SkillTreeType.WARRIORTREE:
					return new WarriorTreeBlueprint();
			}
			return null;
		}
		
		public void OnWindowOpened(object sender, PanelState State){
			if(State == PanelState.Enabled){
				UpdateManager.CursorShown = true;
				Player.View.LockMouse = false;
				Player.Movement.Check = false;
				Player.View.Check = false;
				this.SetBlueprint(this.ParseType(this.Type));
				this.UpdatePoints();
				this.UpdateBag();
			}else{
				if(Constants.CHARACTER_CHOOSED)
					UpdateManager.CursorShown = false;
				Player.View.LockMouse = true;
				Player.Movement.Check = true;
				Player.View.Check = true;
				this.HideBag();
			}
		}	
		
		public void UpdatePoints(){
			if(PointsText != null)
				PointsText.Text = "AVAILABLE POINTS: "+(Player.Level - UsedPoints).ToString();
		
			for(int i = 0; i < Player.Skills.Skills.Length; i++){
				Player.Skills.Skills[i].Level = 0;
			}
			
			for(int i = 0; i < Slots.Length; i++){
				for(int j = 0; j < Slots[i].Length; j++){
					
					if(Slots[i][j].Level == 0 && !Slots[i][j].Locked){
						if(IsPreviousSet(i,j)){
							Slots[i][j].Button.Texture.Grayscale = false;
						}
					}
					
					Type Ability = this.Slots[i][j].AbilityType;
							
					if(Ability == null)continue;
					if(this.Slots[i][j].Level == 0)continue;
					
					for(int k = 0; k < Player.Skills.Skills.Length; k++){
						if( Player.Skills.Skills[k].GetType() == Ability ){
							Player.Skills.Skills[k].Level = this.Slots[i][j].Level;
						}
					}
					if(Slots[i][j].Locked)
						Slots[i][j].LevelText.UiText.UiText.Opacity = 0;
				}
			}
		}
		
		public void SetPoints(Type Type, int Level){
			for(int i = 0; i < Slots.Length; i++){
				for(int j = 0; j < Slots[i].Length; j++){
					if(Slots[i][j].AbilityType == Type)
						Slots[i][j].Level = Level;
				}
			}
			this.UpdatePoints();
		}
		
		public int AvailablePoints{
			get{ 
				int Used = UsedPoints;
				return Player.Level - Used;
			}
		}
		
		public int UsedPoints{
			get{ 
				int Used = 0;
				for(int i = 0; i < Slots.Length; i++){
					for(int j = 0; j < Slots[i].Length; j++){
						Used += Slots[i][j].Level;
					}
				}
				return Used;
			}
		}
		
		private bool m_Show;
		public bool Show{
			get{ return m_Show; }
			set{
				if(Player.Inventory.Show || Scenes.SceneManager.Game.IsLoading || m_Show == value || Player.Trade.Show)
					return;
				
				m_Show = value;
				Sound.SoundManager.PlaySoundInPlayersLocation(Sound.SoundType.OnOff, 1.0f, 0.6f);
				if(value){
					if(Player.UI.GamePanel.Enabled){
						Player.UI.GamePanel.SkillTreeMsg.Disable();
					}
					this.SkillsPanel.Enable();
				}else{
					if(Player.UI.GamePanel.Enabled){
						Player.UI.GamePanel.SkillTreeMsg.Enable();
					}
					this.SkillsInfoPanel.Disable(); 
					this.SkillsPanel.Disable();
				}
			}
		}
		
		public byte[] Save(){
			BinaryFormatter Formatter = new BinaryFormatter();
			MemoryStream Ms = new MemoryStream();
			Ms.WriteByte( (byte) this.Type );
			for(int i = 0; i < Slots.Length; i++){
				for(int j = 0; j < Slots[i].Length; j++){
					Ms.WriteByte( (byte) Slots[i][j].Level);
				}
			}
			//Formatter.Serialize(Ms, Slots);
			return Ms.ToArray();
		}
		public SkillTree Load(PlayerData PData){
			//Before 0.8
			//this.Type = SkillTreeType.FIRETREE;
			//this.SetTreeTemplate(this.Type);
			//return this;
			//After 0.8
			byte[] Data = PData.SkillsData;
			
			if(PData.ClassType == Class.Warrior)
				this.Type = SkillTreeType.WARRIORTREE;
			
			if(PData.ClassType == Class.Rogue)
				this.Type = SkillTreeType.ROGUETREE;
			
			if(PData.ClassType == Class.Mage)
				this.Type = SkillTreeType.FIRETREE;
			
			if(PData.ClassType == Class.Archer)
				this.Type = SkillTreeType.ARCHERTREE;
			
			
			if(Data.Length > 0 && (SkillTreeType) Data[0] != this.Type){
				//Reset points because is old version
				return this;
			}
			
			
			if(Data == null || Data.Length == 0) return this;
			
			
			
			byte[] Dest = new byte[Data.Length-1];
			Array.Copy(Data,1, Dest, 0, Dest.Length);
			
			using(MemoryStream Ms = new MemoryStream(Dest)){
				using(BinaryReader Reader = new BinaryReader(Ms)){
					for(int i = 0; i < Slots.Length; i++){
						for(int j = 0; j < Slots[i].Length; j++){
							Slots[i][j].Level = Reader.ReadByte();
						}
					}
				}
			}
			//this.SetTreeTemplate( (SkillTreeType) Data[0] );
			this.SetTreeTemplate(this.Type);
			this.UpdatePoints();
			
			if(this.AvailablePoints < 0){
				this.Reset();
				this.UpdatePoints();
				this.PointsText.Disable();
			}
			
			return this;
		}
		
		public void UpdateBag(){
			Vector2 Offsets = Mathf.ScaleGUI(TargetResolution, new Vector2(0.05725f*.45f, 0.05f * 1.8f));
			float SOffsetX = 0.05725f * 1.4f;
			float SOffsetY = .165f;
			float AddY = 0;
			int x = 0;
			for(int i = 0; i < Player.Skills.Skills.Length; i++){
				
				if(Player.Skills.Skills[i].Position == Player.Skills.SkillPositions[0] ||
				   Player.Skills.Skills[i].Position == Player.Skills.SkillPositions[1] ||
				   Player.Skills.Skills[i].Position == Player.Skills.SkillPositions[2] ||
				   Player.Skills.Skills[i].Position == Player.Skills.SkillPositions[3] || Player.Skills.Skills[i].Passive )continue;
				
				bool IsInTree = false;
				for(int j = 0; j < this.Slots.Length; j++){
					for(int k = 0; k < this.Slots[j].Length; k++){
						if(this.Slots[j][k].AbilityType != null && 
						   this.Slots[j][k].AbilityType.ToString() == Player.Skills.Skills[i].GetType().ToString() ){
							IsInTree = true;
							break;
						}
					}
				}
				if(!IsInTree) continue;
				
				if(i != 0 && i % 4 == 0){
				AddY -= Offsets.Y + Player.Skills.Skills[i].Scale.Y;
					x = 0;
				}
					Player.Skills.Skills[i].Active = true;
					Player.Skills.Skills[i].Position = new Vector2(Player.Skills.Skills[i].Scale.X * x * 2f  - BagScale.X + Offsets.X * x + SOffsetX, AddY + BagScale.Y - SOffsetY);
					Player.Skills.Skills[i].UseMask = false;
				x++;
			}
		}
		
		public void HideBag(){
			for(int i = 0; i < Player.Skills.Skills.Length; i++){
				
				if(Player.Skills.Skills[i].Position == Player.Skills.SkillPositions[0] ||
				   Player.Skills.Skills[i].Position == Player.Skills.SkillPositions[1] ||
				   Player.Skills.Skills[i].Position == Player.Skills.SkillPositions[2] ||
				   Player.Skills.Skills[i].Position == Player.Skills.SkillPositions[3])continue;
				
				Player.Skills.Skills[i].Active = false;
			}
		}
		
		private bool IsPreviousSet(int k, int j){
			if(j == 3){
	    			if(Slots[k][2].Enabled)
	    				if(Slots[k][2].Level == 0)
	    					return false;
	    			
	    			if(Slots[k][1].Enabled)
	    				if(Slots[k][1].Level == 0)
	    					return false;
	    			
	    			if(Slots[k][0].Enabled)
	    				if(Slots[k][0].Level == 0)
	    					return false;
	    		}
	    		else if(j == 2){
	    			
	    			if(Slots[k][1].Enabled)
	    				if(Slots[k][1].Level == 0)
	    					return false;
	    			
	    			if(Slots[k][0].Enabled)
	    				if(Slots[k][0].Level == 0)
	    					return false;
	    		}else if(j == 1){
	    			
	    			if(Slots[k][0].Enabled)
	    				if(Slots[k][0].Level == 0)
	    					return false;
	    		}
			return true;
		}
		
		public void Reset(){
			for(int i = 0; i < Slots.Length; i++){
				for(int j = 0; j < Slots[i].Length; j++){
					Slots[i][j].Level = 0;
				}
			}
			this.BuildRelationshipLines();
			this.UpdatePoints();
		}
	}
	
	public enum SkillTreeType{
		FIRETREE,
		WATERTREE,
		AIRTREE,
		EARTHTREE,
		WARRIORTREE,
		ARCHERTREE,
		ROGUETREE
	}
}
