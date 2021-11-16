using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using MozaikaApp.Views;
using Prism.Mvvm;

namespace MozaikaApp.ViewModels
{
    public class SettingTabVm : BindableBase
    {
        private TabItem _selectedTab;
        public UserControl Content { get; set; }
        public TabItem SelectedTab
        {
            get => _selectedTab;
            set
            {
                _selectedTab = value;
                string firstHeader = Application.Current.FindResource("MainSettings")?.ToString();
                string secondHeader = Application.Current.FindResource("PathSettings")?.ToString();
                string thirdHeader = Application.Current.FindResource("PrintSettings")?.ToString();
                string forthHeader = Application.Current.FindResource("OtherSettings")?.ToString();
                string fifthHeader = Application.Current.FindResource("FillAlgorithm")?.ToString();
                string sixthHeader = Application.Current.FindResource("License")?.ToString();
                if (string.Equals(firstHeader, value.Header.ToString()))
                {
                    ClearResources?.Invoke(this, new EventArgs());
                    Content = null;
                    GC.Collect(GC.MaxGeneration, GCCollectionMode.Default);
                    Content = new MainSettingsView();
                }
                if (string.Equals(secondHeader, value.Header.ToString()))
                {
                    ClearResources?.Invoke(this, new EventArgs());
                    Content = null;
                    GC.Collect(GC.MaxGeneration, GCCollectionMode.Default);
                    Content = new ChoosePathView();
                }
                if (string.Equals(thirdHeader, value.Header.ToString()))
                {
                    ClearResources?.Invoke(this, new EventArgs());
                    Content = null;
                    GC.Collect(GC.MaxGeneration, GCCollectionMode.Default);
                    Content = new PrintSettingsView();
                }
                if (string.Equals(forthHeader, value.Header.ToString()))
                {
                    ClearResources?.Invoke(this, new EventArgs());
                    Content = null;
                    GC.Collect(GC.MaxGeneration, GCCollectionMode.Default);
                    Content = new OtherSettingsView();
                }
                if (string.Equals(fifthHeader, value.Header.ToString()))
                {
                    ClearResources?.Invoke(this, new EventArgs());
                    Content = null;
                    GC.Collect(GC.MaxGeneration, GCCollectionMode.Default);
                    Content = new FillAlgorithmView();
                }
                if (string.Equals(sixthHeader, value.Header.ToString()))
                {
                    ClearResources?.Invoke(this, new EventArgs());
                    Content = null;
                    GC.Collect(GC.MaxGeneration, GCCollectionMode.Default);
                    Content = new LicenseView();
                }
                RaisePropertyChanged($"Content");
            }
        }
        public static event EventHandler ClearResources;
    }
}
