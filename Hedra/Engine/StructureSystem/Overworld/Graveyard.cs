/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 11/06/2017
 * Time: 03:58 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System.Linq;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Generation;
using Hedra.Engine.Localization;
using Hedra.Engine.Management;
using Hedra.Engine.QuestSystem;
using Hedra.Engine.WorldBuilding;
using OpenTK;

namespace Hedra.Engine.StructureSystem.Overworld
{
    /// <inheritdoc cref="BaseStructure" />
    /// <summary>
    /// Description of Cementary.
    /// </summary>
    public sealed class Graveyard : BaseStructureWithChest, IUpdatable, ICompletableStructure
    {
        private readonly GraveyardAmbientHandler _ambientHandler;
        public Entity[] Enemies { get; set; }
        public float Radius { get; }
        public bool Completed => EnemiesLeft == 0;
        public int EnemiesLeft => Enemies.Count(E => !E.IsDead);
        public HighlightedAreaWrapper AreaWrapper { get; set; }

        public Graveyard(Vector3 Position, float Radius) : base(Position)
        {
            this.Radius = Radius;
            _ambientHandler = new GraveyardAmbientHandler(this);
            UpdateManager.Add(this);
        }
        
        public void Update()
        {
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
        
        public ItemDescription DeliveryItem => 
            ItemDescription.FromItem(Chest.ItemSpecification, Translations.Get("quest_pickup_chest_description", Chest.ItemSpecification.DisplayName));
    }
}
