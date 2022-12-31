/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 11/06/2017
 * Time: 03:58 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System.Linq;
using System.Numerics;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Generation;
using Hedra.Engine.Management;
using Hedra.Engine.QuestSystem;
using Hedra.Engine.WorldBuilding;

namespace Hedra.Engine.StructureSystem.Overworld
{
    /// <inheritdoc cref="BaseStructure" />
    /// <summary>
    ///     Description of Cementary.
    /// </summary>
    public sealed class Graveyard : BaseStructureWithChest, IStructureWithRadius, IUpdatable, ICompletableStructure
    {
        private readonly StructureAmbientHandler _ambientHandler;

        public Graveyard(Vector3 Position, float Radius) : base(Position)
        {
            this.Radius = Radius;
            _ambientHandler = new StructureAmbientHandler(this, GraveyardDesign.GraveyardSkyTime);
            UpdateManager.Add(this);
        }

        public Entity[] Enemies { get; set; }
        public float Radius { get; }
        public int EnemiesLeft => Enemies.Count(E => E == null || !E.IsDead);
        public HighlightedAreaWrapper AreaWrapper { get; set; }
        public bool Completed => Enemies != null && EnemiesLeft == 0;

        public override void Dispose()
        {
            if (Enemies != null)
                for (var i = 0; i < Enemies.Length; i++)
                    Enemies[i]?.Dispose();
            AreaWrapper?.Dispose();
            _ambientHandler.Dispose();
            UpdateManager.Remove(this);
            base.Dispose();
        }

        public void Update()
        {
            _ambientHandler.Update();
            if (Completed && AreaWrapper != null)
            {
                AreaWrapper?.Dispose();
                AreaWrapper = null;
            }
        }
    }
}