using System;
using System.Collections.ObjectModel;
using MozaikaApp.Models;
using Prism.Commands;
using Prism.Mvvm;
using System.Windows;
using System.Windows.Media;
using Xceed.Wpf.Toolkit.Core.Input;

namespace MozaikaApp.ViewModels
{
    public class ProgressLiveVm : BindableBase
    {
        private readonly ProgressLiveModel _model = new ProgressLiveModel();
        public Visibility VisiblePuzzle => _model.VisiblePuzzle;
        public Visibility VisibleMenu => _model.VisibleMenu;
        public Visibility VisibleLoading => _model.VisibleLoading;
        public Visibility VisibleMenuButtons => _model.VisibleMenuButtons;
        public Visibility VisibleSeveralPrinting => _model.UseSeveralPhotos?Visibility.Visible: Visibility.Collapsed;
        public int SelectedX
        {
            get => ProgressLiveModel.SelectedX;
            set => ProgressLiveModel.SelectedX = value;
        }
        public int SelectedY
        {
            get => ProgressLiveModel.SelectedY;
            set => ProgressLiveModel.SelectedY = value;
        }
        public int CountCells => _model.CountCells;
        public int Added => _model.Added;
        public string PauseContent => _model.PauseContent;
        public int FreeCells => _model.FreeCells;

        public int FillCount
        {
            get => _model.CountFill;
            set => _model.CountFill = value;
        }

        public bool AutoPrint
        {
            get
            {
                try
                {
                    return _model.PrintWatcher.EnableRaisingEvents;
                }
                catch (Exception)
                {
                    return false;
                }
            }
            set => _model.PrintWatcher.EnableRaisingEvents = value;
        }
        public string PrintStatus => _model.PrintPaths.Count + "/" + _model.PrintCount;
        public string ReadyStatus => _model.ReadyStatus;
        public int HeightCanvas => _model.HeightCanvas;
        public int WidthCanvas => _model.WidthCanvas;
        public DelegateCommand FillCopies { get; set; }
        public DelegateCommand AddScreen { get; set; }
        public DelegateCommand Print { get; set; }
        public DelegateCommand NewPuzzle { get; set; }
        public DelegateCommand ResetLiveCanvas { get; set; }
        public DelegateCommand PrintNow { get; set; }
        public DelegateCommand DeleteCell { get; set; }
        public DelegateCommand LoadPuzzle { get; set; }
        public DelegateCommand Pause { get; set; }
        public DelegateCommand Stop { get; set; }
        public DelegateCommand Save { get; set; }
        public DelegateCommand LinkPhoto { get; set; }
        public DelegateCommand Unloaded { get; set; }
        public ObservableCollection<BaseThing> LiveCanvas => _model.LiveCanvas;
        public KeyModifierCollection KeyModifiers => _model.KeyModifiers;
        public ProgressLiveVm()
        {
            _model.PropertyChanged += (s, e) => RaisePropertyChanged(e.PropertyName);
            RaisePropertyChanged($"HeightCanvas");
            RaisePropertyChanged($"WidthCanvas");
            RaisePropertyChanged($"LiveCanvas");
            Print = new DelegateCommand(delegate
            {
                if (SelectedX == 0 || SelectedY == 0) return;
                try
                {
                    _model.PrintImage();
                }
                catch (Exception)
                {
                    //ignored
                }
            });
            ResetLiveCanvas = new DelegateCommand(_model.ResetLiveCanvas);
            NewPuzzle = new DelegateCommand(_model.NewPuzzle);
            FillCopies = new DelegateCommand(_model.FillCopied);
            DeleteCell = new DelegateCommand(_model.DeleteCell);
            Pause = new DelegateCommand(_model.Pause);
            Stop = new DelegateCommand(_model.Stop);
            Save = new DelegateCommand(_model.Save);
            LinkPhoto = new DelegateCommand(_model.LinkPhoto);
            LoadPuzzle = new DelegateCommand(_model.LoadPuzzle);
            AddScreen = new DelegateCommand(_model.AddScreen);
            Unloaded = new DelegateCommand(_model.Unloaded);
            PrintNow = new DelegateCommand(_model.PrintNow);
        }
    }
    public class BaseThing:BindableBase
    {
        private double _left;
        public double Left
        {
            get => _left;
            set
            {
                _left = value;
                RaisePropertyChanged();
            }
        }
        private double _top;
        public double Top
        {
            get => _top;
            set
            {
                _top = value;
                RaisePropertyChanged();
            } }

        private double _width;
        public double Width
        {
            get => _width;
            set
            {
                _width = value;
                RaisePropertyChanged();
            }
        }
        private double _height;
        public double Height
        {
            get => _height;
            set
            {
                _height = value;
                RaisePropertyChanged();
            }
        }
        public Tuple<int, int> Tag { get; set; }
        public string Content { get; set; }
        private int _ZIndex;
        public int ZIndex
        {
            get => _ZIndex;
            set
            {
                _ZIndex = value;
                RaisePropertyChanged();
            }
        }
    }

    public class LabelVm : BaseThing
    {
        public int FontSize { get; set; }
    }
    public class ImageVm : BaseThing
    {
        private ImageSource _source;
        public ImageSource Source
        {
            get => _source;
            set
            {
                _source = value;
                RaisePropertyChanged();
            }
        }
    }
    public class RectangleVm : BaseThing
    {
        public int StrokeThickness { get; set; }
        private Brush _backgroundColor;
        public Brush BackgroundColor
        {
            get=> _backgroundColor;
            set
            {
                _backgroundColor = value;
                RaisePropertyChanged();
            }
        }
    }
}

