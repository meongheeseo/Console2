using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using Microsoft.Win32; // SaveFileDialog
using System.IO; // File

namespace Console2
{
    /// <summary>
    /// Interaction logic for DecodeWindow.xaml
    /// </summary>
    public partial class DecodeWindow : Window
    {
        public DecodeWindow()
        {
            InitializeComponent();
        }

        private void save_button_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFile = new SaveFileDialog();

            // if you don't need to keep the color of the text, use *.txt instead.
            saveFile.Filter = "Text (*.txt)|*.txt";
            saveFile.DefaultExt = "txt";
            saveFile.AddExtension = true;

            if (saveFile.ShowDialog() == true && saveFile.FileName.Length > 0)
            {
                File.WriteAllText(saveFile.FileName, decode_textBox.Text);
            }
        }

        private void clear_button_Click(object sender, RoutedEventArgs e)
        {
            decode_textBox.Clear();
        }
    }
}
