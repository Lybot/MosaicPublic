using System;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Markup;
using MozaikaApp.Models;
using Newtonsoft.Json;
using Prism.Commands;
using Prism.Mvvm;
using Xceed.Wpf.Toolkit;

namespace MozaikaApp.ViewModels
{
    public class OtherSettingsVm:BindableBase
    {
        private readonly OtherSettingsModel _model = new OtherSettingsModel();
        public ObservableCollection<string> QualityTypes = new ObservableCollection<string>(){"High quality", "Normal quality"};
        public bool UseSerialLink
        {
            get => _model.UseSerialLink;
            set
            {
                _model.UseSerialLink = value;
                RaisePropertyChanged($"SerialLinkVisibility");
            }
        }
        public string SerialLinkFolder
        {
            get => _model.SerialLinkFolder;
            set => _model.SerialLinkFolder=value;
        }
        public string CountFrequencyLink
        {
            get => _model.CountFrequencyLink.ToString();
            set
            {
                if (int.TryParse(value, out var result))
                    _model.CountFrequencyLink = result;
            }
        }
        public string ServerFolder { 
            get => _model.ServerFolder;
            set => _model.ServerFolder = value;
        }
        public string PreviewHtmlText { 
            get => _model.PreviewHtmlText;
            set => _model.PreviewHtmlText = value;
        } 
        public bool SerialLinkQuality
        {
            get => _model.SerialLinkQuality;
            set
            {
                if (value)
                    _model.SerialLinkQuality = true;
            }
        }
        public bool NormalQuality
        {
            get => !_model.SerialLinkQuality;
            set
            {
                if (value)
                    _model.SerialLinkQuality = false;
            }
        }
        public bool SerialLinkAddresses
        {
            get => _model.SerialLinkAddresses;
            set => _model.SerialLinkAddresses = value;
        }
        public bool ImageVisibility { 
            get => _model.ImageVisibility;
            set => _model.ImageVisibility = value;
        }
        public bool ScreenSaver
        {
            get => _model.ScreenSaver;
            set => _model.ScreenSaver = value;
        }

        public bool LinkButton
        {
            get => _model.LinkButton;
            set => _model.LinkButton=value;
        }
        public bool AddPhotoButton
        {
            get => _model.AddPhotoButton;
            set => _model.AddPhotoButton = value;
        }
        public string AddButtonText
        {
            get => _model.AddButtonText;
            set => _model.AddButtonText = value;
        }
        public bool InstagramPhotoLoading
        {
            get => _model.InstagramPhotoLoading;
            set
            {
                _model.InstagramPhotoLoading = value; 
                RaisePropertyChanged($"InstagramSettingsVisibility");
            }
        }
        public bool InstagramAuthorization
        {
            get => _model.InstagramAuthorization;
            set
            {
                _model.InstagramAuthorization = value; 
                RaisePropertyChanged($"InstagramLoginVisibility");
            }
        }
        public DateTime InstagramAfter
        {
            get => _model.InstagramAfter;
            set => _model.InstagramAfter = value;
        }
        public DateTime InstagramBefore
        {
            get => _model.InstagramBefore;
            set => _model.InstagramBefore = value;
        }
        public int InstagramUpdateMinutes
        {
            get => _model.InstagramUpdateMinutes;
            set => _model.InstagramUpdateMinutes = value;
        }
        public string InstagramLogin
        {
            get => _model.InstagramLogin;
            set => _model.InstagramLogin = value;
        }
        public string InstagramPassword
        {
            get => _model.InstagramPassword;
            set => _model.InstagramPassword = value;
        }
        public string LinkAddress
        {
            get => _model.LinkAddress;
            set => _model.LinkAddress = value;
        }

        public string LinkText
        {
            get => _model.LinkText;
            set => _model.LinkText = value;
        }

        public string BackgroundImagePath
        {
            get => _model.BackgroundImagePath;
            set => _model.BackgroundImagePath = value;
        }

        public ObservableCollection<Hashtag> InstagramHashtags => _model.InstagramHashtags;
        public ObservableCollection<Question> Questions => _model.Questions;

        public Visibility InstagramSettingsVisibility =>
            _model.InstagramPhotoLoading ? Visibility.Visible : Visibility.Collapsed;
        public Visibility InstagramLoginVisibility =>
            _model.InstagramAuthorization? Visibility.Visible : Visibility.Collapsed;
        public Visibility SerialLinkVisibility => _model.UseSerialLink ? Visibility.Visible : Visibility.Collapsed;
        public Visibility ServerSettingsVisibility => _model.ServerSettingsVisibility; 
        public DelegateCommand ChooseServerFolder { get; set; }
        public DelegateCommand ChooseFolder { get; set; }
        public DelegateCommand AddQuestion { get; set; }
        public DelegateCommand SaveSettings { get; set; }
        public DelegateCommand ChooseLogo { get; set; }
        public DelegateCommand ChooseBackgroundImage { get; set; }
        public DelegateCommand AddHashtag { get; set; }
        public OtherSettingsVm()
        {
            _model.PropertyChanged += (s, e) => RaisePropertyChanged(e.PropertyName);
            ChooseServerFolder = new DelegateCommand(_model.ChooseServerFolder);
            ChooseFolder = new DelegateCommand(_model.ChooseFolder);
            AddQuestion = new DelegateCommand(_model.AddQuestion);
            SaveSettings = new DelegateCommand(_model.SaveSettings);
            ChooseLogo = new DelegateCommand(_model.ChooseLogo);
            ChooseBackgroundImage = new DelegateCommand(_model.ChooseBackgroundImagePath);
            AddHashtag = new DelegateCommand(_model.AddHashTag);
            //NormalQuality = !_model.SerialLinkQuality;
        }
    }
    public class Question:BindableBase
    {
        private string _text;
        [JsonProperty]
        public string Text { 
            get => _text; 
            set { 
                _text = value; 
                RaisePropertyChanged(); 
            } 
        }
        private int _size { get; set; }
        [JsonProperty]
        public string SizeString { get => _size.ToString();
            set { 
                if (int.TryParse(value, out var result))
                {
                    if (result > 0 && result <= 6)
                    {
                        _size = result;
                        RaisePropertyChanged();
                    }
                }
            }
        }
        private int _lineNumber;
        [JsonProperty]
        public string LineNumber { get => _lineNumber.ToString();
            set
            {
                if (int.TryParse(value,out var result))
                {
                    if (result >= 1 && result < 10)
                    {
                        _lineNumber = result;
                        RaisePropertyChanged();
                    }
                }
            }
        }
        [JsonIgnore]
        public DelegateCommand<Question> Delete { get; set; }
        [JsonIgnore]
        public Question Info => this;
    }
    public class Hashtag : BindableBase
    {
        private string _tag;

        public string Tag
        {
            get => _tag;
            set
            {
                _tag = value;
                RaisePropertyChanged();
            }
        }
        public DelegateCommand<Hashtag> Delete { get; set; }
        public Hashtag Info => this;
    }
}
