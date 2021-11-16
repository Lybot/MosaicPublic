using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Threading;
using MozaikaApp.Properties;
using MozaikaApp.ViewModels;
using Prism.Mvvm;
using Xceed.Wpf.Toolkit.Core.Input;
using Size = System.Drawing.Size;
using Color = System.Windows.Media.Color;
using ColorDrawing = System.Drawing.Color;
using FontFamily = System.Drawing.FontFamily;
using MessageBox = System.Windows.MessageBox;
using Pen = System.Drawing.Pen;

namespace MozaikaApp.Models
{
    class ElementSettingsModel : BindableBase
    {
        public static Action FullScreen;
        private readonly Dispatcher _dispatcher;
        public bool PngChecked { get; set; }
        public Visibility LoadVisibility { get; set; } = Visibility.Collapsed;
        public Visibility ImageVisibility { get; set; } = Visibility.Collapsed;

        public int PrintDpi
        {
            get => Settings.Default.BackgroundPrintDpi;
            set
            {
                Settings.Default.BackgroundPrintDpi = value;
                Settings.Default.Save();
                RaisePropertyChanged();
                Refresh();
            }
        }
        public string BackgroundFontSize
        {
            get => Settings.Default.BackgroundFontSize.ToString();
            set
            {
                Settings.Default.BackgroundFontSize = int.Parse(value);
                Settings.Default.Save();
                RaisePropertyChanged();
                Refresh();
            }
        }

        public string BackgroundLineThickness
        {
            get => Settings.Default.BackgroundLineThickness.ToString();
            set
            {
                Settings.Default.BackgroundLineThickness = int.Parse(value);
                Settings.Default.Save();
                RaisePropertyChanged();
                Refresh();
            }
        }

        public string BackgroundGap
        {
            get => Settings.Default.BackgroundGap.ToString();
            set
            {
                Settings.Default.BackgroundGap = int.Parse(value);
                Settings.Default.Save();
                RaisePropertyChanged();
                Refresh();
            }
        }

        public Color BackgroundColor
        {
            get => Settings.Default.BackgroundColor;
            set
            {
                Settings.Default.BackgroundColor = value;
                Settings.Default.Save();
                RaisePropertyChanged();
                Refresh();
            }
        }

        public Color BackgroundColorText
        {
            get => Settings.Default.BackgroundColorText;
            set
            {
                Settings.Default.BackgroundColorText = value;
                Settings.Default.Save();
                RaisePropertyChanged();
                Refresh();
            }
        }

        public Color BackgroundColorThickness
        {
            get => Settings.Default.BackgroundColorThickness;
            set
            {
                Settings.Default.BackgroundColorThickness = value;
                Settings.Default.Save();
                Refresh();
                RaisePropertyChanged();
            }
        }

        public Size SizePrint => PhotoProcessing.CalculatePixels(Settings.Default.WidthMm, Settings.Default.HeightMm,
            Settings.Default.BackgroundPrintDpi, Settings.Default.CountWidth, Settings.Default.CountHeight);

        public string ResultResolution { get; set; }
        public KeyModifierCollection KeyModifiers { get; } = new KeyModifierCollection() {KeyModifier.None};
        public ImageSource PreviewImage;
        private Bitmap _saveImage;
        public void Refresh()
        {
            if (SizePrint.Height <= 0 || SizePrint.Width <= 0)
            {
                MessageBox.Show(Functions.FindStringResource("ErrorSettings"), Functions.FindStringResource("Error"), MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            LoadVisibility = Visibility.Visible;
            ImageVisibility = Visibility.Collapsed;
            RaisePropertyChanged($"ImageVisibility");
            RaisePropertyChanged($"ButtonVisibility");
            RaisePropertyChanged($"LoadVisibility");
            var task = new Task(() =>
            {
                var gap = int.Parse(BackgroundGap);
                var thicknessColor = ColorDrawing.FromArgb(BackgroundColorThickness.A, BackgroundColorThickness.R, BackgroundColorThickness.G, BackgroundColorThickness.B);
                var thicknessSize = int.Parse(BackgroundLineThickness);
                var backgroundColor = ColorDrawing.FromArgb(BackgroundColor.A, BackgroundColor.R, BackgroundColor.G, BackgroundColor.B);
                var textColor = ColorDrawing.FromArgb(BackgroundColorText.A, BackgroundColorText.R, BackgroundColorText.G, BackgroundColorText.B);
                int backgroundFontSize = int.Parse(BackgroundFontSize);
                int countHeight = Settings.Default.CountHeight;
                int countWidth = Settings.Default.CountWidth;
                var width = SizePrint.Width + gap * (countWidth-1) + int.Parse(BackgroundLineThickness);
                var height = SizePrint.Height + gap * (countHeight-1) + int.Parse(BackgroundLineThickness);
                ResultResolution = $"{width - gap} x {height - gap}";
                var background = new Bitmap(width-gap, height-gap);
                int widthCell = width / countWidth - gap;
                int heightCell = height / countHeight - gap;
                var gra = Graphics.FromImage(background);
                var sf = new StringFormat
                {
                    LineAlignment = StringAlignment.Center,
                    Alignment = StringAlignment.Center
                };
                gra.FillRectangle(new SolidBrush(backgroundColor), new RectangleF(0,0,width,height));
                for (int i = 1; i <= countWidth; i++)
                {
                    int num3 = (widthCell + gap) * (i - 1);
                    for (int j = 1; j <= countHeight; j++)
                    {
                        int num4 = (heightCell + gap) * (j - 1);
                        gra.DrawRectangle(new Pen(thicknessColor,thicknessSize), num3, num4, widthCell, heightCell);
                        string text = $"R{j}/C{i}";
                        gra.DrawString(text,new Font(FontFamily.GenericSerif, backgroundFontSize), new SolidBrush(textColor), new RectangleF(num3, num4, widthCell,
                            heightCell), sf);
                    }
                }
                gra.Save();
                gra.Dispose();
                _saveImage = background.Clone() as Bitmap;
                _dispatcher.Invoke(delegate
                {
                    PreviewImage = Functions.BitmapToImageSource(background, false);
                    background.Dispose();
                    LoadVisibility = Visibility.Collapsed;
                    ImageVisibility = Visibility.Visible;
                    RaisePropertyChanged($"ImageVisibility");
                    RaisePropertyChanged($"LoadVisibility");
                    RaisePropertyChanged($"PreviewImage");
                    RaisePropertyChanged($"ResultForegroundResolution");
                });
                Thread.Sleep(350);
                _dispatcher.Invoke(FullScreen);
            });
            task.Start();
        }
        public ElementSettingsModel()
        {
            _dispatcher = Dispatcher.CurrentDispatcher;
            MainWindowVm.ClearResources += ClearResources;
            Refresh();
        }

        private void ClearResources(object sender, EventArgs e)
        {
            try
            {
                PreviewImage = null;
                _saveImage.Dispose();
            }
            catch
            {
                //igonred
            }
        }

        public void SaveBillboard()
        {
            var dialog = new SaveFileDialog {FileName = "Billboard"};
            if (PngChecked)
            {
                dialog.DefaultExt = ".png";
                dialog.Filter = @"Image(*.png)|*.png";
            }
            else
            {
                dialog.DefaultExt = ".jpg";
                dialog.Filter = @"Image(*.jpg)|*.jpg";
            }
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                Task.Run(delegate
                {
                    var temp = _saveImage;
                    temp.SetResolution(Settings.Default.BackgroundPrintDpi,Settings.Default.BackgroundPrintDpi);
                    temp.Save(dialog.FileName);
                    temp.Dispose();
                });
            }
        }
    }
}

