using MobileLogs;
using MobileMap;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;

namespace MobileDebug_WPF.Models
{
    public struct HeatPoint
    {
        public System.Drawing.Point Point;
        public int X => Point.X;
        public int Y => Point.Y;
        public byte Intensity;
        public HeatPoint(int iX, int iY, byte bIntensity)
        {
            Point = new System.Drawing.Point(iX, iY);
            Intensity = bIntensity;
        }
        public HeatPoint(System.Drawing.Point point, byte bIntensity)
        {
            Point = point;
            Intensity = bIntensity;
        }

    }

    public class HeatMap
    {
        private MapFile MapFile;
        private List<HeatPoint> HeatPoints { get; set; } = new List<HeatPoint>();

        public void GetHeatMapPoints()
        {
            if (MapFile.Map.LoggedWifi != null)
            {
                System.Drawing.Rectangle r = new System.Drawing.Rectangle((int)MapFile.Map.Header.WidthOffset, (int)MapFile.Map.Header.HeightOffset, (int)MapFile.Map.Header.Width, (int)MapFile.Map.Header.Height);

                foreach (var s in MapFile.Map.LoggedWifi)
                    foreach (var sEntry in s.Value)
                        if (r.Contains(sEntry.Position))
                        {
                            if (HeatPoints.Count() == 0)
                            {
                                HeatPoints.Add(new HeatPoint(sEntry.Position, (byte)Math.Abs(sEntry.Baud)));
                                continue;
                            }

                            int i = 0;
                            bool found = false;
                            foreach (HeatPoint hp in HeatPoints.ToArray())
                            {
                                if (new System.Drawing.Rectangle(hp.X, hp.Y, 500, 500).Contains(sEntry.Position))
                                {
                                    HeatPoints[i] = new HeatPoint(hp.Point, (byte)((hp.Intensity + Math.Abs(sEntry.Baud)) / 2));
                                    found = true;
                                    break;
                                }
                                i++;
                            }
                            if (!found)
                            {
                                HeatPoints.Add(new HeatPoint(sEntry.Position, (byte)Math.Abs(sEntry.Baud)));
                            }
                        }
            }
        }

