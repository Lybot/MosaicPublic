using System.Windows;
using MozaikaApp.Models;

namespace MozaikaApp
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Closing += ProgressLiveModel.MainWindow_Closing;
            LicenseModel.CloseWindow = Close;
        }
    }
}
