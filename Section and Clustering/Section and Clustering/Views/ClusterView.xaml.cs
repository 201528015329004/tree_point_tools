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
using Section_and_Clustering.ViewModels;

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

        }
    }
}
