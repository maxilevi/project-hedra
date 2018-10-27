/*
 * Author: Zaphyk
 * Date: 05/02/2016
 * Time: 05:12 a.m.
 *
 */
using System;
using Hedra.Engine.Scenes;

namespace Hedra.Engine.Management
{
    /// <summary>
    /// An interface which is implemented by objects which can be renderer through the DrawManager
    /// </summary>
    public interface IRenderable
    {
        void Draw();
    }
}
