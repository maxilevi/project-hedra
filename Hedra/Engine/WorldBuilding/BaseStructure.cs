/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 13/09/2016
 * Time: 08:51 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using Hedra.Engine.Core;
using System.Numerics;
using Hedra.Engine.Management;
using Hedra.EntitySystem;
using Microsoft.Scripting.Utils;

namespace Hedra.Engine.WorldBuilding
{
    /// <inheritdoc />
    /// <summary>
    /// Description of Structure.
    /// </summary>
    public abstract class BaseStructure : IDisposable, IStructure, ISearchable
    {
        private readonly List<BaseStructure> _children;
        private readonly List<IEntity> _npcs;

        protected BaseStructure(List<BaseStructure> Children, List<IEntity> Npcs)
        {
            _children = Children;
            _npcs = Npcs;
        }

        public BaseStructure[] Children => _children.ToArray();
        public IEntity[] NPCs => _npcs.ToArray();
        public virtual Vector3 Position { get; set; }
        public bool Disposed { get; protected set; }

        protected BaseStructure(Vector3 Position)
        {
            this.Position = Position;
            _npcs = new List<IEntity>();
            _children = new List<BaseStructure>();
        }
        
        public void AddChildren(params BaseStructure[] Children)
        {
            for (var i = 0; i < Children.Length; ++i)
            {
                if(Children[i] == null)
                    throw new ArgumentNullException($"Cannot add a null children");
                _children.Add(Children[i]);
            }
        }

        public T SearchFirst<T>() where T : BaseStructure => Search<T>().First();
        
        public T[] Search<T>() where T : BaseStructure
        {
            var list = new List<T>();
            for (var i = 0; i < _children.Count; ++i)
            {
                if (_children[i] is T)
                {
                    list.Add((T)_children[i]);
                }
                list.AddRange(_children[i].Search<T>());
            }
            return list.ToArray();
        }

        public void AddNPCs(params IEntity[] NPCs)
        {
            for (var i = 0; i < NPCs.Length; ++i)
            {
                if(_npcs.Contains(NPCs[i]))
                    throw new ArgumentException($"This NPC has already been added to the list.");
                if(NPCs[i] == null)
                    throw new ArgumentNullException("Cannot add a null NPC");
                _npcs.Add(NPCs[i]);
            }
        }
        
        public virtual void Dispose()
        {
            Disposed = true;
            for (var i = 0; i < _children.Count; i++)
            {
                _children[i].Dispose();
            }
            _children.Clear();
            for (var i = 0; i < _npcs.Count; i++)
            {
                _npcs[i].Dispose();
            }
            _npcs.Clear();
        }
    }
}
