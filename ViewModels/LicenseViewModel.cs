using System.Windows;
using MozaikaApp.Models;
using Prism.Commands;
using Prism.Mvvm;
namespace MozaikaApp.ViewModels
{
    public class LicenseViewModel:BindableBase
    {
        private LicenseModel _model = new LicenseModel();
        public DelegateCommand Deactivate { get; set; }
        public string ButtonText => _model.ButtonText;
        public Visibility ActivateVisibility => _model.ActivateVisibility;
        public Visibility DeactivateVisibility => _model.DeactivateVisibility;
        public LicenseViewModel()
        {
            _model.PropertyChanged += (s, e) => RaisePropertyChanged(e.PropertyName);
            Deactivate = new DelegateCommand(_model.Deactivate);
        }
    }
}
