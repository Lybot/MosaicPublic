using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interactivity;
using System.Windows.Threading;
using Emgu.CV;
using Emgu.CV.Structure;
using MozaikaApp.Properties;
using MozaikaApp.ViewModels;
using Prism.Mvvm;
using Xceed.Wpf.Toolkit.Core.Input;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using Size = System.Drawing.Size;

namespace MozaikaApp.Models
{
    public class FillAlgorithmModel : BindableBase
    {
        public static Action FullScreen;
        public Size SizePrint => PhotoProcessing.CalculatePixels(Settings.Default.WidthMm, Settings.Default.HeightMm, Settings.Default.ViewDpi,
            Settings.Default.CountWidth, Settings.Default.CountHeight);
        public int WidthCanvas => SizePrint.Width;
        public int HeightCanvas => SizePrint.Height;
        public static ObservableCollection<BaseThing> AlgorithmCanvas { get; set; }
        public KeyModifierCollection KeyModifiers { get; } = new KeyModifierCollection() { KeyModifier.None };
        public static List<int> TempList = new List<int>();
        private Dispatcher _disp;
        private List<int> SavedFillAlgorithm
        {
            get => Settings.Default.FillAlgorithm;
            set
            {
                Settings.Default.FillAlgorithm = value;
                Settings.Default.Save();
            }
        }
        public void Save()
        {
            SavedFillAlgorithm = new List<int>();
            foreach (var temp in TempList)
            {
                SavedFillAlgorithm.Add(temp);   
            }
            if (TempList.Count == 0)
            {
                SavedFillAlgorithm = new List<int>();
            }
        }
        public void Clear()
        {
            TempList.Clear();
            AlgorithmCanvas = Functions.Generate(Settings.Default.ViewDpi, Settings.Default.WidthMm,
                Settings.Default.HeightMm, Settings.Default.CountWidth, Settings.Default.CountHeight,
                14);
            RaisePropertyChanged($"AlgorithmCanvas");
        }

        public void Load()
        {
            var dialog = new OpenFileDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                new Task(() =>
                {
                    var image = new Bitmap(dialog.FileName);

                    for (int i = 0; i < Settings.Default.CountHeight; i++)
                    {
                        for (int j = 0; j < Settings.Default.CountWidth; j++)
                        {
                            var tempImage = PhotoProcessing.CutUnderImage(image, Settings.Default.WidthMm,
                                Settings.Default.HeightMm, Settings.Default.ViewDpi, i, j, Settings.Default.CountWidth,
                                Settings.Default.CountHeight).ToImage<Bgra,byte>();
                            var avg = tempImage[3].GetAverage();
                            if (avg.Intensity > 0.5d)
                            {
                                var count = i * Settings.Default.CountWidth + j;
                                _disp.Invoke(delegate
                                {
                                    var position = AlgorithmCanvas[count];
                                    var rectangle = new RectangleVm() { Tag = position.Tag, Height = position.Height, Left = position.Left, Top = position.Top, Width = position.Width, StrokeThickness = 3 };
                                    AlgorithmCanvas.Add(rectangle);
                                });
                                TempList.Add(count);
                            }
                        }
                    }
                    _disp.Invoke(FullScreen);
                }).Start();
            }
        }
        public FillAlgorithmModel()
        {
            AlgorithmCanvas = Functions.Generate(Settings.Default.ViewDpi, Settings.Default.WidthMm,
                Settings.Default.HeightMm, Settings.Default.CountWidth, Settings.Default.CountHeight,
                14);
            AlgorithmCanvas.CollectionChanged += AlgorithmCanvas_CollectionChanged;
            _disp = Dispatcher.CurrentDispatcher;
            try
            {
                if (SavedFillAlgorithm.Count > 0)
                {
                    if (!SavedFillAlgorithm.TrueForAll(value =>
                        value <= Settings.Default.CountHeight * Settings.Default.CountWidth)) 
                        throw new Exception();
                    TempList = new List<int>();
                    foreach (var count in SavedFillAlgorithm)
                    {
                        var cell = AlgorithmCanvas[count];
                        var rectangle = new RectangleVm() { Height = cell.Height, Left = cell.Left, StrokeThickness = 3, Tag = cell.Tag, Top = cell.Top, Width = cell.Width };
                        AlgorithmCanvas.Add(rectangle);
                        TempList.Add(count);
                    }
                }
            }
            catch
            {
                TempList = new List<int>();
                SavedFillAlgorithm = new List<int>();
            }
            new Task(() =>
            {
                Thread.Sleep(300);
                _disp.Invoke(FullScreen);
            }).Start();
        }
        private void AlgorithmCanvas_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            _disp.Invoke(delegate
            {
                RaisePropertyChanged($"AlgorithmCanvas");
            });
        }
    }
    public class FillAlgorithmBehaviour : Behavior<UIElement>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.PreviewMouseLeftButtonDown+= ClickEvent;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.PreviewMouseLeftButtonDown -= ClickEvent;
        }

        private void ClickEvent(object sender, MouseEventArgs e)
        {
            //if (e.LeftButton == MouseButtonState.Released)
            //    return;
            if (sender is System.Windows.Controls.Button element)
            {
                var elementsInCollection =
                    FillAlgorithmModel.AlgorithmCanvas.Where(cell => Equals(cell.Tag, element.Tag));
                var inCollection = elementsInCollection.ToList();
                if (inCollection.Count > 1)
                {
                    foreach (var cell in inCollection)
                    {
                        if (cell is RectangleVm rectangle)
                        {
                            FillAlgorithmModel.AlgorithmCanvas.Remove(rectangle);
                            FillAlgorithmModel.TempList.Remove(
                                Settings.Default.CountWidth * (rectangle.Tag.Item1-1) + rectangle.Tag.Item2-1);
                        }
                    }
                }
                else
                {
                    if (inCollection[0] is LabelVm cell)
                    {
                        var rectangle = new RectangleVm()
                        {
                            Height = cell.Height,
                            Width = cell.Width,
                            Left = cell.Left,
                            Tag = cell.Tag,
                            StrokeThickness = 3,
                            Top = cell.Top
                        };
                        FillAlgorithmModel.AlgorithmCanvas.Add(rectangle);
                        var count = Settings.Default.CountWidth * (rectangle.Tag.Item1 - 1) + rectangle.Tag.Item2 - 1;
                        if (!FillAlgorithmModel.TempList.Contains(count))
                            FillAlgorithmModel.TempList.Add(count);
                    }
                }
            }
            //if (sender is System.Windows.Controls.Label label)
            //{
            //    if (FillAlgorithmModel.AlgorithmCanvas.FirstOrDefault(temp => Equals(temp.Tag, label.Tag)) is LabelVm cell)
            //    {
            //        var rectangle = new RectangleVm()
            //        {
            //            Height = cell.Height,
            //            Width = cell.Width,
            //            Left = cell.Left,
            //            Tag = cell.Tag,
            //            BackgroundColor = Colors.Black,
            //            StrokeThickness = 3,
            //            Top = cell.Top
            //        };
            //        FillAlgorithmModel.AlgorithmCanvas.Add(rectangle);
            //    }
            //}
        }
    }

}
