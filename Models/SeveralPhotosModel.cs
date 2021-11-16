using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using MozaikaApp.Properties;
using MozaikaApp.ViewModels;
using Prism.Mvvm;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;

namespace MozaikaApp.Models
{
    public class SeveralPhotosModel:BindableBase
    {
        public static Action CenterScreen;
        public double WidthPrinter
        {
            get => Settings.Default.WidthTestPrinter;
            set
            {
                Settings.Default.WidthTestPrinter = value;
                Settings.Default.Save();
                RaisePropertyChanged();
                RaisePropertyChanged($"CanvasSize");
                RaisePropertyChanged($"WidthCanvas");
                RaisePropertyChanged($"HeightCanvas");
                CenterScreen();
            }
        }
        public double HeightPrinter
        {
            get => Settings.Default.HeightTestPrinter;
            set
            {
                Settings.Default.HeightTestPrinter = value;
                Settings.Default.Save();
                RaisePropertyChanged();
                RaisePropertyChanged($"CanvasSize");
                RaisePropertyChanged($"WidthCanvas");
                RaisePropertyChanged($"HeightCanvas");
                CenterScreen();
            }
        }
        public int CountPhotos
        {
            get
            {
                try
                {
                    return PhotosCanvas.Count;
                }
                catch
                {
                    return 0;
                }
            }
            set
            {
                if (value > PhotosCanvas.Count)
                {
                    //if (value > 7)
                    //    return;
                    AddPhoto();
                }
                else
                    DeletePhoto();
                Settings.Default.CountPrinterPhotos = value;
                Settings.Default.Save();
                RaisePropertyChanged();
            }
        }
        public List<PointF> SeveralPhotosIntends
        {
            get => Settings.Default.SeveralPhotosIntends;
            set
            {
                Settings.Default.SeveralPhotosIntends = value;
                Settings.Default.Save();
            }
        }
        private Size PhotoSize => PhotoProcessing.CalculatePixels(Settings.Default.WidthMm, Settings.Default.HeightMm,
            Settings.Default.ViewDpi);

        private Dispatcher _dispatcher;
        public Size CanvasSize => PhotoProcessing.CalculatePixels(Settings.Default.WidthTestPrinter,
            Settings.Default.HeightTestPrinter, Settings.Default.ViewDpi);
        public ObservableCollection<BaseThing> PhotosCanvas { get; set; } = new ObservableCollection<BaseThing>();

