using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MozaikaApp.Models;

namespace MozaikaApp.Views
{
    /// <summary>
    /// Логика взаимодействия для SeveralPhotosSettings.xaml
    /// </summary>
    public partial class SeveralPhotosSettings : Window
    {
        public SeveralPhotosSettings()
        {
            InitializeComponent();
            ZoomBox.Cursor = Cursors.Arrow;
            SeveralPhotosModel.CenterScreen = () =>
            {
                ZoomBox.UpdateLayout();
                ZoomBox.FitToBounds();
            };
        }
    }
}
