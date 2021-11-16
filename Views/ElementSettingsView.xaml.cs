using System.Windows.Controls;
using System.Windows.Input;
using MozaikaApp.Models;
using MozaikaApp.ViewModels;

namespace MozaikaApp.Views
{
    /// <summary>
    /// Логика взаимодействия для ElementSettingsView.xaml
    /// </summary>
    public partial class ElementSettingsView : UserControl
    {
        public ElementSettingsView()
        {
            var vm = new ElementSettingsVm();
            InitializeComponent();
            DataContext = vm;
            ZoomBox.Cursor = Cursors.Arrow;
            ElementSettingsModel.FullScreen = delegate { ZoomBox.FitToBounds(); };
        }
    }
}