        private System.Windows.Controls.Image DrawWiFiHeatMap(int width, int height)
        {

            //UpdateStatus("Drawing WiFi Heat Map");

            MapFile.Map.Header.SetScaleFactor(width, height);

            //HeatMap.HeatMapImage img = new HeatMap.HeatMapImage((int)MapFile.Map.Header.WidthScaled, (int)MapFile.Map.Header.HeightScaled, 100, 10);

            //List<HeatMap.DataType> lst = new List<HeatMap.DataType>();

            //foreach(HeatPoint hp1 in HeatPoints)
            //{
            //    img.SetAData(new HeatMap.DataType() { X = (int)((hp1.X - MapFile.Map.Header.WidthOffset) * MapFile.Map.Header.ScaleFactor), Y = (int)((hp1.Y - MapFile.Map.Header.HeightOffset - MapFile.Map.Header.Height) * -MapFile.Map.Header.ScaleFactor), Weight = hp1.Intensity + 256 });
            //}
            System.Drawing.Bitmap bmp = new System.Drawing.Bitmap((int)MapFile.Map.Header.WidthScaled, (int)MapFile.Map.Header.HeightScaled, System.Drawing.Imaging.PixelFormat.Format16bppRgb555);//img.GetHeatMap();//

            bmp = DrawWiFiHeatMapFromLog(bmp);

            using (System.Drawing.Pen blackPen = new System.Drawing.Pen(System.Drawing.Color.Black, 20))
            using (System.Drawing.Brush blackBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Black),
                                        whiteBrush = new System.Drawing.SolidBrush(System.Drawing.Color.White))
            using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bmp))
            {
                g.TranslateTransform(-(int)MapFile.Map.Header.WidthOffset, -(int)MapFile.Map.Header.HeightOffset - (int)MapFile.Map.Header.Height);
                //g.ScaleTransform((int)MapFile.Map.Header.ScaleFactor, -(int)MapFile.Map.Header.ScaleFactor, System.Drawing.Drawing2D.MatrixOrder.Append);

                foreach (MapGeometry.Line ln in MapFile.Map.Geometry.Lines)
                    g.DrawLine(blackPen, ln.Start, ln.End);

                foreach (System.Drawing.Point ln in MapFile.Map.Geometry.Points)
                    g.FillRectangle(blackBrush, ln.X, ln.Y, 20, 20);

                foreach (HeatPoint hp in HeatPoints)
                    g.FillRectangle(blackBrush, hp.X, hp.Y, 20, 20);

            }
            string SigBase64;
            using (MemoryStream memory = new MemoryStream())
            {
                bmp.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                SigBase64 = Convert.ToBase64String(memory.GetBuffer());
            }

            return new System.Windows.Controls.Image() { Source = Base64StringToBitmap(SigBase64) };
        }
        private System.Drawing.Bitmap DrawWiFiHeatMapFromLog(System.Drawing.Bitmap bSurface)
        {
            bSurface = CreateIntensityMask(bSurface);

            bSurface = Colorize(bSurface, 1);

            return bSurface;
        }
        private System.Drawing.Bitmap CreateIntensityMask(System.Drawing.Bitmap bSurface)
        {
            // Create new graphics surface from memory bitmap
            using (System.Drawing.Graphics DrawSurface = System.Drawing.Graphics.FromImage(bSurface))
            {
                DrawSurface.TranslateTransform(-MapFile.Map.Header.WidthOffset, -MapFile.Map.Header.HeightOffset - MapFile.Map.Header.Height);
                DrawSurface.ScaleTransform(MapFile.Map.Header.ScaleFactor, -MapFile.Map.Header.ScaleFactor, System.Drawing.Drawing2D.MatrixOrder.Append);
                // Set background color to white so that pixels can be correctly colorized
                DrawSurface.Clear(System.Drawing.Color.White);
                double scale = MapFile.Map.Header.Width / MapFile.Map.Header.WidthScaled;
                // Traverse heat point data and draw masks for each heat point
                int cnt = HeatPoints.Count();
                int i = 1;
                int ii = 1;

                foreach (HeatPoint DataPoint in HeatPoints)
                {
                    // Render current heat point on draw surface
                    if (ii == 100)
                    {
                        //UpdateStatus($"Drawing WiFi Heat Map: {i} of {cnt}: {TimeSpan.FromMilliseconds(sw.ElapsedMilliseconds).ToString()}");
                        ii = 0;
                    }
                    i++;
                    ii++;
                    DrawHeatPoint(DrawSurface, DataPoint, (int)(500 * scale));
                }
            }
            return bSurface;
        }
        private void DrawHeatPoint(System.Drawing.Graphics Canvas, HeatPoint HeatPoint, int Radius)
        {
            // Create points generic list of points to hold circumference points
            List<System.Drawing.Point> CircumferencePointsList = new List<System.Drawing.Point>();

            // Create an empty point to predefine the point struct used in the circumference loop
            System.Drawing.Point CircumferencePoint;

            // Create an empty array that will be populated with points from the generic list
            System.Drawing.Point[] CircumferencePointsArray;

            // Calculate ratio to scale byte intensity range from 0-255 to 0-1
            float fRatio = 1F / Byte.MaxValue;
            // Precalulate half of byte max value
            byte bHalf = Byte.MaxValue / 2;
            // Flip intensity on it's center value from low-high to high-low
            int iIntensity = (byte)(HeatPoint.Intensity - ((HeatPoint.Intensity - bHalf) * 2));
            // Store scaled and flipped intensity value for use with gradient center location
            float fIntensity = iIntensity * fRatio;

            // Loop through all angles of a circle
            // Define loop variable as a double to prevent casting in each iteration
            // Iterate through loop on 10 degree deltas, this can change to improve performance
            for (double i = 0; i <= 360; i += 10)
            {
                // Replace last iteration point with new empty point struct
                CircumferencePoint = new System.Drawing.Point
                {

                    // Plot new point on the circumference of a circle of the defined radius
                    // Using the point coordinates, radius, and angle
                    // Calculate the position of this iterations point on the circle
                    X = Convert.ToInt32(HeatPoint.X + Radius * Math.Cos(ConvertDegreesToRadians(i))),
                    Y = Convert.ToInt32(HeatPoint.Y + Radius * Math.Sin(ConvertDegreesToRadians(i)))
                };
                // Add newly plotted circumference point to generic point list
                CircumferencePointsList.Add(CircumferencePoint);
            }

            // Populate empty points system array from generic points array list
            // Do this to satisfy the datatype of the PathGradientBrush and FillPolygon methods
            CircumferencePointsArray = CircumferencePointsList.ToArray();

            // Create new PathGradientBrush to create a radial gradient using the circumference points
            System.Drawing.Drawing2D.PathGradientBrush GradientShaper = new System.Drawing.Drawing2D.PathGradientBrush(CircumferencePointsArray);
            // Create new color blend to tell the PathGradientBrush what colors to use and where to put them
            System.Drawing.Drawing2D.ColorBlend GradientSpecifications = new System.Drawing.Drawing2D.ColorBlend(3)
            {
                // Define positions of gradient colors, use intesity to adjust the middle color to
                // show more mask or less mask
                Positions = new float[3] { 0, fIntensity, 1 },
                // Define gradient colors and their alpha values, adjust alpha of gradient colors to match intensity
                Colors = new System.Drawing.Color[3]
                {
                    System.Drawing.Color.FromArgb(0, System.Drawing.Color.White),
                    System.Drawing.Color.FromArgb(HeatPoint.Intensity, System.Drawing.Color.Black),
                    System.Drawing.Color.FromArgb(HeatPoint.Intensity, System.Drawing.Color.Black)
                }
            };

            // Pass off color blend to PathGradientBrush to instruct it how to generate the gradient
            GradientShaper.InterpolationColors = GradientSpecifications;
            // Draw polygon (circle) using our point array and gradient brush
            Canvas.FillPolygon(GradientShaper, CircumferencePointsArray);
        }
        private double ConvertDegreesToRadians(double degrees)
        {
            double radians = (Math.PI / 180) * degrees;
            return (radians);
        }
        public System.Drawing.Bitmap Colorize(System.Drawing.Bitmap Mask, byte Alpha)
        {
            // Create new bitmap to act as a work surface for the colorization process
            System.Drawing.Bitmap Output = new System.Drawing.Bitmap(Mask.Width, Mask.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            // Create a graphics object from our memory bitmap so we can draw on it and clear it's drawing surface
            System.Drawing.Graphics Surface = System.Drawing.Graphics.FromImage(Output);
            //Surface.TranslateTransform(-map.Header.WidthOffset, -map.Header.HeightOffset - map.Header.Height);
            //Surface.ScaleTransform(map.Header.ScaleFactor.Width, -map.Header.ScaleFactor.Height, System.Drawing.Drawing2D.MatrixOrder.Append);

            Surface.Clear(System.Drawing.Color.Transparent);
            // Build an array of color mappings to remap our greyscale mask to full color
            // Accept an alpha byte to specify the transparancy of the output image
            System.Drawing.Imaging.ColorMap[] Colors = CreatePaletteIndex(Alpha);
            // Create new image attributes class to handle the color remappings
            // Inject our color map array to instruct the image attributes class how to do the colorization
            System.Drawing.Imaging.ImageAttributes Remapper = new System.Drawing.Imaging.ImageAttributes();
            Remapper.SetRemapTable(Colors);
            // Draw our mask onto our memory bitmap work surface using the new color mapping scheme
            Surface.DrawImage(Mask, new System.Drawing.Rectangle(0, 0, Mask.Width, Mask.Height), 0, 0, Mask.Width, Mask.Height, System.Drawing.GraphicsUnit.Pixel, Remapper);
            // Send back newly colorized memory bitmap
            return Output;
        }
        private System.Drawing.Imaging.ColorMap[] CreatePaletteIndex(byte Alpha)
        {
            System.Drawing.Imaging.ColorMap[] OutputMap = new System.Drawing.Imaging.ColorMap[256];
            // Change this path to wherever you saved the palette image.
            System.Drawing.Bitmap Palette = (System.Drawing.Bitmap)System.Drawing.Bitmap.FromFile($"{System.IO.Directory.GetCurrentDirectory()}\\Assets\\Palette.bmp");
            // Loop through each pixel and create a new color mapping
            for (int X = 0; X <= 255; X++)
            {
                OutputMap[X] = new System.Drawing.Imaging.ColorMap
                {
                    OldColor = System.Drawing.Color.FromArgb(X, X, X),
                    NewColor = System.Drawing.Color.FromArgb(Alpha, Palette.GetPixel(X, 0))
                };
            }
            return OutputMap;
        }
        public BitmapImage Base64StringToBitmap(string base64String)
        {
            BitmapImage bitmapImage = new BitmapImage();
            byte[] byteBuffer = Convert.FromBase64String(base64String);
            using (MemoryStream memoryStream = new MemoryStream(byteBuffer))
            {
                memoryStream.Position = 0;

                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memoryStream;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
            }

            return bitmapImage;
        }
    }
}
