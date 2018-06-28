/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 27/01/2017
 * Time: 04:55 a.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using Hedra.Engine.AISystem;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Generation;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Management;
using OpenTK;

namespace Hedra.Engine.Player
{
    /// <summary>
    ///     Description of PetManager.
    /// </summary>
    internal class PetManager
    {
        public Entity Pet { get; private set; }
        private readonly LocalPlayer _player;
        private readonly Timer _deadTimer;
        private Item _previousPetItem;
        private bool _timerSet;
        private bool _petPreviousEnabled = true;
        private float _petPreviousAlpha = 1;
        private bool _wasRiding;

        public PetManager(LocalPlayer Player)
        {
            _player = Player;
            _deadTimer = new Timer(8f);
        }

        public void Update()
        {
            if (Pet != null)
            {
                if (_player.IsRiding || !Pet.Model.Enabled)
                {
                    if (!_wasRiding && _player.IsRiding)
                    {
                        _petPreviousEnabled = Pet.Model.Enabled;
                        _petPreviousAlpha = Pet.Model.Alpha;
                    }
                    Pet.Model.Alpha = _player.Model.Alpha;
                    Pet.Model.Enabled = _player.Model.Enabled;
                }
                else
                {
                    Pet.Model.Alpha = _petPreviousAlpha;
                    Pet.Model.Enabled = _petPreviousEnabled;
                }
                _wasRiding = _player.IsRiding;
            }

            if (Pet != null && Pet.IsDead && !_timerSet)
            {
                _deadTimer.Reset();
                _timerSet = true;
            }

            var petItem = _player.Inventory.Pet;
            if (petItem != _previousPetItem || Pet != null && Pet.IsDead && _deadTimer.Tick())
            {
                this.SpawnPet(petItem);
                _timerSet = false;
            }
        }

        private void SpawnPet(Item PetItem)
        {
            Pet?.Dispose();
            Pet = null;
            _previousPetItem = PetItem;
            if (PetItem != null)
            {
                Pet = World.SpawnMob(PetItem.GetAttribute<string>("MobType"),
                    _player.BlockPosition + Vector3.UnitX * 12f, Utils.Rng);

                Pet.SearchComponent<DamageComponent>().Immune = true;
                Pet.Health = Pet.MaxHealth;

                Pet.Level = 1;
                Pet.RemoveComponent(Pet.SearchComponent<HealthBarComponent>());
                Pet.AddComponent(new HealthBarComponent(Pet, "Mount"));
                Pet.AddComponent(new MountAIComponent(Pet, _player,
                    (MountAIType) Enum.Parse(typeof(MountAIType), PetItem.GetAttribute<string>("MountAIType")))
                );
                Pet.RemoveComponent(Pet.SearchComponent<BasicAIComponent>());
                Pet.Removable = false;
                ((QuadrupedModel) Pet.Model).IsMountable = true;
            }
        }
    }
}