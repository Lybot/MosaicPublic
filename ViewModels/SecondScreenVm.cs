using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using MozaikaApp.Models;
using Prism.Commands;
using Prism.Mvvm;

namespace MozaikaApp.ViewModels
{
    public class SecondScreenVm: BindableBase
    {
        private SecondScreenModel _model = new SecondScreenModel();

        public int WidthWindow
        {
            get => _model.WidthWindow;
            set => _model.WidthWindow = value;
        }

        public int HeightWindow
        {
            get => _model.HeightWindow;
            set => _model.HeightWindow = value;
        }

        public WindowStyle Style => _model.Style;

        public WindowState State
        {
            get => _model.State;
            set => _model.State = value;
        }

        public Visibility ButtonVisibility => _model.ButtonVisibility;
        public ObservableCollection<BaseThing> LiveCanvas => _model.LiveCanvas;
        public DelegateCommand StartSecondScreen { get; set; }
        public DelegateCommand Close { get; set; }
        public SecondScreenVm(List<ImageVm> sources)
        {
            _model.Sources = sources;
            _model.PropertyChanged += (s, e) => RaisePropertyChanged(e.PropertyName);
            Close = new DelegateCommand(_model.Close);
            StartSecondScreen = new DelegateCommand(_model.StartPuzzle);
        }
    }
}
