using Ruler.Shared.Enums;
using Ruler.Shared.Models;
using Ruler.Wpf.ViewModels;

using System;
using System.Collections.Generic;
using System.Globalization;
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

namespace Ruler.Wpf.Windows
{
    /// <summary>
    /// Interaction logic for SetSizeWindow.xaml
    /// </summary>
    public partial class SetSizeWindow : Window
    {
        private readonly SetSizeViewModel _viewModel;

        public int UpdatedWidth => _viewModel.UpdatedWidth;
        public int UpdatedHeight => _viewModel.UpdatedHeight;
        public RulerUnit UpdatedUnit => _viewModel.UpdatedUnit;

        public SetSizeWindow(RulerInfo targetRuler)
        {
            InitializeComponent();

            double baseDpi = 96.0 * VisualTreeHelper.GetDpi(this).PixelsPerDip;
            _viewModel = new SetSizeViewModel(targetRuler, baseDpi);

            _viewModel.RequestClose = (dialogResult) =>
            {
                DialogResult = dialogResult;
                Close();
            };

            DataContext = _viewModel;
        }

        private void HandleTextBoxFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                textBox.SelectAll();
            }
        }
    }
}
