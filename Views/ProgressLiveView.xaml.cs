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
using MozaikaApp.ViewModels;

namespace MozaikaApp.Views
{
    /// <summary>
    /// Логика взаимодействия для ProgressLiveView.xaml
    /// </summary>
    public partial class ProgressLiveView : UserControl
    {
        public ProgressLiveView()
        {
            ProgressLiveModel.FullScreen = delegate
            {
                ZoomBox.UpdateLayout();
                ZoomBox.FitToBounds();
            };
            InitializeComponent();
            ZoomBox.Cursor = Cursors.Arrow;
        }
        private void UIElement_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var image = sender as Image;
            if (image?.Tag is Tuple<int, int> tag)
            {
                ProgressLiveModel.SelectedX = tag.Item1;
                ProgressLiveModel.SelectedY = tag.Item2;
                SelectedX.Text = tag.Item1.ToString();
                SelectedY.Text = tag.Item2.ToString();
            }
        }
    }
}
