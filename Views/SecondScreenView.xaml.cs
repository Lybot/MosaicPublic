using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using MozaikaApp.ViewModels;

namespace MozaikaApp.Views
{
    /// <summary>
    /// Логика взаимодействия для SecondScreenView.xaml
    /// </summary>
    public partial class SecondScreenView : Window
    {
        public SecondScreenView(List<ImageVm> sources)
        {
            InitializeComponent();
            var vm = new SecondScreenVm(sources);
            DataContext = vm;
            KeyDown += SecondScreenView_KeyDown;
        }

        private void SecondScreenView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
        }
    }
}
