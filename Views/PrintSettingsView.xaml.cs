using System.Windows.Controls;
using System.Windows.Input;
using MozaikaApp.Models;

namespace MozaikaApp.Views
{
    /// <summary>
    /// Логика взаимодействия для PrintSettingsView.xaml
    /// </summary>
    // ReSharper disable once RedundantExtendsListEntry
    public partial class PrintSettingsView : UserControl
    {
        public PrintSettingsView()
        {
            PrintSettingsModel.FullScreen = delegate
            {
                ZoomBox.FitToBounds();
            };
            InitializeComponent();
            ZoomBox.Cursor = Cursors.Arrow;
        }
    }
}
