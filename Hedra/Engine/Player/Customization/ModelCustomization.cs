namespace Hedra.Engine.Player.Customization
{
    public class ModelCustomization
    {
        static ModelCustomization()
        {
            Default = new ModelCustomization();    
        }

        private ModelCustomization() { }

        public static ModelCustomization Default { get; }
    }
}
