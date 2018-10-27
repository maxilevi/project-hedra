/*
 * Author: Zaphyk
 * Date: 22/02/2016
 * Time: 12:29 a.m.
 *
 */
using System;

namespace Hedra.Engine.Rendering.Effects
{
    /// <summary>
    /// Description of IEffect.
    /// </summary>
    public interface IEffect
    {
        bool Enabled{get; set;}
        void Draw();
        void Clear();
        void CaptureData();
        void UnCaptureData();
    }
}
