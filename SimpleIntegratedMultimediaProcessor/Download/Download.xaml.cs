using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
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

namespace SimpleIntegratedMultimediaProcessor.Download
{
    /// <summary>
    /// Interaction logic for Download.xaml
    /// </summary>
    public partial class Download : UserControl
    {
        public DownloadModel Model;

        public Download()
        {
            InitializeComponent();
        }
        private void Download_Click(object sender, RoutedEventArgs e)
        {
            Model?.DoDownload();
        }

        private void DownloadBrowse_Click(object sender, RoutedEventArgs e)
        {
            var bb = new CommonOpenFileDialog();
            bb.IsFolderPicker = Model.UseDefaultFileName;
            bool res = bb.ShowDialog() == CommonFileDialogResult.Ok;

            if(res == true)
            {
                Model.DownloadPath = bb.FileName;
            }
        }


        private void Kill_Click(object sender, RoutedEventArgs e)
        {
            Model?.KillDownload();
        }
    }
}
