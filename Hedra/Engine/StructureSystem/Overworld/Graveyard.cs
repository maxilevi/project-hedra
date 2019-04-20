/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 11/06/2017
 * Time: 03:58 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using Hedra.Engine.EntitySystem;
using Hedra.Engine.Generation;
using Hedra.Engine.Management;
using Hedra.Engine.WorldBuilding;
using OpenTK;

namespace Hedra.Engine.StructureSystem.Overworld
{
    /// <inheritdoc cref="BaseStructure" />
    /// <summary>
    /// Description of Cementary.
    /// </summary>
    public sealed class Graveyard : BaseStructure, IUpdatable
    {
        private readonly GraveyardAmbientHandler _ambientHandler;
        public Entity[] Enemies { get; set; }
        public bool Restored { get; private set; }
        public float Radius { get; }
        public HighlightedAreaWrapper AreaWrapper { get; set; }

        public Graveyard(Vector3 Position, float Radius) : base(Position)
        {
            this.Radius = Radius;
            _ambientHandler = new GraveyardAmbientHandler(this);
            UpdateManager.Add(this);
        }
        
        public void Update()
        {

            if (Enemies != null)
            {
                var allDead = true;
                for (var i = 0; i < Enemies.Length; i++)
                {
                    if (Enemies[i] != null && !Enemies[i].IsDead && (Enemies[i].BlockPosition - Position).Xz.LengthSquared
                        < Radius * Radius * .9f * .9f)
                    {
                        allDead = false;
                    }
                }

                this.Restored = allDead;
            }

            _ambientHandler.Update();
        }
        
        public override void Dispose()
        {
            if (Enemies != null)
            {
                for (var i = 0; i < Enemies.Length; i++)
                {
                    Enemies[i].Dispose();
                }
            }
            AreaWrapper?.Dispose();
            _ambientHandler.Dispose();
            UpdateManager.Remove(this);
            base.Dispose();
        }
    }
}
