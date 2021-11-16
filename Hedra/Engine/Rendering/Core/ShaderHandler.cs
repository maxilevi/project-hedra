namespace Hedra.Engine.Rendering.Core
{
    public class ShaderHandler
    {
        public uint Id { get; private set; }

        public int Skipped { get; private set; }

        public void ResetStats()
        {
            Skipped = 0;
        }

        public void Use(uint Id)
        {
            if (this.Id == Id)
            {
                ++Skipped;
                return;
            }

            Renderer.Provider.UseProgram(Id);
            this.Id = Id;
        }
    }
}