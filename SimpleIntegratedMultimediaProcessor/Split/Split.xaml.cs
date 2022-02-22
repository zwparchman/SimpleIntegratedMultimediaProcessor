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

namespace SimpleIntegratedMultimediaProcessor.Split
{
    /// <summary>
    /// Interaction logic for Split.xaml
    /// </summary>
    public partial class Split : UserControl
    {
        public SplitModel Model;

        public Split()
        {
            InitializeComponent();
        }

        private void StartingFile_Click(object sender, RoutedEventArgs e)
        {
            var bb = new OpenFileDialog();
            bool? res = bb.ShowDialog();

            bb.Title = "Select starting file";

            if(res == true)
            {
                Model.StartFile = bb.FileName;
            }
        }

        private void OutputDirectory_Click(object sender, RoutedEventArgs e)
        {
            var bb = new CommonOpenFileDialog();
            bb.IsFolderPicker = true;
            bool res = bb.ShowDialog() == CommonFileDialogResult.Ok;

            if(res == true)
            {
                Model.OutputDirectory = bb.FileName;
            }
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            Model.StartSplitting();
        }

        private void Abort_Click(object sender, RoutedEventArgs e)
        {
            Model.AbortSplitting();
        }

        private void ClearErrors_Click(object sender, RoutedEventArgs e)
        {
            Model.Error = "";
        }
    }
}
