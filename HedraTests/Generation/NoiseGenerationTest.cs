using System;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using Hedra.Engine.Game;
using Hedra.Engine.Generation;
using Hedra.Engine.Native;
using NUnit.Framework;

namespace HedraTests.Generation
{
    [NonParallelizable]
    public class NoiseGenerationTest
    {
        private delegate float[] GenerateSet(Vector3 Offset, Vector3 Size, Vector3 Scale, float Frequency);

        [TestCase("Perlin")]
        [TestCase("PerlinFractal")]
        [TestCase("Simplex")]
        [TestCase("SimplexFractal")]
        [TestCase("Cubic")]
        [TestCase("CubicFractal")]
        [TestCase("Value")]
        [TestCase("ValueFractal")]
        [TestCase("Cellular")]
        public void SetsMatchNativeReferenceBitForBit(string Kind)
        {
            GameLoader.LoadArchitectureSpecificFilesIfNecessary(GameLoader.AppPath);
            using var reference = new ReferenceNoise(Kind);
            using var noise = new FastNoiseSIMD(43);
            var generate = Bind(noise, Kind);
            foreach (var seed in new[] { 43, -546068172 })
            foreach (PerturbType perturb in Enum.GetValues(typeof(PerturbType)))
            {
                noise.Seed = seed;
                noise.PerturbType = perturb;
                noise.PerturbAmp = .0075f;
                noise.PerturbFrequency = 32;
                reference.Configure(seed, perturb);
                foreach (var size in new[] { Vector3.One, new Vector3(2, 3, 2), new Vector3(7, 3, 5),
                    new Vector3(8, 4, 8), new Vector3(19, 9, 19), new Vector3(35, 1, 35) })
                {
                    var offset = new Vector3(-4096.25f, 17.5f, 12288.5f);
                    var scale = new Vector3(8, 32, 8);
                    var expected = reference.Generate(offset, size, scale, .0025f);
                    var actual = generate(offset, size, scale, .0025f);
                    Assert.AreEqual(expected.Length, actual.Length);
                    for (var i = 0; i < actual.Length; ++i)
                        Assert.AreEqual(BitConverter.SingleToInt32Bits(expected[i]), BitConverter.SingleToInt32Bits(actual[i]),
                            $"{Kind} seed={seed} perturb={perturb} size={size} index={i}");
                }
            }
        }

        [TestCase("Perlin", 19, 33)]
        [TestCase("Simplex", 19, 33)]
        [TestCase("PerlinFractal", 19, 33)]
        [TestCase("Perlin", 35, 1)]
        [TestCase("Simplex", 35, 1)]
        [TestCase("PerlinFractal", 35, 1)]
        public void MeasureNoiseGeneration(string Kind, int Width, int Height)
        {
            if (Environment.GetEnvironmentVariable("HEDRA_NOISE_BENCHMARK") != "1") Assert.Ignore("Opt-in CPU benchmark");
            GameLoader.LoadArchitectureSpecificFilesIfNecessary(GameLoader.AppPath);
            using var reference = new ReferenceNoise(Kind);
            using var noise = new FastNoiseSIMD(43);
            reference.Configure(43, PerturbType.None);
            var generate = Bind(noise, Kind);
            var offset = new Vector3(-4096, 0, 12288);
            var size = new Vector3(Width, Height, Width);
            var scale = new Vector3(8, 32, 8);
            var baseline = new double[15];
            var optimized = new double[15];
            Func<float[]> old = () => reference.Generate(offset, size, scale, .0025f);
            Func<float[]> next = () => generate(offset, size, scale, .0025f);
            for (var i = 0; i < 20; ++i) { old(); next(); }
            for (var i = 0; i < baseline.Length; ++i)
            {
                // Alternate order to avoid favoring one implementation during warmup.
                if ((i & 1) == 0) { baseline[i] = Measure(old); optimized[i] = Measure(next); }
                else { optimized[i] = Measure(next); baseline[i] = Measure(old); }
            }
            Array.Sort(baseline);
            Array.Sort(optimized);
            TestContext.Progress.WriteLine($"NOISE_BENCH kind={Kind} size={size} baseline_ms={baseline[7]:F5} optimized_ms={optimized[7]:F5} speedup={baseline[7] / optimized[7]:F2} runtime={RuntimeInformation.FrameworkDescription}");
        }

