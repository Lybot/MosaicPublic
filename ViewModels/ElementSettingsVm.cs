using System;
using System.Windows;
using System.Windows.Media;
using MozaikaApp.Models;
using Prism.Commands;
using Prism.Mvvm;
using Xceed.Wpf.Toolkit.Core.Input;
using Color = System.Windows.Media.Color;

namespace MozaikaApp.ViewModels
{
    class ElementSettingsVm:BindableBase
    {
        private readonly ElementSettingsModel _model = new ElementSettingsModel();
        public bool PngChecked
        {
            get => _model.PngChecked;
            set => _model.PngChecked = value;
        }

        public bool PsdChecked
        {
            get => !_model.PngChecked;
            set => _model.PngChecked = !value;
        }
        public string BackgroundFontSize
        {
            get => _model.BackgroundFontSize;
            set
            {
                if (!int.TryParse(value, out int temp)) return;
                if (temp > 0)
                    _model.BackgroundFontSize = value;
            }
        }
        public string PrintDpi
        {
            get => _model.PrintDpi.ToString();
            set
            {
                if (!int.TryParse(value, out int temp)) return;
                if (temp > 0)
                    _model.PrintDpi = temp;
            }
        }
        public string BackgroundLineThickness
        {
            get => _model.BackgroundLineThickness;
            set
            {
                if (!int.TryParse(value, out int temp)) return;
                if (temp > 0)
                    _model.BackgroundLineThickness = value;
            }
        }
        public string BackgroundGap
        {
            get => _model.BackgroundGap;
            set
            {
                if (!int.TryParse(value, out int temp)) return;
                if (temp >= 0)
                    _model.BackgroundGap = value;
            }
        }
        public Color BackgroundColor
        {
            get => _model.BackgroundColor;
            set => _model.BackgroundColor = value;
        }
        public Color BackgroundColorText
        {
            get => _model.BackgroundColorText;
            set => _model.BackgroundColorText = value;
        }
        public Color BackgroundColorThickness
        {
            get => _model.BackgroundColorThickness;
            set => _model.BackgroundColorThickness = value;
        }
        public KeyModifierCollection KeyModifiers => _model.KeyModifiers;
        public ImageSource PreviewImage => _model.PreviewImage;
        public DelegateCommand Refresh { get; set; }
        public DelegateCommand SaveBillboard { get; set; }
        public string ResultForegroundResolution => _model.ResultResolution;
        public Visibility ImageVisibility => _model.ImageVisibility;
        public Visibility LoadVisibility => _model.LoadVisibility;
        public ElementSettingsVm()
        {
            _model.PropertyChanged += (s, e) => RaisePropertyChanged(e.PropertyName);
            Refresh= new DelegateCommand(_model.Refresh);
            SaveBillboard = new DelegateCommand(_model.SaveBillboard);
        }
    }
}
