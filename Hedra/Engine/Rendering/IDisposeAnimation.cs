namespace Hedra.Engine.Rendering
{
    public interface IDisposeAnimation
    {
        /// <summary>
        ///     Gets or sets the time of the dispose animation
        /// </summary>
        float DisposeTime { get; set; }

        /// <summary>
        ///     Prepares the model for using the death shader
        /// </summary>
        void DisposeAnimation();

        /// <summary>
        ///     Reset rendering to before the dispose animation
        /// </summary>
        void Recompose();
    }
}