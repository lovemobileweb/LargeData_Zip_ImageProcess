using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Drawing.Imaging;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace ZipTool
{
    class Utilities
    {
        /// <summary>
        /// if don't export to a file
        /// </summary>
        public static bool IsDontWrite2File
        {
            get; set;
        }

        /// <summary>
        /// enum for result of compare
        /// </summary>
        public enum ComapreResult
        {
            Error = -1,
            Equal = 0,
            DifferentSize = 1,
            DifferentContent = 2
        }

        /// <summary>
        /// enum for result of action
        /// </summary>
        public enum ActionResult
        {
            Sucess = 0
        }

        /// <summary>
        /// print log or message to console
        /// </summary>
        public static void Log(string log, bool breakLine = true)
        {
            if (breakLine)
                Console.WriteLine(log);
            else
                Console.Write(log);
        }

        /// <summary>
        /// get config value by int
        /// </summary>
        public static int GetIntConfig(string key, int defaultValue = 0)
        {
            int value = defaultValue;
            try
            {
                value = int.Parse(ConfigurationSettings.AppSettings[key]);
            }
            catch (Exception)
            {
            }
            return value;
        }

        /// <summary>
        /// get config value by string
        /// </summary>
        public static string GetStringConfig(string key, string defaultValue = "")
        {
            string value = defaultValue;
            try
            {
                value = ConfigurationSettings.AppSettings[key];
            }
            catch (Exception)
            {
            }
            return value;
        }

        /// <summary>
        /// create a file with path, initial size and chunkSize
        /// </summary>
        public static bool CreateFile(string path, long initSize, int chunkSize)
        {
            try
            {
                using (FileStream fs = new FileStream(path, FileMode.Create))
                {
                    fs.SetLength(initSize);
                }
                return true;
            }
            catch (Exception ex)
            {
                Utilities.Log(string.Format("Error [CreateFile] {0}", ex.Message));
            }
            return false;
        }

        /// <summary>
        /// compare two files
        /// </summary>
        public static ComapreResult CompareFiles(string path1, string path2, int chunkSize)
        {
            long chunks = 0;
            try
            {
                byte[] chunk1 = new byte[chunkSize];
                byte[] chunk2 = new byte[chunkSize];
                using (FileStream fs1 = new FileStream(path1, FileMode.Open))
                {
                    using (FileStream fs2 = new FileStream(path2, FileMode.Open))
                    {
                        int readSize = 0;
                        do
                        {
                            if ((readSize = fs1.Read(chunk1, 0, chunkSize)) != fs2.Read(chunk2, 0, chunkSize)) // if size is different
                            {
                                Log(string.Format("[CompareFiles({0}, {1})] Size is different at chunk {2}", path1, path2, chunks));
                                return ComapreResult.DifferentSize;
                            }
                            if (chunk1.SequenceEqual(chunk2) == false)
                            {
                                Log(string.Format("[CompareFiles({0}, {1})] Content is different at chunk {2}", path1, path2, chunks));
                                return ComapreResult.DifferentContent;
                            }
                            Utilities.Log(string.Format("."), false);
                            chunks++;
                        } while (readSize > 0);
                    }
                }
                return ComapreResult.Equal;
            }
            catch (Exception ex)
            {
                Utilities.Log(string.Format("Error [CompareFiles] Compared chunks {0}, Error {1}", chunks, ex.Message));
            }
            return ComapreResult.Error;
        }

        /// <summary>
        /// Transparency using winforms
        /// </summary>
        public static ActionResult Transparency(string pixelFormat = null, string imageFormat = null, bool resaturate = false, bool compositCopy = false)
        {
            var pf = PixelFormat.Format32bppPArgb;
            var imf = ImageFormat.Png;
            var mime = "image/png";

            if (!string.IsNullOrEmpty(pixelFormat))
            {
                pixelFormat = pixelFormat.ToLower();
                switch (pixelFormat)
                {
                    case "gdi":
                        pf = PixelFormat.Gdi;
                        break;
                    case "format24bpprgb":
                        pf = PixelFormat.Format24bppRgb;
                        break;
                    case "format32bppargb":
                        pf = PixelFormat.Format32bppArgb;
                        break;
                    case "format64bppargb":
                        pf = PixelFormat.Format64bppArgb;
                        break;
                    case "format64bpppargb":
                        pf = PixelFormat.Format64bppPArgb;
                        break;
                    case "format8bppindexed":
                        pf = PixelFormat.Format8bppIndexed;
                        break;
                    case "canonical":
                        pf = PixelFormat.Canonical;
                        break;
                    case "indexed":
                        pf = PixelFormat.Indexed;
                        break;
                }
            }

            if (!string.IsNullOrEmpty(imageFormat))
            {
                imageFormat = imageFormat.ToLower();
                switch (imageFormat)
                {
                    case "jpg":
                    case "jpeg":
                        imf = ImageFormat.Jpeg;
                        mime = "image/jpeg";
                        break;
                    case "gif":
                        imf = ImageFormat.Gif;
                        mime = "image/gif";
                        break;
                    case "tiff":
                        imf = ImageFormat.Tiff;
                        mime = "image/tiff";
                        break;
                }
            }

            var brush1 = new SolidBrush(Color.FromArgb(100, 255, 0, 0));
            var brush2 = new SolidBrush(Color.FromArgb(100, 0, 0, 255));

            MemoryStream imagebytes = new MemoryStream();
            using (Bitmap bmp = new Bitmap(256, 256, pf))
            {
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    if (compositCopy)
                    {
                        g.CompositingMode = CompositingMode.SourceCopy;
                    }

                    g.Clear(Color.Transparent);

                    g.FillRectangle(brush1, 0, 0, 200, 200);
                    g.FillRectangle(brush2, 50, 50, 200, 200);

                    if (resaturate)
                    {
                        // fixing colors by alpha value
						FixByAlpha(bmp);

                        ImageAttributes imgAttributes = new ImageAttributes();

                        var matrix = new ColorMatrix();
                        var alphaInverse = 2.55f;

                        matrix[0, 0] = alphaInverse;
                        matrix[1, 1] = alphaInverse;
                        matrix[2, 2] = alphaInverse;

                        imgAttributes.SetColorMatrix(matrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

                        Bitmap bmp2 = new Bitmap(256, 256, pf);
                        Graphics g2 = Graphics.FromImage(bmp2);
                        g2.DrawImage(bmp, new Rectangle(0, 0, bmp.Width, bmp.Height), 0, 0, bmp.Width, bmp.Height, GraphicsUnit.Pixel, imgAttributes);

                        // fixing colors by alpha value
                        FixByAlpha(bmp2);
                        bmp2.Save(imagebytes, imf);
                    }
                    else
                    {
                        // fixing colors by alpha value
                        FixByAlpha(bmp);
                        bmp.Save(imagebytes, imf);
                    }
                }
            }
            imagebytes.Position = 0;

            return File(imagebytes, mime);
        }

        /// <summary>
        /// fixing colors by alpha value
        /// </summary>
        private static void FixByAlpha(Bitmap bmp)
        {
            if (IsMustFixByAlpha()) // if have to fix colors by alpha value ... in other words if on linux
            {
                BitmapWrapper bmpWrapper = new BitmapWrapper(bmp);
                bmpWrapper.LockBits();
                for (var x = bmpWrapper.Width - 1; x >= 0; x--)
                {
                    for (var y = bmpWrapper.Width - 1; y >= 0; y--)
                    {
                        bmpWrapper.FixPixelByAlpha(x, y);
                    }
                }
                bmpWrapper.UnlockBits();
            }
        }

        /// <summary>
        /// test to fix colors by alpha value
        /// </summary>
        private static bool IsMustFixByAlpha()
        {
            // print a sample bitmap and test the colors
            var brush = new SolidBrush(Color.FromArgb(128, 255, 0, 0));
            using (Bitmap bmp = new Bitmap(4, 4, PixelFormat.Format32bppPArgb))
            {
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.FillRectangle(brush, 0, 0, bmp.Width, bmp.Height);
                    Color col = bmp.GetPixel(0, 0);
                    return col.R != 255;
                }
            }
        }

        /// <summary>
        /// write stream to a file
        /// </summary>
        private static ActionResult File(Stream stream, string mime)
        {
            if (Utilities.IsDontWrite2File == false)
            {
                byte[] databuf = new byte[stream.Length];
                stream.Read(databuf, 0, databuf.Length);
                using (FileStream fs = new FileStream("c:\\transparency." + mime.Replace('/', '.'), FileMode.Create))
                    fs.Write(databuf, 0, databuf.Length);
            }
            return ActionResult.Sucess;
        }

        /// <summary>
        /// Transparency using GTK#
        /// </summary>
        public static void Transparency_Gtk(string pixelFormat = null, string imageFormat = null, bool resaturate = false, bool compositCopy = false)
        {
            var imf = "png";
            var mime = "image/png";

            if (!string.IsNullOrEmpty(imageFormat))
            {
                imageFormat = imageFormat.ToLower();
                switch (imageFormat)
                {
                    case "jpg":
                    case "jpeg":
                        imf = "jpeg";
                        mime = "image/jpeg";
                        break;
                }
            }

            MemoryStream imagebytes = new MemoryStream();

            Gdk.Pixbuf pix = new Gdk.Pixbuf(Gdk.Colorspace.Rgb, true, 8, 500, 500); // bound of 500, 500
            Gdk.Pixbuf pix1 = new Gdk.Pixbuf(Gdk.Colorspace.Rgb, true, 8, 200, 200); // bound of 200, 200
            Gdk.Pixbuf pix2 = new Gdk.Pixbuf(Gdk.Colorspace.Rgb, true, 8, 200, 200); // bound of 200, 200
            pix.Fill(0x00000000); // transparent
            pix1.Fill(0xFF000064); // red color and alpha is 100
            pix2.Fill(0x0000FF64); // blue color and alpha is 100
            pix1.Composite(pix, 50, 50, 200, 200, 0.0, 0.0, 1.0, 1.0, Gdk.InterpType.Nearest, 255); // rect of 50, 50, 200, 200
            if (compositCopy)
                pix2.CopyArea(0, 0, 200, 200, pix, 150, 150); // rect of 150, 150, 200, 200
            else
                pix2.Composite(pix, 150, 150, 200, 200, 0.0, 0.0, 1.0, 1.0, Gdk.InterpType.Nearest, 255); // rect of 150, 150, 200, 200

            if (resaturate)
            {
                // color matrix
                ColorMatrix mat = new ColorMatrix();
                var a = 2.55f;
                mat[0, 0] = a;
                mat[1, 1] = a;
                mat[2, 2] = a;

                // adjustment color matrix
                byte[] rgbaCol = new byte[4];
                byte[] rgbaCol1 = new byte[4];
                int kk = 0;

                for (int i = 0; i < 500; i++)
                {
                    for (int j = 0; j < 500; j++, kk += 4)
                    {
                        int val = System.Runtime.InteropServices.Marshal.ReadInt32(pix.Pixels, kk);
                        if (val == 0)
                            continue;
                        rgbaCol[0] = (byte)(val); val = val >> 8;
                        rgbaCol[1] = (byte)(val); val = val >> 8;
                        rgbaCol[2] = (byte)(val); val = val >> 8;
                        rgbaCol[3] = (byte)(val);
                        for (int k = 0; k < 4; k++)
                        {
                            rgbaCol1[k] = 0;
                            if (rgbaCol[k] == 0)
                                continue;
                            int kl = 0;
                            for (int l = 0; l < 4; l++)
                            {
                                kl += (int)((int)rgbaCol[l] * mat[k, l]);
                            }
                            if (kl > 255) kl = 255;
                            if (kl < 0) kl = 0;
                            rgbaCol1[k] = (byte)kl;
                        }
                        val = rgbaCol1[3]; val = val << 8;
                        val |= rgbaCol1[2]; val = val << 8;
                        val |= rgbaCol1[1]; val = val << 8;
                        val |= rgbaCol1[0];
                        System.Runtime.InteropServices.Marshal.WriteInt32(pix.Pixels, kk, val);
                    }
                }
            }

            // get byte array by imf
            byte[] databuf = pix.SaveToBuffer(imf);

            // write databuf to imagebytes
            imagebytes.Write(databuf, 0, databuf.Length);

            // save to file
            using (FileStream fs = new FileStream("c:\\transparency." + imf, FileMode.Create))
                fs.Write(databuf, 0, databuf.Length);
            //pix.Save("c:\\test.png", "png");
            //return File(imagebytes, mime);
        }

        /// <summary>
        /// draw a ellipse using winforms
        /// </summary>
        static public MemoryStream DrawTile()
        {
            MemoryStream tilestream = new MemoryStream();

            using (Bitmap bmp = new Bitmap(256, 256, PixelFormat.Format32bppPArgb))
            using (Graphics g = Graphics.FromImage(bmp))
            using (GraphicsPath path = new GraphicsPath())
            {
                // Create solid brush.
                var semitransparentBrush = new SolidBrush(Color.FromArgb(128, Color.Red));

                // Create graphics path object and add ellipse.
                //GraphicsPath path = new GraphicsPath();
                path.AddEllipse(0, 0, 200, 100);

                g.FillPath(semitransparentBrush, path);

                FixByAlpha(bmp);
                bmp.Save(tilestream, ImageFormat.Png);
            }
            tilestream.Position = 0;

            // save to file
            byte[] databuf = tilestream.ToArray();
            using (FileStream fs = new FileStream("c:\\DrawTile.png", FileMode.Create))
                fs.Write(databuf, 0, databuf.Length);
            ///////////////

            return tilestream;
        }

        /*static public MemoryStream DrawTile_Gtk()
        {
            Gtk.Application.Init();
            MemoryStream tilestream = new MemoryStream();

            int width = 200;
            int height = 100;

            Cairo.ImageSurface surface = new Cairo.ImageSurface(Cairo.Format.Argb32, width, height);
            using (Cairo.Context cr = new Cairo.Context(surface))
            {
                cr.SetSourceRGBA(1.0, 0.0, 0.0, 0.5);
                OvalPath(cr, 200, 100);
                cr.Fill();
            }
            surface.WriteToPng("C:\\DrawTile_Gtk.png");

            /Gdk.Pixbuf pix = new Gdk.Pixbuf(Gdk.Colorspace.Rgb, true, 8, width, height);
            int k = 0;
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    k = k + 4;
                    uint val = (uint)System.Runtime.InteropServices.Marshal.ReadInt32(surface.DataPtr, k);
                    byte red = (byte)val;
                    byte blue = (byte)(val >> 16);
                    val = (uint)((uint)(val & 0xFF00FF00) | (uint)(red << 16) | (uint)blue);
                    System.Runtime.InteropServices.Marshal.WriteInt32(pix.Pixels, k, (int)val);
                }
            }

            // load stream from file
            using (FileStream fs = new FileStream("c:\\DrawTile_Gtk.png", FileMode.Open))
            {
                byte[] databuf = new byte[1024 * 256];
                int readSize = 0;
                while ((readSize = fs.Read(databuf, 0, databuf.Length)) > 0)
                    tilestream.Write(databuf, 0, readSize);
            }

            return tilestream;

        }*/
    }
}
