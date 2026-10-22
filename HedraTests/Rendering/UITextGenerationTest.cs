using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Core;
using Hedra.Engine.Rendering.UI;
using Hedra.Engine.Windowing;
using Hedra.Game;
using Hedra.Rendering;
using Hedra.Rendering.UI;
using NUnit.Framework;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using TextOptions = Hedra.Engine.Rendering.UI.TextOptions;

namespace HedraTests.Rendering
{
    [TestFixture, NonParallelizable]
    public class UITextGenerationTest
    {
        private readonly List<GUIText> _labels = new List<GUIText>();
        private RecordingGL _gl;
        private CountingTextProvider _text;
        private IGLProvider _previousGL;
        private ITextProvider _previousText;
        private ITexture2DProvider _previousTextures;
        private IAssetProvider _previousAssets;
        private int _previousThread;
        private int _previousWidth;
        private int _previousHeight;
        private float _previousScaling;
        private bool _previousTesting;
        private int _cacheCount;

        [SetUp]
        public void Setup()
        {
            _previousGL = Renderer.Provider;
            _previousText = GUIText.Provider;
            _previousTextures = Graphics2D.Provider;
            _previousAssets = AssetManager.Provider;
            _previousThread = Hedra.Engine.Loader.Hedra.RenderingThreadId;
            _previousWidth = GameSettings.Width;
            _previousHeight = GameSettings.Height;
            _previousScaling = GameSettings.UIScaling;
            _previousTesting = GameSettings.TestingMode;
            GameSettings.TestingMode = true;
            GameSettings.Width = 1920;
            GameSettings.Height = 1080;
            GameSettings.UIScaling = 1;
            SetRenderThread(Thread.CurrentThread.ManagedThreadId);
            _gl = new RecordingGL();
            _text = new CountingTextProvider();
            Renderer.Provider = _gl;
            GUIText.Provider = _text;
            Graphics2D.Provider = new CountingTextureProvider();
            AssetManager.Provider = new DummyAssetProvider();
            _cacheCount = TextCache.Count;
        }

        [TearDown]
        public void Teardown()
        {
            foreach (var label in _labels) label.Dispose();
            _labels.Clear();
            Executer.Update();
            var remaining = TextCache.Count;
            Renderer.Provider = _previousGL;
            GUIText.Provider = _previousText;
            Graphics2D.Provider = _previousTextures;
            AssetManager.Provider = _previousAssets;
            SetRenderThread(_previousThread);
            GameSettings.Width = _previousWidth;
            GameSettings.Height = _previousHeight;
            GameSettings.UIScaling = _previousScaling;
            GameSettings.TestingMode = _previousTesting;
            Assert.AreEqual(_cacheCount, remaining, "Labels must release all text-cache references");
        }

        private static void SetRenderThread(int Id)
        {
            typeof(Hedra.Engine.Loader.Hedra).GetProperty(nameof(Hedra.Engine.Loader.Hedra.RenderingThreadId))
                .SetValue(null, Id);
        }

        private GUIText Label(string Text = "ui-generation-regression", Font Font = null)
        {
            var label = new GUIText(Text, Vector2.Zero, Color.White, Font ?? FontCache.GetNormal(12));
            _labels.Add(label);
            return label;
        }

        [Test]
        public void FractionalFontSizesRemainDistinct()
        {
            var first = FontCache.GetNormal(12.1f);
            var second = FontCache.GetNormal(12.9f);
            Assert.AreEqual(12.1f, first.Size);
            Assert.AreEqual(12.9f, second.Size);
            Assert.AreNotSame(first, second);
            Assert.AreSame(second, FontCache.GetNormal(12.9f));
        }

        [Test]
        public void FontCacheSupportsConcurrentRequests()
        {
            Parallel.For(0, 100, i => Assert.AreEqual(17.125f, FontCache.GetBold(17.125f).Size));
        }

        [Test]
        public void SharedLabelsSkipRasterizationAndDeleteOnlyAfterLastRelease()
        {
            var first = Label();
            var second = Label();
            var id = first.UIText.TextureId;
            Assert.AreEqual(id, second.UIText.TextureId);
            Assert.AreEqual(1, _text.Builds);
            first.Dispose();
            Assert.False(_gl.Deleted.Contains(id));
            second.Dispose();
            second.Dispose();
            Assert.AreEqual(1, _gl.Deleted.Count(value => value == id));
        }

        [Test]
        public void QueuedUpdatesCannotOverwriteANewerTexture()
        {
            var label = Label();
            Task.Run(() => label.Text = "queued old text").GetAwaiter().GetResult();
            label.Text = "latest text";
            var latest = label.UIText.TextureId;
            Executer.Update();
            Assert.AreEqual(latest, label.UIText.TextureId);
            Assert.True(TextCache.Exists(latest));
            Assert.AreEqual(_cacheCount + 1, TextCache.Count);
            Assert.True(_text.Bitmaps.All(IsDisposed));
        }

        [Test]
        public void DisposingBeforeUploadDiscardsThePendingBitmap()
        {
            GUIText label = null;
            Task.Run(() => label = Label()).GetAwaiter().GetResult();
            label.Dispose();
            Executer.Update();
            Assert.AreEqual(0, label.UIText.TextureId);
            Assert.AreEqual(_cacheCount, TextCache.Count);
            Assert.True(_text.Bitmaps.All(IsDisposed));
        }

