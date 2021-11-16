using System;
using System.Drawing;
using System.Drawing.Printing;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using MozaikaApp.Properties;
using MozaikaApp.Views;
using Prism.Mvvm;
using Xceed.Wpf.Toolkit.Core.Input;
using Color = System.Windows.Media.Color;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;

namespace MozaikaApp.Models
{
    public class PrintSettingsModel : BindableBase
    {
        public static Action FullScreen { get; set; }
        private Dispatcher Dispatcher { get; set; }
        //private SeveralPhotosSettings SeveralPhotosSettingsWindow { get; set; }
        public string PrintDpi
        {
            get => Settings.Default.PrintDpi.ToString();
            set
            {
                Settings.Default.PrintDpi = int.Parse(value);
                Settings.Default.Save();
                RaisePropertyChanged();
                UpdateResult();
            }
        }
        public string TextSize
        {
            get => Settings.Default.TextSize.ToString(CultureInfo.InvariantCulture);
            set
            {
                Settings.Default.TextSize = double.Parse(value);
                Settings.Default.Save();
                RaisePropertyChanged();
                UpdateResult();
            }
        }
        public Color FirstStringColor
        {
            get => Settings.Default.FirstStringColor;
            set
            {
                Settings.Default.FirstStringColor = value;
                Settings.Default.Save();
                RaisePropertyChanged();
                UpdateResult();
            }
        }
        public bool UseIndents
        {
            get => Settings.Default.UseIndents;
            set
            {
                Settings.Default.UseIndents = value;
                Settings.Default.Save();
                RaisePropertyChanged();
            }
        }

        public bool UseSeveralPhotos
        {
            get => Settings.Default.UseSeveralPhotos;
            set
            {
                Settings.Default.UseSeveralPhotos = value;
                Settings.Default.Save();
                RaisePropertyChanged();
            }
        }
        public string ThicknessSize
        {
            get => Settings.Default.ThicknessSize.ToString();
            set
            {
                Settings.Default.ThicknessSize = int.Parse(value);
                Settings.Default.Save();
                RaisePropertyChanged();
                UpdateResult();
            }
        }
        public Color SecondStringColor
        {
            get => Settings.Default.SecondStringColor;
            set
            {
                Settings.Default.SecondStringColor = value;
                Settings.Default.Save();
                RaisePropertyChanged();
                UpdateResult();
            }
        }
        private Color _backgroundColor = Colors.DarkGray;
        public Color BackgroundColor
        {
            get => _backgroundColor;
            set
            {
                _backgroundColor = value;
                UpdateResult();
            }
        }
        public string FirstPrinter
        {
            get => Settings.Default.FirstPrinter;
            set
            {
                Settings.Default.FirstPrinter = value;
                Settings.Default.Save();
                RaisePropertyChanged();
            }
        }
        public string SecondPrinter
        {
            get => Settings.Default.SecondPrinter;
            set
            {
                Settings.Default.SecondPrinter = value;
                Settings.Default.Save();
                RaisePropertyChanged();
            }
        }
        public bool SecondPrinterAvailable
        {
            get => Settings.Default.SecondPrinterAvailable;
            set
            {
                Settings.Default.SecondPrinterAvailable = value;
                Settings.Default.Save();
                RaisePropertyChanged();
                RaisePropertyChanged($"SecondPrinterVisibility");
            }
        }
        public Visibility SecondPrinterVisibility
        {
            get
            {
                if (SecondPrinterAvailable)
                    return Visibility.Visible;
                return Visibility.Collapsed;
            }
        }


        //public string WidthPrinter
        //{
        //    get => Settings.Default.WidthTestPrinter.ToString(Settings.Default.DefaultLanguage);
        //    set
        //    {
        //        Settings.Default.WidthTestPrinter = double.Parse(value);
        //        Settings.Default.Save();
        //        UpdateResult();
        //    }
        //}
        //public string HeightPrinter
        //{
        //    get => Settings.Default.HeightTestPrinter.ToString(Settings.Default.DefaultLanguage);
        //    set
        //    {
        //        Settings.Default.HeightTestPrinter = double.Parse(value);
        //        Settings.Default.Save();
        //        UpdateResult();
        //    }
        //}
        public string TopIndent
        {
            get => Settings.Default.TopIndent.ToString(Settings.Default.DefaultLanguage);
            set
            {
                Settings.Default.TopIndent = double.Parse(value);
                Settings.Default.Save();
                //UpdateResult();
            }
        }

