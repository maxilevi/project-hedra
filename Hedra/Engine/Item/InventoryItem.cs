/*
 * Created by SharpDevelop.
 * User: Maxi Levi
 * Date: 27/04/2016
 * Time: 08:03 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using OpenTK;
using Hedra.Engine.Rendering;
using Hedra.Engine.EntitySystem;
using System.Globalization;
using Hedra.Engine.Player;

namespace Hedra.Engine.Item
{
	/// <summary>
	/// Description of InventoryItem.
	/// </summary>
	[Serializable]
	public class InventoryItem
	{
		private bool _isDirty;
		private bool _isDirtyM;
		
		
		public InventoryItem(ItemType Type)
		{
			this.Type = Type;
			if(this.Type == ItemType.Random){
				this.Type = (ItemType) Utils.Rng.Next(0, (int) ItemType.MaxItems);
					
				if( this.Type == ItemType.Stackable || this.Type == ItemType.Mount)
					this.Type = ItemType.Coin;
			}
			this.Info = ItemInfo.Random(this.Type);
		}
		
		public InventoryItem(ItemType Type, ItemInfo Info)
		{
			if(this.Type == ItemType.Random){
				RNG:
					this.Type = (ItemType) Utils.Rng.Next(0, (int) ItemType.MaxItems);
					
				if( this.Type == ItemType.Stackable || this.Type == ItemType.Mount || this.Type == ItemType.Coin || this.Type == ItemType.Food)
					goto RNG;
			}
			this.Type = Type;
			this.Info = Info;
		}
		
		private ItemType _type;
		public ItemType Type{
			get{ return _type;}
			set{ this._type = value;
				this._isDirty = true;
				this._isDirtyM = true;
			}
		}
		private ItemInfo _info;
		public ItemInfo Info{
			get{return _info;}
			set{ this._info = value;
				this._isDirty = true;
				this._isDirtyM = true;
			}
		}
		
				
		public string Name{
			get{
				string name = Type.ToString().ToLowerInvariant();
			    name = name.Replace("doubleblades", "blades");
                name = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(name);
				name = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(Info.MaterialType.ToString().ToLowerInvariant()) + " "+ name;
			    name = name.Replace("RAPIER", "SWORD");
                

                switch (Type)
			    {
			        case ItemType.Glider:
			            return "GLIDER";
			        case ItemType.Stackable:
			            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase( Info.MaterialType.ToString().ToPascalString() );
                    case ItemType.Mount:
                        return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(Info.MaterialType.ToString().ToPascalString());
                }

			    return name;
			}
		}
		
		[NonSerializedAttribute]
		private EntityMesh _cachedMesh = null;
		public EntityMesh GetMesh(Vector3 Scale){
			if(_cachedMesh != null && !_isDirtyM){
				_cachedMesh.Scale = Scale;
				return _cachedMesh;
			}
			
			_isDirtyM = false;
			_cachedMesh = EntityMesh.FromVertexData(ItemPool.GetItem(Type, Info));
			_cachedMesh.Scale = Scale;
			return _cachedMesh;
		}
		
		public EntityMesh GetMesh(float Scale){
			return this.GetMesh(new Vector3(Scale, Scale, Scale));
		}
		
		public VertexData MeshFile => ItemPool.GetItem(Type, Info);

	    [NonSerializedAttribute]
		private Weapon _cache = null;
		public Weapon Weapon{
			get{
				if(_cache != null && !_isDirty && !_cache.Disposed)
					return _cache;
				
				string Name = Type.ToString();
				string[] Parts = Name.Split('_');
				Name = Parts[Parts.Length-1].ToUpperInvariant();
				
				Weapon WeaponObject;
				switch(Name){
					case "SWORD":
						WeaponObject = new Rapier(ItemPool.GetItem(Type, Info));
						break;
					case "BOW":
						WeaponObject = new Bow(ItemPool.GetItem(Type, Info));
						break;
					case "DAGGER":
						WeaponObject = new ThrowingKnife(ItemPool.GetItem(Type, Info), this);
						break;
					case "AXE":
						WeaponObject = new Axe(ItemPool.GetItem(Type, Info));
						break;
					case "HAMMER":
						WeaponObject = new Hammer(ItemPool.GetItem(Type, Info));
						break;
					case "KNIFE":
						WeaponObject = new Knife(ItemPool.GetItem(Type, Info));
						break;
					case "KATAR":
						WeaponObject = new Katar(ItemPool.GetItem(Type, Info));
						break;
					case "CLAW":
						WeaponObject = new Claw(ItemPool.GetItem(Type, Info));
						break;
					case "DOUBLEBLADES":
						WeaponObject = new Blade(ItemPool.GetItem(Type, Info));
						break;
					default:
						throw new ArgumentException("The name of the weapon could not be found");
				}
				_cache = WeaponObject;
				_isDirty = false;
				return WeaponObject;
			}
		}
	}
}
