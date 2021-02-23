using MobileMap;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MobileDebug_WPF
{
    public class Draw2DMap
    {
        public Map Map { get; set; }

        public void Draw(Grid grid)
        {
            grid.Width = Map.Header.WidthScaled;
            grid.Height = Map.Header.HeightScaled;

            grid.Background = new ImageBrush(Base64StringToBitmap(DrawMap()));

            TransformGroup trg = new TransformGroup();
            trg.Children.Add(new TranslateTransform(-Map.Header.Max_X, -Map.Header.Min_Y));
            trg.Children.Add(new ScaleTransform(-1, 1));
            trg.Children.Add(new RotateTransform(180, Map.Header.Width / 2, Map.Header.Height / 2));

            ///grid.Children.Add(new Image() { Source = Base64StringToBitmap(DrawMap()), Stretch = Stretch.UniformToFill });

            //foreach (MapGeometry.Line ln in Map.Geometry.Lines)
            //{
            //    grid.Children.Add(new Line()
            //    {
            //        Stroke = System.Windows.Media.Brushes.LightSteelBlue,
            //        StrokeThickness = 100,
            //        X1 = ln.Start.X,
            //        Y1 = ln.Start.Y,
            //        X2 = ln.End.X,
            //        Y2 = ln.End.Y,
            //        RenderTransform = trg,
            //        HorizontalAlignment = HorizontalAlignment.Left,
            //        VerticalAlignment = VerticalAlignment.Top,
            //    });
            //}
            //foreach (System.Drawing.Point p in Map.Geometry.Points)
            //{
            //    grid.Children.Add(new Line()
            //    {
            //        Stroke = System.Windows.Media.Brushes.LightSteelBlue,
            //        StrokeThickness = 100,
            //        X1 = p.X,
            //        Y1 = p.Y,
            //        X2 = p.X+100,
            //        Y2 = p.Y+100,
            //        RenderTransform = trg,
            //        HorizontalAlignment = HorizontalAlignment.Left,
            //        VerticalAlignment = VerticalAlignment.Top,
            //    });
            //}
        }
        private string DrawMap()
        {
            System.Drawing.Bitmap bmp = new System.Drawing.Bitmap((int)Map.Header.WidthScaled, (int)Map.Header.HeightScaled, System.Drawing.Imaging.PixelFormat.Format16bppRgb555);

            using (System.Drawing.Pen blackPen = new System.Drawing.Pen(System.Drawing.Color.Black, 20))
            using (System.Drawing.Brush blackBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Black),
                                        whiteBrush = new System.Drawing.SolidBrush(System.Drawing.Color.White))
            using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bmp))
            {
                g.FillRectangle(whiteBrush, 0, 0, (int)Map.Header.WidthScaled, (int)Map.Header.HeightScaled);
                g.TranslateTransform(-(int)Map.Header.WidthOffset, -(int)Map.Header.HeightOffset - (int)Map.Header.Height);
                g.ScaleTransform(Map.Header.ScaleFactor, -Map.Header.ScaleFactor, System.Drawing.Drawing2D.MatrixOrder.Append);
                //g.FillRectangle(whiteBrush, 0, 0, (int)Map.Header.Width, (int)Map.Header.Height);
                //g.TranslateTransform(-Map.Header.Max_X, -Map.Header.Min_Y);
                //g.ScaleTransform(Map.Header.ScaleFactor, -Map.Header.ScaleFactor, System.Drawing.Drawing2D.MatrixOrder.Append);
                //g.TranslateTransform(Map.Header.Width / 2, Map.Header.Height / 2, System.Drawing.Drawing2D.MatrixOrder.Append);
                //g.RotateTransform(180, System.Drawing.Drawing2D.MatrixOrder.Append);

                //foreach (MapGeometry.Line ln in map.Geometry.Lines)
                //    g.DrawLine(blackPen, ln.Start, ln.End);

                foreach (System.Drawing.Point ln in Map.Geometry.Points)
                    g.FillRectangle(blackBrush, ln.X, ln.Y, 20, 20);
            }

            using (MemoryStream memory = new MemoryStream())
            {
                bmp.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                var SigBase64 = Convert.ToBase64String(memory.GetBuffer());
                return SigBase64;
            }
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
