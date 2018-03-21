/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 05/08/2016
 * Time: 11:43 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using Hedra.Engine.Rendering.UI;

namespace Hedra.Engine.Player
{
	/// <summary>
	/// Description of TreeSlot.
	/// </summary>
	[Serializable]
	public class TreeSlot
	{
		public Type AbilityType;
		
		private bool m_Locked = false;
		public bool Locked{
			get{ return m_Locked; }
			set{
				m_Locked = value;
				if(m_Button != null){
					Button.Texture.Opacity = (m_Enabled) ? 1 : 0;
					m_LevelText.UIText.UIText.Opacity = ( ((m_Enabled) ? true : false) && m_Level > 0) ? 1 : 0;
					if(Texture != null)
						Texture.TextureElement.Opacity = (m_Enabled && !Locked && m_Level > 0) ? 1 : 0;
				}
			}
		}
		
		private int m_Level;
		public int Level{
			get{ return m_Level; }
			set{
				m_Level = value;
				if(m_Button != null){
					m_Button.Texture.Grayscale = !(m_Level > 0);
					m_LevelText.Text = m_Level.ToString();
					m_LevelText.UIText.UIText.Opacity = (m_Level > 0) ? 1 : 0;
					if(Texture != null)
						Texture.TextureElement.Opacity = (m_Enabled && !Locked && Level > 0) ? 1 : 0;
				}
			}
		}
		
		[NonSerialized]
		private RenderableText m_LevelText;
		public RenderableText LevelText{ 
			get{
				return m_LevelText;
			}
			set{
				m_LevelText = value;
				if(m_Enabled)
					LevelText.Enable();
				else
					LevelText.Disable();
				m_LevelText.UIText.UIText.Opacity = (m_Enabled) ? 1 : 0;
			}
		}
		
		[NonSerialized]
		private Texture m_Texture;
		public Texture Texture{ 
			get{
				return m_Texture;
			}
			set{
				m_Texture = value;
				if(m_Enabled)
					m_Texture.Enable();
				else
					m_Texture.Disable();
				m_Texture.TextureElement.Opacity = (m_Enabled && !Locked && Level > 0) ? 1 : 0;
			}
		}
		
		[NonSerialized]
		private Button m_Button;
		public Button Button{ 
			get{
				return m_Button;
			}
			set{
				m_Button = value;
				if(m_Enabled)
					Button.Enable();
				else
					Button.Disable();
				m_Button.Texture.Opacity = (m_Enabled) ? 1 : 0;
			}
		}
		
		[NonSerialized]
		private uint m_Image;
		public uint Image{
			get{ return m_Image; }
			set{
				m_Image = value;
				if(Button != null)
					Button.Texture.TextureId = m_Image;
			}
		}
		
		[NonSerialized]
		private bool m_Enabled = false;
		public bool Enabled{
			get{ return m_Enabled; }
			set{
				m_Enabled = value;
				if(Button != null){
					if(m_Enabled){
						Button.Enable();
						LevelText.Enable();
						if(Texture != null && !Locked && Level > 0)
						Texture.Enable();
					}else{
						Button.Disable();
						LevelText.Disable();
						if(Texture != null)
						Texture.Disable();
					}
					Button.Texture.Opacity = (m_Enabled) ? 1 : 0;
					if(Texture != null)
					m_Texture.TextureElement.Opacity = (m_Enabled && !Locked && Level > 0) ? 1 : 0;
				}
			}
		}
	}
}
