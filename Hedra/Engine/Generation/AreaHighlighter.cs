using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Hedra.Core;
using Hedra.Engine.IO;
using Hedra.Engine.Management;
using Hedra.Game;
using Hedra.Numerics;

namespace Hedra.Engine.Generation
{
    public class AreaHighlighter
    {
        public AreaHighlighter()
        {
            Highlights = new HighlightedArea[8];
            for (var i = 0; i < Highlights.Length; i++)
                Highlights[i] = new HighlightedArea();
        }

        public Vector4[] AreaPositions
        {
            get
            {
                var areaPositions = new List<Vector4>();
                for (var i = 0; i < Highlights.Length; i++)
                    areaPositions.Add(Highlights[i].AreaPosition);

                return areaPositions.ToArray();
            }
        }

        public Vector4[] AreaColors
        {
            get
            {
                var areaColors = new List<Vector4>();
                for (var i = 0; i < Highlights.Length; i++)
                    areaColors.Add(Highlights[i].AreaColor);

                return areaColors.ToArray();
            }
        }

        public int AreaCount => Highlights.Sum(H => !H.IsEmpty ? 1 : 0);

        public HighlightedArea[] Highlights { get; }

        public HighlightedAreaWrapper HighlightAreaPermanently(Vector3 Position, Vector4 Color, float Radius)
        {
            return HighlightArea(Position, Color, Radius, -1);
        }

        public HighlightedAreaWrapper HighlightArea(Vector3 Position, Vector4 Color, float Radius, float Seconds)
        {
            const float fadeSpeed = 32f;

            var area = new HighlightedArea(Position, Color, Radius);
            int i;
            for (i = 0; i < Highlights.Length; i++)
            {
                if (!Highlights[i].IsEmpty) continue;
                Highlights[i] = area;
                break;
            }

            var k = i;
            if (k >= Highlights.Length)
            {
                Log.WriteLine(
                    "There are no available highlights. Skipping..."); //throw new ArgumentException($"There are no available highlights");
                return null;
            }

            var isPermanent = Seconds < 0;
            if (isPermanent)
            {
                var wrapper = new HighlightedAreaWrapper();
                RoutineManager.StartRoutine(CycleHighlight, Highlights[k], World.Seed, wrapper);
                return wrapper;
            }

            area.Position = Position - Vector3.UnitY * 16;
            RoutineManager.StartRoutine(FadeHighlight, area, Position, Seconds, false);
            const float fadeTime = 1.5f;
            TaskScheduler.After(Seconds,
                () => RoutineManager.StartRoutine(FadeHighlight, Highlights[k], area.Position - Vector3.UnitY * 8,
                    fadeTime, true));
            return null;
        }

        private IEnumerator FadeHighlight(object[] Params)
        {
            var area = (HighlightedArea)Params[0];
            var targetPosition = (Vector3)Params[1];
            var time = (float)Params[2];
            var isFadingOut = (bool)Params[3];

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

                area.Reset();
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
            var seed = (int)Params[1];
            var wrapper = (HighlightedAreaWrapper)Params[2];
            var areaClone = wrapper.Area = new HighlightedArea(area.Position, area.Color, area.Radius);
            var player = GameManager.Player;

            while (World.Seed == seed && !areaClone.Stop)
            {
                if ((player.Position.Xz() - areaClone.Position.Xz()).LengthFast() < areaClone.Radius + 256)
                {
                    if (area == null)
                    {
                        for (var i = 0; i < Highlights.Length; i++)
                            if (Highlights[i].IsEmpty)
                            {
                                area = Highlights[i];
                                break;
                            }

                        if (area == null) throw new ArgumentException("Highlighted areas exceded maxium count.");
                        area.Copy(areaClone);
                    }
                }
                else
                {
                    if (area != null)
                    {
                        area.Reset();
                        area = null;
                    }
                }

                area?.Copy(areaClone);
                yield return null;
            }

            if (area != null) area.Reset();
        }

        public void Reset()
        {
            for (var i = 0; i < Highlights.Length; i++)
            {
                Highlights[i].Position = Vector3.Zero;
                Highlights[i].Color = Vector4.Zero;
                Highlights[i].Radius = 0f;
            }
        }
    }
}