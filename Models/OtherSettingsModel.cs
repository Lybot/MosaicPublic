using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net.Configuration;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using MozaikaApp.Properties;
using MozaikaApp.ViewModels;
using MozaikaApp.Views;
using Newtonsoft.Json;
using Prism.Commands;
using Prism.Mvvm;

namespace MozaikaApp.Models
{
    public class OtherSettingsModel: BindableBase
    {
        public string SerialLinkFolder
        {
            get => Settings.Default.SerialLinkFolder;
            set
            {
                Settings.Default.SerialLinkFolder = value;
                Settings.Default.Save();
                RaisePropertyChanged();
            }
        }
        public string ServerFolder { 
            get => Settings.Default.ServerFolder;
            set { 
                Settings.Default.ServerFolder = value;
                Settings.Default.Save();
                RaisePropertyChanged();
                RaisePropertyChanged("ServerSettingsVisibility");
            }
        }
        private string _previewHtmlText;
        public string PreviewHtmlText
        {
            get => _previewHtmlText;
            set
            {
                _previewHtmlText = value;
                RaisePropertyChanged();
                SaveSettings();
            }
        }
        public int CountFrequencyLink
        {
            get => Settings.Default.CountFrequencyLink;
            set
            {
                Settings.Default.CountFrequencyLink=value;
                Settings.Default.Save();
                RaisePropertyChanged();
            }
        }
        public bool UseSerialLink
        {
            get => Settings.Default.UseSerialLink;
            set
            {
                Settings.Default.UseSerialLink = value;
                Settings.Default.Save();
                RaisePropertyChanged();
            }
        }
        public bool SerialLinkQuality
        {
            get
            {
                return Settings.Default.SerialLinkQuality;
            }
            set
            {
                Settings.Default.SerialLinkQuality = value;
                Settings.Default.Save();
                RaisePropertyChanged();
                RaisePropertyChanged($"NormalQuality");
            }
        }
        public bool SerialLinkAddresses
        {
            get => Settings.Default.SerialLinkAddresses;
            set
            {
                Settings.Default.SerialLinkAddresses = value;
                Settings.Default.Save();
                RaisePropertyChanged();
            }
        }
        private bool _screenSaver;
        private bool _linkButton;
        public bool InstagramPhotoLoading
        {
            get => Settings.Default.InstagramPhotoLoading;
            set
            {
                Settings.Default.InstagramPhotoLoading = value;
                Settings.Default.Save();
                RaisePropertyChanged();
            }
        }

        public bool InstagramAuthorization
        {
            get => Settings.Default.InstagramAuthorization;
            set
            {
                Settings.Default.InstagramAuthorization = value;
                Settings.Default.Save();
                SaveCommandString();
                RaisePropertyChanged();
            }
        }
        public DateTime InstagramAfter
        {
            get => Settings.Default.InstagramAfter;
            set
            {
                Settings.Default.InstagramAfter = value;
                Settings.Default.Save();
                SaveCommandString();
                RaisePropertyChanged();
            }
        }

        public DateTime InstagramBefore
        {
            get => Settings.Default.InstagramBefore;
            set
            {
                Settings.Default.InstagramBefore = value;
                Settings.Default.Save();
                SaveCommandString();
                RaisePropertyChanged();
            }
        }

        public int InstagramUpdateMinutes
        {
            get => Settings.Default.InstagramUpdateMinutes;
            set
            {
                Settings.Default.InstagramUpdateMinutes = value;
                Settings.Default.Save();
                RaisePropertyChanged();
            }
        }

        public string InstagramLogin
        {
            get => Settings.Default.InstagramLogin;
            set
            {
                Settings.Default.InstagramLogin = value;
                Settings.Default.Save();
                SaveCommandString();
                RaisePropertyChanged();
            }
        }
        public string InstagramPassword
        {
            get => Settings.Default.InstagramPassword;
            set
            {
                Settings.Default.InstagramPassword = value;
                Settings.Default.Save();
                SaveCommandString();
                RaisePropertyChanged();
            }
        }
        private string InstagramCommandString
        {
            get => Settings.Default.InstagramCommandString; 
            set
            {
                Settings.Default.InstagramCommandString = value;
                Settings.Default.Save();
            }
        }
        private ObservableCollection<Hashtag> _instagramHashtags;

        public ObservableCollection<Hashtag> InstagramHashtags
        {
            get => _instagramHashtags;
            set
            {
                _instagramHashtags = value;
                SaveTags();
                RaisePropertyChanged();
            }
        }
        public bool ImageVisibility { 
            get => Settings.Default.BackgroundImageVisibility;
            set
            {
                Settings.Default.BackgroundImageVisibility= value;
                Settings.Default.Save();
                RaisePropertyChanged();
            }
        }
        public bool ScreenSaver
        {
            get => _screenSaver;
            set
            {
                _screenSaver = value;
                RaisePropertyChanged();
                SaveSettings();
            }
        }

