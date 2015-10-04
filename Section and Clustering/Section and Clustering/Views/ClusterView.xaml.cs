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
    /// Interaction logic for ClusterView.xaml
    /// </summary>
    public partial class ClusterView : UserControl
    {
        private ClusterViewModel viewModel;

        public ClusterView()
        {
            InitializeComponent();
            this.viewModel = new ClusterViewModel();
            this.DataContext = this.viewModel;
        }

        private void BrowseFileOnClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            DialogResult result = dialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                this.viewModel.InputFile = dialog.FileName;
            }
        }

        private void ExecuteClusteringOnClick(object sender, RoutedEventArgs e)
        {
            this.viewModel.ExecuteClustering();
        }
    }
}
