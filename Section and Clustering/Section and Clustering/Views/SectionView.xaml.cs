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
using Section_and_Clustering.ViewModels;

namespace Section_and_Clustering.Views
{
    /// <summary>
    /// Interaction logic for SectionView.xaml
    /// </summary>
    public partial class SectionView : System.Windows.Controls.UserControl
    {
        private SectionViewModel viewModel;

        public SectionView()
        {
            InitializeComponent();
            this.viewModel = new SectionViewModel();
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

        private void ExecuteSectionOnClick(object sender, RoutedEventArgs e)
        {
            this.viewModel.ExecuteSectioning();
        }
    }
}
