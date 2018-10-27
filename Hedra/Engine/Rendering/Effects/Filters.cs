/*
 * Author: Zaphyk
 * Date: 05/02/2016
 * Time: 01:00 a.m.
 *
 */
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;

namespace Hedra.Engine.Rendering
{
    public static class Filters
    {
        /*public static bool FilterWhite(Bitmap b){
            BitmapData bmData = b.LockBits(new Rectangle(0, 0, b.Width, b.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            int stride = bmData.Stride; 
            System.IntPtr Scan0 = bmData.Scan0;
            unsafe 
            { 
                byte * p = (byte *)(void *)Scan0;
                int nOffset = stride - b.Width*3; 
                int nWidth = b.Width * 3;
                for(int y=0;y < b.Height;++y)
                {
                    for(int x=0; x < nWidth; ++x )
                    {
                        p[3] = (byte)(255);
                        p++;
                    }
                    p += nOffset;
                }
            }
        
            b.UnlockBits(bmData);
        
            return true;
        }*/
    }
}
