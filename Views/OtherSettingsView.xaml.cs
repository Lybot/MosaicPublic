using System.Windows.Controls;
using MozaikaApp.ViewModels;

namespace MozaikaApp.Views
{
    /// <summary>
    /// Interaction logic for OtherSettingsView.xaml
    /// </summary>
    public partial class OtherSettingsView : UserControl
    {
        public OtherSettingsView()
        {
            var vm = new OtherSettingsVm();
            InitializeComponent();
            DataContext = vm;
        }
    }
}