        public bool LinkButton
        {
            get => _linkButton;
            set
            {
                _linkButton = value;
                RaisePropertyChanged();
                SaveSettings();
            }
        }
        private bool _addPhotoButton;
        public bool AddPhotoButton 
        { 
            get => _addPhotoButton;
            set
            {
                _addPhotoButton = value;
                RaisePropertyChanged();
                SaveSettings();
            }
        }
        private string _addButtonText;
        public string AddButtonText
        {
            get => _addButtonText;
            set
            {
                _addButtonText = value;
                RaisePropertyChanged();
                SaveSettings();
            }
        }
        private string _linkAddress;
        private string _linkText;
        public string LinkAddress
        {
            get => _linkAddress;
            set
            {
                _linkAddress = value;
                RaisePropertyChanged();
                SaveSettings();
            }
        }
        public string LinkText
        {
            get => _linkText;
            set
            {
                _linkText = value;
                RaisePropertyChanged();
                SaveSettings();
            }
        }

        public string BackgroundImagePath
        {
            get => Settings.Default.BackgroundImagePath;
            set
            {
                Settings.Default.BackgroundImagePath = value;
                Settings.Default.Save();
                RaisePropertyChanged();
            }
        }

        public Visibility ServerSettingsVisibility => string.IsNullOrEmpty(ServerFolder) ? Visibility.Collapsed : Visibility.Visible;
        private ObservableCollection<Question> _questions = new ObservableCollection<Question>();
        public ObservableCollection<Question> Questions { get => _questions; set { _questions = value; RaisePropertyChanged(); SaveSettings(); } }