        private static double Measure(Func<float[]> Generate)
        {
            const int iterations = 300;
            var start = Stopwatch.GetTimestamp();
            for (var i = 0; i < iterations; ++i) Generate();
            return (Stopwatch.GetTimestamp() - start) * 1000.0 / Stopwatch.Frequency / iterations;
        }

        private static GenerateSet Bind(FastNoiseSIMD Noise, string Kind) =>
            (GenerateSet)typeof(FastNoiseSIMD).GetMethod($"Get{Kind}SetWithFrequency",
                new[] { typeof(Vector3), typeof(Vector3), typeof(Vector3), typeof(float) })
                .CreateDelegate(typeof(GenerateSet), Noise);

        // Optional saved native library makes this a regression check across native
        // rebuilds. Without it, the scalar reference still checks managed copying.
        private sealed class ReferenceNoise : IDisposable
        {
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            private delegate IntPtr Create(int Seed);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            private delegate void Release(IntPtr Pointer);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            private delegate void SetInt(IntPtr Pointer, int Value);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            private delegate void SetPerturb(IntPtr Pointer, PerturbType Value);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            private delegate void SetFloat(IntPtr Pointer, float Value);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            private delegate IntPtr GetSet(IntPtr Pointer, float X, float Y, float Z,
                int Width, int Height, int Depth, float ScaleX, float ScaleY, float ScaleZ);

            private readonly IntPtr _library;
            private readonly IntPtr _noise;
            private readonly Release _delete, _free;
            private readonly SetInt _seed;
            private readonly SetPerturb _perturb;
            private readonly SetFloat _frequency, _amplitude, _perturbFrequency;
            private readonly GetSet _get;

            public ReferenceNoise(string Kind)
            {
                var path = Environment.GetEnvironmentVariable("HEDRA_NOISE_BASELINE_NATIVE");
                if (!string.IsNullOrEmpty(path)) _library = NativeLibrary.Load(path);
                T Resolve<T>(string Name) where T : Delegate => _library != IntPtr.Zero
                    ? Marshal.GetDelegateForFunctionPointer<T>(NativeLibrary.GetExport(_library, Name))
                    : (T)typeof(HedraCoreNative).GetMethod(Name).CreateDelegate(typeof(T));
                _delete = Resolve<Release>("fastnoise_deleteObject");
                _free = Resolve<Release>("fastnoise_freeNoise");
                _seed = Resolve<SetInt>("fastnoise_setSeed");
                _perturb = Resolve<SetPerturb>("fastnoise_setPerturbType");
                _frequency = Resolve<SetFloat>("fastnoise_setFrequency");
                _amplitude = Resolve<SetFloat>("fastnoise_setPerturbAmp");
                _perturbFrequency = Resolve<SetFloat>("fastnoise_setPerturbFrequency");
                _get = Resolve<GetSet>($"fastnoise_get{Kind}Set");
                _noise = Resolve<Create>("fastnoise_createObject")(43);
            }

            public void Configure(int Seed, PerturbType Perturb)
            {
                _seed(_noise, Seed);
                _perturb(_noise, Perturb);
                _amplitude(_noise, .0075f);
                _perturbFrequency(_noise, 32);
            }

            public unsafe float[] Generate(Vector3 Offset, Vector3 Size, Vector3 Scale, float Frequency)
            {
                _frequency(_noise, Frequency);
                var pointer = _get(_noise, Offset.X, Offset.Y, Offset.Z, (int)Size.X, (int)Size.Y, (int)Size.Z,
                    Scale.X, Scale.Y, Scale.Z);
                try
                {
                    var buffer = new float[(int)(Size.X * Size.Y * Size.Z)];
                    for (var i = 0; i < buffer.Length; ++i) buffer[i] = ((float*)pointer)[i];
                    return buffer;
                }
                finally { _free(pointer); }
            }

            public void Dispose()
            {
                _delete(_noise);
                if (_library != IntPtr.Zero) NativeLibrary.Free(_library);
            }
        }
    }
}
