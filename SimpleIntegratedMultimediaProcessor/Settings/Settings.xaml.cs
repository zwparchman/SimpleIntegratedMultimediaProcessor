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

namespace SimpleIntegratedMultimediaProcessor.Settings
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : UserControl
    {

        public SettingsModel Model;

        public Settings()
        {
            InitializeComponent();
        }
        private void BrowseFfmpeg_Click(object sender, RoutedEventArgs e)
        {
            var bb = new OpenFileDialog();
            bb.Title = "Select ffmpeg file";
            bool? res = bb.ShowDialog();

            if(res == true)
            {
                Model.FFMpegPath = bb.FileName;
            }
        }

        private void BrowseDownloader_Click(object sender, RoutedEventArgs e)
        {
            var bb = new OpenFileDialog();
            bb.Title = "Select youtube-dl or replacement file";
            bool? res = bb.ShowDialog();

            if(res == true)
            {
                Model.DownloadProgram = bb.FileName;
            }
        }

        private void SaveSettings_Click(object sender, RoutedEventArgs e)
        {
            Model.Save();
        }

        private void RevertSettings_Click(object sender, RoutedEventArgs e)
        {
            Model.Revert();
        }
    }
}
