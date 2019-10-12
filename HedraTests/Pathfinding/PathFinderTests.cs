using Hedra.Engine.Pathfinding;
using NUnit.Framework;
using OpenToolkit.Mathematics;

namespace HedraTests.Pathfinding
{
    [TestFixture]
    public class PathFinderTests
    {
        [Test]
        public void NonExistingPathShouldReturnEmptyArray()
        {
            var grid = GridCatalog.GridWithEnclosedCenterTile();
            var path = grid.GetPath(new Vector2(0, 0), new Vector2(4, 4));

            Assert.NotNull(path);
            Assert.IsEmpty(path);
        }

        [Test]
        public void ShouldFindPathInUnobstructedGrid()
        {
            var grid = GridCatalog.UnobstructedGrid();
            var path = grid.GetPath(new Vector2(0, 0), new Vector2(8, 8));

            Assert.NotNull(path);
            Assert.AreEqual(9, path.Length);
        }

        [Test]
        public void ShouldFindPathWithNonZeroStartingPosition()
        {
            var grid = GridCatalog.UnobstructedGrid();
            var path = grid.GetPath(new Vector2(3, 3), new Vector2(5, 3));

            Assert.NotNull(path);
            Assert.AreEqual(3, path.Length);
        }

        [Test]
        public void ShouldRespectBlockedCells()
        {
            var grid = GridCatalog.GridWithBlockedCenterTile();
            var path = grid.GetPath(new Vector2(0, 0), new Vector2(8, 8));

            Assert.NotNull(path);
            Assert.That(path, Has.No.Member(new Vector2(4, 4)));
            Assert.AreEqual(10, path.Length);
        }

        [Test]
        public void ShouldRespectCellCost()
        {
            var grid = GridCatalog.GridWithHighCostCenterTile();
            var path = grid.GetPath(new Vector2(0, 0), new Vector2(8, 8));

            Assert.NotNull(path);
            Assert.That(path, Has.No.Member(new Vector2(4, 4)));
            Assert.AreEqual(10, path.Length);
        }
/*
        [Test]
        public void ShouldRespectIterationLimit()
        {
            var grid = GridCatalog.UnobstructedGrid();
            var path = grid.GetPath(new Vector2(0, 0), new Vector2(8, 8), MovementPatterns.Full, 8);

            Assert.NotNull(path);
            Assert.IsEmpty(path);            
        }*/

        [Test]
        public void ShouldRespectMovementPattern()
        {
            var grid = GridCatalog.GridWithObstructedDiagonals();
            var path = grid.GetPath(new Vector2(0, 0), new Vector2(8, 8), MovementPatterns.LateralOnly);

            Assert.NotNull(path);
            Assert.IsEmpty(path);
        }
    }
}
