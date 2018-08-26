using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Game2048.Miscellaneous
{
    static class ScreenshotHelper
    {
        public static byte[] GetJpgImage(this FrameworkElement source)
        {
            FrameworkElement element = source;
            // you can set the size as you need.
            Size targetSize = new Size(source.ActualWidth + 100, source.ActualHeight + 100);
            element.Measure(new System.Windows.Size(double.PositiveInfinity, double.PositiveInfinity));
            element.Arrange(new Rect(targetSize));
            // to affect the changes in the UI, you must call this method at the end to apply the new changes
            element.UpdateLayout();

            double dpiScale = 300.0 / 96;

            double dpiX = 300.0;
            double dpiY = 300.0;
            RenderTargetBitmap renderTarget = new RenderTargetBitmap(
                Convert.ToInt32((targetSize.Width) * dpiScale), 
                Convert.ToInt32((targetSize.Height) * dpiScale), 
                dpiX, dpiY, PixelFormats.Pbgra32);

            DrawingVisual drawingVisual = new DrawingVisual();
            DrawingContext drawingContext = drawingVisual.RenderOpen();
            using (drawingContext)
            {
                drawingContext.DrawRectangle(new SolidColorBrush(Color.FromArgb(255, 100, 100, 100)), null, new Rect(new Point(0, 0), new Point(targetSize.Width, targetSize.Height)));
                drawingContext.DrawRectangle(new VisualBrush(element), null, new Rect(new Point(0, 0), new Point(targetSize.Width, targetSize.Height)));
            }
            renderTarget.Render(drawingVisual);
            //renderTarget.Render(element);

            element.Measure(new System.Windows.Size());
            element.Arrange(new Rect());
            element.UpdateLayout();

            JpegBitmapEncoder jpgEncoder = new JpegBitmapEncoder();
            jpgEncoder.QualityLevel = 100;
            jpgEncoder.Frames.Add(BitmapFrame.Create(renderTarget));

            byte[] _imageArray;

            using (MemoryStream outputStream = new MemoryStream())
            {
                jpgEncoder.Save(outputStream);
                _imageArray = outputStream.ToArray();
            }

            return _imageArray;
        }

        public static byte[] GetShareImage(byte[] screenshotImage, long score)
        {
            Typeface boldType = (from t in Fonts.SystemFontFamilies where t.Source == "Segoe UI" select t)
                .First().GetTypefaces().Where(x => x.Style.ToString() == "Normal" && x.Weight.ToOpenTypeWeight() == 700).First();
            Typeface regType = (from t in Fonts.SystemFontFamilies where t.Source == "Segoe UI" select t)
                .First().GetTypefaces().Where(x => x.Style.ToString() == "Normal" && x.Weight.ToOpenTypeWeight() == 400).First();


            JpegBitmapDecoder decoder = new JpegBitmapDecoder(new MemoryStream(screenshotImage), BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
            BitmapSource source = decoder.Frames[0];
            RenderTargetBitmap renderTarget = new RenderTargetBitmap(1000, 1500, 300, 300, PixelFormats.Pbgra32);
            DrawingVisual drawingVisual = new DrawingVisual();
            DrawingContext drawingContext = drawingVisual.RenderOpen();
            using (drawingContext)
            {
                ImageBrush brush = new ImageBrush(source);
                brush.Stretch = Stretch.Uniform;
                drawingContext.DrawRectangle(new SolidColorBrush(Color.FromArgb(255, 100, 100, 100)), null, new Rect(new Point(0, 0), new Point(1000, 1500)));
                //drawingContext.DrawRectangle(brush, null, new Rect(new Point(50, 0), new Point(950, 900)));
                drawingContext.DrawRectangle(brush, null, new Rect(new Point(16, 50), new Point(304, 338))); //Factor = 3.125 (320 Full Width)
                drawingContext.DrawText(new FormattedText($"I just scored", System.Globalization.CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, regType, 18, Brushes.White), new Point(16, 350));
                drawingContext.DrawText(new FormattedText($"{score}", System.Globalization.CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, boldType, 24, Brushes.White), new Point(16,373));
                drawingContext.DrawText(new FormattedText($"points on 2048!", System.Globalization.CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, regType, 18, Brushes.White), new Point(16, 400));
            }
            renderTarget.Render(drawingVisual);

            JpegBitmapEncoder jpgEncoder = new JpegBitmapEncoder();
            jpgEncoder.QualityLevel = 100;
            jpgEncoder.Frames.Add(BitmapFrame.Create(renderTarget));
            byte[] _imageArray;

            using (MemoryStream outputStream = new MemoryStream())
            {
                jpgEncoder.Save(outputStream);
                _imageArray = outputStream.ToArray();
            }

            return _imageArray;
        }
    }
}
