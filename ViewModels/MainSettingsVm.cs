using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using MozaikaApp.Models;
using Prism.Commands;
using Prism.Mvvm;
using Xceed.Wpf.Toolkit.Core.Input;

namespace MozaikaApp.ViewModels
{
    class MainSettingsVm:BindableBase
    {
        private readonly MainSettingsModel _model = new MainSettingsModel();
        public DelegateCommand CheckBackground { get; set; }
        public DelegateCommand Preview { get; set; }
        public DelegateCommand SaveSettings { get; set; }
        public DelegateCommand ChooseForegroundPath { get; set; }
        public Visibility ButtonVisibility => _model.ButtonVisibility;
        public Visibility ImageVisibility => _model.ImageVisibility;
        public Visibility LoadVisibility => _model.LoadVisibility;
        public ImageSource Background => _model.Background;
        public bool IsEditable => Activation.IsEditable;
        public string HeightMm
        {
            get => _model.HeightMm;
            set
            {
                if (!double.TryParse(value, out double temp)) return;
                if(temp>0)
                    _model.HeightMm = value;
            }
        }

        public string WidthMm
        {
            get => _model.WidthMm;
            set
            {
                if (!double.TryParse(value, out double temp)) return;
                if (temp > 0)
                    _model.WidthMm = value;
            }
        }

        public string TextSize
        {
            get => _model.TextSize;
            set
            {
                if (!int.TryParse(value, out int temp)) return;
                if (temp > 0)
                    _model.TextSize = value;
            }
        }

        public string CountHeight
        {
            get => _model.CountHeight;
            set
            {
                if (!int.TryParse(value, out int temp)) return;
                if (temp > 0)
                    _model.CountHeight = value;
            }
        }

        public string CountWidth
        {
            get => _model.CountWidth;
            set
            {
                if (!int.TryParse(value, out int temp)) return;
                if (temp > 0)
                    _model.CountWidth = value;
            }
        }
        public string SelectedLanguage
        {
            get => _model.SelectedLanguage;
            set => _model.SelectedLanguage = value;
        }
        public string ForegroundPath => _model.ForegroundPath;
        public string StatusPhotos => _model.StatusPhotos;
        public double Alpha
        {
            get => _model.Alpha;
            set => _model.Alpha = value;
        }
        public double Beta
        {
            get => _model.Beta;
            set => _model.Beta = value;
        }
        public double Gamma
        {
            get => _model.Gamma;
            set => _model.Gamma = value;
        }

        public double Transparency
        {
            get => _model.Transparency;
            set => _model.Transparency = value;
        }

        public bool ProMode
        {
            get => _model.ProMode;
            set => _model.ProMode = value;
        }
        public bool LightMode
        {
            get => _model.LightMode;
            set => _model.LightMode = value;
        }
        public bool CenterMode
        {
            get => _model.CenterMode;
            set => _model.CenterMode = value;
        }
        public bool FaceMode
        {
            get => _model.FaceMode;
            set => _model.FaceMode = value;
        }

        public bool ExperimentalProcess
        {
            get => _model.ExperimentalProcess;
            set
            {
                _model.ExperimentalProcess = value;
                RaisePropertyChanged($"DefaultChecked");
                RaisePropertyChanged($"ExperimentalChecked");
            }
        }

        public bool DefaultChecked
        {
            get=> !ExperimentalProcess;
            set
            {
                if (value)
                    ExperimentalProcess = false;
            }
        }

        public bool ExperimentalChecked
        {
            get=> ExperimentalProcess;
            set
            {
                if (value)
                    ExperimentalProcess = true;
            }
        } 
        public List<string> Languages => _model.Languages;
        public KeyModifierCollection KeyModifiers => _model.KeyModifiers;
        public Visibility PreviewButtonVisibility => _model.PreviewButtonVisibility;
        public Visibility LightVisibility => _model.LightVisibility;

        public Visibility ProVisibility => _model.ProVisibility;
        public Brush ProColor => _model.ProColor;
        public Brush LiteColor => _model.LiteColor;
        public FontWeight ProFont => _model.ProFont;
        public FontWeight LiteFont => _model.LiteFont;
        public MainSettingsVm()
        {
            _model.PropertyChanged+=(s,e)=>RaisePropertyChanged(e.PropertyName);
            CheckBackground = new DelegateCommand(_model.GenerateBackground);
            Preview = new DelegateCommand(_model.Preview);
            ChooseForegroundPath = new DelegateCommand(_model.ChooseForegroundPath);
            //SaveSettings = new DelegateCommand(_model.SaveSettings);
        }
    }
}
