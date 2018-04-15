/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 29/11/2016
 * Time: 04:10 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using Hedra.Engine.Management;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Hedra.Engine.Rendering
{
	/// <summary>
	/// Description of OpenGLStateManager.
	/// </summary>
	public static class GraphicsLayer
	{
		public static int FBOBound { get; set; }
		public static int ShaderBound { get; set; }
	    private static readonly StateManager FboManager;
	    private static readonly StateManager ShaderManager;

        static GraphicsLayer()
	    {
	        FboManager = new StateManager();
            FboManager.RegisterStateItem( () => FBOBound, O => FBOBound = (int) O);
            ShaderManager = new StateManager();
	        ShaderManager.RegisterStateItem(() => ShaderBound, O => ShaderBound = (int)O);
        }

	    public static void PushFBO()
	    {
	        FboManager.CaptureState();
	    }

	    public static void PushShader()
	    {
	        ShaderManager.CaptureState();
	    }

	    public static void PopFBO()
	    {
	        FboManager.ReleaseState();
	    }

	    public static void PopShader()
	    {
	        ShaderManager.ReleaseState();
	    }

	    public static void BindShader(int Id)
	    {
	        GL.UseProgram(Id);
	    }

	    public static void BindFramebuffer(FramebufferTarget Target, int Id)
	    {
	        GL.BindFramebuffer(Target, Id);
	    }

        public static void MultiDrawElements(PrimitiveType Type, int[] Counts, DrawElementsType ElementsType, IntPtr[] Offsets, int Length)
	    {
	        CompatibilityManager.MultiDrawElementsMethod(Type, Counts, ElementsType, Offsets, Length);
        }

        public static int QueryAvailableMemory()
	    {
	        int mem;
            GL.GetInteger( (GetPName) AtiMeminfo.VboFreeMemoryAti, out mem);
            GL.GetError();

	        if (mem == 0)
	        {
	            GL.GetInteger((GetPName) NvxGpuMemoryInfo.GpuMemoryInfoCurrentAvailableVidmemNvx, out mem);
	            GL.GetError();
            }

	        return mem / 1024;
	    }

        /// <summary>
        /// Returns the current MVP Matrix. SLOW METHOD
        /// </summary>
        /// <returns></returns>
	    public static Matrix4 MvpMatrix()
        {
            Matrix4 mv;
            Matrix4 p;
            GL.GetFloat(GetPName.ProjectionMatrix, out p);
            GL.GetFloat(GetPName.ModelviewMatrix, out mv);
            return mv * p;
        }

	    /// <summary>
	    /// Returns the current MV Matrix. SLOW METHOD
	    /// </summary>
	    /// <returns></returns>
	    public static Matrix4 MvMatrix()
	    {
	        Matrix4 mv;
	        GL.GetFloat(GetPName.ModelviewMatrix, out mv);
	        return mv;
	    }

	    public static void MatrixMode(MatrixMode Mode)
	    {
	        GL.MatrixMode(Mode);
	    }

        public static void LoadIdentity()
	    {
	        GL.LoadIdentity();
	    }

        public static void PushMatrix()
	    {
	        GL.PushMatrix();
	    }

	    public static void PopMatrix()
	    {
	        GL.PopMatrix();
	    }

	    public static void Translate(Vector3 Translation)
	    {
	        GL.Translate(Translation);
	    }

	    public static void Rotate(float Angle, Vector3 Axis)
	    {
	        GL.Rotate(Angle, Axis);
	    }

	    public static void Scale(Vector3 Scale)
	    {
	        GL.Scale(Scale);
	    }
    }
}
