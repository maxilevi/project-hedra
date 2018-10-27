using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hedra.Engine.Rendering
{
    public interface IDisposeAnimation
    {
        /// <summary>
        /// Prepares the model for using the death shader
        /// </summary>
        void DisposeAnimation();

        /// <summary>
        /// Gets or sets the time of the dispose animation
        /// </summary>
        float DisposeTime { get; set; }

        /// <summary>
        /// Reset rendering to before the dispose animation
        /// </summary>
        void Recompose();
    }
}
