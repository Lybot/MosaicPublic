using System.Windows.Controls;
using System.Windows.Input;
using MozaikaApp.Models;

namespace MozaikaApp.Views
{
    /// <summary>
    /// Логика взаимодействия для FillAlgorithmView.xaml
    /// </summary>
    public partial class FillAlgorithmView : UserControl
    {
        public FillAlgorithmView()
        {
            InitializeComponent();
            FillAlgorithmModel.FullScreen = () => 
            {
                ZoomBox.UpdateLayout();
                ZoomBox.FitToBounds();
            };
            ZoomBox.Cursor = Cursors.Arrow;
        }
    }
}
