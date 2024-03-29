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
using System.Numerics;
using Hedra.Engine.Core;
using Hedra.EntitySystem;

namespace Hedra.Engine.WorldBuilding
{
    /// <inheritdoc />
    /// <summary>
    ///     Description of Structure.
    /// </summary>
    public abstract class BaseStructure : IDisposable, IStructure, ISearchable
    {
        private readonly List<BaseStructure> _children;
        private readonly object _childrenLock = new object();
        private readonly object _npcLock = new object();
        private readonly List<IEntity> _npcs;

        protected BaseStructure(List<BaseStructure> Children, List<IEntity> Npcs)
        {
            _children = Children;
            _npcs = Npcs;
        }

        protected BaseStructure(Vector3 Position)
        {
            this.Position = Position;
            _npcs = new List<IEntity>();
            _children = new List<BaseStructure>();
        }

        public IEntity[] NPCs
        {
            get
            {
                lock (_npcLock)
                {
                    return _npcs.ToArray();
                }
            }
        }

        public virtual void Dispose()
        {
            Disposed = true;
            lock (_childrenLock)
            {
                for (var i = 0; i < _children.Count; i++) _children[i]?.Dispose();
                _children.Clear();
            }

            lock (_npcLock)
            {
                for (var i = 0; i < _npcs.Count; i++) _npcs[i]?.Dispose();
                _npcs.Clear();
            }
        }

        public virtual Vector3 Position { get; set; }
        public bool Disposed { get; protected set; }

        public void AddChildren(params BaseStructure[] Children)
        {
            lock (_childrenLock)
            {
                for (var i = 0; i < Children.Length; ++i)
                {
                    if (Children[i] == null)
                        throw new ArgumentNullException("Cannot add a null children");
                    _children.Add(Children[i]);
                }
            }
        }

        public BaseStructure[] Children
        {
            get
            {
                lock (_childrenLock)
                {
                    return _children.ToArray();
                }
            }
        }

        public T SearchFirst<T>() where T : BaseStructure
        {
            return Search<T>().First();
        }

        public T[] Search<T>() where T : BaseStructure
        {
            lock (_childrenLock)
            {
                var list = new List<T>();
                for (var i = 0; i < _children.Count; ++i)
                {
                    if (_children[i] is T) list.Add((T)_children[i]);

                    list.AddRange(_children[i].Search<T>());
                }

                return list.ToArray();
            }
        }

        public void AddNPCs(params IEntity[] NPCs)
        {
            lock (_npcLock)
            {
                for (var i = 0; i < NPCs.Length; ++i)
                {
                    if (_npcs.Contains(NPCs[i]))
                        throw new ArgumentException("This NPC has already been added to the list.");
                    if (NPCs[i] == null)
                        throw new ArgumentNullException("Cannot add a null NPC");
                    _npcs.Add(NPCs[i]);
                }
            }
        }
    }
}