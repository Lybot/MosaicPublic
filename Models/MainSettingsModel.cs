using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Threading;
using Emgu.CV;
using Emgu.CV.Structure;
using MozaikaApp.Properties;
using MozaikaApp.ViewModels;
using Prism.Mvvm;
using Xceed.Wpf.Toolkit.Core.Input;
using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brushes;
using Color = System.Drawing.Color;
using FontFamily = System.Drawing.FontFamily;
using MessageBox = System.Windows.Forms.MessageBox;
using Pen = System.Drawing.Pen;
using Size = System.Drawing.Size;

namespace MozaikaApp.Models
{
    class MainSettingsModel : BindableBase
    {
        private Size SizeView =>
            PhotoProcessing.CalculatePixels(Settings.Default.WidthMm, Settings.Default.HeightMm, Settings.Default.ViewDpi,
                Settings.Default.CountWidth, Settings.Default.CountHeight);
        private string _selectedPathPreview = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
        private int _donePhotos;
        public int DonePhotos { 
            get => _donePhotos;
            set
            {
                _donePhotos = value;
                RaisePropertyChanged();
                RaisePropertyChanged($"StatusPhotos");
            }
        }
        private int _needPhotos;
        public int NeedPhotos
        {
            get => _needPhotos;
            set
            {
                _needPhotos = value;
                RaisePropertyChanged();
                RaisePropertyChanged($"StatusPhotos");
            }
        }
        public string StatusPhotos =>"Process "+ DonePhotos + "/" + NeedPhotos; 
        public ImageSource Background { get; set; }
        private Dispatcher Dispatcher { get; }
        public static Action FullScreen;
        public List<string> Languages { get; set; } = new List<string>() { "Russian", "English" };
        public KeyModifierCollection KeyModifiers { get; } = new KeyModifierCollection() { KeyModifier.None };
        public bool ExperimentalProcess
        {
            get => Settings.Default.ExperimentalProcess;
            set
            {
                Settings.Default.ExperimentalProcess = value;
                Settings.Default.Save();
                RaisePropertyChanged();
            }
        }
        public bool FaceMode
        {
            get => Settings.Default.FaceMode;
            set
            {
                Settings.Default.FaceMode = value;
                Settings.Default.Save();
                RaisePropertyChanged($"FaceMode");
                RaisePropertyChanged($"CenterMode");
            }
        }
        public bool CenterMode
        {
            get => !FaceMode;
            set
            {
                FaceMode = !value;
                RaisePropertyChanged($"FaceMode");
                RaisePropertyChanged($"CenterMode");
            }
        }
        public string HeightMm
        {
            get => Settings.Default.HeightMm.ToString(Settings.Default.DefaultLanguage);
            set
            {
                Settings.Default.HeightMm = double.Parse(value);
                Settings.Default.Save();
                GenerateBackground();
                RaisePropertyChanged();
                RaisePropertyChanged($"ResultForegroundResolution");
            }
        }
        public string WidthMm
        {
            get => Settings.Default.WidthMm.ToString(Settings.Default.DefaultLanguage);
            set
            {
                Settings.Default.WidthMm = double.Parse(value);
                Settings.Default.Save();
                GenerateBackground();
                RaisePropertyChanged();
                RaisePropertyChanged($"ResultForegroundResolution");
            }
        }
        public string TextSize
        {
            get => Settings.Default.TextSize.ToString(CultureInfo.InvariantCulture);
            set
            {
                Settings.Default.TextSize = int.Parse(value);
                Settings.Default.Save();
                RaisePropertyChanged();
            }
        }
        public string CountHeight
        {
            get => Settings.Default.CountHeight.ToString();
            set
            {
                Settings.Default.CountHeight = int.Parse(value);
                Settings.Default.Save();
                GenerateBackground();
                RaisePropertyChanged();
                RaisePropertyChanged($"ResultForegroundResolution");
            }
        }
        public double Transparency
        {
            get => Settings.Default.Transparency;
            set
            {
                try
                {
                    if (Math.Abs(Settings.Default.Transparency - value) < 0.001)
                        return;
                    double transparency = value;
                    if (transparency < 0 || transparency > 1)
                    {
                        MessageBox.Show(Functions.FindStringResource("WrongValue"));
                        return;
                    }
                    transparency = Math.Round(transparency, 2);
                    Settings.Default.Transparency = transparency;
                    Settings.Default.Save();
                    if (!string.IsNullOrEmpty(Settings.Default.ForegroundPath))
                        if (File.Exists(Settings.Default.ForegroundPath))
                            GenerateBackground();
                    RaisePropertyChanged();
                }
                catch (Exception)
                {
                    MessageBox.Show(Functions.FindStringResource("WrongFormat"));
                }
            }
        }
        public double Alpha
        {
            get => Settings.Default.Alpha;
            set
            {
                try
                {
                    double alpha = value;
                    if (alpha < 0 || alpha > 1)
                    {
                        MessageBox.Show(Functions.FindStringResource("WrongValue"));
                        return;
                    }
                    alpha = Math.Round(alpha, 2);
                    Settings.Default.Alpha = alpha;
                    Settings.Default.Save();
                    RaisePropertyChanged();
                }
                catch (Exception)
                {
                    MessageBox.Show(Functions.FindStringResource("WrongFormat"));
                }
            }
        }
        public double Beta
        {
            get => Settings.Default.Beta;
            set
            {
                try
                {
                    double beta = value;
                    if (beta < 0 || beta > 1)
                    {
                        MessageBox.Show(Functions.FindStringResource("WrongValue"));
                        return;
                    }

                    beta = Math.Round(beta, 2);
                    Settings.Default.Beta = beta;
                    Settings.Default.Save();
                    RaisePropertyChanged();
                }
                catch (Exception)
                {
                    MessageBox.Show(Functions.FindStringResource("WrongFormat"));
                }
            }
        }
        public double Gamma
        {
            get => Settings.Default.Gamma;
            set
            {
                try
                {
                    double gamma = value;
                    if (gamma < -255 || gamma > 255)
                    {
                        MessageBox.Show(Functions.FindStringResource("WrongValue"));
                        return;
                    }
                    gamma = Math.Round(gamma, MidpointRounding.ToEven);
                    Settings.Default.Gamma = gamma;
                    Settings.Default.Save();
                    RaisePropertyChanged();
                }
                catch (Exception)
                {
                    MessageBox.Show(Functions.FindStringResource("WrongFormat"));
                }
            }
        }
        public string CountWidth
        {
            get => Settings.Default.CountWidth.ToString();
            set
            {
                Settings.Default.CountWidth = int.Parse(value);
                Settings.Default.Save();
                GenerateBackground();
                RaisePropertyChanged();
                RaisePropertyChanged($"ResultForegroundResolution");
            }
        }
        public bool ProMode
        {
            get => Settings.Default.ProMode;
            set
            {
                Settings.Default.ProMode = value;
                Settings.Default.Save();
                if (value)
                {
                    LiteColor = Brushes.DarkRed;
                    ProColor = Brushes.DarkGreen;
                    ProFont = FontWeights.Bold;
                    LiteFont = FontWeights.Normal;
                }
                else
                {
                    LiteColor = Brushes.DarkGreen;
                    ProColor = Brushes.DarkRed;
                    ProFont = FontWeights.Normal;
                    LiteFont = FontWeights.Bold;
                }
                RaisePropertyChanged($"LightVisibility");
                RaisePropertyChanged($"ProVisibility");
                RaisePropertyChanged($"LightMode");
                RaisePropertyChanged($"ProMode");
                RaisePropertyChanged($"ProColor");
                RaisePropertyChanged($"LiteColor");
                RaisePropertyChanged($"LiteFont");
                RaisePropertyChanged($"ProFont");
            }
        }
        public bool LightMode
        {
            get => !ProMode;
            set => ProMode = !value;
        }
        public string SelectedLanguage
        {
            get
            {
                var language = App.Language;
                switch (language.Name)
                {
                    case "ru-RU":
                        {
                            return "Russian";
                        }
                    case "en-US":
                        {
                            return "English";
                        }
                }
                return "";
            }
            set
            {
                switch (value)
                {
                    case "Russian":
                        {
                            App.Language = new CultureInfo("ru-RU");
                            return;
                        }

                    case "English":
                        {
                            App.Language = new CultureInfo("en-US");
                            return;
                        }
                }
            }
        }
        public string ForegroundPath => Settings.Default.ForegroundPath;
        public Visibility PreviewButtonVisibility { get; set; } = Visibility.Visible;
        public Visibility LoadVisibility { get; set; } = Visibility.Collapsed;
        public Visibility ImageVisibility { get; set; } = Visibility.Collapsed;
        public Visibility ButtonVisibility { get; set; } = Visibility.Visible;
        public Visibility LightVisibility
        {
            get
            {
                if (LightMode)
                    return Visibility.Visible;
                return Visibility.Collapsed;
            }
        }
        public Visibility ProVisibility
        {
            get
            {
                if (ProMode)
                    return Visibility.Visible;
                return Visibility.Collapsed;
            }
        }
        public Brush LiteColor { get; set; }
        public Brush ProColor { get; set; }
        public FontWeight LiteFont { get; set; }
        public FontWeight ProFont { get; set; }
        private Task ProcessBoard { get; set; }
        private Task WaitingThread { get; set; }
        private CancellationTokenSource CancelProcessBoard { get; set; } = new CancellationTokenSource();
        public void ChooseForegroundPath()
        {
            //if (string.IsNullOrEmpty(Settings.Default.WorkFolder))
            //{
            //    MessageBox.Show(Functions.FindStringResource("PictureError"), Functions.FindStringResource("Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            //    return;
            //}
            FileDialog dialog = new OpenFileDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                Settings.Default.ForegroundPath = dialog.FileName;
                RaisePropertyChanged($"ForegroundPath");
                Settings.Default.Save();
                GenerateBackground();
            }
        }
        public void GenerateBackground()
        {
            ButtonVisibility = Visibility.Collapsed;
            LoadVisibility = Visibility.Visible;
            ImageVisibility = Visibility.Collapsed;
            RaisePropertyChanged($"ImageVisibility");
            RaisePropertyChanged($"ButtonVisibility");
            RaisePropertyChanged($"LoadVisibility");
            if (ProcessBoard != null && !(ProcessBoard.IsCompleted || ProcessBoard.IsCanceled))
            {
                if (WaitingThread!=null&& !WaitingThread.IsCompleted)
                    return;
                CancelProcessBoard.Cancel(true);
                WaitingThread = new Task(() =>
                {
                    while (!ProcessBoard.IsCanceled&& !ProcessBoard.IsCompleted)
                    {
                        Thread.Sleep(100);
                    }
                });
                WaitingThread.GetAwaiter().OnCompleted(delegate
                {
                    CancelProcessBoard = new CancellationTokenSource();
                    GenerateBackground();
                });
                WaitingThread.Start();
            }
            else
            {
                ProcessBoard?.Dispose();
                ProcessBoard = new Task(() =>
                {
                    Dispatcher.Invoke(delegate { Background = null; });
                    var thicknessRectangle = (int) Math.Min(Settings.Default.HeightMm, Settings.Default.WidthMm) / 25;
                    var background = new Bitmap(SizeView.Width + thicknessRectangle,
                        SizeView.Height + thicknessRectangle);
                    var widthCell = SizeView.Width / Settings.Default.CountWidth;
                    int heightCell = SizeView.Height / Settings.Default.CountHeight;
                    int fontSize = (int) Math.Min(Settings.Default.HeightMm, Settings.Default.WidthMm) / 2;
                    if (fontSize == 0)
                        fontSize = 1;
                    var sf = new StringFormat
                    {
                        LineAlignment = StringAlignment.Center,
                        Alignment = StringAlignment.Center
                    };
                    NeedPhotos = Settings.Default.CountHeight * Settings.Default.CountWidth;
                    int donePhotos = 0;
                    using (var gra = Graphics.FromImage(background))
                    {
                        gra.FillRectangle(System.Drawing.Brushes.White,
                            new Rectangle(0, 0, SizeView.Width + thicknessRectangle,
                                SizeView.Height + thicknessRectangle));
                        if (!string.IsNullOrEmpty(ForegroundPath))
                        {
                            if (File.Exists(ForegroundPath))
                            {
                                try
                                {
                                    Image<Bgra,byte> backgroundImage = new Bitmap(ForegroundPath).ToImage<Bgra,byte>();
                                    backgroundImage[3] *= Transparency;
                                    var backgroundImage2 = PhotoProcessing.ResizeImage(backgroundImage.AsBitmap(), SizeView.Width, SizeView.Height);
                                    gra.DrawImage(backgroundImage2, 0, 0, SizeView.Width, SizeView.Height);
                                    backgroundImage.Dispose();
                                    backgroundImage2.Dispose();
                                }
                                catch
                                {
                                    //igonred
                                }
                            }
                        }

                        for (int i = 1; i <= Settings.Default.CountWidth; i++)
                        {
                            int num3 = (widthCell) * (i - 1);
                            for (int j = 1; j <= Settings.Default.CountHeight; j++)
                            {
                                int num4 = (heightCell) * (j - 1);
                                var text = $"R{j}/C{i}";
                                gra.DrawRectangle(new Pen(Color.Black, thicknessRectangle),
                                    new Rectangle(num3, num4, widthCell, heightCell));
                                gra.DrawString(text, new Font(FontFamily.GenericSerif, fontSize),
                                    System.Drawing.Brushes.Red, new RectangleF(num3, num4, widthCell, heightCell), sf);
                                donePhotos++;
                                DonePhotos = donePhotos;
                            }
                        }

                        gra.Save();
                    }

                    Dispatcher.Invoke(delegate
                    {
                        Background = Functions.BitmapToImageSource(background, false);
                        background.Dispose();
                        LoadVisibility = Visibility.Collapsed;
                        ImageVisibility = Visibility.Visible;
                        RaisePropertyChanged($"ImageVisibility");
                        RaisePropertyChanged($"LoadVisibility");
                        RaisePropertyChanged($"Background");
                        DonePhotos = 0;
                    });
                    Thread.Sleep(300);
                    Dispatcher.Invoke(FullScreen);
                    //GC.Collect(GC.MaxGeneration, GCCollectionMode.Default); 
                }, CancelProcessBoard.Token);
                ProcessBoard.Start();
            }
        }
        //public void SaveSettings()
        //{
        //    var dialog = new SaveFileDialog();
        //    dialog.FileName = "settingsMosaic.json";
        //    if (Directory.Exists(Settings.Default.WorkFolder)&&string.IsNullOrEmpty(Settings.Default.WorkFolder))
        //        dialog.InitialDirectory = Settings.Default.WorkFolder;
        //    if (dialog.ShowDialog() == DialogResult.OK)
        //    {
        //        var data = new SettingsServer()
        //        {
        //            CountWidth = Settings.Default.CountWidth,
        //            CountHeight = Settings.Default.CountHeight,
        //            FullWidth = (int)Background.Width,
        //            FullHeight = (int)Background.Height,
        //            Question = QuestionServer
        //        };
        //        var json = JsonConvert.SerializeObject(data);
        //        File.WriteAllText(dialog.FileName, json);
        //    }
        //}
        public void GenerateBackground(Bitmap bigImage, FileInfo[] files, Func<int, bool> donePhoto, Func<int,bool> needPhoto)
        {
            ButtonVisibility = Visibility.Collapsed;
            LoadVisibility = Visibility.Visible;
            ImageVisibility = Visibility.Collapsed;
            RaisePropertyChanged($"ImageVisibility");
            RaisePropertyChanged($"ButtonVisibility");
            RaisePropertyChanged($"LoadVisibility");
            //Background = null;
            int donePhotoInt=0, needPhotoInt = Settings.Default.CountWidth*Settings.Default.CountHeight;
            donePhoto(donePhotoInt);
            needPhoto(needPhotoInt);
            if (ProcessBoard != null && ProcessBoard.Status == TaskStatus.Running)
            {
                if (WaitingThread != null && !WaitingThread.IsCompleted)
                    return;
                CancelProcessBoard.Cancel(true);
                var thread = new Task(() =>
                {
                    while (!ProcessBoard.IsCanceled)
                    {
                        Thread.Sleep(50);
                    }
                });
                thread.GetAwaiter().OnCompleted(GenerateBackground);
                thread.Start();
            }
            ProcessBoard = new Task(() =>
            {
                Dispatcher.Invoke(delegate { Background = null; });
                var background = new Bitmap(SizeView.Width, SizeView.Height);
                var graphics = Graphics.FromImage(background);
                var widthCell = SizeView.Width / Settings.Default.CountWidth;
                int heightCell = SizeView.Height / Settings.Default.CountHeight;
                var random = new Random();
                for (int i = 1; i <= Settings.Default.CountWidth; i++)
                {
                    int num3 = (widthCell) * (i - 1);
                    for (int j = 1; j <= Settings.Default.CountHeight; j++)
                    {
                        int num4 = (heightCell) * (j - 1);
                        var sourceImage = new Bitmap(files[random.Next(files.Length)].FullName).ToImage<Bgra, byte>();
                        var backgroundImage = PhotoProcessing.CutUnderImage(bigImage, Settings.Default.WidthMm,
                            Settings.Default.HeightMm, Settings.Default.ViewDpi,
                            j - 1, i - 1, Settings.Default.CountWidth, Settings.Default.CountHeight);
                        var resultImage = PhotoProcessing.PreviewProcess(Settings.Default.Alpha, Settings.Default.Beta,
                            Settings.Default.Gamma, Settings.Default.Transparency, sourceImage, backgroundImage.ToImage<Bgra,byte>(), ProMode);
                        backgroundImage.Dispose();
                        sourceImage.Dispose();
                        graphics.DrawImage(resultImage.AsBitmap(), num3, num4);
                        resultImage.Dispose();
                        donePhotoInt++;
                        donePhoto(donePhotoInt);
                    }
                }
                bigImage.Dispose();
                graphics.Save();
                graphics.Dispose();
                Dispatcher.Invoke(delegate
                {
                    Background = Functions.BitmapToImageSource(background, false);
                    background.Dispose();
                    LoadVisibility = Visibility.Collapsed;
                    ImageVisibility = Visibility.Visible;
                    PreviewButtonVisibility = Visibility.Visible;
                    RaisePropertyChanged($"PreviewButtonVisibility");
                    RaisePropertyChanged($"ImageVisibility");
                    RaisePropertyChanged($"LoadVisibility");
                    RaisePropertyChanged($"Background");
                });
                Thread.Sleep(300);
                Dispatcher.Invoke(FullScreen);
            }, CancelProcessBoard.Token);
            ProcessBoard.Start();
        }
        public void Preview()
        {
            if (string.IsNullOrEmpty(Settings.Default.ForegroundPath))
            {
                new Thread(() =>
                    {
                        MessageBox.Show(Functions.FindStringResource("PreviewPictureError"),
                            Functions.FindStringResource("Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }).Start();
                PreviewButtonVisibility = Visibility.Visible;
                RaisePropertyChanged($"PreviewButtonVisibility");
                return;
            }
            PreviewButtonVisibility = Visibility.Collapsed;
            RaisePropertyChanged($"PreviewButtonVisibility");
            var dialog = new FolderBrowserDialog { Description = Functions.FindStringResource("PreviewDescription"), SelectedPath = _selectedPathPreview };
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                _selectedPathPreview = dialog.SelectedPath;
                var bigImage = new Bitmap(Settings.Default.ForegroundPath);
                var folder = new DirectoryInfo(dialog.SelectedPath);
                var files = folder.GetFiles().Where(str => str.Name.EndsWith(".png") || str.Name.EndsWith(".jpg")).ToArray();
                if (files.Length == 0)
                {
                    new Thread(() =>
                    {
                        MessageBox.Show(Functions.FindStringResource("PreviewFilesError"),
                        Functions.FindStringResource("Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }).Start();
                    PreviewButtonVisibility = Visibility.Visible;
                    RaisePropertyChanged($"PreviewButtonVisibility");
                    return;
                } 
                GenerateBackground(bigImage, files, (int result)=> { DonePhotos = result; return true; }, (int result) => { NeedPhotos = result; return true; });
            }
        }
        public MainSettingsModel()
        {
            MainWindowVm.ClearResources += ClearResources;
            SettingTabVm.ClearResources += ClearResources;
            Dispatcher = Dispatcher.CurrentDispatcher;
            if (WidthMm != "0" && HeightMm != "0" && CountHeight != "0" & CountWidth != "0")
                GenerateBackground();
            ProMode = Settings.Default.ProMode;
        }
        private void ClearResources(object sender, EventArgs e)
        {
            Background = null;
        }
    }
}
