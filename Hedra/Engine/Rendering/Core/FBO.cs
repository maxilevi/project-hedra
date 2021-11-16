/*
 * Author: Zaphyk
 * Date: 21/02/2016
 * Time: 05:12 a.m.
 *
 */

using System;
using System.Diagnostics;
using System.Numerics;
using Hedra.Engine.IO;
using Hedra.Engine.Windowing;
using SixLabors.ImageSharp;
using GLDrawBuffersEnum = Silk.NET.OpenGL.GLEnum;

namespace Hedra.Engine.Rendering.Core
{
    /// <summary>
    ///     Description of FBO.
    /// </summary>
    public sealed class FBO : GLObject<FBO>
    {
        private readonly bool _multisample;
        private uint _id;
        private int _samples;

        public FBO(int Width, int Height, bool Multisample = false, int Samples = 2,
            FramebufferAttachment Attachment = FramebufferAttachment.ColorAttachment0,
            PixelInternalFormat Format = PixelInternalFormat.Rgba8, bool Mipmaps = false, bool Depth = true)
            : this(new Size(Width, Height), new[] { Attachment }, new[] { Format }, Mipmaps, Multisample, Samples,
                Depth)
        {
        }

        public FBO(int Width, int Height, FramebufferAttachment Attachment, PixelInternalFormat Format,
            params TextureParameter[] TextureParameters)
            : this(new Size(Width, Height), new[] { Attachment }, new[] { Format }, false, false, 0, false,
                TextureMinFilter.Linear, TextureParameters)
        {
        }

        /// <summary>
        ///     Creates a framebuffer object and its associated resources (depth and pbuffers).
        /// </summary>
        /// <param name="Size">Specifies the size (in pixels) of the framebuffer and it's associated buffers.</param>
        /// <param name="Attachments">Specifies the attachment to use for the pbuffer.</param>
        /// <param name="Format">Specifies the internal pixel format for the pbuffer.</param>
        public FBO(Size Size, FramebufferAttachment Attachment = FramebufferAttachment.ColorAttachment0,
            PixelInternalFormat Format = PixelInternalFormat.Rgba8, bool Mipmaps = false, bool Multisample = false,
            int Samples = 2, bool Depth = true)
            : this(Size, new[] { Attachment }, new[] { Format }, Mipmaps, Multisample, Samples, Depth)
        {
        }


