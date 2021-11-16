using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Threading;
using MozaikaApp.Models;
using MozaikaApp.Properties;
using MozaikaApp.Views;
using Prism.Mvvm;
using Application = System.Windows.Application;
using MessageBox = System.Windows.Forms.MessageBox;
using UserControl = System.Windows.Controls.UserControl;

namespace MozaikaApp.ViewModels
{
    public class MainWindowVm : BindableBase
    {
        private TabItem _selectedTab;
        public ObservableCollection<TabItem> Items { get; set; } = new ObservableCollection<TabItem>()
        {
            new TabItem(){Header = Application.Current.FindResource("PuzzleSettings")?.ToString()},
            new TabItem(){Header =Application.Current.FindResource("Billboard")?.ToString()},
            new TabItem(){Header = "Live"}
        };
        public string AppName => $"{Application.Current.FindResource("Puzzle")} v{Assembly.GetExecutingAssembly().GetName().Version.Major}.{Assembly.GetExecutingAssembly().GetName().Version.Minor}.{Assembly.GetExecutingAssembly().GetName().Version.Build}";
        public bool IsEditable => !Activation.WasStart;
        public UserControl Content { get; 
            set; } /*= new SettingsTab();*/
        public TabItem SelectedTab
        {
            get => _selectedTab;
            set
            {
                if (value==null)
                    return;
                _selectedTab = value;
                var firstHeader = Application.Current.FindResource("PuzzleSettings")?.ToString();
                var secondHeader = Application.Current.FindResource("Billboard")?.ToString();
                var thirdHeader = "Live";
                if (string.Equals(firstHeader,value.Header.ToString()))
                {
                    ClearResources?.Invoke(this,new EventArgs());
                    Content = null;
                    GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                    Content = new SettingsTab();
                }
                if (string.Equals(secondHeader, value.Header.ToString()))
                {
                    ClearResources?.Invoke(this, new EventArgs());
                    Content = null;
                    GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                    Content = new ElementSettingsView();
                }
                if (string.Equals(thirdHeader, value.Header.ToString()))
                {
                    if (string.IsNullOrEmpty(Settings.Default.WorkFolder) ||
                        string.IsNullOrEmpty(Settings.Default.ForegroundPath) || Settings.Default.HotFolders.Count == 0)
                    {
                        new Thread(() =>
                        {
                            MessageBox.Show(Functions.FindStringResource("SettingsError"), Functions.FindStringResource("Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                            _dispatcher.Invoke(delegate
                            {
                                Items[0].IsSelected = true;
                                RaisePropertyChanged($"Items");
                            });
                        }).Start();
                        SelectedTab = Items[0];
                        return;
                    }
                    ClearResources?.Invoke(this, new EventArgs());
                    Content = null;
                    GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                    Content = new ProgressLiveView();
                }
                RaisePropertyChanged($"Content");
            }
        }
        public static event EventHandler ClearResources;
        private Dispatcher _dispatcher;
        public MainWindowVm()
        {
            _dispatcher = Dispatcher.CurrentDispatcher;
            App.LanguageChanged += LanguageChanged;
            if (!string.IsNullOrEmpty(Settings.Default.WorkFolder) && Directory.Exists(Settings.Default.WorkFolder + "\\ForegroundPrintImages") && 
                Directory.Exists(Settings.Default.WorkFolder + "\\ForegroundViewImages") && Settings.Default.CountHeight != 0 && Settings.Default.CountWidth!=0)
            {
                int needCount = Settings.Default.CountHeight * Settings.Default.CountWidth;
                var dir1 = new DirectoryInfo(Settings.Default.WorkFolder + "\\ForegroundPrintImages");
                var dir2 = new DirectoryInfo(Settings.Default.WorkFolder + "\\ForegroundViewImages");
                if (dir1.GetFiles().Length == needCount && dir2.GetFiles().Length == needCount)
                {
                    Activation.IsEditable = false;
                    SelectedTab = Items[2];
                }
            }
            ProgressLiveModel.WasStart += WasStarted;
            ProgressLiveModel.UpdateTab += UpdateTab;
            if (!string.IsNullOrEmpty(Settings.Default.WorkFolder))
            {
                if (!Directory.Exists(Settings.Default.WorkFolder+"\\OverlaidImages"))
                {
                    Directory.CreateDirectory(Settings.Default.WorkFolder + "\\OverlaidImages");
                }
            }
        }

        private void UpdateTab(object sender, EventArgs e)
        {
            SelectedTab = SelectedTab;
        }

        private void LanguageChanged(object sender, EventArgs e)
        {
            Items[0]= new TabItem() {Header = Application.Current.FindResource("PuzzleSettings")?.ToString()};
            Items[1]= new TabItem() {Header = Application.Current.FindResource("Billboard")?.ToString()};
//Items[2]= new TabItem() {Header = "Live"}
            RaisePropertyChanged($"Items");
        }

        private void WasStarted(object sender, EventArgs e)
        {
            RaisePropertyChanged($"IsEditable");
        }
    }
}