        [Test]
        public void UpdatesPreserveTextureObjectAndPresentationState()
        {
            var label = Label();
            var texture = label.UIText;
            label.Enable();
            label.Opacity = .4f;
            label.Grayscale = true;
            label.TextColor = Color.Red;
            Assert.AreSame(texture, label.UIText);
            Assert.True(label.Enabled);
            Assert.True(label.Grayscale);
            Assert.AreEqual(.4f, label.Opacity);
        }

        [Test]
        public void CacheDistinguishesFontFamilyStrokeAndRasterScale()
        {
            var normal = Label();
            var alternateFamily = SystemFonts.Families.First(f => !f.Equals(normal.TextFont.Family));
            var otherFont = Label(Font: new Font(alternateFamily, normal.TextFont.Size));
            Assert.AreNotEqual(normal.UIText.TextureId, otherFont.UIText.TextureId);
            var stroke = Label();
            stroke.Stroke = true;
            Assert.AreNotEqual(normal.UIText.TextureId, stroke.UIText.TextureId);
            var otherStroke = Label();
            otherStroke.StrokeColor = Color.Red;
            otherStroke.Stroke = true;
            Assert.AreNotEqual(stroke.UIText.TextureId, otherStroke.UIText.TextureId);
            var wideStroke = Label();
            wideStroke.StrokeWidth = 4;
            wideStroke.Stroke = true;
            Assert.AreNotEqual(stroke.UIText.TextureId, wideStroke.UIText.TextureId);
            GameSettings.UIScaling = 2;
            var scaled = Label();
            Assert.AreNotEqual(normal.UIText.TextureId, scaled.UIText.TextureId);
        }

        [Test]
        public void FontChangesRefreshTheTexture()
        {
            var label = Label();
            var original = label.UIText.TextureId;
            label.TextFont = FontCache.GetBold(16);
            Assert.AreNotEqual(original, label.UIText.TextureId);
            Assert.AreEqual(2, _text.Builds);
        }

        [Test]
        public void HoverRasterizesOnlyOncePerColorChange()
        {
            using var button = new Button(Vector2.Zero, Vector2.One, "hover-regression", Color.White,
                FontCache.GetNormal(12)) { PlaySound = false };
            var initial = _text.Builds;
            button.OnHoverEnter();
            Assert.AreEqual(initial + 1, _text.Builds);
            button.OnHoverExit();
            Assert.AreEqual(initial + 2, _text.Builds);
        }

        [Test]
        public void EmptyTextReleasesTheTextureWithoutRasterizing()
        {
            var label = Label();
            var builds = _text.Builds;
            label.Text = string.Empty;
            Assert.AreEqual(builds, _text.Builds);
            Assert.AreEqual(0, label.UIText.TextureId);
        }

        [Test]
        public void RegistryDeletesGpuTexturesOnTheRenderThread()
        {
            const uint id = 900001;
            TextureRegistry.Add(id, "ui-regression", TextureMinFilter.Linear, TextureMagFilter.Linear,
                TextureWrapMode.ClampToBorder);
            Task.Run(() => TextureRegistry.Dispose(id)).GetAwaiter().GetResult();
            Assert.False(_gl.Deleted.Contains(id));
            Assert.False(TextureRegistry.Contains("ui-regression", TextureMinFilter.Linear, TextureMagFilter.Linear,
                TextureWrapMode.ClampToBorder, out _));
            Executer.Update();
            Assert.Contains(id, _gl.Deleted);
            Assert.AreEqual(Thread.CurrentThread.ManagedThreadId, _gl.DeleteThread);
        }

        private static bool IsDisposed(Image<Rgba32> Bitmap)
        {
            try { var pixel = Bitmap[0, 0]; return false; }
            catch (ObjectDisposedException) { return true; }
        }

        private sealed class CountingTextProvider : ITextProvider
        {
            public int Builds;
            public readonly List<Image<Rgba32>> Bitmaps = new List<Image<Rgba32>>();
            public Image<Rgba32> BuildText(string Text, Font Font, Color Color) =>
                BuildText(Text, Font, Color, new TextOptions());
            public Image<Rgba32> BuildText(string Text, Font Font, Color Color, TextOptions Options)
            {
                Builds++;
                var image = new Image<Rgba32>(30, 12);
                Bitmaps.Add(image);
                return image;
            }
        }

        private sealed class CountingTextureProvider : ITexture2DProvider
        {
            private static int _ids = 10000;
            public uint LoadTexture(BitmapObject Bitmap, TextureMinFilter Min, TextureMagFilter Mag,
                TextureWrapMode Wrap) => (uint)Interlocked.Increment(ref _ids);
        }

        private sealed class RecordingGL : DummyGLProvider
        {
            public readonly List<uint> Deleted = new List<uint>();
            public int DeleteThread;
            public override void DeleteTextures(int Count, params uint[] Ids)
            {
                Deleted.AddRange(Ids.Take(Count));
                DeleteThread = Thread.CurrentThread.ManagedThreadId;
            }
        }
    }
}
