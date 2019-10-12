using System.Collections.Generic;
using System.Drawing;
using Hedra.Engine.Pathfinding;
using NUnit.Framework;
using OpenToolkit.Mathematics;

namespace HedraTests.Pathfinding
{
    [TestFixture]
    public class FinderTest
    {
        [Test]
        public void TestPathIsComputedAsExpected()
        {
            var grid = new Grid(5, 4);
            grid.BlockCell(new Vector2(2, 0));
            grid.BlockCell(new Vector2(2, 1));
            grid.BlockCell(new Vector2(2, 2));
            var path = grid.GetPath(Vector2.Zero, new Vector2(4, 0));
            Assert.AreEqual(new []
            {
                new Vector2(0, 0),
                new Vector2(1, 1),
                new Vector2(1, 2),
                new Vector2(2, 3),
                new Vector2(3, 2),
                new Vector2(4, 1),
                new Vector2(4, 0),
            }, path);
        }
        
        [Test]
        public void TestIsConnectedWith()
        {
            var grid = new Grid(3, 2);
            Assert.True(Finder.IsConnectedWith(grid, Vector2.Zero, new Vector2(2, 1), new HashSet<Vector2>()));
        }
        
        [Test]
        public void TestNearestIsDoneInSpiral()
        {
            var grid = new Grid(10, 10);
            for (var x = 2; x < 8; x++)
            {
                for (var y = 2; y < 8; y++)
                {
                    grid.BlockCell(new Vector2(x, y));
                }
            }
            var center = new Vector2((int)(10 / 2f), (int) (10 / 2f));
            var nearest = Finder.NearestUnblockedCell(grid, center, Vector2.Zero);
            Assert.AreEqual(new Vector2(8, 5), nearest);
        }
    }
}