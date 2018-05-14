/*
 * Author: Zaphyk
 * Date: 21/02/2016
 * Time: 05:12 a.m.
 *
 */
using System;
using System.Collections.Generic;
using OpenTK;
using System.Drawing;
using OpenTK.Graphics.OpenGL;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering.Effects;

namespace Hedra.Engine.Rendering
{
	/// <summary>
	/// Description of FBO.
	/// </summary>
	public class FBO : IDisposable
	{
        /// <summary>
        /// The ID for the entire framebuffer object.
        /// </summary>
        public uint BufferID { get; private set; }

        /// <summary>
        /// The IDs for each of the renderbuffer attachments.
        /// </summary>
        public uint[] TextureID { get; private set; }

        /// <summary>
        /// The ID for the sinGLe depth buffer attachment.
        /// </summary>
        public uint DepthID { get; private set; }

        /// <summary>
        /// The size (in pixels) of all renderbuffers associated with this framebuffer.
        /// </summary>
        public Size Size { get; private set; }

        /// <summary>
        /// The attachments used by this framebuffer.
        /// </summary>
        public FramebufferAttachment[] Attachments { get; private set; }

        /// <summary>
        /// The internal pixel format for each of the renderbuffers (depth buffer not included).
        /// </summary>
        public PixelInternalFormat[] Formats { get; private set; }

        private bool mipmaps;
        private bool multisample;
        private int samples;

        public FBO(int width, int height, bool multisample = false, int samples = 2, FramebufferAttachment attachment = FramebufferAttachment.ColorAttachment0, PixelInternalFormat format = PixelInternalFormat.Rgba8, bool mipmaps = false, bool depth = true)
            : this(new Size(width, height), new [] { attachment }, new [] { format }, mipmaps, multisample, samples, depth)
        {
        }

        /// <summary>
        /// Creates a framebuffer object and its associated resources (depth and pbuffers).
        /// </summary>
        /// <param name="Size">Specifies the size (in pixels) of the framebuffer and it's associated buffers.</param>
        /// <param name="Attachments">Specifies the attachment to use for the pbuffer.</param>
        /// <param name="Format">Specifies the internal pixel format for the pbuffer.</param>
        public FBO(Size Size, FramebufferAttachment Attachment = FramebufferAttachment.ColorAttachment0, PixelInternalFormat Format = PixelInternalFormat.Rgba8, bool Mipmaps = false, bool Multisample = false, int Samples = 2, bool Depth = true)
            : this(Size, new [] { Attachment }, new [] { Format }, Mipmaps,Multisample, Samples, Depth)
        {
        }

        /// <summary>
        /// Creates a framebuffer object and its associated resources (depth and pbuffers).
        /// </summary>
        /// <param name="Size">Specifies the size (in pixels) of the framebuffer and it's associated buffers.</param>
        /// <param name="Attachments">Specifies the attachments to use for the frame buffer.</param>
        /// <param name="Format">Specifies the internal pixel format for the frame buffer.</param>
        /// <param name="Mipmaps">Specified whether to build mipmaps after the frame buffer is unbound.</param>
        public FBO(Size Size, FramebufferAttachment[] Attachments, PixelInternalFormat[] Formats, bool Mipmaps, bool Multisample, int Samples, bool Depth, TextureMinFilter filterType = TextureMinFilter.Linear)
        {
            this.Size = Size;
            this.Attachments = Attachments;
            this.Formats = Formats;
            this.mipmaps = Mipmaps;
            this.multisample = Multisample;
            this.samples = (multisample) ? Samples : 0;
            DisposeManager.Add(this);
            
            // First create the framebuffer
            BufferID = (uint) GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, BufferID);
            GraphicsLayer.FBOBound = (int) BufferID;

