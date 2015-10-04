using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Section_and_Clustering.ViewModels;
using UserControl = System.Windows.Controls.UserControl;

namespace Section_and_Clustering.Views
{
    /// <summary>
    /// Interaction logic for ClusterProcessingView.xaml
    /// </summary>
    public partial class ClusterProcessingView : UserControl
    {

        private ClusterProcessingViewModel viewModel;

        public ClusterProcessingView()
        {
            InitializeComponent();
            this.viewModel = new ClusterProcessingViewModel();
            this.DataContext = this.viewModel;
        }

        private void BrowseFileOnClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Cluster File *.cluster | *.cluster";
            DialogResult result = dialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                this.viewModel.InputFile = dialog.FileName;
            }
        }

        private void ExecuteProcessingOnClick(object sender, RoutedEventArgs e)
        {
            this.viewModel.Process();
        }
    }
}