        public string LeftIndent
        {
            get => Settings.Default.LeftIndent.ToString(Settings.Default.DefaultLanguage);
            set
            {
                Settings.Default.LeftIndent = double.Parse(value);
                Settings.Default.Save();
                //UpdateResult();
            }
        }
        public KeyModifierCollection KeyModifiers { get; } = new KeyModifierCollection() { KeyModifier.None };
        public PrinterSettings.StringCollection Printers = PrinterSettings.InstalledPrinters;
        private Bitmap _resultImage;
        public ImageSource ResultImage => Functions.BitmapToImageSource(_resultImage,false);
        private readonly string _testString = "R10C17";

        public PrintSettingsModel()
        {
            Dispatcher = Dispatcher.CurrentDispatcher;
        }

        private Size Size => PhotoProcessing.CalculatePixels(Settings.Default.WidthMm, Settings.Default.HeightMm,
            Settings.Default.PrintDpi);
        public void UpdateResult()
        {
            if (Size.Height <= 0 || Size.Width <= 0)
            {
                MessageBox.Show(Functions.FindStringResource("ErrorSettings"));
                return;
            }
            var testImage = new Image<Bgra, byte>(Size.Width, Size.Height,
                new Bgra(BackgroundColor.B, BackgroundColor.G, BackgroundColor.R, BackgroundColor.A));
            var firstBgr = new Bgra(FirstStringColor.B, FirstStringColor.G, FirstStringColor.R, FirstStringColor.A);
            var secondBgr = new Bgra(SecondStringColor.B, SecondStringColor.G, SecondStringColor.R, SecondStringColor.A);
            int textSize = (int)Settings.Default.TextSize;
            int strokeSize = textSize + Settings.Default.ThicknessSize;
            testImage.Draw(Rectangle.FromLTRB(0, 0, testImage.Width - 2, testImage.Height - 2),
                new Bgra(0,0,0,255), 2);
            testImage.Draw(_testString, new Point(10, testImage.Height - 10), FontFace.HersheyPlain, textSize,
                secondBgr, strokeSize);
            testImage.Draw(_testString, new Point(10, testImage.Height - 10), FontFace.HersheyPlain, textSize,
                firstBgr, textSize);
            //if (UseIndents)
            //{
            //    double width = Settings.Default.WidthTestPrinter;
            //    double height = Settings.Default.HeightTestPrinter;
            //    double top = Settings.Default.TopIndent;
            //    double left = Settings.Default.LeftIndent;
            //    var indent = PhotoProcessing.CalculatePixels(left, top, Settings.Default.PrintDpi, 1, 1);
            //    var size = new Size((int)(PhotoProcessing.ConvertMmToInch(width) * Settings.Default.PrintDpi),
            //        (int)(PhotoProcessing.ConvertMmToInch(height) * Settings.Default.PrintDpi));
            //    var fullImage = new Image<Bgr, byte>(size.Width, size.Height, new Bgr(System.Drawing.Color.White));
            //    var g = Graphics.FromImage(fullImage.Bitmap);
            //    g.DrawImage(testImage.Bitmap, new Point(indent.Width, indent.Height));
            //    g.Save();
            //    _resultImage = fullImage.Clone();
            //    fullImage.Dispose();
            //    testImage.Dispose();
            //}
            //else
            //_resultImage.Dispose();
            //_resultImage = new Bitmap(Size.Width, Size.Height);
            _resultImage = testImage.ToBitmap();
            testImage.Dispose();
            RaisePropertyChanged($"ResultImage");
            RaisePropertyChanged($"_resultImage");
            Task.Run(delegate
            {
                Thread.Sleep(300);
                Dispatcher.Invoke(FullScreen);
            });
        }

        public void SeveralPhotosSettings()
        {
            new SeveralPhotosSettings().ShowDialog();
        }
        public void PrintTestPhoto()
        {
            try
            {
                using MemoryStream memory = new MemoryStream();
                _resultImage.SetResolution(Settings.Default.PrintDpi, Settings.Default.PrintDpi);
                _resultImage.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
                var img = Image.FromStream(memory);
                using var pd = new PrintDocument();
                pd.PrintPage += (o, e) =>
                {
                    if (Settings.Default.UseIndents)
                    {
                        e.Graphics.DrawImage(img, new PointF((float)Settings.Default.LeftIndent, (float)Settings.Default.TopIndent));
                    }
                    else
                        e.Graphics.DrawImage(img, new Point(0, 0));
                };
                var printer = new PrinterSettings { PrinterName = Settings.Default.FirstPrinter };
                pd.PrinterSettings = printer;
                pd.Print();
                memory.Dispose();
                pd.Dispose();
            }
            catch
            {
                // ignored
            }
        }
    }
}

