namespace Hedra.Engine.Sound
{
    public class SoundItem
    {
        public bool Locked;

        public SoundSource Source;

        public SoundItem(SoundSource Source)
        {
            this.Source = Source;
        }
    }
}