        public void SaveCommandString()
        {
            var builder = new StringBuilder();
            //builder.Append("/C "+Environment.CurrentDirectory+"\\instaloader ");
            if (InstagramAuthorization && !string.IsNullOrEmpty(InstagramLogin) &&
                !string.IsNullOrEmpty(InstagramPassword))
            {
                builder.Append($"--login={InstagramLogin} --password={InstagramPassword} ");
            }
            builder.Append($"--fast-update --no-videos --no-captions --no-profile-pic --no-metadata-json --no-compress-json --post-filter \"date_local >= datetime({InstagramAfter.Year},{InstagramAfter.Month},{InstagramAfter.Day},{InstagramAfter.Hour},{InstagramAfter.Minute}) and date_local <= datetime({InstagramBefore.Year}, {InstagramBefore.Month}, {InstagramBefore.Day}, {InstagramBefore.Hour}, {InstagramBefore.Minute})\" ");
            InstagramCommandString = builder.ToString();
        }
        public void SaveTags()
        {
            var collection = new StringCollection();
            foreach (var hashtag in InstagramHashtags)
            {
                if (hashtag.Tag != "#"&& !string.IsNullOrEmpty(hashtag.Tag)) 
                    collection.Add(hashtag.Tag);
            }
            Settings.Default.InstagramHashTags = collection;
            Settings.Default.Save();
            RaisePropertyChanged();
        }
        public void LoadTags()
        {
            var collections = new ObservableCollection<Hashtag>();
            if (Settings.Default.InstagramHashTags == null)
            {
                Settings.Default.InstagramHashTags = new StringCollection();
                Settings.Default.Save();
            }
            foreach (var hashTag in Settings.Default.InstagramHashTags)
            {
                var listItem = new Hashtag
                {
                    Tag = hashTag, Delete = new DelegateCommand<Hashtag>(hash => collections.Remove(hash))
                };
                collections.Add(listItem);
            }
            if (collections.Count==0)
            { 
                var listItem = new Hashtag
                {
                    Tag = "#",
                    Delete = new DelegateCommand<Hashtag>(hash => collections.Remove(hash))
                };
                collections.Add(listItem);
            }
            _instagramHashtags = collections;
            if (InstagramAfter == DateTime.MinValue)
                InstagramAfter = DateTime.Now;
            if (InstagramBefore == DateTime.MinValue)
                InstagramBefore = DateTime.Now.AddDays(1);
        }
        public void AddHashTag()
        {
            var hashTag = new Hashtag
            {
                Tag = "#", Delete = new DelegateCommand<Hashtag>(hash => InstagramHashtags.Remove(hash))
            };
            InstagramHashtags.Add(hashTag);
            SaveTags();
        }
        public void ChooseBackgroundImagePath()
        {
            var dialog = new OpenFileDialog { DefaultExt = ".png|.bmp|.jpg" };
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                BackgroundImagePath = dialog.FileName;
                if (!string.IsNullOrEmpty(ServerFolder))
                {
                    try
                    {
                        File.Copy(dialog.FileName, ServerFolder + "\\ClientApp\\build\\backgroundImage.png", true);
                    }
                    catch
                    {
                        //ignored
                    }
                }

                if (!string.IsNullOrEmpty(Settings.Default.WorkFolder))
                {
                    try
                    {
                        File.Copy(dialog.FileName, Settings.Default.WorkFolder + "\\SavedForegroundImage\\backgroundImage.png", true);
                    }
                    catch
                    {
                        //ignored
                    }
                }
            }
        }
        public void ChooseServerFolder()
        {
            var dialog = new FolderBrowserDialog { RootFolder = Environment.SpecialFolder.MyComputer };
            if (dialog.ShowDialog()== DialogResult.OK)
            {
                var chooseModel = new ChoosePathModel();
                chooseModel.WorkFolder = dialog.SelectedPath + "\\ClientApp\\build";
                Settings.Default.HotFolders = new System.Collections.Specialized.StringCollection() { dialog.SelectedPath + "\\ClientApp\\build\\WorkFolder" };
                Settings.Default.Save();
                ServerFolder = dialog.SelectedPath;
                LoadSettings();
            }
        }
        public void AddQuestion()
        {
            var question = new Question()
            {
                SizeString = "5",
                Text = "",
                LineNumber = "1",
                Delete = new DelegateCommand<Question>((item)=> { Questions.Remove(item); })
            };
            Questions.Add(question);
            SaveSettings();
        }
        public void AddQuestion(Question quest)
        {
            var question = new Question()
            {
                SizeString = quest.SizeString,
                Text = quest.Text,
                LineNumber = quest.LineNumber,
                Delete = new DelegateCommand<Question>((item) => { Questions.Remove(item); })
            };
            Questions.Add(question);
            SaveSettings();
        }
        public void ChooseFolder()
        {
            var dialog = new FolderBrowserDialog { RootFolder = Environment.SpecialFolder.MyComputer };
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                SerialLinkFolder = dialog.SelectedPath;
            }
        }
        public void LoadSettings()
        {
            if (!string.IsNullOrEmpty(ServerFolder))
            {
                try
                {
                    _questions = new ObservableCollection<Question>();
                    _previewHtmlText = "";
                    var jsonText = File.ReadAllText(ServerFolder + "\\ClientApp\\build\\settingsMosaic.json");
                    var json = JsonConvert.DeserializeObject<SettingsServer>(jsonText);
                    foreach (var quest in json.Questions)
                    {
                        quest.Delete = new DelegateCommand<Question>((item) => { Questions.Remove(item); });
                        _questions.Add(quest);
                    }
                    _screenSaver = json.ScreenSaver;
                    ImageVisibility = json.ImageVisibility;
                    _previewHtmlText = json.PreviewText;
                    _linkButton = json.LinkButton.Enabled;
                    _linkAddress = json.LinkButton.Link;
                    _linkText = json.LinkButton.Text;
                    _addPhotoButton = json.AddPhotoButton.Enabled;
                    _addButtonText = json.AddPhotoButton.Text;
                    RaisePropertyChanged($"LinkButton");
                    RaisePropertyChanged($"LinkAddress");
                    RaisePropertyChanged($"LinkText");
                    RaisePropertyChanged($"AddPhotoButton");
                    RaisePropertyChanged($"AddButtonText");
                    RaisePropertyChanged($"ScreenSaver");
                    RaisePropertyChanged($"ImageVisibility");
                    RaisePropertyChanged($"PreviewHtmlText");
                    RaisePropertyChanged($"Questions");
                }
                catch
                {
                    if (!Directory.Exists(ServerFolder + "\\ClientApp"))
                    {
                        System.Windows.MessageBox.Show("Incorrect folder, can't find server", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        ServerFolder = "";
                    }
                    AddQuestion();
                }
            }
        }
        public void SaveSettings()
        {
            var size = PhotoProcessing.CalculatePixels(Settings.Default.WidthMm, Settings.Default.HeightMm, Settings.Default.ViewDpi, Settings.Default.CountWidth, Settings.Default.CountHeight);
            var linkButton = new LinkButtonInfo()
            {
                Enabled = LinkButton,
                Link = LinkAddress,
                Text = LinkText
            };
            var addPhotoButton = new AddButtonInfo()
            {
                Enabled = AddPhotoButton,
                Text = AddButtonText
            };
            var settings = new SettingsServer()
            {
                CountHeight = Settings.Default.CountHeight,
                CountWidth = Settings.Default.CountWidth,
                FullHeight = size.Height,
                FullWidth = size.Width,
                Questions = Questions.ToList(),
                PreviewText = PreviewHtmlText,
                ImageVisibility = ImageVisibility,
                ScreenSaver = ScreenSaver,
                LinkButton = linkButton,
                AddPhotoButton = addPhotoButton
            };
            var jsonText = JsonConvert.SerializeObject(settings);
            File.WriteAllText(ServerFolder + "\\ClientApp\\build\\settingsMosaic.json", jsonText);
        }
        public void ChooseLogo()
        {
            var dialog = new OpenFileDialog {DefaultExt = ".png|.bmp|.jpg"};
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                File.Copy(dialog.FileName, ServerFolder + "\\ClientApp\\build\\previewImage.png",true);
            }
        }
        public OtherSettingsModel()
        {
            LoadSettings();
            LoadTags();
            SettingTabVm.ClearResources += SaveSettings;
        }

        private void SaveSettings(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(ServerFolder))
                SaveSettings();
            if (InstagramPhotoLoading)
            {
                SaveTags();
                SaveCommandString();
            }
            //SettingTabVm.ClearResources -= SaveSettings;
        }
    }
}
