﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hedra.Engine.EntitySystem
{
    /// <inheritdoc cref="IEffectComponent" />
    /// <summary>
    /// Description of FireComponent.
    /// </summary>
    internal class KnockComponent : EntityComponent, IEffectComponent
    {
        public int Chance { get; set; } = 20;
        public float Damage { get; set; } = 30;
        public float Duration { get; set; } = 3;

        private float _cooldown;

        public KnockComponent(Entity Parent) : base(Parent)
        {
            Parent.OnAttacking += this.Apply;
        }

        public override void Update()
        {
            _cooldown -= Time.IndependantDeltaTime;
        }

        public void Apply(Entity Victim, float Amount)
        {
            if (_cooldown > 0) return;

            bool shouldKnock = Utils.Rng.NextFloat() <= Chance * 0.01f;

            if (!shouldKnock) return;
            if (!Victim.Knocked) Victim.KnockForSeconds(Duration);
            _cooldown = Duration + 2;
        }
    }
}
