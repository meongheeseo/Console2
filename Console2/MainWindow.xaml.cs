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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using Microsoft.Win32;

namespace Console2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            //msgbox.AppendText("HI", "Red");
            position_gpbox.Visibility = Visibility.Hidden;
        }

        private void manu_shinwoo_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                position_gpbox.Visibility = Visibility.Hidden;
                pos_leu.Visibility = Visibility.Visible;
            }
            catch (Exception exception) { }
        }

        private void manu_bombardier_Checked(object sender, RoutedEventArgs e)
        {
            position_gpbox.Visibility = Visibility.Visible;
            pos_leu.Visibility = Visibility.Visible;
        }

        private void manu_siemens_Checked(object sender, RoutedEventArgs e)
        {
            position_gpbox.Visibility = Visibility.Hidden;
            pos_leu.Visibility = Visibility.Hidden;
        }
        
        //
        // Position Group
        //
        Boolean balise_checked = true;
        
        // ----- CHECKS IF BALISE IS SELECTED ----- //
        private void pos_balise_Checked(object sender, RoutedEventArgs e)
        {
            balise_checked = true;
        }

        private void pos_leu_Checked(object sender, RoutedEventArgs e)
        {
            balise_checked = false;
        }

        // ----- READ, WRITE, RESET BUTTON CONTROL ----- //
        private void posread_btn_Click(object sender, RoutedEventArgs e)
        {
            if (balise_checked)
            {
                MessageBox.Show("Connect Balise Cable", "Warning", MessageBoxButton.OKCancel);
            }
            else
            {
                MessageBox.Show("Connect LEU Cable", "Warning", MessageBoxButton.OKCancel);
            }
        }

        private void poswrite_btn_Click(object sender, RoutedEventArgs e)
        {
            if (balise_checked)
            {
                MessageBox.Show("Connect Balise Cable", "Warning", MessageBoxButton.OKCancel);
            }
            else
            {
                MessageBox.Show("Connect LEU Cable and Check the LEU Power", "Warning", MessageBoxButton.OKCancel);
            }
        }

        private void reset_btn_Click(object sender, RoutedEventArgs e)
        {
            countrycode_txt.Text = String.Empty;
            groupcode_txt.Text = String.Empty;
            poscode_txt.Text = String.Empty;
        }

        private void msgClear_btn_Click(object sender, RoutedEventArgs e)
        {
            msgbox.Document.Blocks.Clear();
        }

        private void msgSave_btn_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFile = new SaveFileDialog();

            // if you don't need to keep the color of the text, use *.txt instead.
            saveFile.Filter = "Rich Text Format (*.rtf)|*.rtf";
            saveFile.DefaultExt = "rtf";
            saveFile.AddExtension = true;

            if (saveFile.ShowDialog() == true && saveFile.FileName.Length > 0)
            {
                TextRange range;
                FileStream fStream;

                // if you don't need to keep the color of the text, use RichTextBoxStreamType.PlainText
                range = new TextRange(msgbox.Document.ContentStart, msgbox.Document.ContentEnd);
                fStream = new FileStream(saveFile.FileName, FileMode.Create);
                range.Save(fStream, DataFormats.Rtf);
                fStream.Close();
            }
        }
    }

    public static class RichTextBoxExtensions
    {
        public static void AppendText(this RichTextBox box, String text, String color)
        {
            BrushConverter bc = new BrushConverter();
            TextRange tr = new TextRange(box.Document.ContentEnd, box.Document.ContentEnd);
            tr.Text = text;
            try
            {
                tr.ApplyPropertyValue(TextElement.ForegroundProperty, bc.ConvertFromString(color));
            }
            catch (NotSupportedException) { }
        }
    }
}
