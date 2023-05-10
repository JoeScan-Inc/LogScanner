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

namespace RawViewer.Grid
{
    /// <summary>
    /// Interaction logic for RawProfileGridView.xaml
    /// </summary>
    public partial class RawProfileGridView : UserControl
    {
        public RawProfileGridView()
        {
            InitializeComponent();
        }

        private void DataGrid_OnSorting(object sender, DataGridSortingEventArgs e)
        {
            
        }
    }
}
