using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ZipTool
{
    public class BitmapWrapper
    {
        // if use LockBits
        public static bool IsLockBits
        {
            get; set;
        }

        Bitmap source = null;
        IntPtr Iptr = IntPtr.Zero;
        BitmapData bitmapData = null;

        public byte[] Pixels { get; set; }
        public int Depth { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }

        public BitmapWrapper(Bitmap source)
        {
            this.source = source;
        }

        /// <summary>
        /// Lock bitmap data
        /// </summary>
        public void LockBits()
        {
            try
            {
                // Get width and height of bitmap
                Width = source.Width;
                Height = source.Height;

                if (BitmapWrapper.IsLockBits == false)
                    return;

                // get total locked pixels count
                int PixelCount = Width * Height;

                // Create rectangle to lock
                Rectangle rect = new Rectangle(0, 0, Width, Height);

                // get source bitmap pixel format size
                Depth = System.Drawing.Bitmap.GetPixelFormatSize(source.PixelFormat);

                // Check if bpp (Bits Per Pixel) is 8, 24, or 32
                if (Depth != 8 && Depth != 24 && Depth != 32)
                {
                    throw new ArgumentException("Only 8, 24 and 32 bpp images are supported.");
                }

                // Lock bitmap and return bitmap data
                bitmapData = source.LockBits(rect, ImageLockMode.ReadWrite,
                                             source.PixelFormat);

                // create byte array to copy pixel values
                int step = Depth / 8;
                Pixels = new byte[PixelCount * step];
                Iptr = bitmapData.Scan0;

                // Copy data from pointer to array
                Marshal.Copy(Iptr, Pixels, 0, Pixels.Length);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Unlock bitmap data
        /// </summary>
        public void UnlockBits()
        {
            try
            {
                if (BitmapWrapper.IsLockBits == false)
                    return;

                // Copy data from byte array to pointer
                Marshal.Copy(Pixels, 0, Iptr, Pixels.Length);

                // Unlock bitmap data
                source.UnlockBits(bitmapData);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Get the color of the specified pixel
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public Color GetPixel(int x, int y)
        {
            if (BitmapWrapper.IsLockBits == false)
                return source.GetPixel(x, y);

            Color clr = Color.Empty;

            // Get color components count
            int cCount = Depth / 8;

            // Get start index of the specified pixel
            int i = ((y * Width) + x) * cCount;

            if (i > Pixels.Length - cCount)
                throw new IndexOutOfRangeException();

            if (Depth == 32) // For 32 bpp get Red, Green, Blue and Alpha
            {
                byte b = Pixels[i];
                byte g = Pixels[i + 1];
                byte r = Pixels[i + 2];
                byte a = Pixels[i + 3]; // a
                clr = Color.FromArgb(a, r, g, b);
            }
            else if (Depth == 24) // For 24 bpp get Red, Green and Blue
            {
                byte b = Pixels[i];
                byte g = Pixels[i + 1];
                byte r = Pixels[i + 2];
                clr = Color.FromArgb(r, g, b);
            }
            else if (Depth == 8)
            // For 8 bpp get color value (Red, Green and Blue values are the same)
            {
                byte c = Pixels[i];
                clr = Color.FromArgb(c, c, c);
            }
            return clr;
        }

        /// <summary>
        /// Set the color of the specified pixel
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="color"></param>
        public void SetPixel(int x, int y, Color color)
        {
            if (BitmapWrapper.IsLockBits == false)
            {
                source.SetPixel(x, y, color);
                return;
            }

            // Get color components count
            int cCount = Depth / 8;

            // Get start index of the specified pixel
            int i = ((y * Width) + x) * cCount;

            if (Depth == 32) // For 32 bpp set Red, Green, Blue and Alpha
            {
                Pixels[i] = color.B;
                Pixels[i + 1] = color.G;
                Pixels[i + 2] = color.R;
                Pixels[i + 3] = color.A;
            }
            else if (Depth == 24) // For 24 bpp set Red, Green and Blue
            {
                Pixels[i] = color.B;
                Pixels[i + 1] = color.G;
                Pixels[i + 2] = color.R;
            }
            else if (Depth == 8)
            // For 8 bpp set color value (Red, Green and Blue values are the same)
            {
                Pixels[i] = color.B;
            }
        }

        /// <summary>
        /// Fixing color of a pixel by alpha value
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="color"></param>
        public void FixPixelByAlpha(int x, int y)
        {
            if (BitmapWrapper.IsLockBits == false)
            {
                Color col = source.GetPixel(x, y);

                if (col.A > 0 && col.A < 255)
                {
                    double rate = col.A / 255.0d; // calculate rate
                    // calculate color value
                    var red = (int)((double)col.R / rate);
                    var green = (int)((double)col.G / rate);
                    var blue = (int)((double)col.B / rate);
                    if (red > 255)
                        red = 255;
                    if (red < 0)
                        red = 0;
                    if (green > 255)
                        green = 255;
                    if (green < 0)
                        green = 0;
                    if (blue > 255)
                        blue = 255;
                    if (blue < 0)
                        blue = 0;
                    Color newCol = Color.FromArgb(col.A, red, green, blue);
                    // replace color
                    source.SetPixel(x, y, newCol);
                }
                return;
            }

            // Get color components count
            int cCount = Depth / 8;

            // Get start index of the specified pixel
            int i = ((y * Width) + x) * cCount;

            if (Depth == 32) // For 32 bpp set Red, Green, Blue and Alpha
            {
                var alpha = Pixels[i + 3];
                if (alpha > 0 && alpha < 255)
                {
                    double rate = Pixels[i + 3] / 255.0d; // calculate rate
                    var red = (int)((double)Pixels[i] / rate);
                    var green = (int)((double)Pixels[i + 1] / rate);
                    var blue = (int)((double)Pixels[i + 2] / rate);
                    if (red > 255)
                        red = 255;
                    if (red < 0)
                        red = 0;
                    if (green > 255)
                        green = 255;
                    if (green < 0)
                        green = 0;
                    if (blue > 255)
                        blue = 255;
                    if (blue < 0)
                        blue = 0;
                    Pixels[i] = (byte)red;
                    Pixels[i + 1] = (byte)green;
                    Pixels[i + 2] = (byte)blue;
                }
            }
        }
    }
}
