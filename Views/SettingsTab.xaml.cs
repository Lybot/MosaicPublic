using System.Windows.Controls;
using MozaikaApp.ViewModels;

namespace MozaikaApp.Views
{
    /// <summary>
    /// Логика взаимодействия для SettingsTab.xaml
    /// </summary>
    public partial class SettingsTab : UserControl
    {
        public SettingsTab()
        {
            var vm = new SettingTabVm();
            DataContext = vm;
            InitializeComponent();
        }
    }
}
