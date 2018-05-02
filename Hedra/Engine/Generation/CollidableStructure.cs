/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 27/08/2017
 * Time: 12:27 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.PhysicsSystem;
using OpenTK;
using Hedra.Engine.QuestSystem;
using Hedra.Engine.StructureSystem;

namespace Hedra.Engine.Generation
{
    /// <summary>
    /// Description of Town.
    /// </summary>
    public class CollidableStructure
    {
        public Vector3 Position;
        public Plateau Mountain;
        public bool Generated = false;
        private readonly List<ICollidable> _colliders;
        public StructureDesign Design { get; set; }
        public AttributeArray Parameters { get; }

        public CollidableStructure(StructureDesign Design, Vector3 Position, Plateau Mountain)
        {
            this.Position = Position;
            this.Mountain = Mountain;
            this.Design = Design;
            this.Parameters = new AttributeArray();
            this._colliders = new List<ICollidable>();

        }

        public ICollidable[] Colliders
        {
            get
            {
                lock(_colliders)
                    return _colliders.ToArray();
            }
        }

        public void AddCollisionShape(params ICollidable[] IColliders)
	    {
	        lock (_colliders)
	        {
                for(var i = 0; i < IColliders.Length; i++)
	                _colliders.Add(IColliders[i]);
	        }
	    }
	}
}
