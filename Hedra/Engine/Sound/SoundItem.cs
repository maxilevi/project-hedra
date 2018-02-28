﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hedra.Engine.Sound
{
    public class SoundItem
    {
        public SoundItem(SoundSource Source)
        {
            this.Source = Source;
        }

        public SoundSource Source;
        public bool Locked;
    }
}
