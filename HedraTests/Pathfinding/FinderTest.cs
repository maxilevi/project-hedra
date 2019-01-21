using System.Collections.Generic;
using Hedra.Engine.Pathfinding;
using NUnit.Framework;
using OpenTK;

namespace HedraTests.Pathfinding
{
    [TestFixture]
    public class FinderTest
    {
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