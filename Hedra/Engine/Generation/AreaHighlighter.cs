using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using Hedra.Core;
using Hedra.Engine.Game;
using Hedra.Engine.Management;
using Hedra.Engine.Player;
using Hedra.Engine.IO;
using Hedra.Game;
using OpenTK;

namespace Hedra.Engine.Generation
{
    public class AreaHighlighter
    {
        private readonly HighlightedArea[] _highlightedAreas;

        public AreaHighlighter()
        {
            _highlightedAreas = new HighlightedArea[8];
            for (var i = 0; i < _highlightedAreas.Length; i++)
                _highlightedAreas[i] = new HighlightedArea();
        }

        public Vector4[] AreaPositions
        {
            get
            {
                var areaPositions = new List<Vector4>();
                for (var i = 0; i < _highlightedAreas.Length; i++)
                    areaPositions.Add(_highlightedAreas[i].AreaPosition);

                return areaPositions.ToArray();
            }
        }

        public Vector4[] AreaColors
        {
            get
            {
                var areaColors = new List<Vector4>();
                for (var i = 0; i < _highlightedAreas.Length; i++)
                    areaColors.Add(_highlightedAreas[i].AreaColor);

                return areaColors.ToArray();
            }
        }

        public int AreaCount => _highlightedAreas.Sum(H => !H.IsEmpty ? 1 : 0);
        
        public HighlightedAreaWrapper HighlightArea(Vector3 Position, Vector4 Color, float Radius, float Seconds)
        {
            const float fadeSpeed = 32f;

            var area = new HighlightedArea(Position, Color, Radius);
            int i;
            for (i = 0; i < _highlightedAreas.Length; i++)
            {

                if (!_highlightedAreas[i].IsEmpty) continue;
                _highlightedAreas[i] = area;
                break;
            }
            var k = i;
            if (k >= _highlightedAreas.Length)
            {
                Log.WriteLine($"There are no available highlights. Skipping...");//throw new ArgumentException($"There are no available highlights");
                return null;
            }

            var isPermanent = Seconds < 0;
            if (isPermanent)
            {
                var wrapper = new HighlightedAreaWrapper();
                RoutineManager.StartRoutine(CycleHighlight, _highlightedAreas[k], World.Seed, wrapper);
                return wrapper;
            }
            area.Position = Position - Vector3.UnitY * 16;
            RoutineManager.StartRoutine(FadeHighlight, area, Position, Seconds, false);
            const float fadeTime = 1.5f;
            TaskScheduler.After(Seconds, () => RoutineManager.StartRoutine(FadeHighlight, _highlightedAreas[k], area.Position - Vector3.UnitY * 8, fadeTime, true));        
            return null;
        }

        private IEnumerator FadeHighlight(object[] Params)
        {
            var area = (HighlightedArea)Params[0];
            var targetPosition = (Vector3)Params[1];
            var time = (float) Params[2];
            var isFadingOut = (bool) Params[3];

            var passedTime = 0f;
            const float fadeSpeed = 32;
            if (isFadingOut)
            {
                while (passedTime < time)
                {
                    area.Position -= Vector3.UnitY * fadeSpeed * Time.DeltaTime;
                    passedTime += Time.DeltaTime;
                    yield return null;
                }
                area.Radius = 0;
                area.Color = Vector4.Zero;
                area.Position = Vector3.Zero;
            }
            else
            {
                while (passedTime < time)
                {
                    area.Position = Mathf.Lerp(area.Position, targetPosition, Time.DeltaTime);
                    passedTime += Time.DeltaTime;
                    yield return null;
                }
            }
        }

        private IEnumerator CycleHighlight(object[] Params)
        {
            var area = (HighlightedArea)Params[0];
            var seed = (int) Params[1];
            var wrapper = (HighlightedAreaWrapper) Params[2];
            var areaClone = wrapper.Area = new HighlightedArea(area.Position, area.Color, area.Radius);
            var player = GameManager.Player;

            while (World.Seed == seed && !areaClone.Stop)
            {
                if ((player.Position.Xz - areaClone.Position.Xz).LengthFast < areaClone.Radius + 256)
                {
                    if (area == null)
                    {
                        for (var i = 0; i < _highlightedAreas.Length; i++)
                        {
                            if (_highlightedAreas[i].IsEmpty)
                            {
                                area = _highlightedAreas[i];
                                break;
                            }
                        }
                        if (area == null) throw new ArgumentException("Highlighted areas exceded maxium count.");
                        area.Position = areaClone.Position;
                        area.Radius = areaClone.Radius;
                        area.Color = areaClone.Color;
                    }
                }
                else
                {
                    if (area != null)
                    {
                        area.Position = Vector3.Zero;
                        area.Radius = 0;
                        area.Color = Vector4.Zero;
                        area = null;
                    }
                }
                yield return null;
            }

            if (area != null)
            {
                area.Position = Vector3.Zero;
                area.Radius = 0;
                area.Color = Vector4.Zero;
            }

        }

        public void Reset()
        {
            for (int i = 0; i < _highlightedAreas.Length; i++)
            {
                _highlightedAreas[i].Position = Vector3.Zero;
                _highlightedAreas[i].Color = Vector4.Zero;
                _highlightedAreas[i].Radius = 0f;
            }
        }
    }
}
