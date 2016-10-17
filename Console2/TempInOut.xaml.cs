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

using Microsoft.Windows.Controls; // WPF Graph
using System.IO; // FileStream
using Microsoft.Win32; // SaveFileDialog

namespace Console2
{
    /// <summary>
    /// Interaction logic for TempInOut.xaml
    /// </summary>
    public partial class TempInOut : Window
    {
        public TempInOut()
        {
            InitializeComponent();
            progressLabel_textBlock.Text = String.Format("{0}%", progressBar.Value);
        }

        private void reset_button_Click(object sender, RoutedEventArgs e)
        {
            rawdata_richTextBox.Document.Blocks.Clear();
        }

        private void save_button_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();

            // Save filter
            saveFileDialog.Filter = "Text File (*.txt)|*.txt";
            saveFileDialog.DefaultExt = "txt";
            saveFileDialog.AddExtension = true;

            if (saveFileDialog.ShowDialog() == true && saveFileDialog.FileName.Length > 0)
            {
                TextRange range;
                FileStream fStream = new FileStream(saveFileDialog.FileName, FileMode.Create);

                AddText(fStream, Properties.Settings.Default.OptionsMin.ToString(), false);
                AddText(fStream, Properties.Settings.Default.OptionsMax.ToString(), false);
                AddText(fStream, Properties.Settings.Default.OptionsSteps.ToString(), false);
                AddText(fStream, Properties.Settings.Default.OptionsInterval.ToString(), true);

                range = new TextRange(rawdata_richTextBox.Document.ContentStart, rawdata_richTextBox.Document.ContentEnd);
                range.Save(fStream, DataFormats.Text);
                fStream.Close();
            }
        }

        private void options_button_Click(object sender, RoutedEventArgs e)
        {
            Options options = new Options();
            options.ShowDialog();
        }

        private static void AddText(FileStream fs, String value, Boolean isNewline)
        {
            byte[] info = new UTF8Encoding(true).GetBytes(value);
            fs.Write(info, 0, info.Length);

            if (isNewline)
            {
                byte[] newline = Encoding.ASCII.GetBytes(Environment.NewLine);
                fs.Write(newline, 0, newline.Length);
            }
            else
            {
                info = new UTF8Encoding(true).GetBytes(", ");
                fs.Write(info, 0, info.Length);
            }
        }

        private void progressBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            progressLabel_textBlock.Text = String.Format("{0}%", progressBar.Value);
        }
    }
}
