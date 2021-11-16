using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MozaikaApp.Properties;
using MozaikaApp.ViewModels;
using Newtonsoft.Json;
using Color = System.Drawing.Color;
using FontFamily = System.Drawing.FontFamily;
using Size = System.Drawing.Size;
namespace MozaikaApp.Models
{
    public static class Functions
    {
        public static string FindStringResource(string needKey)
        {
            try
            {
                string result = Application.Current.FindResource(needKey) as string;
                return result;
            }
            catch
            {
                return "";
            }
        }
        private static byte[] GetHash(string inputString)
        {
            using HashAlgorithm algorithm = SHA256.Create();
            return algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));
        }
        public static string GetHashString(string inputString)
        {

            StringBuilder sb = new StringBuilder();
            foreach (byte b in GetHash(inputString))
                sb.Append(b.ToString("X2"));

            return sb.ToString();
        }
        public static ImageSource BitmapFromUri(Uri source)
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = source;
            bitmap.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            return bitmap;
        }

        public static ImageSource BitmapToImageSource(Bitmap bitmap, bool disposable)
        {
            try
            {
                using var memory = new MemoryStream();
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
                BitmapImage bitmapImage = new BitmapImage();
                //memory.Position = 0;
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                if(disposable)
                    bitmap.Dispose();
                memory.Dispose();
                return bitmapImage;
            }
            catch
            {
                using var memory = new MemoryStream();
                var bitmapError = new Bitmap(200,200);
                Graphics g = Graphics.FromImage(bitmapError);
                g.DrawString(FindStringResource("ErrorSettings"), new Font(FontFamily.GenericSerif, 14), new SolidBrush(Color.Black), bitmapError.Width/2f, bitmapError.Height/2f  );
                g.Flush();
                bitmapError.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmap.Dispose();
                memory.Dispose();
                return bitmapImage;
            }
        }
        public static Size GetSizeString(int sizeString, int count)
        {
            var width = sizeString * 10*count;
            var height = sizeString * 10;
            var size = new Size(width, height);
            return size;
        }
        /// <summary>
        /// Generates random queue, where count - is number of elements from 0 to count
        /// </summary>
        public static Queue<int> GenerateRandomList(int count, Random random)
        {
            int[] perm = Enumerable.Range(0, count).ToArray(); // 0 1 2 ... (n - 1)
            var queue = new Queue<int>();
            // а то значения будут одни и те же
            for (int i = count - 1; i >= 1; i--)
            {
                int j = random.Next(i + 1);
                // exchange perm[j] and perm[i]
                int temp = perm[j];
                perm[j] = perm[i];
                perm[i] = temp;
            }
            foreach (var value in perm)
            {
                queue.Enqueue(value);
            }
            return queue;
        }
        public static Queue<int> GenerateRandomList(int count, List<int> queueBegin, Random random)
        {
            int[] perm = Enumerable.Range(0, count).ToArray(); // 0 1 2 ... (n - 1)
            var queue = new Queue<int>();
            foreach (var value in queueBegin)
            {
                queue.Enqueue(value);
            }
            // а то значения будут одни и те же
            for (int i = count - 1; i >= 1; i--)
            {
                int j = random.Next(i + 1);
                // exchange perm[j] and perm[i]
                int temp = perm[j];
                perm[j] = perm[i];
                perm[i] = temp;
            }
            foreach (var value in perm)
            {
                if (!queue.Contains(value))
                    queue.Enqueue(value);
            }
            return queue;
        }
        /// <summary>
        /// Mix input array and put in queue
        /// </summary>
        public static Queue<int> GenerateRandomList(int[] input, Random random)
        {
            int[] perm = input;
            var queue = new Queue<int>();
            // а то значения будут одни и те же
            for (int i = input.Length - 1; i >= 1; i--)
            {
                int j = random.Next(i + 1);
                // exchange perm[j] and perm[i]
                int temp = perm[j];
                perm[j] = perm[i];
                perm[i] = temp;
            }
            foreach (var value in perm)
            {
                queue.Enqueue(value);
            }
            return queue;
        }
        public static bool IsFileReady(string filename)
        {
            // If the file can be opened for exclusive access it means that the file
            // is no longer locked by another process.
            try
            {
                using FileStream inputStream = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.None);
                return inputStream.Length > 0;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public static bool ZipFolder(string inputDirectory, string outputDirectory)
        {
            ZipFile.CreateFromDirectory(inputDirectory, outputDirectory);
            return true;
        }
        public static bool UnzipFolder(string inputDirectory, string outputDirectory)
        {
            ZipFile.ExtractToDirectory(inputDirectory, outputDirectory);
            return true;
        }
        public static string SaveSettings()
        {
            var settings = new SaveSettings(Settings.Default.RandomList,Settings.Default.PrintDpi,Settings.Default.TextSize, 
                Settings.Default.CountWidth,Settings.Default.CountHeight,Settings.Default.Alpha,Settings.Default.Beta,
                Settings.Default.Gamma,Settings.Default.Transparency,Settings.Default.ThicknessSize,Settings.Default.HeightMm,
                Settings.Default.WidthMm, Settings.Default.ProMode,Settings.Default.FaceMode,Settings.Default.FirstStringColor,Settings.Default.SecondStringColor);
            var json = JsonConvert.SerializeObject(settings);
            return json;
        }
        public static void LoadSettings(string json)
        {

            var settings = JsonConvert.DeserializeObject<SaveSettings>(json);
            Settings.Default.RandomList = settings.RandomList;
            Settings.Default.PrintDpi = settings.PrintDpi;
            Settings.Default.TextSize = settings.TextSize;
            Settings.Default.CountWidth = settings.CountWidth;
            Settings.Default.CountHeight = settings.CountHeight;
            Settings.Default.Alpha = settings.Alpha;
            Settings.Default.Beta = settings.Beta;
            Settings.Default.Gamma = settings.Gamma;
            Settings.Default.Transparency = settings.Transparency;
            Settings.Default.ThicknessSize = settings.ThicknessSize;
            Settings.Default.HeightMm = settings.HeightMm;
            Settings.Default.WidthMm = settings.WidthMm;
            Settings.Default.ProMode = settings.ProMode;
            Settings.Default.FaceMode = settings.FaceMode;
            Settings.Default.FirstStringColor = settings.FirstStringColor;
            Settings.Default.SecondStringColor = settings.SecondStringColor;
            Settings.Default.Save();
        }
        public static string Encrypt(string text,string key)
        {
            var rc6 = new RC6(256,Encoding.UTF8.GetBytes(key));
            var result= rc6.EncodeRc6(text);
            return Convert.ToBase64String(result);
        }
        public static string Decrypt(string cryptoText,string key)
        {
            var rc6= new RC6(256, Encoding.UTF8.GetBytes(key));
            var result = rc6.DecodeRc6(Convert.FromBase64String(cryptoText));
            return Encoding.UTF8.GetString(result);
        }
        /// <summary>
        /// Generate elements for canvas widthMm and heightMm for 1 cell
        /// </summary>
        public static ObservableCollection<BaseThing> Generate(int viewDpi, double widthMm, double heightMm, int widthCount, int heightCount, int billBoardFontSize, int billBoardLineWidth)
        {
            var arrayList = new ObservableCollection<BaseThing>();
            try
            {
                int widthCell = (int)(PhotoProcessing.ConvertMmToInch(widthMm) * viewDpi);
                int heightCell = (int)(PhotoProcessing.ConvertMmToInch(heightMm) * viewDpi);
                for (int i = 1; i <= heightCount; i++)
                {
                    int num3 = (heightCell) * (i - 1);
                    for (int j = 1; j <= widthCount; j++)
                    {
                        int num4 = (widthCell) * (j - 1);
                        LabelVm label = new LabelVm
                        {
                            Tag = new Tuple<int, int>(i, j),
                            Width = widthCell,
                            Height = heightCell,
                            Content = $"R{i}/C{j}",
                            FontSize = billBoardFontSize,
                            Left = num4,
                            Top = num3,
                            ZIndex = 1
                        };
                        arrayList.Add(label);
                    }
                }
                for (int i = 1; i <= heightCount; i++)
                {
                    int num3 = (heightCell) * (i - 1);
                    for (int j = 1; j <= widthCount; j++)
                    {
                        int num4 = (widthCell) * (j - 1);
                        RectangleVm rectangle = new RectangleVm
                        {
                            BackgroundColor = System.Windows.Media.Brushes.Transparent,
                            StrokeThickness = (int)(PhotoProcessing.ConvertMmToInch(billBoardLineWidth) * viewDpi),
                            Width = widthCell,
                            Height = heightCell,
                            Tag = new Tuple<int, int>(0, 0),
                            Left = num4,
                            Top = num3,
                            ZIndex = 0
                        };
                        arrayList.Add(rectangle);
                    }
                }
                return arrayList;
            }
            catch (Exception)
            {
                return new ObservableCollection<BaseThing>();
            }
        }
        /// <summary>
        /// Generate only text elements for canvas widthMm and heightMm for 1 cell
        /// </summary>
        public static ObservableCollection<BaseThing> Generate(int viewDpi, double widthMm, double heightMm, int widthCount, int heightCount, int billBoardFontSize)
        {
            var arrayList = new ObservableCollection<BaseThing>();
            try
            {
                int widthCell = (int)(PhotoProcessing.ConvertMmToInch(widthMm) * viewDpi);
                int heightCell = (int)(PhotoProcessing.ConvertMmToInch(heightMm) * viewDpi);
                for (int i = 1; i <= heightCount; i++)
                {
                    int num3 = (heightCell) * (i - 1);
                    for (int j = 1; j <= widthCount; j++)
                    {
                        int num4 = (widthCell) * (j - 1);
                        LabelVm label = new LabelVm
                        {
                            Tag = new Tuple<int, int>(i, j),
                            Width = widthCell,
                            Height = heightCell,
                            Content = $"R{i}/C{j}",
                            FontSize = billBoardFontSize,
                            Left = num4,
                            Top = num3,
                            ZIndex = 1
                        };
                        arrayList.Add(label);
                    }
                }
                return arrayList;
            }
            catch (Exception)
            {
                return new ObservableCollection<BaseThing>();
            }
        }
        /// <summary>
        /// Generate elements for full screen canvas, intend need to place canvas in center of screen.
        /// </summary>
        public static ObservableCollection<BaseThing> Generate(int viewDpi, double widthMm, double heightMm, int widthFull, int heightFull, int widthCount, int heightCount, bool secondScreen)
        {
            var arrayList = new ObservableCollection<BaseThing>();
            try
            {
                int needWidth = (int)(PhotoProcessing.ConvertMmToInch(widthMm) * viewDpi*widthCount);
                int needHeight = (int)(PhotoProcessing.ConvertMmToInch(heightMm) * viewDpi*heightCount);
                double needFormat = needWidth / (double)needHeight;
                double screenFormat = widthFull / (double) heightFull;
                int intend = 0;
                bool sideIntend = false; // true - top intend, false - left intend
                if (Math.Abs(needFormat - screenFormat) > 0.05)
                { 
                    if (needFormat >= screenFormat)
                    {
                        intend = heightFull;
                        heightFull = (int)(heightFull * screenFormat / needFormat);
                        intend = (intend - heightFull) / 2;
                        sideIntend = true;
                    }
                    else
                    {
                        intend = widthFull;
                        widthFull = (int)(widthFull * needFormat / screenFormat);
                        intend = (intend - widthFull) / 2;
                    }
                }
                int widthCell = (int)Math.Round(widthFull / (double) widthCount, 0, MidpointRounding.ToEven)-1;
                int heightCell = (int) Math.Round(heightFull /(double) heightCount,0, MidpointRounding.ToEven)-1;
                var min = Math.Min(widthCell, heightCell);
                for (int i = 1; i <= heightCount; i++)
                {
                    int num3 = (heightCell) * (i - 1);
                    if (sideIntend && intend != 0)
                        num3 += intend;
                    for (int j = 1; j <= widthCount; j++)
                    {
                        int num4 = (widthCell) * (j - 1);
                        if (!sideIntend && intend != 0)
                            num4 += intend;
                        LabelVm label = new LabelVm
                        {
                            Tag = new Tuple<int, int>(i, j),
                            Width = widthCell,
                            Height = heightCell,
                            Content = $"R{i}/C{j}",
                            FontSize = Math.Max(min/5,1),
                            Left = num4,
                            Top = num3,
                            ZIndex = 1
                        };
                        arrayList.Add(label);
                    }
                }
                for (int i = 1; i <= heightCount; i++)
                {
                    int num3 = (heightCell) * (i - 1);
                    if (sideIntend && intend != 0)
                        num3 += intend;
                    for (int j = 1; j <= widthCount; j++)
                    {
                        int num4 = (widthCell) * (j - 1);
                        if (!sideIntend && intend != 0)
                            num4 += intend;
                        RectangleVm rectangle = new RectangleVm
                        {
                            StrokeThickness = (int)(PhotoProcessing.ConvertMmToInch(0.5) * viewDpi),
                            Width = widthCell,
                            Height = heightCell,
                            Tag = new Tuple<int, int>(0, 0),
                            Left = num4,
                            Top = num3,
                            ZIndex = 0
                        };
                        arrayList.Add(rectangle);
                    }
                }
                return arrayList;
            }
            catch (Exception)
            {
                return new ObservableCollection<BaseThing>();
            }
        }
    }
    public struct SaveSettings
    {
        public List<int> RandomList;
        public int PrintDpi;
        public double TextSize;
        public int CountWidth;
        public int CountHeight;
        public double Alpha;
        public double Beta;
        public double Gamma;
        public double Transparency;
        public int ThicknessSize;
        public double HeightMm;
        public double WidthMm;
        public bool ProMode;
        public bool FaceMode;
        public System.Windows.Media.Color FirstStringColor;
        public System.Windows.Media.Color SecondStringColor;
        public SaveSettings(List<int> randomList,int printDpi, double textSize, int countWidth, int countHeight,double alpha, double beta, double gamma,
            double transparency, int thicknessSize, double heightMm, double widthMm, bool proMode, bool faceMode, System.Windows.Media.Color firstStringColor, System.Windows.Media.Color secondStringColor)
        {
            RandomList = randomList;
            PrintDpi = printDpi;
            TextSize = textSize;
            CountWidth = countWidth;
            CountHeight = countHeight;
            Alpha = alpha;
            Beta = beta;
            Gamma = gamma;
            Transparency = transparency;
            ThicknessSize = thicknessSize;
            HeightMm = heightMm;
            WidthMm = widthMm;
            ProMode = proMode;
            FaceMode = faceMode;
            FirstStringColor = firstStringColor;
            SecondStringColor = secondStringColor;
        }
    }
}
