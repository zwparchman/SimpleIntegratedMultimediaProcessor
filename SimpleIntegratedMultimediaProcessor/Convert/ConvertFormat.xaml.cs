using Microsoft.Win32;
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

namespace SimpleIntegratedMultimediaProcessor.Convert
{
    /// <summary>
    /// Interaction logic for ConvertFormat.xaml
    /// </summary>
    public partial class ConvertFormat : UserControl
    {
        public ConvertFormat()
        {
            InitializeComponent();
        }

        public ConvertModel Model;

        private void Abort_Click(object sender, RoutedEventArgs e)
        {
            Model.KillConversion();
        }

        private void Convert_Click(object sender, RoutedEventArgs e)
        {
            Model.DoConversion();
        }

        private void OpenBrowse_Click(object sender, RoutedEventArgs e)
        {
            var bb = new OpenFileDialog();
            bool? res = bb.ShowDialog();

            bb.Title = "Select starting file";

            if(res == true)
            {
                Model.StartFile = bb.FileName;
            }
        }

        private void SaveBrowse_Click(object sender, RoutedEventArgs e)
        {
            var bb = new SaveFileDialog();
            bool? res = bb.ShowDialog();
            bb.Title = "Select ending file";

            if(res == true)
            {
                Model.EndFile = bb.FileName;
            }
        }

    }
}
