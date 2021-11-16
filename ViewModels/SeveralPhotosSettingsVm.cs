using System.Collections.ObjectModel;
using MozaikaApp.Models;
using MozaikaApp.Properties;
using Prism.Commands;
using Prism.Mvvm;
using Xceed.Wpf.Toolkit.Core.Input;

namespace MozaikaApp.ViewModels
{
    public class SeveralPhotosSettingsVm: BindableBase
    {
        private SeveralPhotosModel _model = new SeveralPhotosModel();
        public ObservableCollection<BaseThing> PhotosCanvas => _model.PhotosCanvas;
        public string WidthPrinter
        {
            get => _model.WidthPrinter.ToString(Settings.Default.DefaultLanguage);
            set
            {
                double.TryParse(value, out var result);
                if(result>0)
                    _model.WidthPrinter = result;
            }
        }
        public string HeightPrinter
        {
            get => _model.HeightPrinter.ToString(Settings.Default.DefaultLanguage);
            set
            {
                double.TryParse(value, out var result);
                if (result > 0)
                    _model.HeightPrinter = result;
            }
        }
        public string CountPhotos
        {
            get => _model.CountPhotos.ToString(Settings.Default.DefaultLanguage);
            set
            {
                int.TryParse(value, out var result);
                if (result > 0)
                    _model.CountPhotos = result;
            }
        }
        public KeyModifierCollection KeyModifiers { get; } = new KeyModifierCollection();
        public int HeightCanvas => _model.CanvasSize.Height;
        public int WidthCanvas => _model.CanvasSize.Width;
        public DelegateCommand Save { get; set; }
        public DelegateCommand Clear { get; set; }
        public SeveralPhotosSettingsVm()
        {
            Save = new DelegateCommand(_model.Save);
            Clear = new DelegateCommand(_model.Clear);
            _model.PropertyChanged += (s, e) => RaisePropertyChanged(e.PropertyName);
        }   
    }
    public class PrinterImageVm : ImageVm
    {
        private float _topIntend;

        /// <summary>
        /// Number of photo in list
        /// </summary>
        private int _number;

        public string Number
        {
            get => Functions.FindStringResource("Photo") + $" {_number}";
            set
            {
                int.TryParse(value, out var result);
                if (result >= 0)
                    _number = result;
            }
        }

        /// <summary>
        /// Top Intend in mm. need to show it in ListBox
        /// </summary>
        public string TopIntend
        {
            get => _topIntend.ToString(Settings.Default.DefaultLanguage);
            set
            {
                float.TryParse(value, out var result);
                if (result >= 0)
                {
                    _topIntend = result;
                    Top = PhotoProcessing.CalculatePixels(_leftIntend, _topIntend, Settings.Default.ViewDpi)
                        .Height;
                }
            }
        }
        private float _leftIntend;
        /// <summary>
        /// Left Intend in mm. need to show it in ListBox
        /// </summary>
        public string LeftIntend
        {
            get => _leftIntend.ToString(Settings.Default.DefaultLanguage);
            set
            {
                float.TryParse(value, out var result);
                if (result >= 0)
                {
                    _leftIntend = result;
                    Left = PhotoProcessing.CalculatePixels(_leftIntend, _topIntend, Settings.Default.ViewDpi)
                        .Width;
                }
            }
        }
    }
}
