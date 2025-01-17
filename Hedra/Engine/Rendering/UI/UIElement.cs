/*
 * Author: Zaphyk
 * Date: 27/02/2016
 * Time: 08:14 p.m.
 *
 */

using System;
using System.Numerics;

namespace Hedra.Engine.Rendering.UI
{
    /// <summary>
    ///     Description of UIElement.
    /// </summary>
    public interface UIElement : IDisposable
    {
        Vector2 Position { get; set; }

        Vector2 Scale { get; set; }
        void Enable();

        void Disable();

        void Dispose();
    }
}