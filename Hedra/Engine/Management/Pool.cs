/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 30/09/2017
 * Time: 02:23 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System.Collections.Generic;
using Hedra.Core;
using Hedra.Engine.IO;

namespace Hedra.Engine.Management
{
    /// <summary>
    ///     Description of ByteArray.
    /// </summary>
    public class Pool<T> where T : new()
    {
        private const int GCDelay = 1;
        private readonly List<PoolItem<T>> _items;

        public Pool()
        {
            _items = new List<PoolItem<T>>
            {
                new PoolItem<T>
                {
                    Item = new T(),
                    Locked = true
                }
            };
        }

        public T Grab()
        {
            var selectedItem = default(PoolItem<T>);
            for (var i = 0; i < _items.Count; i++)
                if (!_items[i].Locked)
                {
                    selectedItem = _items[i];
                    break;
                }

            if (selectedItem == null)
            {
                selectedItem = new PoolItem<T>
                {
                    Item = new T(),
                    Locked = true
                };
                _items.Add(selectedItem);
                Log.WriteLine($"[CACHE] Registered new T, Total = {_items.Count}");
            }

            ScheduleGC(selectedItem);
            return selectedItem.Item;
        }

        private static void ScheduleGC(PoolItem<T> Item)
        {
            TaskScheduler.DelayFrames(GCDelay, () => Item.Locked = false);
        }
    }

    public class PoolItem<T> where T : new()
    {
        public T Item { get; set; }
        public bool Locked { get; set; }
    }
}