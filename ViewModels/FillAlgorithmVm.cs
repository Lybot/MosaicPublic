using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MozaikaApp.Models;
using Prism.Commands;
using Prism.Mvvm;
using Xceed.Wpf.Toolkit.Core.Input;

namespace MozaikaApp.ViewModels
{
    public class FillAlgorithmVm:BindableBase
    {
        private readonly FillAlgorithmModel _model = new FillAlgorithmModel();
        public int WidthCanvas => _model.WidthCanvas;
        public int HeightCanvas => _model.HeightCanvas;
        public ObservableCollection<BaseThing> AlgorithmCanvas => FillAlgorithmModel.AlgorithmCanvas;
        public KeyModifierCollection KeyModifiers => _model.KeyModifiers;
        public DelegateCommand Save { get; set; }
        public DelegateCommand Load { get; set; }
        public DelegateCommand Clear { get; set; }
        public FillAlgorithmVm()
        {
            _model.PropertyChanged += (s, e) => RaisePropertyChanged(e.PropertyName);
            Save = new DelegateCommand(_model.Save);
            Clear = new DelegateCommand(_model.Clear);
            Load = new DelegateCommand(_model.Load);
        }
    }
}
