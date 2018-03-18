/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 27/01/2017
 * Time: 04:55 a.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Generation;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Networking;
using OpenTK;

namespace Hedra.Engine.Player
{
    /// <summary>
    ///     Description of PetManager.
    /// </summary>
    public class PetManager
    {
        private readonly LocalPlayer _player;
        private Item _previousMount;
        private float DeadTimer = 48;
        public Entity MountEntity;

        public PetManager(LocalPlayer Player)
        {
            _player = Player;
        }

        public void Update()
        {
            var mountItem = _player.Inventory.Mount;
            if (MountEntity != null)
            {
                MountEntity.Model.Enabled = _player.Model.Enabled;
                mountItem.SetAttribute("Health", MountEntity.MaxHealth);
            }

            if (MountEntity != null && (MountEntity != null && MountEntity.IsDead 
                || _previousMount != mountItem 
                || (MountEntity.BlockPosition.Xz - _player.BlockPosition.Xz).LengthSquared > 192 * 192) && !_player.IsRiding)
            {
                if (MountEntity != null && MountEntity.IsDead)
                {
                    DeadTimer -= Time.ScaledFrameTimeSeconds;
                    if (DeadTimer > 0)
                        return;
                }
                DeadTimer = 4; //60
                if (MountEntity != null)
                {
                    if (MountEntity.IsDead)
                        mountItem.SetAttribute("Health", MountEntity.MaxHealth);
                    MountEntity.Dispose();
                }
                MountEntity = World.SpawnMob(mountItem.GetAttribute<string>("MobType"),
                    _player.BlockPosition + Vector3.UnitX * 12f,mountItem.GetAttribute<int>("MountSeed"));

                MountEntity.SearchComponent<DamageComponent>().Immune = true;
                MountEntity.Health = MountEntity.MaxHealth;


                MountEntity.Level = 1;
                MountEntity.RemoveComponent(MountEntity.SearchComponent<HealthBarComponent>());
                MountEntity.AddComponent(new HealthBarComponent(MountEntity, "Mount"));
                MountEntity.SearchComponent<HealthBarComponent>().DistanceFromBase = 3;
                MountEntity.AddComponent(new MountAIComponent(MountEntity, _player, mountItem.GetAttribute<MountAIType>("MountAIType")));
                MountEntity.RemoveComponent(MountEntity.SearchComponent<AIComponent>());
                MountEntity.Removable = false;
                ((QuadrupedModel) MountEntity.Model).IsMountable = true;
                _previousMount = mountItem;
            }
            else if (mountItem == null)
            {
                _previousMount = null;
                MountEntity?.Dispose();
                MountEntity = null;
            }
        }
    }
}