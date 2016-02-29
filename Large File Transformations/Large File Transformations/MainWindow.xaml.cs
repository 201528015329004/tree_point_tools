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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;

namespace Large_File_Transformations
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private AppViewModel viewModel;

        public MainWindow()
        {
            InitializeComponent();
            this.viewModel = new AppViewModel();
            this.DataContext = viewModel;
        }

        private void BrowseButton_OnClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            DialogResult dialogResult = dialog.ShowDialog();
            if (dialogResult == System.Windows.Forms.DialogResult.OK)
            {
                viewModel.InputFileName = dialog.FileName;
            }
        }

        private void ExecuteThinning_OnClick(object sender, RoutedEventArgs e)
        {
            this.viewModel.ExecuteTransformation();
        }
    }
}
