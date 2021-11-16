using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Emgu.CV;
using Emgu.CV.Structure;
using MozaikaApp.Models;
using Prism.Commands;
using Prism.Mvvm;
using Xceed.Wpf.Toolkit.Core.Input;

//using Color = System.Windows.Media;

namespace MozaikaApp.ViewModels
{
    class PrintSettingsVm : BindableBase
    {
        private readonly PrintSettingsModel _model = new PrintSettingsModel();
        public string PrintDpi
        {
            get => _model.PrintDpi;
            set
            {
                if (!int.TryParse(value, out int temp)) return;
                if (temp > 0) 
                    _model.PrintDpi = value;
            }
        }
        public string ThicknessSize
        {
            get => _model.ThicknessSize;
            set
            {
                if (int.TryParse(value, out _))
                    _model.ThicknessSize = value;
            }
        }
        public string TextSize
        {
            get => _model.TextSize;
            set
            {
                if (double.TryParse(value, out _))
                    _model.TextSize = value;
            }
        }
        public Color FirstStringColor
        {
            get => _model.FirstStringColor;
            set => _model.FirstStringColor = value;
        }
        public Color SecondStringColor
        {
            get => _model.SecondStringColor;
            set => _model.SecondStringColor = value;
        }
        public Color BackgroundColor
        {
            get => _model.BackgroundColor;
            set => _model.BackgroundColor = value;
        }
        public string FirstPrinter
        {
            get => _model.FirstPrinter;
            set => _model.FirstPrinter = value;
        }
        public bool IsEditable => Activation.IsEditable;
        public string SecondPrinter
        {
            get => _model.SecondPrinter;
            set => _model.SecondPrinter = value;
        }

        public bool SecondPrinterAvailable
        {
            get => _model.SecondPrinterAvailable;
            set => _model.SecondPrinterAvailable = value;
        }
        public bool UseIndents
        {
            get => _model.UseIndents;
            set
            {
                _model.UseIndents = value;
                if (value&& UseSeveralPhotosSettings)
                    UseSeveralPhotosSettings = false;
                RaisePropertyChanged();
                RaisePropertyChanged($"IndentsVisibility");
                RaisePropertyChanged($"SeveralPhotosVisibility");
            }
        }
        public bool UseSeveralPhotosSettings
        {
            get => _model.UseSeveralPhotos;
            set
            {
                _model.UseSeveralPhotos = value;
                if (value && UseIndents)
                    UseIndents = false;
                RaisePropertyChanged();
                RaisePropertyChanged($"IndentsVisibility");
                RaisePropertyChanged($"SeveralPhotosVisibility");
            }
        }

        public string TopIndent
        {
            get => _model.TopIndent;
            set
            {
                if (!double.TryParse(value, out double temp)) return;
                if (temp >= 0)
                {
                    _model.TopIndent = value;
                    RaisePropertyChanged();
                }
            }
        }

        public string LeftIndent
        {
            get => _model.LeftIndent;
            set
            {
                if (!double.TryParse(value, out double temp)) return;
                if (temp >= 0)
                {
                    _model.LeftIndent = value;
                    RaisePropertyChanged();
                }
            }
        }

        public Visibility IndentsVisibility => UseIndents ? Visibility.Visible : Visibility.Collapsed;
        public Visibility SeveralPhotosVisibility => UseSeveralPhotosSettings ? Visibility.Visible : Visibility.Collapsed;
        public Visibility SecondPrinterVisibility => _model.SecondPrinterVisibility;
        public PrinterSettings.StringCollection Printers => _model.Printers;
        public ImageSource ResultImage => _model.ResultImage;
        public KeyModifierCollection KeyModifiers => _model.KeyModifiers;
        public DelegateCommand Refresh { get; set; }
        public DelegateCommand SeveralPhotosSettings { get; set; }
        public PrintSettingsVm()
        {
            _model.PropertyChanged += (s, e) => RaisePropertyChanged(e.PropertyName);
            try
            {
                _model.UpdateResult();
            }
            catch
            {
                //ignored
            }
            Refresh = new DelegateCommand(_model.PrintTestPhoto);
            SeveralPhotosSettings = new DelegateCommand(_model.SeveralPhotosSettings);
        }
    }
}
