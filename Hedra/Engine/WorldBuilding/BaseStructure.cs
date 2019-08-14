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
using OpenTK;
using Hedra.Engine.Management;

namespace Hedra.Engine.WorldBuilding
{
    /// <inheritdoc />
    /// <summary>
    /// Description of Structure.
    /// </summary>
    public abstract class BaseStructure : IDisposable, IStructure, ISearchable
    {
        private readonly List<BaseStructure> _children;
        public BaseStructure[] Children => _children.ToArray();
        public virtual Vector3 Position { get; set; }
        public bool Disposed { get; protected set; }

        protected BaseStructure(Vector3 Position)
        {
            this.Position = Position;
            _children = new List<BaseStructure>();
        }
        
        public void AddChildren(params BaseStructure[] Children)
        {
            for (var i = 0; i < Children.Length; ++i)
            {
                if(Children[i] == null)
                    throw new ArgumentNullException($"Cannot add a null children");
                _children.AddRange(Children);
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
        }
    }
}