        /// <summary>
        /// Save changes to settings
        /// </summary>
        public void Save()
        {
            SeveralPhotosIntends = new List<PointF>();
            foreach (var photo in PhotosCanvas)
            {
                if (photo is PrinterImageVm imageVm)
                {
                    SeveralPhotosIntends.Add(new PointF(float.Parse(imageVm.LeftIntend), float.Parse(imageVm.TopIntend)));
                }
            }
        }
        /// <summary>
        /// Clear canvas (NOT SAVE TO SETTINGS)
        /// </summary>
        public void Clear()
        {
            SeveralPhotosIntends = new List<PointF>();
            PhotosCanvas.Clear();
            CountPhotos = 0;
        }
        /// <summary>
        /// Add new default photo
        /// </summary>
        public void AddPhoto()
        {
            var image = new Bitmap(PhotoSize.Width, PhotoSize.Height);
            var graphics = Graphics.FromImage(image);
            graphics.FillRectangle(Brushes.DimGray, new Rectangle(new Point(0, 0), PhotoSize));
            graphics.DrawString((CountPhotos+1).ToString(), new Font(FontFamily.GenericSerif, 14), Brushes.DarkRed, PhotoSize.Width / 2f, PhotoSize.Height / 2f);
            graphics.Save();
            graphics.Dispose();
            var imageSource = Functions.BitmapToImageSource(image, true);
            var leftIntend = Math.Round(PhotoProcessing.GetMmFromPixels(10, Settings.Default.ViewDpi),4).ToString(Settings.Default.DefaultLanguage);
            var imageVm = new PrinterImageVm() {Number = (CountPhotos + 1).ToString(), Height = PhotoSize.Height, Width = PhotoSize.Height, Left = 10, Top = 10, Source = imageSource, LeftIntend = leftIntend, TopIntend = leftIntend};
            PhotosCanvas.Add(imageVm);
            RaisePropertyChanged($"PhotoCanvas");
        }
        /// <summary>
        /// Add photo in special point and number
        /// </summary>
        /// <param name="point">Top-left point in pixels</param>
        /// <param name="number">Number of photo</param>
        public void AddPhoto(PointF point, int number)
        {
            var image = new Bitmap(PhotoSize.Width, PhotoSize.Height);
            var graphics = Graphics.FromImage(image);
            graphics.FillRectangle(Brushes.DimGray, new Rectangle(new Point(0, 0), PhotoSize));
            graphics.DrawString(number.ToString(), new Font(FontFamily.GenericSerif, 14), Brushes.DarkRed, PhotoSize.Width / 2f, PhotoSize.Height / 2f);
            graphics.Save();
            graphics.Dispose();
            var imageSource = Functions.BitmapToImageSource(image, true);
            var leftIntend = Math.Round(PhotoProcessing.GetMmFromPixels(point.X, Settings.Default.ViewDpi), 5).ToString(Settings.Default.DefaultLanguage);
            var topIntend = Math.Round(PhotoProcessing.GetMmFromPixels(point.Y, Settings.Default.ViewDpi), 5).ToString(Settings.Default.DefaultLanguage);
            var imageVm = new PrinterImageVm() {Number = number.ToString(), Height = PhotoSize.Height, Width = PhotoSize.Height, Left = point.X, Top = point.Y, Source = imageSource, LeftIntend = leftIntend, TopIntend = topIntend };
            PhotosCanvas.Add(imageVm);
            RaisePropertyChanged($"PhotoCanvas");
        }
        /// <summary>
        /// Delete last photo
        /// </summary>
        public void DeletePhoto()
        {
            try
            {
                PhotosCanvas.RemoveAt(CountPhotos-1);
                RaisePropertyChanged($"PhotoCanvas");
            }
            catch
            {
                //ignored
            }
        }
        /// <summary>
        /// Load list from settings and place photos
        /// </summary>
        public void LoadSettings()
        {
            if (SeveralPhotosIntends==null||SeveralPhotosIntends.Count==0)
                return;
            int i = 1;
            foreach (var point in SeveralPhotosIntends)
            {
                //var size = PhotoProcessing.CalculatePixels(point.X, point.Y, Settings.Default.ViewDpi);
                var pixelPoint = PhotoProcessing.CalculatePixelsPoint(point.X, point.Y, Settings.Default.ViewDpi);
                AddPhoto(pixelPoint,i);
                i++;
            }
        }
        public SeveralPhotosModel()
        {
            LoadSettings();
            _dispatcher = Dispatcher.CurrentDispatcher;
            new Task(() =>
            {
                Thread.Sleep(300);
                _dispatcher.Invoke(CenterScreen);
            }).Start();

        }
    }

    //public class SeveralPhotosBehaviour:Behavior<UIElement>
    //{
    //    protected override void OnAttached()
    //    {
    //        base.OnAttached();
    //        PrinterImageVm.IntendChanged += IntendChanged;
    //    }

    //    private void IntendChanged(object sender, Tuple<float, float> e)
    //    {
    //        var size = PhotoProcessing.CalculatePixels(e.Item2, e.Item1, Settings.Default.ViewDpi);
    //        Canvas.SetTop(AssociatedObject,size.Height);
    //        Canvas.SetLeft(AssociatedObject, size.Width);
    //        //AssociatedObject.SetValue(Canvas.LeftProperty, (double)size.Width);
    //    }

    //    protected override void OnDetaching()
    //    {
    //        base.OnDetaching();
    //        PrinterImageVm.IntendChanged -= IntendChanged;
    //    }
    //}
}
