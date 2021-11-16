using System.Windows.Input;
using MozaikaApp.Models;
using MozaikaApp.ViewModels;
using UserControl = System.Windows.Controls.UserControl;

namespace MozaikaApp.Views
{
    /// <summary>
    /// Логика взаимодействия для MainSettingsView.xaml
    /// </summary>
    public partial class MainSettingsView:UserControl
    {
        public MainSettingsView()
        {
            MainSettingsModel.FullScreen = delegate
            {
                PreviewImage.FitToBounds();
            };
            InitializeComponent();
            PreviewImage.Cursor = Cursors.Arrow;
            var vm = new MainSettingsVm();
            DataContext = vm;
        }
    }
}