            if (Attachments.Length == 1 && Attachments[0] == FramebufferAttachment.DepthAttachment)
            {
                // if this is a depth attachment only
                TextureID = new uint[] { (uint) GL.GenTexture() };
                if(Multisample)
                   	GL.BindTexture(TextureTarget.Texture2DMultisample, TextureID[0]);
                else
                	GL.BindTexture(TextureTarget.Texture2D, TextureID[0]);
				
                if(Multisample)
                	GL.TexImage2DMultisample(TextureTargetMultisample.Texture2DMultisample, Samples, Formats[0], Size.Width, Size.Height, true);
                else
                	GL.TexImage2D(TextureTarget.Texture2D, 0, Formats[0], Size.Width, Size.Height, 0, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int) TextureMagFilter.Nearest);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int) TextureMinFilter.Nearest);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int) TextureWrapMode.ClampToEdge);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int) TextureWrapMode.ClampToEdge);
                float[] BorderColor = { 1.0f, 1.0f, 1.0f, 1.0f };
				GL.GetTexParameter(TextureTarget.Texture2D, GetTextureParameter.TextureBorderColor, BorderColor);

                if(Multisample)
                    GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2DMultisample, TextureID[0], 0);
                else
                    GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureID[0], 0);
                GL.DrawBuffer(DrawBufferMode.None);
                GL.ReadBuffer(ReadBufferMode.None);
            }
            else
            {
                // Create n texture buffers (known by the number of attachments)
                TextureID = new uint[Attachments.Length];
                GL.GenTextures(Attachments.Length, TextureID);

                // Bind the n texture buffers to the framebuffer
                for (int i = 0; i < Attachments.Length; i++)
                {
                    PixelType pixelType =
                        Formats[i] == PixelInternalFormat.Rgba32f || Formats[i] == PixelInternalFormat.Rgba16f
                            ? PixelType.Float
                            : PixelType.UnsignedByte;

                    if (Multisample)
                   		GL.BindTexture(TextureTarget.Texture2DMultisample, TextureID[i]);
                	else
                		GL.BindTexture(TextureTarget.Texture2D, TextureID[i]);
                	
                    if(Multisample)
                		GL.TexImage2DMultisample(TextureTargetMultisample.Texture2DMultisample, Samples, Formats[i], Size.Width, Size.Height, true);
                	else
                		if(Attachments[i] == FramebufferAttachment.DepthAttachment)
                			GL.TexImage2D(TextureTarget.Texture2D, 0, Formats[i], Size.Width, Size.Height, 0, PixelFormat.DepthComponent, PixelType.UnsignedByte, IntPtr.Zero);
                		else
                			GL.TexImage2D(TextureTarget.Texture2D, 0, Formats[i], Size.Width, Size.Height, 0, PixelFormat.Rgba, pixelType, IntPtr.Zero);
                    
                	if (Mipmaps)
                    {
                        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int) TextureMagFilter.Linear);
                        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int) TextureMinFilter.Linear);
                        GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
                    }
                    else
                    {
                    	GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int) filterType);
                        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int) filterType);
                    }
                    if(Multisample)
                    	GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, Attachments[i], TextureTarget.Texture2DMultisample, TextureID[i], 0);
                	else
                    	GL.FramebufferTexture(FramebufferTarget.Framebuffer, Attachments[i], TextureID[i], 0);
                }

                // Create and attach a 24-bit depth buffer to the framebuffer
                if (Depth) {
                    DepthID = (uint) GL.GenTexture();
                    if(Multisample)
                   	    GL.BindTexture(TextureTarget.Texture2DMultisample, DepthID);
                    else
                	    GL.BindTexture(TextureTarget.Texture2D, DepthID);

                    if(Multisample)
                	    GL.TexImage2DMultisample(TextureTargetMultisample.Texture2DMultisample, Samples, PixelInternalFormat.DepthStencil, Size.Width, Size.Height, true);
                    else
               		    GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Depth24Stencil8, Size.Width, Size.Height, 0, PixelFormat.DepthStencil, PixelType.UnsignedInt248, IntPtr.Zero);
                
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int) TextureMagFilter.Nearest);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int) TextureMinFilter.Nearest);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int) TextureWrapMode.ClampToEdge);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int) TextureWrapMode.ClampToEdge);

                    if(Multisample)
                	    GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2DMultisample, DepthID, 0);
                    else
                	    GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, DepthID, 0);
                
                    if(Multisample)
                	    GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.StencilAttachment, TextureTarget.Texture2DMultisample, DepthID, 0);
                    else
                	    GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.StencilAttachment, DepthID, 0);
                }
            }
            GL.Viewport(0, 0, Size.Width, Size.Height);

            // Build the framebuffer and check for errors
            FramebufferErrorCode status = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
            if (status != FramebufferErrorCode.FramebufferComplete)
            {
                Log.WriteLine("Frame buffer did not compile correctly.  Returned {0}, GLError: {1}", status.ToString(), GL.GetError().ToString());
            }

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GraphicsLayer.FBOBound = 0;
        }
        

        /// <summary>
        /// Check to ensure that the FBO was disposed of properly.
        /// </summary>
        ~FBO()
        {
        	ThreadManager.ExecuteOnMainThread( () => this.Dispose() );
            if (DepthID != 0 || BufferID != 0 || TextureID != null)
            {
                System.Diagnostics.Debug.Fail("FBO was not disposed of properly.");
            }
        }

        /// <summary>
        /// Binds the framebuffer and all of the renderbuffers.
        /// Clears the buffer bits and sets viewport size.
        /// Perform all rendering after this call.
        /// </summary>
        /// <param name="clear">True to clear both the color and depth buffer bits of the FBO before enabling.</param>
        public void Bind(bool clear = true)
        {
        	if(GraphicsLayer.FBOBound == BufferID) return;

            GraphicsLayer.FBOBound = (int) BufferID;
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, BufferID);
            if (Attachments.Length == 1)
            {
            	if(!multisample){
                	GL.BindTexture(TextureTarget.Texture2D, TextureID[0]);
               		GL.FramebufferTexture(FramebufferTarget.Framebuffer, Attachments[0], TextureID[0], 0);
            	}else{
            		GL.BindTexture(TextureTarget.Texture2DMultisample, TextureID[0]);
               		GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, Attachments[0], TextureTarget.Texture2DMultisample, TextureID[0], 0);
            	}
            }
            else
            {
                DrawBuffersEnum[] buffers = new DrawBuffersEnum[Attachments.Length];

                for (int i = 0; i < Attachments.Length; i++)
                {
                    //GL.BindTexture(TextureTarget.Texture2D, TextureID[i]);
                    //GL.FramebufferTexture(FramebufferTarget.Framebuffer, Attachments[i], TextureID[i], 0);
                    buffers[i] = (DrawBuffersEnum)Attachments[i];
                }

                //GL.BindTexture(TextureTarget.Texture2D, DepthID);
                //GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, DepthID, 0);

                if (Attachments.Length > 1) GL.DrawBuffers(Attachments.Length, buffers);
            }
			
            GL.Viewport(0, 0, Size.Width, Size.Height);

            // configurably clear the buffer and color bits
            if (true)
            {
                if (Attachments.Length == 1 && Attachments[0] == FramebufferAttachment.DepthAttachment)
                	GL.Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
                else
                	GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
            }
        }

        /// <summary>
        /// Unbinds the framebuffer and then generates the mipmaps of each renderbuffer.
        /// </summary>
        public void UnBind()
        {
            // unbind this framebuffer (does not guarantee the correct framebuffer is bound)
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GraphicsLayer.FBOBound = 0;
        }

        public FBO Resize()
        {
            bool depth = this.DepthID != 0;
            this.Dispose();
        	return new FBO(new Size(GameSettings.Width, GameSettings.Height), Attachments, this.Formats, this.mipmaps, this.multisample, this.samples, depth);
        }
        
		
        public void Dispose()
        {
            if (TextureID != null)
            {
                GL.DeleteTextures(TextureID.Length, TextureID);
                TextureID = null;
            }

            if(BufferID != 0)
            {
                GL.DeleteFramebuffers(1, new uint[] {BufferID});
                BufferID = 0;
            }

            if(DepthID != 0)
            {
                GL.DeleteFramebuffers(1, new uint[] { DepthID });
                DepthID = 0;
            }
        }
	}
}