        /// <summary>
        ///     Creates a framebuffer object and its associated resources (depth and pbuffers).
        /// </summary>
        /// <param name="Size">Specifies the size (in pixels) of the framebuffer and it's associated buffers.</param>
        /// <param name="Attachments">Specifies the attachments to use for the frame buffer.</param>
        /// <param name="Format">Specifies the internal pixel format for the frame buffer.</param>
        /// <param name="Mipmaps">Specified whether to build mipmaps after the frame buffer is unbound.</param>
        public FBO(Size Size, FramebufferAttachment[] Attachments, PixelInternalFormat[] Formats, bool Mipmaps,
            bool Multisample, int Samples,
            bool Depth, TextureMinFilter FilterType = TextureMinFilter.Linear,
            TextureParameter[] TextureParameters = null)
        {
            this.Size = Size;
            this.Attachments = Attachments;
            this.Formats = Formats;
            ViewportSize = new Vector2(Size.Width, Size.Height);
            _multisample = Multisample;
            _samples = _multisample ? Samples : 0;

            // First create the framebuffer
            _id = (uint)Renderer.GenFramebuffer();
            Renderer.BindFramebuffer(FramebufferTarget.Framebuffer, Id);

            if (Attachments.Length == 1 && Attachments[0] == FramebufferAttachment.DepthAttachment)
            {
                // if this is a depth attachment only
                TextureId = new[] { Renderer.GenTexture() };
                if (Multisample)
                    Renderer.BindTexture(TextureTarget.Texture2DMultisample, TextureId[0]);
                else
                    Renderer.BindTexture(TextureTarget.Texture2D, TextureId[0]);

                if (Multisample)
                    Renderer.TexImage2DMultisample(TextureTargetMultisample.Texture2DMultisample, Samples, Formats[0],
                        Size.Width, Size.Height, true);
                else
                    Renderer.TexImage2D(TextureTarget.Texture2D, 0, Formats[0], Size.Width, Size.Height, 0,
                        PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);
                Renderer.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter,
                    (int)TextureMagFilter.Linear);
                Renderer.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
                    (int)TextureMinFilter.Linear);
                Renderer.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS,
                    (int)TextureWrapMode.ClampToEdge);
                Renderer.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT,
                    (int)TextureWrapMode.ClampToEdge);
                if (TextureParameters != null)
                    for (var i = 0; i < TextureParameters.Length; ++i)
                        Renderer.TexParameter(TextureTarget.Texture2D, TextureParameters[i].Name,
                            TextureParameters[i].Value);
                //float[] BorderColor = { 1.0f, 1.0f, 1.0f, 1.0f };
                //Renderer.GetTexParameter(TextureTarget.Texture2D, GetTextureParameter.TextureBorderColor, BorderColor);

                if (Multisample)
                    Renderer.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment,
                        TextureTarget.Texture2DMultisample, TextureId[0], 0);
                else
                    Renderer.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment,
                        TextureId[0], 0);
                Renderer.DrawBuffer(DrawBufferMode.None);
                Renderer.ReadBuffer(ReadBufferMode.None);
            }
            else
            {
                // Create n texture buffers (known by the number of attachments)
                TextureId = new uint[Attachments.Length];
                for (var i = 0; i < Attachments.Length; i++) TextureId[i] = Renderer.GenTexture();

                // Bind the n texture buffers to the framebuffer
                for (var i = 0; i < Attachments.Length; i++)
                {
                    var pixelType =
                        Formats[i] == PixelInternalFormat.Rgba32f || Formats[i] == PixelInternalFormat.Rgba16f
                            ? PixelType.Float
                            : PixelType.UnsignedByte;

                    if (Multisample)
                        Renderer.BindTexture(TextureTarget.Texture2DMultisample, TextureId[i]);
                    else
                        Renderer.BindTexture(TextureTarget.Texture2D, TextureId[i]);

                    if (Multisample)
                        Renderer.TexImage2DMultisample(TextureTargetMultisample.Texture2DMultisample, Samples,
                            Formats[i], Size.Width, Size.Height, true);
                    else if (Attachments[i] == FramebufferAttachment.DepthAttachment)
                        Renderer.TexImage2D(TextureTarget.Texture2D, 0, Formats[i], Size.Width, Size.Height, 0,
                            PixelFormat.DepthComponent, PixelType.UnsignedByte, IntPtr.Zero);
                    else
                        Renderer.TexImage2D(TextureTarget.Texture2D, 0, Formats[i], Size.Width, Size.Height, 0,
                            PixelFormat.Rgba, pixelType, IntPtr.Zero);

                    if (Mipmaps)
                    {
                        Renderer.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter,
                            (int)TextureMagFilter.Linear);
                        Renderer.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
                            (int)TextureMinFilter.Linear);
                        Renderer.GenerateMipmap(GenerateMipmapTarget.Texture2D);
                    }
                    else
                    {
                        Renderer.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter,
                            (int)FilterType);
                        Renderer.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
                            (int)FilterType);
                    }

                    if (Multisample)
                        Renderer.FramebufferTexture2D(FramebufferTarget.Framebuffer, Attachments[i],
                            TextureTarget.Texture2DMultisample, TextureId[i], 0);
                    else
                        Renderer.FramebufferTexture(FramebufferTarget.Framebuffer, Attachments[i], TextureId[i], 0);
                }

                // Create and attach a 24-bit depth buffer to the framebuffer
                if (Depth)
                {
                    DepthId = Renderer.GenTexture();
                    if (Multisample)
                        Renderer.BindTexture(TextureTarget.Texture2DMultisample, DepthId);
                    else
                        Renderer.BindTexture(TextureTarget.Texture2D, DepthId);

                    if (Multisample)
                        Renderer.TexImage2DMultisample(TextureTargetMultisample.Texture2DMultisample, Samples,
                            PixelInternalFormat.DepthStencil, Size.Width, Size.Height, true);
                    else
                        Renderer.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Depth24Stencil8, Size.Width,
                            Size.Height, 0, PixelFormat.DepthStencil, PixelType.UnsignedInt248, IntPtr.Zero);

                    Renderer.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter,
                        (int)TextureMagFilter.Nearest);
                    Renderer.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
                        (int)TextureMinFilter.Nearest);
                    Renderer.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS,
                        (int)TextureWrapMode.ClampToEdge);
                    Renderer.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT,
                        (int)TextureWrapMode.ClampToEdge);

                    if (Multisample)
                        Renderer.FramebufferTexture2D(FramebufferTarget.Framebuffer,
                            FramebufferAttachment.DepthAttachment, TextureTarget.Texture2DMultisample, DepthId, 0);
                    else
                        Renderer.FramebufferTexture(FramebufferTarget.Framebuffer,
                            FramebufferAttachment.DepthAttachment, DepthId, 0);

                    if (Multisample)
                        Renderer.FramebufferTexture2D(FramebufferTarget.Framebuffer,
                            FramebufferAttachment.StencilAttachment, TextureTarget.Texture2DMultisample, DepthId, 0);
                    else
                        Renderer.FramebufferTexture(FramebufferTarget.Framebuffer,
                            FramebufferAttachment.StencilAttachment, DepthId, 0);
                }
            }

            Renderer.Viewport(0, 0, Size.Width, Size.Height);

            // Build the framebuffer and check for errors
            var status = Renderer.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
            if (status != FramebufferErrorCode.FramebufferComplete)
                Log.WriteLine("Frame buffer did not compile correctly.  Returned {0}, GLError: {1}", status.ToString(),
                    Renderer.GetError().ToString());

            Renderer.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        public override uint Id => _id;
        public uint[] TextureId { get; private set; }
        public uint DepthId { get; private set; }
        public Size Size { get; }
        public FramebufferAttachment[] Attachments { get; }
        public PixelInternalFormat[] Formats { get; }
        public Vector2 ViewportSize { get; set; }


        /// <summary>
        ///     Check to ensure that the FBO was disposed of properly.
        /// </summary>
        ~FBO()
        {
            if (Program.GameWindow.IsExiting || Program.IsDummy) return;
            if (DepthId != 0 || Id != 0 || TextureId != null)
                Debug.Fail("FBO was not disposed of properly.");
        }

        /// <summary>
        ///     Binds the framebuffer and all of the renderbuffers.
        ///     Clears the buffer bits and sets viewport size.
        ///     Perform all rendering after this call.
        /// </summary>
        /// <param name="Clear">True to clear both the color and depth buffer bits of the FBO before enabling.</param>
        public void Bind(bool Clear = true)
        {
            if (Renderer.FBOBound == Id) return;

            Renderer.BindFramebuffer(FramebufferTarget.Framebuffer, Id);
            if (Attachments.Length == 1)
            {
                if (!_multisample)
                {
                    Renderer.BindTexture(TextureTarget.Texture2D, TextureId[0]);
                    Renderer.FramebufferTexture(FramebufferTarget.Framebuffer, Attachments[0], TextureId[0], 0);
                }
                else
                {
                    Renderer.BindTexture(TextureTarget.Texture2DMultisample, TextureId[0]);
                    Renderer.FramebufferTexture2D(FramebufferTarget.Framebuffer, Attachments[0],
                        TextureTarget.Texture2DMultisample, TextureId[0], 0);
                }
            }
            else
            {
                var buffers = new GLDrawBuffersEnum[Attachments.Length];

                for (var i = 0; i < Attachments.Length; i++)
                    //Renderer.BindTexture(TextureTarget.Texture2D, TextureID[i]);
                    //Renderer.FramebufferTexture(FramebufferTarget.Framebuffer, Attachments[i], TextureID[i], 0);
                    buffers[i] = (GLDrawBuffersEnum)Attachments[i];

                //Renderer.BindTexture(TextureTarget.Texture2D, DepthID);
                //Renderer.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, DepthID, 0);

                if (Attachments.Length > 1) Renderer.DrawBuffers(Attachments.Length, buffers);
            }

            Renderer.Viewport(0, 0, (int)ViewportSize.X, (int)ViewportSize.Y);

            // configurably clear the buffer and color bits
            if (Clear)
            {
                if (Attachments.Length == 1 && Attachments[0] == FramebufferAttachment.DepthAttachment)
                    Renderer.Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
                else
                    Renderer.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit |
                                   ClearBufferMask.StencilBufferBit);
            }
        }

        public void Unbind()
        {
            Renderer.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        public override void Dispose()
        {
            base.Dispose();
            if (TextureId != null)
            {
                for (var i = 0; i < TextureId.Length; ++i) TextureRegistry.Remove(TextureId[i]);
                TextureId = null;
            }

            if (Id != 0)
            {
                Renderer.DeleteFramebuffers(1, _id);
                _id = 0;
            }

            if (DepthId != 0)
            {
                Renderer.DeleteFramebuffers(1, DepthId);
                DepthId = 0;
            }
        }
    }
}