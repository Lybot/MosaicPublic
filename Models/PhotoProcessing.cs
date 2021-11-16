using System;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using Brushes = System.Drawing.Brushes;
using Color = System.Drawing.Color;
using FontFamily = System.Drawing.FontFamily;
using Pen = System.Drawing.Pen;

namespace MozaikaApp.Models
{
    public static class PhotoProcessing
    {
        private const double InchToMm = 0.0393701;
        public static Image<Bgra, byte> CutImageFaceMode(string sourcePath, string outPath, double format)
        {
            try
            {
                Image<Bgra, byte> source = new Bitmap(sourcePath).ToImage<Bgra, byte>();
                Image<Gray, byte> sourceGray = source.Convert<Gray, byte>();
                CascadeClassifier cascade = new CascadeClassifier("facebase.xml");
                var faces = cascade.DetectMultiScale(sourceGray);
                if (faces.Length > 0)
                {
                    Rectangle biggestRectangle = faces[0];
                    int biggestArea = biggestRectangle.Height * biggestRectangle.Width;
                    foreach (var face in faces)
                    {
                        int tempArea = face.Height * face.Width;
                        if (tempArea > biggestArea)
                        {
                            biggestRectangle = face;
                            biggestArea = tempArea;
                        }

                        //source.Draw(face, new Bgra(Color.Gray.B, Color.Gray.G, Color.Gray.R, 0), 10);
                    }
                    var centerPoint = new Point(biggestRectangle.X + biggestRectangle.Width / 2, biggestRectangle.Y + biggestRectangle.Height / 2);
                    double formatSource = source.Width;
                    formatSource /= source.Height;
                    if (formatSource > 1)
                    {
                        int needWidth = (int)(source.Height * format);
                        if (needWidth <= source.Width)
                        {
                            int halfNeedWidth = needWidth / 2;
                            if (centerPoint.X - halfNeedWidth >= 0 && centerPoint.X + halfNeedWidth <= source.Width)
                            {
                                source.ROI = Rectangle.FromLTRB(centerPoint.X - halfNeedWidth, 0, centerPoint.X + halfNeedWidth, source.Height);
                            }
                            else
                            {
                                if (centerPoint.X - halfNeedWidth < 0)
                                {
                                    source.ROI = Rectangle.FromLTRB(0, 0, needWidth, source.Height);
                                }

                                if (centerPoint.X + halfNeedWidth > source.Width)
                                {
                                    source.ROI = Rectangle.FromLTRB(source.Width - needWidth, 0, source.Width, source.Height);
                                }
                            }
                        }
                        else
                        {
                            int needHeight = (int)(source.Height / format);
                            int halfNeedHeight = needHeight / 2;
                            if (centerPoint.Y - halfNeedHeight >= 0 && centerPoint.Y + halfNeedHeight <= source.Height)
                            {
                                source.ROI = new Rectangle(0, centerPoint.Y - halfNeedHeight, source.Width, needHeight);
                            }
                            else
                            {
                                if (centerPoint.Y - halfNeedHeight < 0)
                                {
                                    source.ROI = new Rectangle(0, 0, source.Width, needHeight);
                                }

                                if (centerPoint.Y + halfNeedHeight > source.Height)
                                {
                                    source.ROI = new Rectangle(0, source.Height - needHeight, source.Width, needHeight);
                                }
                            }
                        }
                    }
                    else
                    {
                        int needHeight = (int)(source.Width / format);
                        if (needHeight <= source.Height)
                        {
                            int halfNeedHeight = needHeight / 2;
                            if (centerPoint.Y - halfNeedHeight >= 0 && centerPoint.Y + halfNeedHeight <= source.Height)
                            {
                                source.ROI = new Rectangle(0, centerPoint.Y - halfNeedHeight, source.Width, needHeight);
                            }
                            else
                            {
                                if (centerPoint.Y - halfNeedHeight < 0)
                                {
                                    source.ROI = new Rectangle(0, 0, source.Width, needHeight);
                                }

                                if (centerPoint.Y + halfNeedHeight > source.Height)
                                {
                                    source.ROI = new Rectangle(0, source.Height - needHeight, source.Width, needHeight);
                                }
                            }
                        }
                        else
                        {
                            int needWidth = (int)(source.Width * format);
                            int halfNeedWidth = needWidth / 2;
                            if (centerPoint.X - halfNeedWidth >= 0 && centerPoint.X + halfNeedWidth <= source.Width)
                            {
                                source.ROI = new Rectangle(centerPoint.X - halfNeedWidth, 0, needWidth, source.Height);
                            }
                            else
                            {
                                if (centerPoint.X - halfNeedWidth < 0)
                                {
                                    source.ROI = new Rectangle(0, 0, needWidth, source.Height);
                                }

                                if (centerPoint.X + halfNeedWidth > source.Width)
                                {
                                    source.ROI = new Rectangle(source.Width - needWidth, 0, needWidth, source.Height);
                                }
                            }
                        }
                    }
                    source.Save(outPath);
                    while (!Functions.IsFileReady(outPath))
                        Thread.Sleep(20);
                    var result = source.Copy();
                    return result;
                }

                return CutImageCenterMode(sourcePath, outPath, format);
            }
            catch (Exception)
            {
                return null;
            }
        }
        /// <summary> Cut image - take the smallest size and cut a half from another size from each side</summary> 
        public static Image<Bgra, byte> CutImageCenterMode(string sourcePath, string outPath, double format)
        {
            try
            {
                Image<Bgra, byte> source = new Bitmap(sourcePath).ToImage<Bgra, byte>();
                double formatSource = source.Width;
                formatSource /= source.Height;
                if (formatSource > 1)
                {
                    int needWidth = (int)(source.Height * format);
                    if (needWidth <= source.Width)
                    {
                        source.ROI = Rectangle.FromLTRB(source.Width / 2 - needWidth / 2, 0,
                            source.Width / 2 + needWidth / 2,
                            source.Height);
                    }
                    else
                    {
                        int needHeight = (int)(source.Height / format);
                        source.ROI = Rectangle.FromLTRB(0, source.Height / 2 - needHeight / 2, source.Width, source.Height / 2 + needHeight / 2);
                    }
                }
                else
                {
                    int needHeight = (int)(source.Width / format);
                    if (needHeight <= source.Height)
                    {
                        source.ROI = Rectangle.FromLTRB(0, source.Height / 2 - needHeight / 2, source.Width, source.Height / 2 + needHeight / 2);
                    }
                    else
                    {
                        int needWidth = (int)(source.Width * format);
                        source.ROI = Rectangle.FromLTRB(source.Width / 2 - needWidth / 2, 0, source.Width / 2 + needWidth / 2, source.Height);
                    }
                }
                source.Save(outPath);
                while (!Functions.IsFileReady(outPath))
                    Thread.Sleep(20);
                var result = source.Copy();
                source.Dispose();
                return result;
            }
            catch (Exception)
            {
                return null;
            }
        }
        /// <summary>Convert millimeters to inches </summary> 
        public static double ConvertMmToInch(double number)
        {
            return number * InchToMm;
        }
        public static double ConvertInchToMm(double number)
        {
            return number / InchToMm;
        }
        public static Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height, image.PixelFormat);
            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);
            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                using var wrapMode = new ImageAttributes();
                wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
            }
            return destImage;
        }

        /// <summary>Resize image and accumulate another picture to it</summary> 
        public static Image<Bgra, byte> ProMode(int dpi, double alpha, double beta, double gamma,
            Image<Bgra, byte> source, string underPath, double width, double height)
        {
            try
            {
                Image<Bgra, byte> resized = source.Resize((int)width, (int)height, Inter.Nearest);
                Image<Bgra, byte> underCvImage = new Bitmap(underPath).ToImage<Bgra, byte>();
                var result = resized.AddWeighted(underCvImage, alpha, beta, gamma);
                result.AsBitmap().SetResolution(dpi, dpi);
                underCvImage.Dispose();
                return result;
            }
            catch (Exception)
            {
                return null;
            }
        }
        public static Image<Bgra, byte> LightMode(int dpi, double transparency,
            Image<Bgra, byte> source, string underPath, double width, double height)
        {
            try
            {
                Image<Bgra, byte> resized = source.Resize((int)width, (int)height, Inter.Nearest);
                Image<Bgra, byte> underCvImage = new Bitmap(underPath).ToImage<Bgra, byte>();
                underCvImage[3] = underCvImage[3].Mul(transparency);
                Graphics g = Graphics.FromImage(resized.AsBitmap());
                g.CompositingMode = CompositingMode.SourceOver;
                g.DrawImage(underCvImage.AsBitmap(), new Point(0, 0));
                g.Dispose();
                underCvImage.Dispose();
                resized.AsBitmap().SetResolution(dpi, dpi);
                return resized;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static Image<Bgra, byte> PreviewProcess(double alpha, double beta, double gamma, double transparency,
            Image<Bgra, byte> source, Image<Bgra, byte> background, bool proMode)
        {
            try
            {
                Image<Bgra, byte> result;
                if (background.Size != source.Size)
                {
                    source = source.Resize(background.Size.Width, background.Size.Height, Inter.Linear);
                }
                if (proMode)
                {
                    result = source.AddWeighted(background, alpha, beta, gamma);
                }
                else
                {
                    var graphics = Graphics.FromImage(source.AsBitmap());
                    graphics.CompositingMode = CompositingMode.SourceOver;
                    background[3] = background[3].Mul(transparency);
                    graphics.DrawImage(background.AsBitmap(), 0, 0);
                    graphics.Save();
                    graphics.Dispose();
                    result = source;
                }
                return result;
            }
            catch (Exception)
            {
                return null;
            }
        }
        /// <summary>Main function which operates each picture has added to app,s folder</summary> 
        public static bool ProcessImage(double heightMm, double widthMm, int printDpi, int viewDpi, double alpha, double beta,
            double gamma, double transparency, Bgra firstBgr, Bgra secondBgr, double textSize, int thicknessSize, string filename, string path, string cutFolder, string viewFolder, string printFolder, string overlaidFolder,
            string underPathView, string underPathPrint, bool copied, bool faceMode, bool proMode)
        {
            Image<Bgra, byte> cuttedImage = null;
            Image<Bgra, byte> printImage = null;
            Image<Bgra, byte> viewImage = null;
            try
            {
                double format = widthMm / heightMm;
                if (!copied)
                {
                    string cutPath = cutFolder + "\\" + filename;
                    if (faceMode)
                        cuttedImage = CutImageFaceMode(path, cutPath, format);
                    else
                        cuttedImage = CutImageCenterMode(path, cutPath, format);
                    if (cuttedImage == null)
                        return false;
                }
                else
                {
                    cuttedImage = new Bitmap(path).ToImage<Bgra, byte>();
                }
                string viewPath = viewFolder + "\\" + filename;
                string printPath = printFolder + "\\" + filename;
                string overlaidPath = overlaidFolder + "\\" + filename;
                double inchHeight = ConvertMmToInch(heightMm);
                double inchWidth = ConvertMmToInch(widthMm);
                double printHeight = inchHeight * printDpi;
                double printWidth = inchWidth * printDpi;
                double viewHeight = inchHeight * viewDpi;
                double viewWidth = inchWidth * viewDpi;
                //string underPath = "";
                if (proMode)
                {
                    printImage = ProMode(printDpi, alpha, beta, gamma, cuttedImage, underPathPrint,
                        printWidth, printHeight);
                    viewImage = ProMode(viewDpi, alpha, beta, gamma, cuttedImage, underPathView,
                        viewWidth, viewHeight);
                }
                else
                {
                    printImage = LightMode(printDpi, transparency, cuttedImage, underPathPrint,
                        printWidth, printHeight);
                    viewImage = LightMode(viewDpi, transparency, cuttedImage, underPathView,
                        viewWidth, viewHeight);
                }
                if (viewImage == null || printImage == null)
                    return false;
                if (Activation.IsTrial)
                {
                    AddDemo(viewImage, 2);
                    if ((int)textSize == 0)
                        textSize = 2;
                    AddDemo(printImage, (int)textSize * 3);
                }
                int strokeSize = (int)textSize + thicknessSize;
                printImage.Save(overlaidPath);
                AddString(printImage, filename.Remove(filename.Length - 4), textSize, (int)textSize,
                    firstBgr, textSize, strokeSize, secondBgr);
                viewImage.Save(viewPath);
                printImage.Save(printPath);
                while (!Functions.IsFileReady(viewPath))
                    Thread.Sleep(20);
                while (!Functions.IsFileReady(printPath))
                    Thread.Sleep(20);
                viewImage?.Dispose();
                printImage?.Dispose();
                cuttedImage?.Dispose();
                return true;
            }
            catch (Exception)
            {
                viewImage?.Dispose();
                printImage?.Dispose();
                cuttedImage?.Dispose();
                //if (File.Exists(viewPath))
                //    viewImage.Save(viewPath);
                //if (!File.Exists(printPath))
                //    printImage.Save(printPath);
                return false;
            }
        }

        /// <summary>
        /// Calculate how much pixels we need to contain all photos with current dpi and length in millimeters
        /// </summary>
        /// <param name="widthMm"></param>
        /// <param name="heightMm"></param>
        /// <param name="dpi"></param>
        /// <param name="countWidth"></param>
        /// <param name="countHeight"></param>
        /// <returns></returns>
        public static Size CalculatePixels(double widthMm, double heightMm, int dpi, int countWidth, int countHeight)
        {
            var inchWidth = ConvertMmToInch(widthMm);
            var inchHeight = ConvertMmToInch(heightMm);
            var widthPixels = (int)(inchWidth * dpi) * countWidth;
            var heightPixels = (int)(inchHeight * dpi) * countHeight;
            return new Size(widthPixels, heightPixels);
        }
        /// <summary>
        /// Calculate size of picture in pixels
        /// </summary>
        /// <param name="widthMm">need width mm</param>
        /// <param name="heightMm">need height mm</param>
        /// <param name="dpi">dpi</param>
        /// <returns></returns>
        public static Size CalculatePixels(double widthMm, double heightMm, int dpi)
        {
            var inchWidth = ConvertMmToInch(widthMm);
            var inchHeight = ConvertMmToInch(heightMm);
            int widthPixels = (int)(inchWidth * dpi);
            int heightPixels = (int)(inchHeight * dpi);
            return new Size(widthPixels, heightPixels);
        }
        /// <summary>
        /// Calculate point in pixels
        /// </summary>
        /// <param name="widthMm">for canvas is left property in mm</param>
        /// <param name="heightMm">for canvas is top property in mm</param>
        /// <param name="dpi">dpi</param>
        /// <returns></returns>
        public static PointF CalculatePixelsPoint(float widthMm, float heightMm, int dpi)
        {
            var inchWidth = ConvertMmToInch(widthMm);
            var inchHeight = ConvertMmToInch(heightMm);
            var widthPixels = (float)(inchWidth * dpi);
            var heightPixels = (float)(inchHeight * dpi);
            return new PointF(widthPixels, heightPixels);
        }
        public static double GetMmFromPixels(int pixels, int dpi)
        {
            var inch = (double)pixels / dpi;
            var mm = ConvertInchToMm(inch);
            return mm;
        }
        public static double GetMmFromPixels(float pixels, int dpi)
        {
            var inch = pixels / dpi;
            var mm = ConvertInchToMm(inch);
            return mm;
        }

        public static bool CutUnderImageExperimental(string path, string underFolder,
            double widthMm, double heightMm, int dpi,
            int countWidth, int countHeight, Func<int, bool> donePhotos = null, Func<int, bool> needPhotos = null)
        {
            try
            {
                var sourceImage = new Bitmap(path);
                needPhotos(countWidth * countHeight);
                var donePhotosInt = 0;
                for (int i = 0; i < countHeight; i++)
                {
                    for (int j = 0; j < countWidth; j++)
                    {
                        var image = CutUnderImage(sourceImage, widthMm, heightMm, dpi, i, j, countWidth, countHeight);
                        image.Save(underFolder + $"/R{i+1}C{j+1}.png");
                        image.SetResolution(dpi,dpi);
                        image.Dispose();
                        donePhotosInt++;
                        donePhotos(donePhotosInt);
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }
        /// <summary>Cut foreground picture to small pictures to matrix (countHeight x countWidth)</summary> 
        public static bool CutUnderImage(string path, string pathSizedImage, string underFolder, double widthMm, double heightMm, int dpi,
            int countWidth, int countHeight, Func<int, bool> donePhotos = null, Func<int, bool> needPhotos = null)
        {
            try
            {
                var sourceImage = new Bitmap(path).ToImage<Bgra,byte>();
                needPhotos(countHeight * countWidth);
                var donePhotosInt = 0;
                var size = CalculatePixels(widthMm, heightMm, dpi, countWidth, countHeight);
                var temp = sourceImage.Resize(size.Width, size.Height, Inter.Linear).AsBitmap();
                sourceImage.Dispose();
                temp.Save(pathSizedImage);
                var x = 0;
                var widthSmall = (int)(ConvertMmToInch(widthMm) * dpi);
                var heightSmall = (int)(ConvertMmToInch(heightMm) * dpi);
                for (int i = 0; x < countHeight; i += heightSmall)
                {
                    x++;
                    var y = 0;
                    for (int j = 0; y < countWidth; j += widthSmall)
                    {
                        y++;
                        var tempPicture = new Bitmap(temp.Clone(new Rectangle(j, i, widthSmall, heightSmall),
                            temp.PixelFormat));
                        temp.SetResolution(dpi, dpi);
                        tempPicture.Save(underFolder + "/R" + x + "C" + y + ".png", ImageFormat.Png);
                        tempPicture.Dispose();
                        donePhotosInt++;
                        donePhotos(donePhotosInt);
                    }
                }
                temp.Dispose();
                //tempBitmap.Dispose();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        /// <summary> (Test function)Return need part of big image</summary> 
        public static Bitmap CutUnderImage(Bitmap bigImage, double widthMm, double heightMm, int dpi,
            int needRow, int needColumn, int countWidth, int countHeight)
        {
            try
            {
                var size = CalculatePixels(widthMm, heightMm, dpi, 1, 1);
                var cellSize = new Size(bigImage.Width / countWidth, bigImage.Height / countHeight);
                var top = cellSize.Height * needRow;
                var left = cellSize.Width * needColumn;
                var image = new Bitmap(bigImage.Clone(new Rectangle(left, top, cellSize.Width, cellSize.Height),
                    bigImage.PixelFormat));
                image = ResizeImage(image, size.Width, size.Height);
                return image;
            }
            catch (Exception)
            {
                return null;
            }
        }
        public static void AddString(Image<Bgra, byte> source, string text, double firstTextSize, int firstThickness, Bgra firstStringColor, double secondTextSize, int secondThickness, Bgra secondStringColor)
        {
            try
            {
                source.Draw(text, new Point(10, source.Size.Height - 10), FontFace.HersheyPlain, firstTextSize, secondStringColor, secondThickness);
                source.Draw(text, new Point(10, source.Size.Height - 10), FontFace.HersheyPlain, secondTextSize, firstStringColor, firstThickness);
                //return source;
            }
            catch (Exception)
            {
                //return null;
            }
        }
        public static void AddDemo(Image<Bgra, byte> source, int textSize)
        {
            try
            {
                var sizeString = Functions.GetSizeString(textSize, 4);
                var bottom = source.Height / 2 + sizeString.Height / 2;
                var left = source.Width / 2 - sizeString.Width / 2;
                source.Draw("DEMO", new Point(left, bottom), FontFace.HersheyPlain,
                    textSize, new Bgra(Color.Red.B, Color.Red.G, Color.Red.R, Color.Red.A), textSize + 1);
                //return source;
            }
            catch (Exception)
            {
                //return null;
            }
        }
        /// <summary>Link all photo to the big one</summary> 
        public static void LinkPhoto(string inputFolder, int widthPhoto, int heightPhoto, int countWidth, int countHeight, string destinationPath, bool useAddresses, bool smallRecursive = false)
        {
            int width = countWidth * widthPhoto;
            int height = countHeight * heightPhoto;
            Bitmap result;
            try
            {
                result = new Bitmap(width, height);
            }
            catch 
            {
                LinkPhoto(inputFolder, widthPhoto/2, heightPhoto/2, countWidth,countHeight, destinationPath,useAddresses,true);
                return;
            }
            //var dir = new DirectoryInfo(inputFolder);
            Graphics graphics = Graphics.FromImage(result);
            for (int i = 0; i < countHeight; i++)
            {
                var pointY = i * heightPhoto;
                for (int j = 0; j < countWidth; j++)
                {
                    var path = $"{inputFolder}\\R{i + 1}C{j + 1}.png";
                    var pointX = j * widthPhoto;
                    if (File.Exists(path))
                    {
                        var file = new Bitmap($"{inputFolder}\\R{i + 1}C{j + 1}.png");
                        if (smallRecursive)
                            file = ResizeImage(file, heightPhoto, widthPhoto);
                        graphics.DrawImage(file, new Point(pointX, pointY));
                        file.Dispose();
                    }
                    else
                    {
                        graphics.DrawRectangle(new Pen(Color.Black, 4), pointX, pointY, widthPhoto, heightPhoto);
                        if (!useAddresses) continue;
                        var sf = new StringFormat
                        {
                            LineAlignment = StringAlignment.Center,
                            Alignment = StringAlignment.Center
                        };
                        var min = Math.Min(widthPhoto, heightPhoto) / 10;
                        graphics.DrawString($"R{i + 1}C{j + 1}", new Font(FontFamily.GenericSerif, min), Brushes.Red, new RectangleF(pointX, pointY, widthPhoto, heightPhoto), sf);
                    }
                }
            }
            result.Save(destinationPath);
            while (!Functions.IsFileReady(destinationPath))
                Thread.Sleep(50);
            result.Dispose();
        }
    }
}
